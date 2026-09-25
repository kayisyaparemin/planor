using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Entities;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqlitePeriodObservationRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqlitePeriodObservationRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_obs_{Guid.NewGuid():N}.db3");
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        new DatabaseSchema().EnsureInitializedAsync(_connection).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _connection.CloseAsync().GetAwaiter().GetResult();
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* cleanup */ }
        }
    }

    [Fact]
    public async Task GetPeriodObservationAsync_KayitYokken_NullDoner()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);

        var result = await repository.GetPeriodObservationAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertPeriodObservationAsync_GozlemVeOdemeleriKaydederVeOkur()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);

        var obsId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var paymentLineId = Guid.NewGuid();

        var payment = new PeriodObservationPayment
        {
            Id = Guid.NewGuid(),
            PeriodObservationId = obsId,
            PeriodPlanPaymentLineId = paymentLineId,
            Status = ActualPaymentStatus.Paid,
            ActualAmount = 2500m,
            ActualPaymentDate = new DateOnly(2026, 9, 20),
            Note = "Zamanında ödendi"
        };

        var observation = new PeriodObservation
        {
            Id = obsId,
            PeriodPlanSnapshotId = planId,
            ObservedOn = new DateOnly(2026, 9, 25),
            ObservedBalance = 45000m,
            ObservedLivingSpend = 8500m,
            Note = "Dönem ortası kontrolü",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Payments = [payment]
        };

        await repository.UpsertPeriodObservationAsync(observation);
        var loaded = await repository.GetPeriodObservationAsync(planId);

        Assert.NotNull(loaded);
        Assert.Equal(obsId, loaded.Id);
        Assert.Equal(45000m, loaded.ObservedBalance);
        Assert.Equal(8500m, loaded.ObservedLivingSpend);
        var loadedPayment = Assert.Single(loaded.Payments);
        Assert.Equal(paymentLineId, loadedPayment.PeriodPlanPaymentLineId);
        Assert.Equal(ActualPaymentStatus.Paid, loadedPayment.Status);
        Assert.Equal(2500m, loadedPayment.ActualAmount);
    }

    [Fact]
    public async Task UpsertPeriodObservationAsync_AyniPlanaYeniGozlemGelirse_EskisiniEzerekGunceller()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);

        var obs1Id = Guid.NewGuid();
        var obs2Id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var obs1 = new PeriodObservation
        {
            Id = obs1Id,
            PeriodPlanSnapshotId = planId,
            ObservedOn = new DateOnly(2026, 9, 20),
            ObservedBalance = 50000m,
            ObservedLivingSpend = 5000m,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        await repository.UpsertPeriodObservationAsync(obs1);

        var obs2 = new PeriodObservation
        {
            Id = obs2Id,
            PeriodPlanSnapshotId = planId,
            ObservedOn = new DateOnly(2026, 9, 25),
            ObservedBalance = 42000m,
            ObservedLivingSpend = 13000m,
            CreatedAtUtc = now,
            UpdatedAtUtc = now.AddDays(5)
        };
        await repository.UpsertPeriodObservationAsync(obs2);

        var loaded = await repository.GetPeriodObservationAsync(planId);
        Assert.NotNull(loaded);
        Assert.Equal(obs2Id, loaded.Id);
        Assert.Equal(42000m, loaded.ObservedBalance);
        Assert.Equal(13000m, loaded.ObservedLivingSpend);

        var planIdStr = planId.ToString();
        var count = await _connection.Table<PeriodObservationEntity>()
            .Where(x => x.PeriodPlanSnapshotId == planIdStr)
            .CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task DeletePeriodObservationAsync_GozlemiSiler_CascadeIleOdemeleriDeTemizler()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);

        var obsId = Guid.NewGuid();
        var payment = new PeriodObservationPayment
        {
            Id = Guid.NewGuid(),
            PeriodObservationId = obsId,
            PeriodPlanPaymentLineId = Guid.NewGuid(),
            Status = ActualPaymentStatus.Paid,
            ActualAmount = 1000m
        };
        var observation = new PeriodObservation
        {
            Id = obsId,
            PeriodPlanSnapshotId = planId,
            ObservedOn = new DateOnly(2026, 9, 25),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Payments = [payment]
        };
        await repository.UpsertPeriodObservationAsync(observation);

        await repository.DeletePeriodObservationAsync(planId);

        var loaded = await repository.GetPeriodObservationAsync(planId);
        Assert.Null(loaded);

        var obsIdStr = obsId.ToString();
        var payments = await _connection.Table<PeriodObservationPaymentEntity>()
            .Where(p => p.PeriodObservationId == obsIdStr)
            .ToListAsync();
        Assert.Empty(payments);
    }

    private async Task SeedPlanSnapshotAsync(Guid planId)
    {
        var snapshotId = Guid.NewGuid().ToString();
        await _connection.ExecuteAsync(
            """
            INSERT INTO financial_snapshots (Id, SnapshotDate, ProjectionAnchorDate, NextSettlementDate, ProjectionOpeningBalance, IncomeDay, Source, IsCurrent, CreatedAtUtc, Note)
            VALUES (?, '2026-09-01', '2026-09-01', '2026-10-01', 0, 15, 0, 1, '2026-09-01T00:00:00Z', '')
            """,
            snapshotId);

        await _connection.ExecuteAsync(
            """
            INSERT INTO period_plan_snapshots (
                Id, FinancialSnapshotId, PeriodStart, PeriodEnd, SettlementAvailableFrom, CreatedAtUtc,
                OpeningBalance, PlannedIncome, PlannedLoanPayments, PlannedCardPayments, PlannedTemporaryPayments,
                PlannedInstallmentPayments, PlannedOtherScheduledPayments, PlannedMandatoryPayments,
                PlannedVariableExpenseAllowance, PlannedLargeExpenses, PlannedCardInterest, PlannedDeficitInterest, PlannedEndingBalance)
            VALUES (?, ?, '2026-09-01', '2026-10-01', '2026-10-01', '2026-09-01T00:00:00Z', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
            """,
            planId.ToString(), snapshotId);
    }
}
