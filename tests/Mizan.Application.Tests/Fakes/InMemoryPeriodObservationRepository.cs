using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Application katmanı birim testlerinde dönem gözlem defteri verilerini bellek içinde saklayan sahte depo çifti.
/// Gerçek depo gibi, aynı plan ve güne yazılan gözlem öncekinin yerine geçer.
/// </summary>
public sealed class InMemoryPeriodObservationRepository : IPeriodObservationRepository
{
    private readonly List<PeriodObservation> _observations = [];
    private readonly List<PeriodPaymentMark> _marks = [];

    public IReadOnlyList<PeriodObservation> Items => _observations;

    public IReadOnlyList<PeriodPaymentMark> Marks => _marks;

    public Task<IReadOnlyList<PeriodObservation>> GetPeriodObservationsAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PeriodObservation>>(
            _observations.Where(x => x.PeriodPlanSnapshotId == periodPlanSnapshotId).OrderBy(x => x.ObservedOn).ToArray());

    public Task UpsertPeriodObservationAsync(
        PeriodObservation observation,
        CancellationToken cancellationToken = default)
    {
        _observations.RemoveAll(x =>
            x.PeriodPlanSnapshotId == observation.PeriodPlanSnapshotId && x.ObservedOn == observation.ObservedOn);
        _observations.Add(observation);
        return Task.CompletedTask;
    }

    public Task DeletePeriodObservationAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        _observations.RemoveAll(x => x.PeriodPlanSnapshotId == periodPlanSnapshotId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PeriodPaymentMark>> GetPaymentMarksAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PeriodPaymentMark>>(
            _marks.Where(x => x.PeriodPlanSnapshotId == periodPlanSnapshotId).ToArray());

    public Task UpsertPaymentMarkAsync(PeriodPaymentMark mark, CancellationToken cancellationToken = default)
    {
        _marks.RemoveAll(x =>
            x.PeriodPlanSnapshotId == mark.PeriodPlanSnapshotId &&
            x.PeriodPlanPaymentLineId == mark.PeriodPlanPaymentLineId);
        _marks.Add(mark);
        return Task.CompletedTask;
    }
}
