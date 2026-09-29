using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence;

/// <summary>
/// v2 → v3 göçünün (S68-2, S68-9) sözünü doğrular: gözlem tablosu yeni şekle geçerken bakiyesi olan her gözlem
/// kimliği ve değeriyle taşınır, bakiyesiz eski satırlar elenir ve tablo gün başına tek gözlemi kısıtla söyler.
/// </summary>
public sealed class ObservationShapeMigrationTests : IAsyncLifetime
{
    private readonly string _yol = Path.Combine(Path.GetTempPath(), $"mizan_test_gozlem_goc_{Guid.NewGuid():N}.db3");
    private SQLiteAsyncConnection _baglanti = null!;

    public async Task InitializeAsync()
    {
        SQLitePCL.Batteries_V2.Init();
        _baglanti = new SQLiteAsyncConnection(_yol);
        await new DatabaseSchema([.. SchemaMigrations.All.Take(2)]).EnsureInitializedAsync(_baglanti);
    }

    public async Task DisposeAsync()
    {
        await _baglanti.CloseAsync();
        foreach (var ek in new[] { "", "-shm", "-wal", "-journal" })
        {
            try { File.Delete(_yol + ek); } catch { /* temizlik */ }
        }
    }

    /// <summary>I91: v2 → v3 göçü bakiyeli gözlemi aynı kimlik, gün ve değerle taşır; kayıt zamanı eski son yazma anıdır; bakiyesiz satırı eler.</summary>
    [Fact]
    public async Task EnsureInitializedAsync_V2denYukseltilirken_BakiyeliGozlemTasinirBakiyesizElenir()
    {
        // Hazırla
        var bakiyeliPlan = Guid.NewGuid();
        var bakiyesizPlan = Guid.NewGuid();
        await PlanYazAsync(bakiyeliPlan);
        await PlanYazAsync(bakiyesizPlan);
        var kimlik = await GozlemYazAsync(bakiyeliPlan, bakiye: "40000.5", olusturma: "2026-09-10T09:00:00Z", guncelleme: "2026-09-12T18:30:00Z");
        await GozlemYazAsync(bakiyesizPlan, bakiye: "NULL", olusturma: "2026-09-10T09:00:00Z", guncelleme: "2026-09-10T09:00:00Z");

        // Uygula
        await new DatabaseSchema().EnsureInitializedAsync(_baglanti);

        // Doğrula
        var depo = new SqlitePeriodObservationRepository(_baglanti);
        var tasinan = Assert.Single(await depo.GetPeriodObservationsAsync(bakiyeliPlan));
        Assert.Equal(Guid.Parse(kimlik), tasinan.Id);
        Assert.Equal(new DateOnly(2026, 9, 10), tasinan.ObservedOn);
        Assert.Equal(40_000.5m, tasinan.ObservedBalance);
        Assert.Equal(new DateTimeOffset(2026, 9, 12, 18, 30, 0, TimeSpan.Zero), tasinan.RecordedAtUtc);
        Assert.Empty(await depo.GetPeriodObservationsAsync(bakiyesizPlan));
        Assert.Equal(SchemaMigrations.CurrentVersion, await _baglanti.ExecuteScalarAsync<int>("PRAGMA user_version;"));
    }

    /// <summary>I90'ın tablo tarafı: aynı plan ve güne ikinci satır doğrudan yazılamaz.</summary>
    [Fact]
    public async Task EnsureInitializedAsync_V3Tablosu_AyniPlanVeGuneIkinciSatiriReddeder()
    {
        // Hazırla
        var plan = Guid.NewGuid();
        await PlanYazAsync(plan);
        await new DatabaseSchema().EnsureInitializedAsync(_baglanti);
        await YeniSekliYazAsync(plan);

        // Uygula
        var ikinci = async () => await YeniSekliYazAsync(plan);

        // Doğrula
        await Assert.ThrowsAnyAsync<SQLiteException>(ikinci);
    }

    private Task<int> YeniSekliYazAsync(Guid plan) =>
        _baglanti.ExecuteAsync(
            """
            INSERT INTO period_observations (Id, PeriodPlanSnapshotId, ObservedOn, ObservedBalance, RecordedAtUtc)
            VALUES (?, ?, '2026-09-10', 1000, '2026-09-10T09:00:00Z')
            """,
            Guid.NewGuid().ToString(), plan.ToString());

    private async Task PlanYazAsync(Guid plan)
    {
        var anlikDurum = Guid.NewGuid().ToString();
        await _baglanti.ExecuteAsync(
            """
            INSERT INTO financial_snapshots (Id, SnapshotDate, ProjectionAnchorDate, NextSettlementDate, ProjectionOpeningBalance, IncomeDay, Source, IsCurrent, CreatedAtUtc, Note)
            VALUES (?, '2026-09-01', '2026-09-01', '2026-10-01', 0, 15, 0, 0, '2026-09-01T00:00:00Z', '')
            """,
            anlikDurum);
        await _baglanti.ExecuteAsync(
            """
            INSERT INTO period_plan_snapshots (
                Id, FinancialSnapshotId, PeriodStart, PeriodEnd, SettlementAvailableFrom, CreatedAtUtc,
                OpeningBalance, PlannedIncome, PlannedLoanPayments, PlannedCardPayments, PlannedTemporaryPayments,
                PlannedInstallmentPayments, PlannedOtherScheduledPayments, PlannedMandatoryPayments,
                PlannedVariableExpenseAllowance, PlannedLargeExpenses, PlannedCardInterest, PlannedDeficitInterest, PlannedEndingBalance)
            VALUES (?, ?, '2026-09-01', '2026-10-01', '2026-10-01', '2026-09-01T00:00:00Z', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
            """,
            plan.ToString(), anlikDurum);
    }

    private async Task<string> GozlemYazAsync(Guid plan, string bakiye, string olusturma, string guncelleme)
    {
        var kimlik = Guid.NewGuid().ToString();
        await _baglanti.ExecuteAsync(
            $"""
            INSERT INTO period_observations (Id, PeriodPlanSnapshotId, ObservedOn, ObservedBalance, ObservedLivingSpend, Note, CreatedAtUtc, UpdatedAtUtc)
            VALUES (?, ?, '2026-09-10', {bakiye}, 0, '', ?, ?)
            """,
            kimlik, plan.ToString(), olusturma, guncelleme);
        return kimlik;
    }
}
