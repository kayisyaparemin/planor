using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Application katmanı birim testlerinde dönem gözlem defteri verilerini bellek içinde saklayan sahte depo çifti.
/// </summary>
public sealed class InMemoryPeriodObservationRepository : IPeriodObservationRepository
{
    private readonly Dictionary<Guid, PeriodObservation> _observations = [];
    private readonly List<PeriodPaymentMark> _marks = [];

    public IReadOnlyDictionary<Guid, PeriodObservation> Items => _observations;

    public IReadOnlyList<PeriodPaymentMark> Marks => _marks;

    public Task<PeriodObservation?> GetPeriodObservationAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        _observations.TryGetValue(periodPlanSnapshotId, out var observation);
        return Task.FromResult(observation);
    }

    public Task UpsertPeriodObservationAsync(
        PeriodObservation observation,
        CancellationToken cancellationToken = default)
    {
        _observations[observation.PeriodPlanSnapshotId] = observation;
        return Task.CompletedTask;
    }

    public Task DeletePeriodObservationAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        _observations.Remove(periodPlanSnapshotId);
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
