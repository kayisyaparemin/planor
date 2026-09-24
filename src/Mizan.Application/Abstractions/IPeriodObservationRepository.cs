using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Açık dönem içindeki anlık nakit bakiyesi ve ara borç ödeme işaretlerini saklayan
/// dönem gözlem defteri veri deposu portu.
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
}
