using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

/// <summary>
/// Domain katmanındaki finansal sözlük enum'larının sayısal değerlerini,
/// isim bütünlüğünü ve iş sözleşmesi karşılıklarını denetleyen testler.
/// </summary>
public sealed class FinanceEnumsTests
{
    [Fact]
    public void CreditCardPaymentStrategy_DegerleriVeKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)CreditCardPaymentStrategy.AskEachStatement);
        Assert.Equal(1, (int)CreditCardPaymentStrategy.Minimum);
        Assert.Equal(2, (int)CreditCardPaymentStrategy.FullStatement);
        Assert.Equal(3, (int)CreditCardPaymentStrategy.FixedAmount);
    }

    [Fact]
    public void ProjectionFallbackStrategy_DegerleriVeKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)ProjectionFallbackStrategy.None);
        Assert.Equal(1, (int)ProjectionFallbackStrategy.Minimum);
        Assert.Equal(2, (int)ProjectionFallbackStrategy.FullStatement);
        Assert.Equal(3, (int)ProjectionFallbackStrategy.FixedAmount);
    }

    [Fact]
    public void CreditCardPaymentType_DegerleriVeKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)CreditCardPaymentType.FixedAmount);
        Assert.Equal(1, (int)CreditCardPaymentType.Minimum);
        Assert.Equal(2, (int)CreditCardPaymentType.FullStatement);
    }

    [Fact]
    public void CurrentStatementPaymentMode_DegerleriVeKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)CurrentStatementPaymentMode.Minimum);
        Assert.Equal(1, (int)CurrentStatementPaymentMode.Full);
        Assert.Equal(2, (int)CurrentStatementPaymentMode.Custom);
    }

    [Fact]
    public void PaymentPlanKind_DegerleriVeKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)PaymentPlanKind.Temporary);
        Assert.Equal(1, (int)PaymentPlanKind.Installment);
        Assert.Equal(2, (int)PaymentPlanKind.Recurring);
        Assert.Equal(3, (int)PaymentPlanKind.OtherScheduled);
    }

    [Fact]
    public void PlannedExpenseStatus_DegerleriVeKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)PlannedExpenseStatus.Planned);
        Assert.Equal(1, (int)PlannedExpenseStatus.Completed);
        Assert.Equal(2, (int)PlannedExpenseStatus.Cancelled);
    }


    [Fact]
    public void LoanKind_6502SayiliKanunKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)LoanKind.Consumer);
        Assert.Equal(1, (int)LoanKind.HousingFixed);
        Assert.Equal(2, (int)LoanKind.HousingVariable);
    }

    [Fact]
    public void LoanPrepaymentMode_DegerleriVeKarsiliklari_DogruTanimlanmali()
    {
        Assert.Equal(0, (int)LoanPrepaymentMode.FullClosure);
        Assert.Equal(1, (int)LoanPrepaymentMode.ReduceTerm);
        Assert.Equal(2, (int)LoanPrepaymentMode.ReduceInstallment);
    }
}
