namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının mevcut finansal projeksiyonu ile simülasyon senaryolarının uygulandığı
/// hipotetik projeksiyonun bütüncül karşılaştırma ve karar destek sonucu.
/// </summary>
public sealed record SimulationResult(
    IReadOnlyList<CashFlowPeriodProjection> Baseline,
    IReadOnlyList<CashFlowPeriodProjection> Scenario,
    IReadOnlyList<SimulationImpactRow> Rows,
    SimulationRiskSummary Risk)
{
    /// <summary>Mevcut baz plana ait 12 dönemlik kümülatif faiz maliyetleri özeti.</summary>
    public ProjectionInterestSummary BaselineInterest =>
        ProjectionInterestSummary.From(Baseline);

    /// <summary>Senaryo planına ait 12 dönemlik kümülatif faiz maliyetleri özeti.</summary>
    public ProjectionInterestSummary ScenarioInterest =>
        ProjectionInterestSummary.From(Scenario);

    /// <summary>Senaryonun getirdiği ek faiz maliyeti (Scenario - Baseline).</summary>
    public decimal AdditionalInterestCost =>
        ScenarioInterest.TotalInterestCost - BaselineInterest.TotalInterestCost;

    /// <summary>Senaryonun sağladığı net faiz tasarrufu (varsa pozitif tutar, yoksa 0).</summary>
    public decimal InterestSaving =>
        Math.Max(0m, -AdditionalInterestCost);

    /// <summary>Senaryodaki kredi erken kapama ve ara ödemelerinin kredilere olan bireysel etkileri.</summary>
    public IReadOnlyList<LoanPrepaymentImpact> LoanImpacts { get; init; } = [];
}
