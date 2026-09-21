using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class LoanTests
{
    [Fact]
    public void Loan_VarsayilanDegerler_DogruAyarlanir()
    {
        var loan = new Loan();

        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.True(loan.IsActive);
        Assert.Equal(LoanKind.Consumer, loan.Kind);
        Assert.Equal(0m, loan.MonthlyPayment);
        Assert.Null(loan.FinalPaymentAmount);
        Assert.Null(loan.RemainingDebt);
        Assert.Null(loan.EarlyClosureAmount);
        Assert.Null(loan.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void LastInstallmentAmount_FinalPaymentNullIse_AylikTaksitTutariniDoner()
    {
        var loan = new Loan
        {
            MonthlyPayment = 5_000m,
            FinalPaymentAmount = null
        };

        Assert.Equal(5_000m, loan.LastInstallmentAmount);
    }

    [Fact]
    public void LastInstallmentAmount_FinalPaymentBelirtilmisse_OzelTutariDoner()
    {
        var loan = new Loan
        {
            MonthlyPayment = 5_000m,
            FinalPaymentAmount = 3_450.50m
        };

        Assert.Equal(3_450.50m, loan.LastInstallmentAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void RemainingInstallmentTotal_KalanTaksitSifirVeyaNegatifse_SifirDoner(int remainingCount)
    {
        var loan = new Loan
        {
            MonthlyPayment = 10_000m,
            RemainingInstallmentCount = remainingCount
        };

        Assert.Equal(0m, loan.RemainingInstallmentTotal);
    }

    [Fact]
    public void RemainingInstallmentTotal_TekTaksitKalmissa_SonTaksitTutariniDoner()
    {
        var loanWithDefaultFinal = new Loan
        {
            MonthlyPayment = 12_000m,
            RemainingInstallmentCount = 1
        };

        var loanWithCustomFinal = new Loan
        {
            MonthlyPayment = 12_000m,
            FinalPaymentAmount = 7_500m,
            RemainingInstallmentCount = 1
        };

        Assert.Equal(12_000m, loanWithDefaultFinal.RemainingInstallmentTotal);
        Assert.Equal(7_500m, loanWithCustomFinal.RemainingInstallmentTotal);
    }

    [Fact]
    public void RemainingInstallmentTotal_CokluTaksitKalmissa_TamToplamiHesaplar()
    {
        // 5 taksit: 4 standart (10.000) + 1 özel son taksit (8.500) = 48.500
        var loan = new Loan
        {
            MonthlyPayment = 10_000m,
            FinalPaymentAmount = 8_500m,
            RemainingInstallmentCount = 5
        };

        Assert.Equal(48_500m, loan.RemainingInstallmentTotal);
    }
}
