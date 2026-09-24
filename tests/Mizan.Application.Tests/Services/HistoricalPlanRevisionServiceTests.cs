using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class HistoricalPlanRevisionServiceTests
{
    private readonly InMemoryPeriodHistoryRepository _repository = new();
    private readonly TestClock _clock = new(new DateOnly(2026, 10, 5), new DateTimeOffset(2026, 10, 5, 10, 0, 0, TimeSpan.Zero));
    private readonly PeriodPlanSnapshotService _planSnapshotService = new(CreateCalculator(), new CashFlowPeriodCalculator());

    private HistoricalPlanRevisionService CreateService() =>
        new(_repository, _clock, _planSnapshotService);

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_PlanProjeksiyonUretemiyorsa_NullDondurur()
    {
        var service = CreateService();
        var emptyPlan = new FinancialPlan();

        var result = await service.CaptureOpenPlanRevisionAsync(emptyPlan, "Tetikleyici");

        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_GuncelSnapshotYoksa_NullDondurur()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);

        var result = await service.CaptureOpenPlanRevisionAsync(plan, "Tetikleyici");

        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_AcikPlanYoksa_NullDondurur()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var frozenPlan = _planSnapshotService.Freeze(plan, snapshot, _clock.UtcNow);

        // Plan kapatılmış olsun (Actual kaydı var)
        var actual = new PeriodActual
        {
            PeriodPlanSnapshotId = frozenPlan.Id,
            PeriodStart = frozenPlan.PeriodStart,
            PeriodEnd = frozenPlan.PeriodEnd,
            ConfirmedEndingBalance = 55_000m
        };

        await _repository.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);
        await _repository.CommitPeriodSettlementAsync(new PeriodSettlementCommit
        {
            Actual = actual,
            NewSnapshot = snapshot with { Id = Guid.NewGuid(), SnapshotDate = frozenPlan.PeriodEnd },
            NewPlan = frozenPlan with { Id = Guid.NewGuid(), PeriodStart = frozenPlan.PeriodEnd }
        });

        var result = await service.CaptureOpenPlanRevisionAsync(plan, "Tetikleyici");

        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_DonemKapanisVadesiGecmisse_NullDondurur()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var frozenPlan = _planSnapshotService.Freeze(plan, snapshot, _clock.UtcNow);
        await _repository.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        // Saat dönem bitişinden sonraki bir tarihe çekiliyor (SettlementAvailableFrom = 2026-11-01)
        _clock.Today = new DateOnly(2026, 11, 2);

        var result = await service.CaptureOpenPlanRevisionAsync(plan, "Geç kalmış tetikleyici");

        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_PlandaFarkYoksa_MukerrerRevizyonUretmez_NullDondurur()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var frozenPlan = _planSnapshotService.Freeze(plan, snapshot, _clock.UtcNow);
        await _repository.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        var result = await service.CaptureOpenPlanRevisionAsync(plan, "Farksız değişiklik");

        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_DonemIciKartHarcamasi_RevizyonTetiklemez_NullDondurur()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);
        var cardId = Guid.NewGuid();
        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            Limit = 50_000m,
            BalanceAsOfDate = new DateOnly(2026, 10, 1),
            StatementClosingDay = 25,
            PaymentDueDay = 5
        };
        plan = plan with { CreditCards = [card] };

        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var frozenPlan = _planSnapshotService.Freeze(plan, snapshot, _clock.UtcNow);
        await _repository.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        // Dönem içine düşen (örneğin 10 Ekim) bir kart harcaması ekleniyor
        var mutatedPlan = plan with
        {
            CreditCards =
            [
                card with
                {
                    Charges =
                    [
                        new CardCharge
                        {
                            Id = Guid.NewGuid(),
                            CreditCardId = cardId,
                            PostingDate = new DateOnly(2026, 10, 10),
                            Amount = 1_500m,
                            Description = "Market Alışverişi"
                        }
                    ]
                }
            ]
        };

        var result = await service.CaptureOpenPlanRevisionAsync(mutatedPlan, "Dönem içi harcama");

        Assert.Null(result);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_KasitliPlanDegisikligi_YeniRevizyonUretirVeKaydeder()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var frozenPlan = _planSnapshotService.Freeze(plan, snapshot, _clock.UtcNow);
        await _repository.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        // Dönem içine yeni bir planlı büyük harcama ekleniyor (kasıtlı planlama kararı)
        var expenseId = Guid.NewGuid();
        var mutatedPlan = plan with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Id = expenseId,
                    Name = "Telefon Alımı",
                    Amount = 25_000m,
                    ExactDate = new DateOnly(2026, 10, 15),
                    Status = PlannedExpenseStatus.Planned
                }
            ]
        };

        var revision = await service.CaptureOpenPlanRevisionAsync(mutatedPlan, "Yeni telefon harcaması planlandı");

        Assert.NotNull(revision);
        Assert.Equal(frozenPlan.Id, revision.PeriodPlanSnapshotId);
        Assert.Equal(1, revision.RevisionNumber);
        Assert.Equal("Yeni telefon harcaması planlandı", revision.Trigger);
        Assert.Equal(25_000m, revision.PlannedLargeExpenses);
        Assert.Equal(frozenPlan.PlannedEndingBalance - 25_000m, revision.PlannedEndingBalance);
        Assert.Contains(revision.PaymentLines, x => x.SourceEntityId == expenseId && x.Name == "Telefon Alımı");

        // Depoya kaydedildiğini doğrula
        var history = await _repository.GetFinancialHistoryAsync();
        Assert.Single(history.Revisions);
        Assert.Equal(revision.Id, history.Revisions[0].Id);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_BirdenFazlaRevizyonda_SiraliNumaraUretirVeEnSonlaKiyaslar()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var frozenPlan = _planSnapshotService.Freeze(plan, snapshot, _clock.UtcNow);
        await _repository.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        // 1. Revizyon: 10.000 TL borç
        var planId1 = Guid.NewGuid();
        var plan1 = plan with
        {
            PaymentPlans =
            [
                new TemporaryPaymentPlan
                {
                    Id = planId1,
                    Name = "Borç 1",
                    Installments = [new TemporaryPaymentInstallment { PlanId = planId1, DueDate = new DateOnly(2026, 10, 15), Amount = 10_000m }]
                }
            ]
        };
        var rev1 = await service.CaptureOpenPlanRevisionAsync(plan1, "1. Borç");
        Assert.NotNull(rev1);
        Assert.Equal(1, rev1.RevisionNumber);

        // Aynı planla tekrar çağrıldığında yeni revizyon üretmemeli
        var duplicate = await service.CaptureOpenPlanRevisionAsync(plan1, "Mükerrer çağrı");
        Assert.Null(duplicate);

        // 2. Revizyon: 5.000 TL ilave borç
        var planId2 = Guid.NewGuid();
        var plan2 = plan1 with
        {
            PaymentPlans =
            [
                plan1.PaymentPlans[0],
                new TemporaryPaymentPlan
                {
                    Id = planId2,
                    Name = "Borç 2",
                    Installments = [new TemporaryPaymentInstallment { PlanId = planId2, DueDate = new DateOnly(2026, 10, 20), Amount = 5_000m }]
                }
            ]
        };
        var rev2 = await service.CaptureOpenPlanRevisionAsync(plan2, "2. Borç");
        Assert.NotNull(rev2);
        Assert.Equal(2, rev2.RevisionNumber);
        Assert.Equal(15_000m, rev2.PlannedTemporaryPayments);
    }

    [Fact]
    public async Task CaptureOpenPlanRevisionAsync_DondurulmusPlanDegismez_I23VeI24()
    {
        var service = CreateService();
        var plan = CreateBasicPlan(50_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var frozenPlan = _planSnapshotService.Freeze(plan, snapshot, _clock.UtcNow);
        await _repository.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        var originalEndingBalance = frozenPlan.PlannedEndingBalance;

        // Kasıtlı değişiklik yapılıyor
        var mutatedPlan = plan with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Name = "Mobilya",
                    Amount = 20_000m,
                    ExactDate = new DateOnly(2026, 10, 15),
                    Status = PlannedExpenseStatus.Planned
                }
            ]
        };

        var revision = await service.CaptureOpenPlanRevisionAsync(mutatedPlan, "Mobilya alındı");
        Assert.NotNull(revision);

        // Orijinal dondurulmuş planın değerleri depoda ve nesnede aynı kalmalıdır (I23/I24)
        var history = await _repository.GetFinancialHistoryAsync();
        var storedPlan = history.Plans.Single(x => x.Id == frozenPlan.Id);
        Assert.Equal(originalEndingBalance, storedPlan.PlannedEndingBalance);
        Assert.Equal(0m, storedPlan.PlannedLargeExpenses);
    }

    private static FinancialSnapshot CreateSnapshot(DateOnly date, decimal balance) => new()
    {
        Id = Guid.NewGuid(),
        SnapshotDate = date,
        ProjectionAnchorDate = date,
        ProjectionOpeningBalance = balance,
        Anchor = new PeriodAnchor(1),
        Source = FinancialSnapshotSource.MonthlyUpdate,
        IsCurrent = true
    };

    private static FinancialPlan CreateBasicPlan(decimal income)
    {
        var id = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 10, 1),
                PeriodVariableExpenseAllowance = 15_000m
            },
            RecurringIncomes = [new RecurringIncome { Id = id, Name = "Maaş", PaymentDay = 1, IsActive = true }],
            IncomeHistories = [new IncomeAmountHistory { RecurringIncomeId = id, Amount = income, EffectiveDate = new DateOnly(2026, 10, 1) }]
        };
    }

    private static FinancialProjectionCalculator CreateCalculator()
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        return new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            new CreditCardStatementCalculator(),
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());
    }

    private sealed class TestClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today { get; set; } = today;
        public DateTimeOffset UtcNow { get; set; } = utcNow;
    }
}
