using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Tüm dondurulmuş finansal durumları, dönem planlarını, plan revizyonlarını
/// ve gerçekleşmeleri içeren salt okuma tarihçe veri paketi (Query DTO).
/// </summary>
public sealed record FinancialHistoryData(
    IReadOnlyList<FinancialSnapshot> Snapshots,
    IReadOnlyList<PeriodPlanSnapshot> Plans,
    IReadOnlyList<PeriodPlanRevision> Revisions,
    IReadOnlyList<PeriodActual> Actuals)
{
    /// <summary>
    /// Kayıt içermeyen boş tarihçe örneği.
    /// </summary>
    public static readonly FinancialHistoryData Empty = new([], [], [], []);
}
