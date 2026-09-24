using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Models;

public sealed class FinancialHistoryDataTests
{
    [Fact]
    public void Empty_BosKoleksiyonlarIcerir()
    {
        var empty = FinancialHistoryData.Empty;

        Assert.Empty(empty.Snapshots);
        Assert.Empty(empty.Plans);
        Assert.Empty(empty.Revisions);
        Assert.Empty(empty.Actuals);
    }

    [Fact]
    public void Baslatma_TumKoleksiyonlariDogruTasir()
    {
        var snapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = new DateOnly(2026, 9, 10),
            ProjectionOpeningBalance = 15000m
        };
        var plan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = new DateOnly(2026, 9, 10),
            PeriodEnd = new DateOnly(2026, 10, 10),
            OpeningBalance = 15000m
        };
        var revision = new PeriodPlanRevision
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = plan.Id,
            RevisionNumber = 1,
            Trigger = "Gelir artışı"
        };
        var actual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = plan.Id,
            PeriodStart = plan.PeriodStart,
            PeriodEnd = plan.PeriodEnd,
            ConfirmedEndingBalance = 18000m
        };

        var history = new FinancialHistoryData(
            [snapshot],
            [plan],
            [revision],
            [actual]);

        Assert.Single(history.Snapshots);
        Assert.Equal(snapshot.Id, history.Snapshots[0].Id);
        Assert.Single(history.Plans);
        Assert.Equal(plan.Id, history.Plans[0].Id);
        Assert.Single(history.Revisions);
        Assert.Equal(revision.Id, history.Revisions[0].Id);
        Assert.Single(history.Actuals);
        Assert.Equal(actual.Id, history.Actuals[0].Id);
    }
}
