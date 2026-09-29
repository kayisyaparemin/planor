using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Açık dönem içindeki anlık nakit bakiyesi gözlemini ve ödeme işaretlerini saklayan
/// dönem gözlem defteri veri deposu portu. İşaretler gözlemden bağımsızdır, plana bağlanır (S68-8).
/// </summary>
public interface IPeriodObservationRepository
{
    /// <summary>
    /// Belirtilen dönem planı kimliğine ait açık gözlem kaydını getirir; yoksa <c>null</c> döner.
    /// </summary>
    Task<PeriodObservation?> GetPeriodObservationAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dönem gözlem defteri kaydını oluşturur veya mevcutsa günceller.
    /// </summary>
    Task UpsertPeriodObservationAsync(
        PeriodObservation observation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen dönem planına ait gözlem defteri kaydını kalıcı olarak siler.
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
