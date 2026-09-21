namespace Mizan.Domain.Models;

/// <summary>
/// Tüm projeksiyon ufkunu (12 dönemi), doğal dönemsellikle gruplanmış yükümlülük planını
/// ve kümülatif faiz maliyetlerini içeren bütüncül projeksiyon sonucu sözleşmesi.
/// </summary>
public sealed record FinancialProjectionResult(
    IReadOnlyList<CashFlowPeriodProjection> Periods,
    PeriodObligationPlan ObligationPlan,
    IReadOnlyList<CreditCardPaymentProjectionStatus> CardPaymentStatuses)
{
    /// <summary>Projeksiyon ufku boyunca oluşan toplam kredi kartı devreden borç faizi (carry faizi).</summary>
    public decimal TotalCreditCardInterest =>
        Periods.Sum(x => x.CardInterestGenerated);

    /// <summary>Projeksiyon ufku boyunca oluşan toplam finansman açığı (KMH) faizi.</summary>
    public decimal TotalDeficitFinancingInterest =>
        Periods.Sum(x => x.DeficitFinancingInterest);

    /// <summary>Projeksiyon ufku boyunca katlanılan toplam finansman ve faiz maliyeti.</summary>
    public decimal TotalInterestCost =>
        TotalCreditCardInterest + TotalDeficitFinancingInterest;
}
