using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

/// <summary>
/// Kredi takvim ödeme modelleri (LoanReplay, LoanScheduledPayment, LoanPaymentKind) testleri.
/// </summary>
public sealed class LoanReplayTests
{
    private static Loan CreateReferenceLoan() => new()
    {
        Name = "Test Kredi",
        Bank = "Test Bank",
        MonthlyPayment = 5_000m,
        PaymentDay = 10,
        NextPaymentDate = new DateOnly(2026, 10, 10),
        RemainingInstallmentCount = 6,
        RemainingDebt = 30_000m
    };

    [Fact]
    public void LoanReplay_OdemelerVerildiginde_TotalVeLastPaymentDateDogruHesaplanir()
    {
        var loan = CreateReferenceLoan();
        var payment1 = new LoanScheduledPayment(new DateOnly(2026, 10, 10), 5_000m, LoanPaymentKind.Installment, loan.Id, false);
        var payment2 = new LoanScheduledPayment(new DateOnly(2026, 11, 10), 5_000m, LoanPaymentKind.Installment, loan.Id, true);

        var replay = new LoanReplay(loan, [payment1, payment2], new Dictionary<Guid, Loan>(), new Dictionary<Guid, decimal>(), false);

        Assert.Equal(10_000m, replay.Total);
        Assert.Equal(new DateOnly(2026, 11, 10), replay.LastPaymentDate);
        Assert.False(replay.IgnoredBecauseUnquotable);
    }

    [Fact]
    public void LoanReplay_BosOdemelerle_TotalSifirVeLastPaymentDateNullDondurur()
    {
        var loan = CreateReferenceLoan();
        var replay = new LoanReplay(loan, [], new Dictionary<Guid, Loan>(), new Dictionary<Guid, decimal>(), true);

        Assert.Equal(0m, replay.Total);
        Assert.Null(replay.LastPaymentDate);
        Assert.True(replay.IgnoredBecauseUnquotable);
    }

    [Fact]
    public void LoanScheduledPayment_TumOzellikleriDogruTasir()
    {
        var id = Guid.NewGuid();
        var date = new DateOnly(2026, 12, 10);
        var payment = new LoanScheduledPayment(date, 7_500m, LoanPaymentKind.EarlyClosure, id, true);

        Assert.Equal(date, payment.Date);
        Assert.Equal(7_500m, payment.Amount);
        Assert.Equal(LoanPaymentKind.EarlyClosure, payment.Kind);
        Assert.Equal(id, payment.SourceId);
        Assert.True(payment.IsFinal);
    }
}
