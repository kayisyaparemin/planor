using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqlitePeriodHistoryRepositoryTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_history_{Guid.NewGuid():N}.db3");
    private SQLiteAsyncConnection _connection = null!;

    public async Task InitializeAsync()
    {
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        await new DatabaseSchema().EnsureInitializedAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* cleanup */ }
        }
    }

    [Fact]
    public async Task GetFinancialHistoryAsync_BosVeritabaninda_BosTarihceDoner()
    {
        var repository = new SqlitePeriodHistoryRepository(_connection);

        var history = await repository.GetFinancialHistoryAsync();

        Assert.Empty(history.Snapshots);
        Assert.Empty(history.Plans);
        Assert.Empty(history.Revisions);
        Assert.Empty(history.Actuals);
    }

    [Fact]
    public async Task SaveCurrentFinancialSnapshotAsync_SnapshotVePlaniKaydeder()
    {
        var repository = new SqlitePeriodHistoryRepository(_connection);
        var snapshotId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var snapshot = CreateSnapshot(snapshotId, "2026-09-01", isCurrent: true);
        var plan = CreatePlan(planId, snapshotId, "2026-09-01", "2026-10-01");

        await repository.SaveCurrentFinancialSnapshotAsync(snapshot, plan);
        var history = await repository.GetFinancialHistoryAsync();

        var loadedSnapshot = Assert.Single(history.Snapshots);
        Assert.Equal(snapshotId, loadedSnapshot.Id);
        Assert.True(loadedSnapshot.IsCurrent);

        var loadedPlan = Assert.Single(history.Plans);
        Assert.Equal(planId, loadedPlan.Id);
        Assert.Single(loadedPlan.PaymentLines);
        Assert.Single(loadedPlan.IncomeLines);
    }

    [Fact]
    public async Task ReplacePendingFinancialSnapshotPlanAsync_AcikPlaniDegistirir()
    {
        var repository = new SqlitePeriodHistoryRepository(_connection);
        var snapshotId = Guid.NewGuid();
        var plan1Id = Guid.NewGuid();
        var plan2Id = Guid.NewGuid();

        var snapshot = CreateSnapshot(snapshotId, "2026-09-01", isCurrent: true);
        var plan1 = CreatePlan(plan1Id, snapshotId, "2026-09-01", "2026-10-01");
        await repository.SaveCurrentFinancialSnapshotAsync(snapshot, plan1);

        var plan2 = CreatePlan(plan2Id, snapshotId, "2026-09-01", "2026-10-01") with
        {
            PlannedIncome = 60000m
        };
        await repository.ReplacePendingFinancialSnapshotPlanAsync(snapshot, plan2);

        var history = await repository.GetFinancialHistoryAsync();
        var currentPlan = Assert.Single(history.Plans);
        Assert.Equal(plan2Id, currentPlan.Id);
        Assert.Equal(60000m, currentPlan.PlannedIncome);
    }

    [Fact]
    public async Task SavePeriodPlanRevisionAsync_RevizyonuKaydeder()
    {
        var repository = new SqlitePeriodHistoryRepository(_connection);
        var snapshotId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var revisionId = Guid.NewGuid();

        var snapshot = CreateSnapshot(snapshotId, "2026-09-01", isCurrent: true);
        var plan = CreatePlan(planId, snapshotId, "2026-09-01", "2026-10-01");
        await repository.SaveCurrentFinancialSnapshotAsync(snapshot, plan);

        var revision = new PeriodPlanRevision
        {
            Id = revisionId,
            PeriodPlanSnapshotId = planId,
            RevisionNumber = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Trigger = "Kart asgari ödeme tercihi",
            PlannedIncome = 50000m,
            PlannedEndingBalance = 15000m,
            PaymentLines = plan.PaymentLines,
            IncomeLines = plan.IncomeLines
        };

        await repository.SavePeriodPlanRevisionAsync(revision);

        var history = await repository.GetFinancialHistoryAsync();
        var loadedRevision = Assert.Single(history.Revisions);
        Assert.Equal(revisionId, loadedRevision.Id);
        Assert.Equal("Kart asgari ödeme tercihi", loadedRevision.Trigger);
    }

    [Fact]
    public async Task CommitPeriodSettlementAsync_KapanisiVeYeniDonemiAtomikKaydeder()
    {
        var repository = new SqlitePeriodHistoryRepository(_connection);
        var snap1Id = Guid.NewGuid();
        var plan1Id = Guid.NewGuid();
        var snap2Id = Guid.NewGuid();
        var plan2Id = Guid.NewGuid();

        var snap1 = CreateSnapshot(snap1Id, "2026-09-01", isCurrent: true);
        var plan1 = CreatePlan(plan1Id, snap1Id, "2026-09-01", "2026-10-01");
        await repository.SaveCurrentFinancialSnapshotAsync(snap1, plan1);

        var actual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = plan1Id,
            SourceFinancialSnapshotId = snap1Id,
            ResultFinancialSnapshotId = snap2Id,
            PeriodStart = new DateOnly(2026, 9, 1),
            PeriodEnd = new DateOnly(2026, 10, 1),
            FinalizedAtUtc = DateTimeOffset.UtcNow,
            ActualIncome = 50000m,
            ConfirmedEndingBalance = 20000m
        };

        var snap2 = CreateSnapshot(snap2Id, "2026-10-01", isCurrent: true) with { ProjectionOpeningBalance = 20000m };
        var plan2 = CreatePlan(plan2Id, snap2Id, "2026-10-01", "2026-11-01") with { OpeningBalance = 20000m };

        var commit = new PeriodSettlementCommit
        {
            Actual = actual,
            NewSnapshot = snap2,
            NewPlan = plan2,
            UpdatedSettings = new UserSettings()
        };

        await repository.CommitPeriodSettlementAsync(commit);

        var history = await repository.GetFinancialHistoryAsync();
        Assert.Equal(2, history.Snapshots.Count);
        var activeSnap = history.FindLatestCurrentSnapshot();
        Assert.NotNull(activeSnap);
        Assert.Equal(snap2Id, activeSnap.Id);
        var loadedActual = Assert.Single(history.Actuals);
        Assert.Equal(20000m, loadedActual.ConfirmedEndingBalance);
    }

    private static FinancialSnapshot CreateSnapshot(Guid id, string date, bool isCurrent) =>
        new()
        {
            Id = id,
            SnapshotDate = DateOnly.ParseExact(date, DatabaseConstants.DateFormat),
            ProjectionAnchorDate = DateOnly.ParseExact(date, DatabaseConstants.DateFormat),
            NextSettlementDate = DateOnly.ParseExact(date, DatabaseConstants.DateFormat).AddMonths(1),
            ProjectionOpeningBalance = 10000m,
            Anchor = new PeriodAnchor(1),
            IsCurrent = isCurrent,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Note = "Test Snapshot"
        };

    private static PeriodPlanSnapshot CreatePlan(Guid id, Guid snapshotId, string start, string end) =>
        new()
        {
            Id = id,
            FinancialSnapshotId = snapshotId,
            PeriodStart = DateOnly.ParseExact(start, DatabaseConstants.DateFormat),
            PeriodEnd = DateOnly.ParseExact(end, DatabaseConstants.DateFormat),
            SettlementAvailableFrom = DateOnly.ParseExact(end, DatabaseConstants.DateFormat),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            OpeningBalance = 10000m,
            PlannedIncome = 50000m,
            PlannedEndingBalance = 25000m,
            PaymentLines =
            [
                new PeriodPlanPaymentLine
                {
                    Id = Guid.NewGuid(),
                    PeriodPlanSnapshotId = id,
                    Name = "Kira",
                    PlannedDate = DateOnly.ParseExact(start, DatabaseConstants.DateFormat).AddDays(5),
                    PlannedAmount = 15000m,
                    SourceType = PlanPaymentSourceType.TemporaryPayment
                }
            ],
            IncomeLines =
            [
                new PeriodPlanIncomeLine
                {
                    Id = Guid.NewGuid(),
                    PeriodPlanSnapshotId = id,
                    Name = "Gelir",
                    PlannedDate = DateOnly.ParseExact(start, DatabaseConstants.DateFormat),
                    PlannedAmount = 50000m,
                    SourceType = IncomeSourceType.Recurring
                }
            ]
        };
}
