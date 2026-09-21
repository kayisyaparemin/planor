namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir dönem dizisinde oluşan kredi kartı ve finansman açığı faiz maliyetlerinin özet sözleşmesi.
/// Simülasyon karşılaştırmalarında ve faiz içgörülerinde kullanılır.
/// </summary>
public sealed record ProjectionInterestSummary(
    decimal CreditCardInterest,
    decimal DeficitFinancingInterest)
{
    /// <summary>Toplam faiz maliyeti (Kredi kartı carry faizi + KMH açık faizi).</summary>
    public decimal TotalInterestCost =>
        CreditCardInterest + DeficitFinancingInterest;

    /// <summary>
    /// Verilen nakit akış dönemleri projeksiyon listesinden toplam faiz özetini üretir.
    /// </summary>
    public static ProjectionInterestSummary From(
        IEnumerable<CashFlowPeriodProjection> periods)
    {
        ArgumentNullException.ThrowIfNull(periods);
        var rows = periods.ToArray();
        return new ProjectionInterestSummary(
            rows.Sum(x => x.CardInterestGenerated),
            rows.Sum(x => x.DeficitFinancingInterest));
    }
}
