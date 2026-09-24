using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Tüm dondurulmuş finansal durumları, dönem planlarını, plan revizyonlarını
/// ve gerçekleşmeleri içeren salt okuma tarihçe veri paketi (Query DTO).
/// </summary>
/// <remarks>
/// "Şu an hangi durumdayız, açık dönem hangisi" soruları tarihçenin kendisine sorulur.
/// Eskide bu sorgu bir servisin statik metoduydu (<c>PeriodProgressService.ResolveOpenPlan</c>)
/// ve enjekte edilmeden dört yerden çağrılıyordu; v2'de de iki servise satır içi kopyalanmıştı
/// (kural M8, düğüm T6). Cevap veriye ait olduğu için veriyle birlikte durur.
/// </remarks>
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

    /// <summary>
    /// Şu anda yürürlükte olan finansal durumu seçer: güncel işaretlilerden en yeni tarihli olanı,
    /// aynı tarihte birden fazlaysa en son oluşturulanı. Güncel durum yoksa <c>null</c> döner.
    /// </summary>
    public FinancialSnapshot? FindLatestCurrentSnapshot() =>
        Snapshots
            .Where(x => x.IsCurrent)
            .OrderByDescending(x => x.SnapshotDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .FirstOrDefault();

    /// <summary>
    /// Açık dönem planını seçer: güncel finansal duruma bağlı, kapanışı (gerçekleşmesi) henüz
    /// kaydedilmemiş en son dondurulan plan. Eski durumlara bağlı yetim planlar ve kapanmış dönemler
    /// açık sayılmaz. Mutabakat tarihi geçmiş ama kapatılmamış dönem hâlâ açıktır.
    /// </summary>
    public PeriodPlanSnapshot? FindOpenPlan()
    {
        var currentSnapshot = FindLatestCurrentSnapshot();
        if (currentSnapshot is null)
        {
            return null;
        }

        return Plans
            .Where(x => x.FinancialSnapshotId == currentSnapshot.Id)
            .Where(x => Actuals.All(actual => actual.PeriodPlanSnapshotId != x.Id))
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefault();
    }

    /// <summary>
    /// Bir dönem planının revizyonlarını kronolojik sırayla verir: önce oluşturulma anı, eşitse
    /// sıra numarası. Son eleman, dönem içinde "planım şu an ne" sorusunun cevabıdır (I24).
    /// </summary>
    public IReadOnlyList<PeriodPlanRevision> FindRevisions(Guid periodPlanSnapshotId) =>
        Revisions
            .Where(x => x.PeriodPlanSnapshotId == periodPlanSnapshotId)
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.RevisionNumber)
            .ToArray();
}
