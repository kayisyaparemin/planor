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

    private static readonly DateTimeOffset Sabah = new(2026, 9, 10, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FindLatestCurrentSnapshot_BirdenFazlaGuncelVarsa_EnYeniTarihliyiSecer()
    {
        // Hazırla
        var eski = Durum(new DateOnly(2026, 8, 10), guncel: true, Sabah.AddDays(-30));
        var yeni = Durum(new DateOnly(2026, 9, 10), guncel: true, Sabah);
        var emekli = Durum(new DateOnly(2026, 10, 10), guncel: false, Sabah.AddDays(30));
        var history = new FinancialHistoryData([eski, emekli, yeni], [], [], []);

        // Uygula
        var secilen = history.FindLatestCurrentSnapshot();

        // Doğrula
        Assert.Equal(yeni.Id, secilen?.Id);
    }

    [Fact]
    public void FindLatestCurrentSnapshot_AyniTarihteIkiGuncelVarsa_EnSonOlusturulaniSecer()
    {
        // Hazırla
        var tarih = new DateOnly(2026, 9, 10);
        var sonraki = Durum(tarih, guncel: true, Sabah.AddHours(2));
        var onceki = Durum(tarih, guncel: true, Sabah);
        var history = new FinancialHistoryData([onceki, sonraki], [], [], []);

        // Uygula
        var secilen = history.FindLatestCurrentSnapshot();

        // Doğrula
        Assert.Equal(sonraki.Id, secilen?.Id);
    }

    [Fact]
    public void FindLatestCurrentSnapshot_GuncelDurumYoksa_NullDondurur()
    {
        // Hazırla
        var history = new FinancialHistoryData([Durum(new DateOnly(2026, 9, 10), guncel: false, Sabah)], [], [], []);

        // Uygula
        var secilen = history.FindLatestCurrentSnapshot();

        // Doğrula
        Assert.Null(secilen);
    }

    [Fact]
    public void FindOpenPlan_GuncelDurumunKapanmamisPlanini_EskiDurumunYetimPlaniYerineSecer()
    {
        // Hazırla — eski duruma bağlı plan daha sonra oluşturulmuş olsa bile açık dönem değildir
        var eskiDurum = Durum(new DateOnly(2026, 8, 10), guncel: false, Sabah.AddDays(-30));
        var guncelDurum = Durum(new DateOnly(2026, 9, 10), guncel: true, Sabah);
        var acikPlan = Plan(guncelDurum.Id, Sabah);
        var yetimPlan = Plan(eskiDurum.Id, Sabah.AddHours(1));
        var history = new FinancialHistoryData([eskiDurum, guncelDurum], [yetimPlan, acikPlan], [], []);

        // Uygula
        var secilen = history.FindOpenPlan();

        // Doğrula
        Assert.Equal(acikPlan.Id, secilen?.Id);
    }

    [Fact]
    public void FindOpenPlan_GuncelDurumunPlaniKapatilmissa_NullDondurur()
    {
        // Hazırla
        var guncelDurum = Durum(new DateOnly(2026, 9, 10), guncel: true, Sabah);
        var plan = Plan(guncelDurum.Id, Sabah);
        var kapanis = new PeriodActual { PeriodPlanSnapshotId = plan.Id, PeriodStart = plan.PeriodStart, PeriodEnd = plan.PeriodEnd };
        var history = new FinancialHistoryData([guncelDurum], [plan], [], [kapanis]);

        // Uygula
        var secilen = history.FindOpenPlan();

        // Doğrula
        Assert.Null(secilen);
    }

    [Fact]
    public void FindOpenPlan_AyniDurumaBagliIkiKapanmamisPlanVarsa_EnSonDondurulaniSecer()
    {
        // Hazırla
        var guncelDurum = Durum(new DateOnly(2026, 9, 10), guncel: true, Sabah);
        var ilkDondurma = Plan(guncelDurum.Id, Sabah);
        var yenidenDondurma = Plan(guncelDurum.Id, Sabah.AddMinutes(5));
        var history = new FinancialHistoryData([guncelDurum], [ilkDondurma, yenidenDondurma], [], []);

        // Uygula
        var secilen = history.FindOpenPlan();

        // Doğrula
        Assert.Equal(yenidenDondurma.Id, secilen?.Id);
    }

    [Fact]
    public void FindOpenPlan_TarihceBossa_NullDondurur()
    {
        // Hazırla
        var history = FinancialHistoryData.Empty;

        // Uygula
        var secilen = history.FindOpenPlan();

        // Doğrula
        Assert.Null(secilen);
    }

    [Fact]
    public void FindRevisions_YalnizIstenenPlaninRevizyonlarini_OlusturulmaVeSiraNumarasinaGoreDizer()
    {
        // Hazırla — ikinci ve üçüncü revizyon aynı anda oluşturulmuş; eşitliği sıra numarası bozar
        var planId = Guid.NewGuid();
        var ucuncu = Revizyon(planId, 3, Sabah.AddHours(1));
        var birinci = Revizyon(planId, 1, Sabah);
        var ikinci = Revizyon(planId, 2, Sabah.AddHours(1));
        var baskaPlaninki = Revizyon(Guid.NewGuid(), 1, Sabah.AddMinutes(30));
        var history = new FinancialHistoryData([], [], [ucuncu, baskaPlaninki, ikinci, birinci], []);

        // Uygula
        var revizyonlar = history.FindRevisions(planId);

        // Doğrula
        Assert.Equal([birinci.Id, ikinci.Id, ucuncu.Id], revizyonlar.Select(x => x.Id));
    }

    private static FinancialSnapshot Durum(DateOnly tarih, bool guncel, DateTimeOffset olusturulma) => new()
    {
        Id = Guid.NewGuid(),
        SnapshotDate = tarih,
        IsCurrent = guncel,
        CreatedAtUtc = olusturulma
    };

    private static PeriodPlanSnapshot Plan(Guid durumId, DateTimeOffset olusturulma) => new()
    {
        Id = Guid.NewGuid(),
        FinancialSnapshotId = durumId,
        PeriodStart = new DateOnly(2026, 9, 10),
        PeriodEnd = new DateOnly(2026, 10, 10),
        SettlementAvailableFrom = new DateOnly(2026, 10, 10),
        CreatedAtUtc = olusturulma
    };

    private static PeriodPlanRevision Revizyon(Guid planId, int siraNumarasi, DateTimeOffset olusturulma) => new()
    {
        Id = Guid.NewGuid(),
        PeriodPlanSnapshotId = planId,
        RevisionNumber = siraNumarasi,
        CreatedAtUtc = olusturulma
    };
}
