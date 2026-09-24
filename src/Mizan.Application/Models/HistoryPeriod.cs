using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kapanmış bir nakit akış döneminin tarihçe görünümü: dönem başında dondurulan orijinal plan,
/// dönem içindeki nihai revizyon, fiilî gerçekleşme, kapanıştan çıkan yeni finansal durum ve
/// bunların karnesi. Geçmiş ekranının planı ve gerçekleşmeyi yan yana, orijinal taahhüdü
/// kaybetmeden gösterebilmesi için vardır.
/// </summary>
public sealed record HistoryPeriod
{
    /// <summary>Dönem başında dondurulan ve hiç değişmeyen orijinal plan taahhüdü.</summary>
    public required PeriodPlanSnapshot OriginalPlan { get; init; }

    /// <summary>
    /// Dönemin kapanışa açıldığı gün dahil o güne kadar oluşturulan en son plan revizyonu
    /// (dönemin nihai planı); plan hiç revize edilmediyse boştur.
    /// </summary>
    public PeriodPlanRevision? Revision { get; init; }

    /// <summary>Nihai plana sayılan (kapanışa açılış günü dahil o güne kadar oluşturulan) revizyon adedi.</summary>
    public int RevisionCount { get; init; }

    /// <summary>Dönem kapanışında kesinleşen fiilî gerçekleşme.</summary>
    public required PeriodActual Actual { get; init; }

    /// <summary>Dönem kapanışıyla üretilen, sonraki döneme devreden finansal durum.</summary>
    public required FinancialSnapshot ResultSnapshot { get; init; }

    /// <summary>Nihai plan ile fiilî gerçekleşmenin kategori bazlı karnesi.</summary>
    public required PlanActualComparison Comparison { get; init; }
}
