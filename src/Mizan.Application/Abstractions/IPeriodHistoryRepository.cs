using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Dondurulmuş dönem planları, finansal durum anlık görüntüleri, plan revizyonları
/// ve dönem gerçekleşmelerini kalıcılaştıran ve sorgulayan dönem tarihçesi veri deposu portu.
/// </summary>
public interface IPeriodHistoryRepository
{
    /// <summary>
    /// Kayıtlı tüm finansal anlık görüntüleri, dönem planlarını, revizyonları ve gerçekleşmeleri getirir.
    /// </summary>
    Task<FinancialHistoryData> GetFinancialHistoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir finansal durum anlık görüntüsünü ve buna bağlı dondurulan dönem planını güncel durum olarak kaydeder.
    /// </summary>
    Task SaveCurrentFinancialSnapshotAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        UserSettings? updatedSettings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Henüz mutabakatı yapılmamış güncel dönem planını düzeltilmiş planla değiştirir.
    /// </summary>
    Task ReplacePendingFinancialSnapshotPlanAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dondurulan dönem planına ait yeni bir planlama revizyonunu kaydeder.
    /// </summary>
    Task SavePeriodPlanRevisionAsync(
        PeriodPlanRevision revision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kapanan dönemin gerçekleşmesini, güncellenen araçları ve yeni dönemin planını atomik olarak taahhüt eder.
    /// </summary>
    Task CommitPeriodSettlementAsync(
        PeriodSettlementCommit commit,
        CancellationToken cancellationToken = default);
}
