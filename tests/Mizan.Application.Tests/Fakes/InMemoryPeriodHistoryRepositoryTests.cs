using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

public sealed class InMemoryPeriodHistoryRepositoryTests
{
    [Fact]
    public async Task SaveCurrentFinancialSnapshotAsync_EskiSnapshotinIsCurrentiniKapatir_VeYeniDurumuEkler()
    {
        var sut = new InMemoryPeriodHistoryRepository();
        var snap1 = new FinancialSnapshot { Id = Guid.NewGuid(), SnapshotDate = new DateOnly(2026, 8, 10), IsCurrent = true };
        var plan1 = new PeriodPlanSnapshot { Id = Guid.NewGuid(), FinancialSnapshotId = snap1.Id, PeriodStart = new DateOnly(2026, 8, 10), PeriodEnd = new DateOnly(2026, 9, 10) };
        await sut.SaveCurrentFinancialSnapshotAsync(snap1, plan1);

        var snap2 = new FinancialSnapshot { Id = Guid.NewGuid(), SnapshotDate = new DateOnly(2026, 9, 10) };
        var plan2 = new PeriodPlanSnapshot { Id = Guid.NewGuid(), FinancialSnapshotId = snap2.Id, PeriodStart = new DateOnly(2026, 9, 10), PeriodEnd = new DateOnly(2026, 10, 10) };
        await sut.SaveCurrentFinancialSnapshotAsync(snap2, plan2);

        var history = await sut.GetFinancialHistoryAsync();
        Assert.Equal(2, history.Snapshots.Count);
        Assert.False(history.Snapshots[0].IsCurrent);
        Assert.True(history.Snapshots[1].IsCurrent);
        Assert.Equal(2, history.Plans.Count);
    }

    [Fact]
    public async Task ReplacePendingFinancialSnapshotPlanAsync_MevcutPlaniGunceller()
    {
        var sut = new InMemoryPeriodHistoryRepository();
        var snap = new FinancialSnapshot { Id = Guid.NewGuid(), SnapshotDate = new DateOnly(2026, 9, 10), IsCurrent = true };
        var plan1 = new PeriodPlanSnapshot { Id = Guid.NewGuid(), FinancialSnapshotId = snap.Id, PlannedIncome = 50000m };
        await sut.SaveCurrentFinancialSnapshotAsync(snap, plan1);

        var plan2 = new PeriodPlanSnapshot { Id = Guid.NewGuid(), FinancialSnapshotId = snap.Id, PlannedIncome = 60000m };
        await sut.ReplacePendingFinancialSnapshotPlanAsync(snap, plan2);

        var history = await sut.GetFinancialHistoryAsync();
        Assert.Single(history.Plans);
        Assert.Equal(60000m, history.Plans[0].PlannedIncome);
    }

    [Fact]
    public async Task SavePeriodPlanRevisionAsync_RevizyonuKaydeder()
    {
        var sut = new InMemoryPeriodHistoryRepository();
        var revision = new PeriodPlanRevision
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = Guid.NewGuid(),
            RevisionNumber = 1,
            Trigger = "Revizyon 1"
        };

        await sut.SavePeriodPlanRevisionAsync(revision);

        var history = await sut.GetFinancialHistoryAsync();
        var saved = Assert.Single(history.Revisions);
        Assert.Equal(revision.Id, saved.Id);
    }

    [Fact]
    public async Task CommitPeriodSettlementAsync_GerceklesmeVeYeniDurumuAtomikKaydeder()
    {
        var sut = new InMemoryPeriodHistoryRepository();
        var oldSnap = new FinancialSnapshot { Id = Guid.NewGuid(), SnapshotDate = new DateOnly(2026, 8, 10), IsCurrent = true };
        var oldPlan = new PeriodPlanSnapshot { Id = Guid.NewGuid(), FinancialSnapshotId = oldSnap.Id, PeriodStart = new DateOnly(2026, 8, 10), PeriodEnd = new DateOnly(2026, 9, 10) };
        await sut.SaveCurrentFinancialSnapshotAsync(oldSnap, oldPlan);

        var actual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = oldPlan.Id,
            SourceFinancialSnapshotId = oldSnap.Id,
            PeriodStart = oldPlan.PeriodStart,
            PeriodEnd = oldPlan.PeriodEnd,
            ConfirmedEndingBalance = 25000m
        };
        var newSnap = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = new DateOnly(2026, 9, 10),
            ProjectionOpeningBalance = 25000m
        };
        var newPlan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = newSnap.Id,
            PeriodStart = new DateOnly(2026, 9, 10),
            PeriodEnd = new DateOnly(2026, 10, 10),
            OpeningBalance = 25000m
        };

        var commit = new PeriodSettlementCommit
        {
            Actual = actual,
            NewSnapshot = newSnap,
            NewPlan = newPlan,
            UpdatedSettings = new UserSettings()
        };

        await sut.CommitPeriodSettlementAsync(commit);

        var history = await sut.GetFinancialHistoryAsync();
        Assert.Single(history.Actuals);
        Assert.Equal(actual.Id, history.Actuals[0].Id);
        Assert.Equal(2, history.Snapshots.Count);
        Assert.False(history.Snapshots[0].IsCurrent);
        Assert.True(history.Snapshots[1].IsCurrent);
        Assert.Equal(2, history.Plans.Count);
        Assert.Single(sut.Commits);
    }

    [Fact]
    public async Task PeriodObservationRepository_UpsertVeGet_BasariylaCalisir()
    {
        var sut = new InMemoryPeriodObservationRepository();
        var planId = Guid.NewGuid();
        var observation = new PeriodObservation
        {
            PeriodPlanSnapshotId = planId,
            ObservedBalance = 42000m,
            ObservedLivingSpend = 8000m
        };

        await sut.UpsertPeriodObservationAsync(observation);

        var retrieved = await sut.GetPeriodObservationAsync(planId);
        Assert.NotNull(retrieved);
        Assert.Equal(42000m, retrieved.ObservedBalance);
        Assert.Equal(8000m, retrieved.ObservedLivingSpend);

        await sut.DeletePeriodObservationAsync(planId);
        Assert.Null(await sut.GetPeriodObservationAsync(planId));
    }
}
