using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

// Kurulum IAsyncLifetime ile beklenir, kurucuda GetAwaiter().GetResult() ile değil: xUnit paralel
// sınıfları çekirdek sayısı kadar iş parçacığıyla çalıştırır; hepsi kurucuda bloklanırsa bekledikleri
// devam adımını çalıştıracak iş parçacığı kalmaz ve test takımı takılır.
public sealed class SqlitePeriodSettlementInstrumentTests : IAsyncLifetime
{
    private static readonly DateOnly PeriodStart = new(2026, 9, 1);
    private static readonly DateOnly PeriodEnd = new(2026, 10, 1);

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_settlement_{Guid.NewGuid():N}.db3");
    private readonly Guid _currentSnapshotId = Guid.NewGuid();
    private readonly Guid _currentPlanId = Guid.NewGuid();
    private SQLiteAsyncConnection _connection = null!;

    public async Task InitializeAsync()
    {
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        await new DatabaseSchema().EnsureInitializedAsync(_connection);
        await new SqlitePeriodHistoryRepository(_connection).SaveCurrentFinancialSnapshotAsync(
            Snapshot(_currentSnapshotId, PeriodStart), Plan(_currentPlanId, _currentSnapshotId, PeriodStart));
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* cleanup */ }
        }
    }

    [Fact]
    public async Task CommitPeriodSettlementAsync_KrediGuncellenince_ErkenOdemeleriKorunurYalnizKaldirilanSilinir()
    {
        var loans = new SqliteLoanRepository(_connection);
        var loan = new Loan
        {
            Id = Guid.NewGuid(), Name = "Taşıt Kredisi", Bank = "Yapı Kredi", MonthlyPayment = 10000m, PaymentDay = 5,
            NextPaymentDate = new DateOnly(2026, 9, 5), RemainingInstallmentCount = 24
        };
        var used = new LoanPrepayment { Id = Guid.NewGuid(), LoanId = loan.Id, Date = new DateOnly(2026, 9, 5), Mode = LoanPrepaymentMode.ReduceTerm, PrincipalAmount = 20000m };
        var planned = new LoanPrepayment { Id = Guid.NewGuid(), LoanId = loan.Id, Date = new DateOnly(2027, 3, 5), Mode = LoanPrepaymentMode.FullClosure };
        await loans.UpsertLoanWithPrepaymentsAsync(loan, [used, planned]);
        var paidLoan = loan with { NextPaymentDate = new DateOnly(2026, 10, 5), RemainingInstallmentCount = 23 };

        await Commit(new PeriodSettlementCommit { UpdatedLoans = [paidLoan], RemovedLoanPrepaymentIds = [used.Id] });

        Assert.Equal(23, Assert.Single(await loans.GetLoansAsync()).RemainingInstallmentCount);
        Assert.Equal([planned], await loans.GetLoanPrepaymentsAsync());
    }

    [Fact]
    public async Task CommitPeriodSettlementAsync_OdemePlaniGuncellenince_TaksitleriYeniHalleriyleYazar()
    {
        var plans = new SqliteTemporaryPaymentPlanRepository(_connection);
        var planId = Guid.NewGuid();
        var september = new TemporaryPaymentInstallment { Id = Guid.NewGuid(), PlanId = planId, DueDate = new DateOnly(2026, 9, 10), Amount = 6000m };
        var october = new TemporaryPaymentInstallment { Id = Guid.NewGuid(), PlanId = planId, DueDate = new DateOnly(2026, 10, 10), Amount = 6000m };
        var plan = new TemporaryPaymentPlan { Id = planId, Name = "Mobilya", Kind = PaymentPlanKind.Installment, Installments = [september, october] };
        await plans.UpsertPaymentPlanAsync(plan);
        var settled = plan with { Installments = [september with { IsPaid = true }, october] };

        await Commit(new PeriodSettlementCommit { UpdatedPaymentPlans = [settled] });

        var saved = Assert.Single(await plans.GetPaymentPlansAsync());
        Assert.Equal([september with { IsPaid = true }, october], saved.Installments);
    }

    [Fact]
    public async Task CommitPeriodSettlementAsync_KartGuncellenince_KartiVeHarcamalariniYazar()
    {
        var cards = new SqliteCreditCardRepository(_connection);
        var cardId = Guid.NewGuid();
        var charge = new CardCharge { Id = Guid.NewGuid(), CreditCardId = cardId, Description = "Market", PostingDate = new DateOnly(2026, 10, 3), Amount = 1500m };
        var card = new CreditCard
        {
            Id = cardId, Name = "Bonus", Bank = "Garanti", Limit = 50000m, CarriedBalance = 8000m,
            BalanceAsOfDate = new DateOnly(2026, 9, 1), StatementClosingDay = 20, PaymentDueDay = 1, Charges = [charge]
        };
        await cards.UpsertCreditCardAsync(card);
        var reconciled = card with { CarriedBalance = 3000m, BalanceAsOfDate = PeriodEnd };

        await Commit(new PeriodSettlementCommit { UpdatedCreditCards = [reconciled] });

        var saved = Assert.Single(await cards.GetCreditCardsAsync());
        Assert.Equal(3000m, saved.CarriedBalance);
        Assert.Equal(PeriodEnd, saved.BalanceAsOfDate);
        Assert.Equal([charge], saved.Charges);
    }

    private Task Commit(PeriodSettlementCommit instruments)
    {
        var nextSnapshotId = Guid.NewGuid();
        return new SqlitePeriodHistoryRepository(_connection).CommitPeriodSettlementAsync(instruments with
        {
            Actual = new PeriodActual
            {
                PeriodPlanSnapshotId = _currentPlanId,
                SourceFinancialSnapshotId = _currentSnapshotId,
                ResultFinancialSnapshotId = nextSnapshotId,
                PeriodStart = PeriodStart,
                PeriodEnd = PeriodEnd,
                FinalizedAtUtc = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero)
            },
            NewSnapshot = Snapshot(nextSnapshotId, PeriodEnd),
            NewPlan = Plan(Guid.NewGuid(), nextSnapshotId, PeriodEnd),
            UpdatedSettings = new UserSettings()
        });
    }

    private static FinancialSnapshot Snapshot(Guid id, DateOnly date) => new()
    {
        Id = id,
        SnapshotDate = date,
        ProjectionAnchorDate = date,
        NextSettlementDate = date.AddMonths(1),
        Anchor = new PeriodAnchor(1),
        IsCurrent = true,
        CreatedAtUtc = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
    };

    private static PeriodPlanSnapshot Plan(Guid id, Guid snapshotId, DateOnly start) => new()
    {
        Id = id,
        FinancialSnapshotId = snapshotId,
        PeriodStart = start,
        PeriodEnd = start.AddMonths(1),
        SettlementAvailableFrom = start.AddMonths(1),
        CreatedAtUtc = new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
    };
}
