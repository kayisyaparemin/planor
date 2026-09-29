using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Açık dönem içindeki anlık nakit bakiyesi gözlemlerini ve ödeme işaretlerini saklayan
/// dönem gözlem defteri veri deposu portu. İşaretler gözlemden bağımsızdır, plana bağlanır (S68-8).
/// </summary>
public interface IPeriodObservationRepository
{
    /// <summary>
    /// Belirtilen dönem planının gözlemlerini gün sırasıyla getirir; gözlem yoksa boş liste döner.
    /// </summary>
    Task<IReadOnlyList<PeriodObservation>> GetPeriodObservationsAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gözlemi yazar; aynı plan ve güne konmuş gözlemin yerine geçer (gün başına tek gözlem, S68-2).
    /// </summary>
    Task UpsertPeriodObservationAsync(
        PeriodObservation observation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen dönem planının gözlemlerini kalıcı olarak siler.
    /// </summary>
    Task DeletePeriodObservationAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen dönem planına konmuş ödeme işaretlerini getirir; işaret yoksa boş liste döner.
    /// </summary>
    Task<IReadOnlyList<PeriodPaymentMark>> GetPaymentMarksAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ödeme işaretini yazar; aynı plan ödeme satırına daha önce konmuş işaretin yerine geçer.
    /// </summary>
    Task UpsertPaymentMarkAsync(
        PeriodPaymentMark mark,
        CancellationToken cancellationToken = default);
}
