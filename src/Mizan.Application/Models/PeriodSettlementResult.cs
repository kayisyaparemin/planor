using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Başarıyla tamamlanan bir dönem kapanış mutabakatının sonucunda üretilen yeni finansal durum görüntüsünü,
/// kesinleşen gerçekleşme karnesini, plan karşılaştırmasını ve yeni dönemin dondurulan başlangıç planını sunan sonuç paketidir.
/// </summary>
public sealed record PeriodSettlementResult
{
    /// <summary>Yeni dönemi başlatan güncel finansal durum görüntüsü.</summary>
    public required FinancialSnapshot NewSnapshot { get; init; }

    /// <summary>Kapanan döneme ait veritabanına kalıcı olarak kaydedilen fiilî gerçekleşme kaydı.</summary>
    public required PeriodActual Actual { get; init; }

    /// <summary>Kapanan dönemin plan-gerçekleşme bütçe karşılaştırması ve Türkçe özet karnesi.</summary>
    public required PlanActualComparison Comparison { get; init; }

    /// <summary>Yeni dönem için dondurulmuş olan başlangıç nakit akış planı.</summary>
    public required PeriodPlanSnapshot NewPlan { get; init; }
}
