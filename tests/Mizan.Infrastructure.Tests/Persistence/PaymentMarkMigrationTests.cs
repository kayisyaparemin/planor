using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence;

/// <summary>
/// v1 → v2 göçünün (S68-8) sözünü doğrular: gözlemin çocuğu olan ödeme kayıtları plana bağlı ödeme işaretine
/// dönüşür, hiçbiri kaybolmaz ve gözlemin kendisi olduğu gibi kalır. Telefondaki gerçek profil bu yoldan geçer.
/// </summary>
public sealed class PaymentMarkMigrationTests : IAsyncLifetime
{
    private readonly string _yol = Path.Combine(Path.GetTempPath(), $"mizan_test_isaret_goc_{Guid.NewGuid():N}.db3");
    private SQLiteAsyncConnection _baglanti = null!;

    public async Task InitializeAsync()
    {
        SQLitePCL.Batteries_V2.Init();
        _baglanti = new SQLiteAsyncConnection(_yol);
        await new DatabaseSchema([SchemaMigrations.All[0]]).EnsureInitializedAsync(_baglanti);
    }

    public async Task DisposeAsync()
    {
        await _baglanti.CloseAsync();
        foreach (var ek in new[] { "", "-shm", "-wal", "-journal" })
        {
            try { File.Delete(_yol + ek); } catch { /* temizlik */ }
        }
    }

    /// <summary>I89: v1'deki gözlem ödemeleri v2'de aynı kimlik ve değerlerle, gözlemin planına bağlı işaret olur.</summary>
    [Fact]
    public async Task EnsureInitializedAsync_V1denYukseltilirken_GozlemOdemeleriPlanaBagliIsaretOlur()
    {
        // Hazırla
        var plan = Guid.NewGuid();
        var kira = Guid.NewGuid();
        var kart = Guid.NewGuid();
        await PlanYazAsync(plan);
        var gozlem = await GozlemYazAsync(plan, bakiye: "40000");
        var kiraIsareti = await OdemeYazAsync(gozlem, kira, durum: 0, tutar: "15000", tarih: "'2026-09-05'", not: "Kira");
        var kartIsareti = await OdemeYazAsync(gozlem, kart, durum: 2, tutar: "0", tarih: "NULL", not: "");

        // Uygula
        await new DatabaseSchema().EnsureInitializedAsync(_baglanti);

        // Doğrula
        var isaretler = (await new SqlitePeriodObservationRepository(_baglanti).GetPaymentMarksAsync(plan))
            .OrderBy(x => x.Status).ToArray();
        Assert.Equal(2, isaretler.Length);
        Assert.Equal(Guid.Parse(kiraIsareti), isaretler[0].Id);
        Assert.Equal(kira, isaretler[0].PeriodPlanPaymentLineId);
        Assert.Equal(ActualPaymentStatus.Paid, isaretler[0].Status);
        Assert.Equal(15_000m, isaretler[0].ActualAmount);
        Assert.Equal(new DateOnly(2026, 9, 5), isaretler[0].ActualPaymentDate);
        Assert.Equal("Kira", isaretler[0].Note);
        Assert.Equal(Guid.Parse(kartIsareti), isaretler[1].Id);
        Assert.Equal(ActualPaymentStatus.Unpaid, isaretler[1].Status);
        Assert.Null(isaretler[1].ActualPaymentDate);
    }

    [Fact]
    public async Task EnsureInitializedAsync_V1denYukseltilirken_EskiOdemeTablosuDuserGozlemKalir()
    {
        // Hazırla
        var plan = Guid.NewGuid();
        await PlanYazAsync(plan);
        var gozlem = await GozlemYazAsync(plan, bakiye: "40000");
        await OdemeYazAsync(gozlem, Guid.NewGuid(), durum: 0, tutar: "1", tarih: "NULL", not: "");

        // Uygula
        await new DatabaseSchema().EnsureInitializedAsync(_baglanti);

        // Doğrula
        var eskiTablo = await _baglanti.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE name = 'period_observation_payments';");
        Assert.Equal(0, eskiTablo);
        var kalan = await new SqlitePeriodObservationRepository(_baglanti).GetPeriodObservationAsync(plan);
        Assert.Equal(40_000m, kalan?.ObservedBalance);
    }

    [Fact]
    public async Task EnsureInitializedAsync_V1denYukseltilirken_IsaretsizVeritabaniBozulmaz()
    {
        // Hazırla
        var plan = Guid.NewGuid();
        await PlanYazAsync(plan);
        await GozlemYazAsync(plan, bakiye: "40000");

        // Uygula
        await new DatabaseSchema().EnsureInitializedAsync(_baglanti);

        // Doğrula
        Assert.Empty(await new SqlitePeriodObservationRepository(_baglanti).GetPaymentMarksAsync(plan));
        Assert.Equal(SchemaMigrations.CurrentVersion,
            await _baglanti.ExecuteScalarAsync<int>("PRAGMA user_version;"));
    }

    private async Task PlanYazAsync(Guid plan)
    {
        var anlikDurum = Guid.NewGuid().ToString();
        await _baglanti.ExecuteAsync(
            """
            INSERT INTO financial_snapshots (Id, SnapshotDate, ProjectionAnchorDate, NextSettlementDate, ProjectionOpeningBalance, IncomeDay, Source, IsCurrent, CreatedAtUtc, Note)
            VALUES (?, '2026-09-01', '2026-09-01', '2026-10-01', 0, 15, 0, 1, '2026-09-01T00:00:00Z', '')
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

    private async Task<string> GozlemYazAsync(Guid plan, string bakiye)
    {
        var kimlik = Guid.NewGuid().ToString();
        await _baglanti.ExecuteAsync(
            $"""
            INSERT INTO period_observations (Id, PeriodPlanSnapshotId, ObservedOn, ObservedBalance, ObservedLivingSpend, Note, CreatedAtUtc, UpdatedAtUtc)
            VALUES (?, ?, '2026-09-10', {bakiye}, 0, '', '2026-09-10T09:00:00Z', '2026-09-10T09:00:00Z')
            """,
            kimlik, plan.ToString());
        return kimlik;
    }

    private async Task<string> OdemeYazAsync(string gozlem, Guid satir, int durum, string tutar, string tarih, string not)
    {
        var kimlik = Guid.NewGuid().ToString();
        await _baglanti.ExecuteAsync(
            $"""
            INSERT INTO period_observation_payments (Id, PeriodObservationId, PeriodPlanPaymentLineId, Status, ActualAmount, ActualPaymentDate, Note)
            VALUES (?, ?, ?, {durum}, {tutar}, {tarih}, ?)
            """,
            kimlik, gozlem, satir.ToString(), not);
        return kimlik;
    }
}
