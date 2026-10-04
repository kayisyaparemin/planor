using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Sunum testleri için sahte dönem gözlem deposu.
/// </summary>
public sealed class FakePeriodObservationRepository : IPeriodObservationRepository
{
    public List<PeriodObservation> Observations { get; } = [];
    public List<PeriodPaymentMark> PaymentMarks { get; } = [];

    public Task<IReadOnlyList<PeriodObservation>> GetPeriodObservationsAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var result = Observations
            .Where(o => o.PeriodPlanSnapshotId == periodPlanSnapshotId)
            .OrderBy(o => o.ObservedOn)
            .ToList();

        return Task.FromResult<IReadOnlyList<PeriodObservation>>(result);
    }

    public Task UpsertPeriodObservationAsync(
        PeriodObservation observation,
        CancellationToken cancellationToken = default)
    {
        Observations.RemoveAll(o =>
            o.PeriodPlanSnapshotId == observation.PeriodPlanSnapshotId &&
            o.ObservedOn == observation.ObservedOn);
        Observations.Add(observation);
        return Task.CompletedTask;
    }

    public Task DeletePeriodObservationAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        Observations.RemoveAll(o => o.PeriodPlanSnapshotId == periodPlanSnapshotId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PeriodPaymentMark>> GetPaymentMarksAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var result = PaymentMarks
            .Where(m => m.PeriodPlanSnapshotId == periodPlanSnapshotId)
            .ToList();

        return Task.FromResult<IReadOnlyList<PeriodPaymentMark>>(result);
    }

    public Task UpsertPaymentMarkAsync(
        PeriodPaymentMark mark,
        CancellationToken cancellationToken = default)
    {
        PaymentMarks.RemoveAll(m =>
            m.PeriodPlanSnapshotId == mark.PeriodPlanSnapshotId &&
            m.PeriodPlanPaymentLineId == mark.PeriodPlanPaymentLineId);
        PaymentMarks.Add(mark);
        return Task.CompletedTask;
    }
}
