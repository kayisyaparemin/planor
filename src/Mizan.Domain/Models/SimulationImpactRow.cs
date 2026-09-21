namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir nakit akış döneminde baz durum (Baseline) ile senaryo durumu (Scenario)
/// arasındaki parasal farkları karşılaştırmalı olarak sunan satır sözleşmesi.
/// </summary>
public sealed record SimulationImpactRow(
    CashFlowPeriodProjection Baseline,
    CashFlowPeriodProjection Scenario)
{
    /// <summary>İlgili nakit akış döneminin yarı açık aralığı [Start, End).</summary>
    public CashFlowPeriod Period => Scenario.Period;

    /// <summary>Dönem başlangıç tarihi.</summary>
    public DateOnly PeriodStart => Scenario.PeriodStart;

    /// <summary>Senaryonun bu dönemde getirdiği zorunlu borç çıkışı farkı (Scenario - Baseline).</summary>
    public decimal MandatoryOutflowDifference =>
        Scenario.MandatoryOutflow - Baseline.MandatoryOutflow;

    /// <summary>Senaryonun bu dönemde kalan tahmini serbest nakit fazlası farkı (Scenario - Baseline).</summary>
    public decimal SurplusDifference =>
        Scenario.EstimatedSurplus - Baseline.EstimatedSurplus;

    /// <summary>Senaryonun dönem sonu kapanış bakiyesinde yarattığı net fark (Scenario - Baseline).</summary>
    public decimal ProjectedBalanceDifference =>
        Scenario.EndingBalance - Baseline.EndingBalance;

    /// <summary>Senaryonun bu dönemde ürettiği toplam faiz yükü farkı (Kart faizi + KMH açık faizi).</summary>
    public decimal InterestDifference =>
        Scenario.TotalInterestGenerated - Baseline.TotalInterestGenerated;
}
