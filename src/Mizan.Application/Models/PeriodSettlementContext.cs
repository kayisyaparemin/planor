using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Dönem kapanışı ekranı veya sihirbazının kullanıcıya dönemin dondurulan orijinal planını,
/// varsa revizyonlarını, önerilen açılış bakiyesini ve mevcutsa karşılaştırmasını sunması için
/// gerekli olan bağlam sözleşmesidir.
/// </summary>
public sealed record PeriodSettlementContext
{
    /// <summary>Kapanan döneme ait kaynak finansal durum görüntüsü.</summary>
    public required FinancialSnapshot Snapshot { get; init; }

    /// <summary>Dönem başında dondurulan orijinal plan taahhüdü.</summary>
    public required PeriodPlanSnapshot OriginalPlan { get; init; }

    /// <summary>Dönem içinde oluşturulmuş nihai plan revizyonu; revizyon yoksa null'dır.</summary>
    public PeriodPlanRevision? Revision { get; init; }

    /// <summary>Dönem içinde kaydedilmiş toplam geçerli plan revizyonu sayısı.</summary>
    public required int RevisionCount { get; init; }

    /// <summary>Dönem daha önce kapatılmışsa kayıtlı fiilî gerçekleşme; açık dönem için null'dır.</summary>
    public PeriodActual? Actual { get; init; }

    /// <summary>Nihai plana göre öngörülen dönem sonu kapanış bakiyesi (yeni dönemin önerilen açılış bakiyesi).</summary>
    public required decimal SuggestedStartingBalance { get; init; }

    /// <summary>Dönem kapatılmışsa hesaplanmış plan-gerçekleşme karnesi; henüz kapatılmadıysa null'dır.</summary>
    public PlanActualComparison? Comparison { get; init; }
}
