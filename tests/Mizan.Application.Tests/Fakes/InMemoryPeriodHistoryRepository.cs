using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Application katmanı birim testlerinde finansal durum ve dönem tarihçesi verilerini
/// bellek içinde saklayan sahte depo çifti.
/// </summary>
public sealed class InMemoryPeriodHistoryRepository : IPeriodHistoryRepository
{
    private readonly List<FinancialSnapshot> _snapshots = [];
    private readonly List<PeriodPlanSnapshot> _plans = [];
    private readonly List<PeriodPlanRevision> _revisions = [];
    private readonly List<PeriodActual> _actuals = [];
    private readonly List<PeriodSettlementCommit> _commits = [];

    public IReadOnlyList<FinancialSnapshot> Snapshots => _snapshots.AsReadOnly();
    public IReadOnlyList<PeriodPlanSnapshot> Plans => _plans.AsReadOnly();
    public IReadOnlyList<PeriodPlanRevision> Revisions => _revisions.AsReadOnly();
    public IReadOnlyList<PeriodActual> Actuals => _actuals.AsReadOnly();
    public IReadOnlyList<PeriodSettlementCommit> Commits => _commits.AsReadOnly();
    public UserSettings? LastUpdatedSettings { get; private set; }

    public Task<FinancialHistoryData> GetFinancialHistoryAsync(CancellationToken cancellationToken = default)
    {
        var data = new FinancialHistoryData(
            _snapshots.OrderBy(s => s.SnapshotDate).ThenBy(s => s.CreatedAtUtc).ToArray(),
            _plans.OrderBy(p => p.PeriodStart).ToArray(),
            _revisions.OrderBy(r => r.CreatedAtUtc).ToArray(),
            _actuals.OrderBy(a => a.PeriodStart).ToArray());

        return Task.FromResult(data);
    }

    public Task SaveCurrentFinancialSnapshotAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        UserSettings? updatedSettings = null,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < _snapshots.Count; i++)
        {
            if (_snapshots[i].IsCurrent)
            {
                _snapshots[i] = _snapshots[i] with { IsCurrent = false };
            }
        }

        _snapshots.Add(snapshot with { IsCurrent = true });
        _plans.Add(plan);

        if (updatedSettings is not null)
        {
            LastUpdatedSettings = updatedSettings;
        }

        return Task.CompletedTask;
    }

    public Task ReplacePendingFinancialSnapshotPlanAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        CancellationToken cancellationToken = default)
    {
        var snapshotIndex = _snapshots.FindIndex(s => s.Id == snapshot.Id);
        if (snapshotIndex >= 0)
        {
            _snapshots[snapshotIndex] = snapshot with { IsCurrent = true };
        }
        else
        {
            _snapshots.Add(snapshot with { IsCurrent = true });
        }

        _plans.RemoveAll(p => p.FinancialSnapshotId == snapshot.Id);
        _plans.Add(plan);

        return Task.CompletedTask;
    }

    public Task SavePeriodPlanRevisionAsync(
        PeriodPlanRevision revision,
        CancellationToken cancellationToken = default)
    {
        _revisions.Add(revision);
        return Task.CompletedTask;
    }

    public Task CommitPeriodSettlementAsync(
        PeriodSettlementCommit commit,
        CancellationToken cancellationToken = default)
    {
        _commits.Add(commit);
        _actuals.Add(commit.Actual);

        for (var i = 0; i < _snapshots.Count; i++)
        {
            if (_snapshots[i].IsCurrent)
            {
                _snapshots[i] = _snapshots[i] with { IsCurrent = false };
            }
        }

        _snapshots.Add(commit.NewSnapshot with { IsCurrent = true });
        _plans.Add(commit.NewPlan);

        if (commit.Revision is not null)
        {
            _revisions.Add(commit.Revision);
        }

        LastUpdatedSettings = commit.UpdatedSettings;

        return Task.CompletedTask;
    }
}
