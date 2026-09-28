using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteLoanRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqliteLoanRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_loans_{Guid.NewGuid():N}.db3");
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        new DatabaseSchema().EnsureInitializedAsync(_connection).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _connection.CloseAsync().GetAwaiter().GetResult();
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* cleanup */ }
        }
    }

    [Fact]
    public async Task GetLoansAsync_Bosken_BosDiziDondurur()
    {
        var repository = new SqliteLoanRepository(_connection);

        var loans = await repository.GetLoansAsync();

        Assert.Empty(loans);
    }

    [Fact]
    public async Task UpsertLoanAsync_KrediEklerVeGeriOkur()
    {
        var repository = new SqliteLoanRepository(_connection);
        var loanId = Guid.NewGuid();
        var loan = new Loan
        {
            Id = loanId,
            Name = "İhtiyaç Kredisi",
            Bank = "Garanti BBVA",
            MonthlyPayment = 4850.50m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 18,
            FinalPaymentAmount = 4850.50m,
            RemainingDebt = 72000m,
            EarlyClosureAmount = 68000m,
            EarlyClosureAmountAsOf = new DateOnly(2026, 9, 20),
            Kind = LoanKind.Consumer,
            IsActive = true
        };

        await repository.UpsertLoanAsync(loan);

        var loans = await repository.GetLoansAsync();
        Assert.Single(loans);
        var read = loans[0];
        Assert.Equal(loanId, read.Id);
        Assert.Equal("İhtiyaç Kredisi", read.Name);
        Assert.Equal("Garanti BBVA", read.Bank);
        Assert.Equal(4850.50m, read.MonthlyPayment);
        Assert.Equal(15, read.PaymentDay);
        Assert.Equal(new DateOnly(2026, 10, 15), read.NextPaymentDate);
        Assert.Equal(18, read.RemainingInstallmentCount);
        Assert.Equal(4850.50m, read.FinalPaymentAmount);
        Assert.Equal(72000m, read.RemainingDebt);
        Assert.Equal(68000m, read.EarlyClosureAmount);
        Assert.Equal(new DateOnly(2026, 9, 20), read.EarlyClosureAmountAsOf);
        Assert.Equal(LoanKind.Consumer, read.Kind);
        Assert.True(read.IsActive);
    }

    [Fact]
    public async Task DeleteLoanAsync_KrediyiSiler_Ve_CascadeIleErkenOdemeleriDeSiler()
    {
        var repository = new SqliteLoanRepository(_connection);
        var loanId = Guid.NewGuid();
        var loan = new Loan
        {
            Id = loanId,
            Name = "Konut Kredisi",
            Bank = "İş Bankası",
            MonthlyPayment = 25000m,
            PaymentDay = 1,
            NextPaymentDate = new DateOnly(2026, 10, 1),
            RemainingInstallmentCount = 120
        };
        await repository.UpsertLoanAsync(loan);

        var prepayment = new LoanPrepayment
        {
            Id = Guid.NewGuid(),
            LoanId = loanId,
            Date = new DateOnly(2026, 11, 1),
            Mode = LoanPrepaymentMode.ReduceTerm,
            PrincipalAmount = 100000m
        };
        await repository.UpsertLoanPrepaymentAsync(prepayment);

        await repository.DeleteLoanAsync(loanId);

        var loans = await repository.GetLoansAsync();
        var prepayments = await repository.GetLoanPrepaymentsAsync();

        Assert.Empty(loans);
        Assert.Empty(prepayments); // ON DELETE CASCADE doğrulaması
    }

    [Fact]
    public async Task UpsertLoanPrepaymentAsync_ErkenOdemeEklerVeSilmeCalisir()
    {
        var repository = new SqliteLoanRepository(_connection);
        var loanId = Guid.NewGuid();
        await repository.UpsertLoanAsync(new Loan { Id = loanId, Name = "Taşıt Kredisi", Bank = "Yapı Kredi", MonthlyPayment = 10000m, NextPaymentDate = new DateOnly(2026, 10, 5), RemainingInstallmentCount = 24 });

        var p1 = new LoanPrepayment { Id = Guid.NewGuid(), LoanId = loanId, Date = new DateOnly(2026, 12, 5), Mode = LoanPrepaymentMode.FullClosure };
        var p2 = new LoanPrepayment { Id = Guid.NewGuid(), LoanId = loanId, Date = new DateOnly(2027, 3, 5), Mode = LoanPrepaymentMode.ReduceInstallment, PrincipalAmount = 50000m };

        await repository.UpsertLoanPrepaymentAsync(p1);
        await repository.UpsertLoanPrepaymentAsync(p2);

        var list = await repository.GetLoanPrepaymentsAsync();
        Assert.Equal(2, list.Count);

        await repository.DeleteLoanPrepaymentAsync(p1.Id);

        var remaining = await repository.GetLoanPrepaymentsAsync();
        Assert.Single(remaining);
        Assert.Equal(p2.Id, remaining[0].Id);
    }

    [Fact]
    public async Task UpsertLoanWithPrepaymentsAsync_KrediyiVeErkenOdemeleriniBirlikteYazar()
    {
        var repository = new SqliteLoanRepository(_connection);
        var loan = TwentyFourInstallmentLoan();
        var closure = Prepayment(loan.Id, new DateOnly(2027, 3, 5), LoanPrepaymentMode.FullClosure, null);
        var partial = Prepayment(loan.Id, new DateOnly(2026, 12, 5), LoanPrepaymentMode.ReduceInstallment, 50000m);

        await repository.UpsertLoanWithPrepaymentsAsync(loan, [closure, partial]);

        Assert.Equal("Taşıt Kredisi", Assert.Single(await repository.GetLoansAsync()).Name);
        var saved = await repository.GetLoanPrepaymentsAsync();
        Assert.Equal([partial, closure], saved);
    }

    [Fact]
    public async Task UpsertLoanWithPrepaymentsAsync_KayitliKrediyiGuncellerErkenOdemeleriKaybetmez()
    {
        // INSERT OR REPLACE krediyi silip yeniden eklediği için ON DELETE CASCADE erken ödemeleri
        // götürüyordu; tek işlemdeki kayıt listeyi krediden sonra yazar (S64-12).
        var repository = new SqliteLoanRepository(_connection);
        var loan = TwentyFourInstallmentLoan();
        var partial = Prepayment(loan.Id, new DateOnly(2026, 12, 5), LoanPrepaymentMode.ReduceTerm, 50000m);
        await repository.UpsertLoanWithPrepaymentsAsync(loan, [partial]);

        await repository.UpsertLoanWithPrepaymentsAsync(loan with { Name = "Taşıt" }, [partial]);

        Assert.Equal("Taşıt", Assert.Single(await repository.GetLoansAsync()).Name);
        Assert.Equal([partial], await repository.GetLoanPrepaymentsAsync());
    }

    [Fact]
    public async Task UpsertLoanWithPrepaymentsAsync_ListedeOlmayaniSilerBaskaKredininkineDokunmaz()
    {
        var repository = new SqliteLoanRepository(_connection);
        var loan = TwentyFourInstallmentLoan();
        var other = TwentyFourInstallmentLoan();
        var removed = Prepayment(loan.Id, new DateOnly(2026, 12, 5), LoanPrepaymentMode.FullClosure, null);
        var kept = Prepayment(loan.Id, new DateOnly(2027, 1, 5), LoanPrepaymentMode.ReduceTerm, 10000m);
        var otherLoans = Prepayment(other.Id, new DateOnly(2027, 2, 5), LoanPrepaymentMode.FullClosure, null);
        await repository.UpsertLoanWithPrepaymentsAsync(loan, [removed, kept]);
        await repository.UpsertLoanWithPrepaymentsAsync(other, [otherLoans]);

        await repository.UpsertLoanWithPrepaymentsAsync(loan, [kept]);

        Assert.Equal([kept, otherLoans], await repository.GetLoanPrepaymentsAsync());
    }

    private static Loan TwentyFourInstallmentLoan() => new()
    {
        Id = Guid.NewGuid(), Name = "Taşıt Kredisi", Bank = "Yapı Kredi", MonthlyPayment = 10000m, PaymentDay = 5,
        NextPaymentDate = new DateOnly(2026, 10, 5), RemainingInstallmentCount = 24
    };

    private static LoanPrepayment Prepayment(Guid loanId, DateOnly date, LoanPrepaymentMode mode, decimal? principal) =>
        new() { Id = Guid.NewGuid(), LoanId = loanId, Date = date, Mode = mode, PrincipalAmount = principal };
}
