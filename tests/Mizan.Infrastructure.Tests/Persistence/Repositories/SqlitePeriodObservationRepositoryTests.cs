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
    public async Task UpsertPeriodObservationAsync_GozlemiKaydederVeOkur()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);

        var obsId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var observation = new PeriodObservation
        {
            Id = obsId,
            PeriodPlanSnapshotId = planId,
            ObservedOn = new DateOnly(2026, 9, 25),
            ObservedBalance = 45000m,
            ObservedLivingSpend = 8500m,
            Note = "Dönem ortası kontrolü",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await repository.UpsertPeriodObservationAsync(observation);
        var loaded = await repository.GetPeriodObservationAsync(planId);

        Assert.NotNull(loaded);
        Assert.Equal(obsId, loaded.Id);
        Assert.Equal(45000m, loaded.ObservedBalance);
        Assert.Equal(8500m, loaded.ObservedLivingSpend);
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
    public async Task DeletePeriodObservationAsync_GozlemiSiler()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        await repository.UpsertPeriodObservationAsync(Gozlem(planId));

        await repository.DeletePeriodObservationAsync(planId);

        Assert.Null(await repository.GetPeriodObservationAsync(planId));
    }

    [Fact]
    public async Task GetPaymentMarksAsync_IsaretYokken_BosListeDoner()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);

        var marks = await repository.GetPaymentMarksAsync(Guid.NewGuid());

        Assert.Empty(marks);
    }

    [Fact]
    public async Task UpsertPaymentMarkAsync_IsaretiKaydederVeOkur()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        var mark = Mark(planId, Guid.NewGuid());

        // Uygula
        await repository.UpsertPaymentMarkAsync(mark);
        var loaded = await repository.GetPaymentMarksAsync(planId);

        // Doğrula
        Assert.Equal([mark], loaded);
    }

    [Fact]
    public async Task UpsertPaymentMarkAsync_OdenmediIsareti_TarihsizVeNotsuzOkur()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        var mark = Mark(planId, Guid.NewGuid()) with
        {
            Status = ActualPaymentStatus.Unpaid,
            ActualAmount = 0m,
            ActualPaymentDate = null,
            Note = string.Empty
        };

        // Uygula
        await repository.UpsertPaymentMarkAsync(mark);

        // Doğrula
        Assert.Equal([mark], await repository.GetPaymentMarksAsync(planId));
    }

    /// <summary>I88: bir ödeme satırına en fazla bir işaret vardır.</summary>
    [Fact]
    public async Task UpsertPaymentMarkAsync_AyniSatiraYeniKimlikliIsaret_OncekininYerineGecer()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        var lineId = Guid.NewGuid();
        await repository.UpsertPaymentMarkAsync(Mark(planId, lineId));
        var sonIsaret = Mark(planId, lineId) with { Status = ActualPaymentStatus.Unpaid, ActualAmount = 0m };

        // Uygula
        await repository.UpsertPaymentMarkAsync(sonIsaret);

        // Doğrula
        Assert.Equal([sonIsaret], await repository.GetPaymentMarksAsync(planId));
    }

    [Fact]
    public async Task UpsertPaymentMarkAsync_AyniKimlikliIsaret_Gunceller()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        var mark = Mark(planId, Guid.NewGuid());
        await repository.UpsertPaymentMarkAsync(mark);

        // Uygula
        await repository.UpsertPaymentMarkAsync(mark with { ActualAmount = 999m });

        // Doğrula
        Assert.Equal(999m, Assert.Single(await repository.GetPaymentMarksAsync(planId)).ActualAmount);
    }

    [Fact]
    public async Task GetPaymentMarksAsync_YalnizIstenenPlaninIsaretleriniGetirir()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        var digerPlanId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        await SeedPlanSnapshotAsync(digerPlanId);
        var mark = Mark(planId, Guid.NewGuid());
        await repository.UpsertPaymentMarkAsync(mark);
        await repository.UpsertPaymentMarkAsync(Mark(digerPlanId, Guid.NewGuid()));

        // Uygula
        var loaded = await repository.GetPaymentMarksAsync(planId);

        // Doğrula
        Assert.Equal([mark.Id], loaded.Select(x => x.Id));
    }

    /// <summary>S68-7: kapanış gözlemi silse de işaretler dönemin tarihçesinde kalır.</summary>
    [Fact]
    public async Task DeletePeriodObservationAsync_IsaretleriSilmez()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        await repository.UpsertPeriodObservationAsync(Gozlem(planId));
        await repository.UpsertPaymentMarkAsync(Mark(planId, Guid.NewGuid()));

        // Uygula
        await repository.DeletePeriodObservationAsync(planId);

        // Doğrula
        Assert.Single(await repository.GetPaymentMarksAsync(planId));
    }

    [Fact]
    public async Task GetPaymentMarksAsync_PlanSilinince_IsaretlerDeGider()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        await repository.UpsertPaymentMarkAsync(Mark(planId, Guid.NewGuid()));

        // Uygula
        await _connection.ExecuteAsync("DELETE FROM period_plan_snapshots WHERE Id = ?", planId.ToString());

        // Doğrula
        Assert.Empty(await repository.GetPaymentMarksAsync(planId));
    }

    private static PeriodObservation Gozlem(Guid planId) => new()
    {
        PeriodPlanSnapshotId = planId,
        ObservedOn = new DateOnly(2026, 9, 25),
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    private static PeriodPaymentMark Mark(Guid planId, Guid lineId) => new()
    {
        PeriodPlanSnapshotId = planId,
        PeriodPlanPaymentLineId = lineId,
        Status = ActualPaymentStatus.DifferentAmount,
        ActualAmount = 2_500m,
        ActualPaymentDate = new DateOnly(2026, 9, 20),
        Note = "Erken ödendi"
    };

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
