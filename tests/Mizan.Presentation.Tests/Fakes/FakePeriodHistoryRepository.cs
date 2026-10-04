using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Sunum testleri için sahte dönem tarihçesi veri deposu.
/// </summary>
public sealed class FakePeriodHistoryRepository : IPeriodHistoryRepository
{
    public List<FinancialSnapshot> Snapshots { get; } = [];
    public List<PeriodPlanSnapshot> Plans { get; } = [];
    public List<PeriodPlanRevision> Revisions { get; } = [];
    public List<PeriodActual> Actuals { get; } = [];

    public Task<FinancialHistoryData> GetFinancialHistoryAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FinancialHistoryData(
            Snapshots.OrderBy(s => s.SnapshotDate).ThenBy(s => s.CreatedAtUtc).ToArray(),
            Plans.OrderBy(p => p.PeriodStart).ToArray(),
            Revisions.OrderBy(r => r.CreatedAtUtc).ToArray(),
            Actuals.OrderBy(a => a.PeriodStart).ToArray()));
    }

    public Task SaveCurrentFinancialSnapshotAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        UserSettings? updatedSettings = null,
        CancellationToken cancellationToken = default)
    {
        Snapshots.Add(snapshot);
        Plans.Add(plan);
        return Task.CompletedTask;
    }

    public Task ReplacePendingFinancialSnapshotPlanAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        CancellationToken cancellationToken = default)
    {
        Snapshots.RemoveAll(s => s.Id == snapshot.Id);
        Snapshots.Add(snapshot);
        Plans.RemoveAll(p => p.Id == plan.Id);
        Plans.Add(plan);
        return Task.CompletedTask;
    }

    public Task CommitPeriodSettlementAsync(PeriodSettlementCommit commit, CancellationToken cancellationToken = default)
    {
        Actuals.Add(commit.Actual);
        Snapshots.Add(commit.NewSnapshot);
        Plans.Add(commit.NewPlan);
        return Task.CompletedTask;
    }

    public Task SavePeriodPlanRevisionAsync(PeriodPlanRevision revision, CancellationToken cancellationToken = default)
    {
        Revisions.Add(revision);
        return Task.CompletedTask;
    }
}
