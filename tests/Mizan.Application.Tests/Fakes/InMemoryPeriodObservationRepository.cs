using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Application katmanı birim testlerinde dönem gözlem defteri verilerini bellek içinde saklayan sahte depo çifti.
/// </summary>
public sealed class InMemoryPeriodObservationRepository : IPeriodObservationRepository
{
    private readonly Dictionary<Guid, PeriodObservation> _observations = [];

    public IReadOnlyDictionary<Guid, PeriodObservation> Items => _observations;

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
}
