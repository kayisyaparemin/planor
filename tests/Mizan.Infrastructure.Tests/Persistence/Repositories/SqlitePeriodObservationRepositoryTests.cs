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
    public async Task GetPeriodObservationsAsync_KayitYokken_BosListeDoner()
    {
        var repository = new SqlitePeriodObservationRepository(_connection);

        var result = await repository.GetPeriodObservationsAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpsertPeriodObservationAsync_GozlemiKaydederVeOkur()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        var kayitAni = new DateTimeOffset(2026, 9, 25, 9, 30, 0, TimeSpan.Zero);
        var observation = Gozlem(planId, new DateOnly(2026, 9, 25), 45_000m) with { RecordedAtUtc = kayitAni };

        // Uygula
        await repository.UpsertPeriodObservationAsync(observation);

        // Doğrula
        var loaded = Assert.Single(await repository.GetPeriodObservationsAsync(planId));
        Assert.Equal(observation, loaded);
    }

    /// <summary>I90: gün başına tek gözlem; aynı plan ve güne ikinci giriş öncekinin yerine geçer, farklı gün ayrı nokta olur.</summary>
    [Fact]
    public async Task UpsertPeriodObservationAsync_AyniGuneYeniGiris_OncekininYerineGecer()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        var gun = new DateOnly(2026, 9, 20);
        await repository.UpsertPeriodObservationAsync(Gozlem(planId, gun, 50_000m));
        await repository.UpsertPeriodObservationAsync(Gozlem(planId, gun.AddDays(-5), 60_000m));
        var duzeltme = Gozlem(planId, gun, 42_000m);

        // Uygula
        await repository.UpsertPeriodObservationAsync(duzeltme);

        // Doğrula
        var loaded = await repository.GetPeriodObservationsAsync(planId);
        Assert.Equal(2, loaded.Count);
        Assert.Equal(new[] { gun.AddDays(-5), gun }, loaded.Select(x => x.ObservedOn).ToArray());
        Assert.Equal(duzeltme.Id, loaded[1].Id);
        Assert.Equal(42_000m, loaded[1].ObservedBalance);
    }

    [Fact]
    public async Task GetPeriodObservationsAsync_DigerPlaninGozlemleriniKatmaz()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        var digerPlanId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        await SeedPlanSnapshotAsync(digerPlanId);
        await repository.UpsertPeriodObservationAsync(Gozlem(planId, new DateOnly(2026, 9, 20), 1m));
        await repository.UpsertPeriodObservationAsync(Gozlem(digerPlanId, new DateOnly(2026, 9, 20), 2m));

        // Uygula
        var loaded = await repository.GetPeriodObservationsAsync(planId);

        // Doğrula
        Assert.Equal(1m, Assert.Single(loaded).ObservedBalance);
    }

    [Fact]
    public async Task DeletePeriodObservationAsync_PlaninButunGozlemleriniSiler()
    {
        // Hazırla
        var repository = new SqlitePeriodObservationRepository(_connection);
        var planId = Guid.NewGuid();
        await SeedPlanSnapshotAsync(planId);
        await repository.UpsertPeriodObservationAsync(Gozlem(planId, new DateOnly(2026, 9, 20), 1m));
        await repository.UpsertPeriodObservationAsync(Gozlem(planId, new DateOnly(2026, 9, 21), 2m));

        // Uygula
        await repository.DeletePeriodObservationAsync(planId);

        // Doğrula
        Assert.Empty(await repository.GetPeriodObservationsAsync(planId));
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
        await repository.UpsertPeriodObservationAsync(Gozlem(planId, new DateOnly(2026, 9, 25), 1m));
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

    private static PeriodObservation Gozlem(Guid planId, DateOnly gun, decimal bakiye) => new()
    {
        PeriodPlanSnapshotId = planId,
        ObservedOn = gun,
        ObservedBalance = bakiye,
        RecordedAtUtc = new DateTimeOffset(gun.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero)
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
