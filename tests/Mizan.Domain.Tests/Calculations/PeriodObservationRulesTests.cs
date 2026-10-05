using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class PeriodObservationRulesTests
{
    private const int Yil = 2026;
    private static readonly Guid PlanId = Guid.NewGuid();

    // Eylül dönemi: 1 Eylül dahil, 1 Ekim (sonraki dönemin ilk günü) hariç. Son gün 30 Eylül.
    private static readonly CashFlowPeriod Eylul = new(new DateOnly(Yil, 9, 1), new DateOnly(Yil, 10, 1));

    [Theory]
    [InlineData(9, 1)]
    [InlineData(9, 4)]
    [InlineData(9, 14)]
    public void CanObserveOn_DonemIcindeBugunVeyaOncesi_Yazilabilir(int ay, int gun)
    {
        // Hazırla
        var bugun = new DateOnly(Yil, 9, 14);

        // Uygula
        var yazilabilir = PeriodObservationRules.CanObserveOn(Eylul, new DateOnly(Yil, ay, gun), bugun);

        // Doğrula
        Assert.True(yazilabilir);
    }

    [Fact]
    public void CanObserveOn_DonemSonGunuBugunse_Yazilabilir()
    {
        // Hazırla
        var sonGun = new DateOnly(Yil, 9, 30);

        // Uygula
        var yazilabilir = PeriodObservationRules.CanObserveOn(Eylul, sonGun, today: sonGun);

        // Doğrula
        Assert.True(yazilabilir);
    }

    [Theory]
    [InlineData(9, 14, 9, 15)]
    [InlineData(8, 25, 9, 1)]
    public void CanObserveOn_GelecekGun_Yazilamaz(int bugunAy, int bugunGun, int ay, int gun)
    {
        // Hazırla
        var bugun = new DateOnly(Yil, bugunAy, bugunGun);

        // Uygula
        var yazilabilir = PeriodObservationRules.CanObserveOn(Eylul, new DateOnly(Yil, ay, gun), bugun);

        // Doğrula
        Assert.False(yazilabilir);
    }

    [Fact]
    public void CanObserveOn_DonemBaslamadanOnce_Yazilamaz()
    {
        // Hazırla
        var bugun = new DateOnly(Yil, 9, 14);

        // Uygula
        var yazilabilir = PeriodObservationRules.CanObserveOn(Eylul, new DateOnly(Yil, 8, 31), bugun);

        // Doğrula
        Assert.False(yazilabilir);
    }

    [Theory]
    [InlineData(10, 1, 9, 30)]
    [InlineData(10, 3, 9, 30)]
    [InlineData(10, 3, 9, 1)]
    [InlineData(10, 3, 10, 3)]
    public void CanObserveOn_DonemBittiKapanmadi_HicbirGuneYazilamaz(int bugunAy, int bugunGun, int ay, int gun)
    {
        // Hazırla: Eylül bitti, Ekim geliri yattı ama Eylül kapatılmadı.
        var bugun = new DateOnly(Yil, bugunAy, bugunGun);

        // Uygula
        var yazilabilir = PeriodObservationRules.CanObserveOn(Eylul, new DateOnly(Yil, ay, gun), bugun);

        // Doğrula
        Assert.False(yazilabilir);
    }

    [Fact]
    public void Record_DonemdeGozlemYok_GozlemiEkler()
    {
        // Hazırla
        var giris = Gozlem(9, 4, 97_000m);

        // Uygula
        var gozlemler = PeriodObservationRules.Record([], giris);

        // Doğrula
        Assert.Equal(giris, Assert.Single(gozlemler));
    }

    [Fact]
    public void Record_GeriyeTarihliGiris_GuneGoreSirayaGirer()
    {
        // Hazırla
        var kayitli = new[] { Gozlem(9, 4, 97_000m), Gozlem(9, 14, 65_000m) };

        // Uygula
        var gozlemler = PeriodObservationRules.Record(kayitli, Gozlem(9, 10, 80_000m));

        // Doğrula
        Assert.Equal(
            new[] { new DateOnly(Yil, 9, 4), new DateOnly(Yil, 9, 10), new DateOnly(Yil, 9, 14) },
            gozlemler.Select(x => x.ObservedOn));
    }

    [Fact]
    public void Record_AyniGunIkinciGiris_OncekininYerineGecer()
    {
        // Hazırla: 4 Eylül sabah 97.000, akşam 95.000.
        var kayitli = new[] { Gozlem(9, 4, 97_000m), Gozlem(9, 14, 65_000m) };

        // Uygula
        var gozlemler = PeriodObservationRules.Record(kayitli, Gozlem(9, 4, 95_000m));

        // Doğrula
        Assert.Equal(95_000m, gozlemler[0].ObservedBalance);
        Assert.Equal(65_000m, gozlemler[1].ObservedBalance);
    }

    [Fact]
    public void Latest_GozlemYok_NullDoner()
    {
        // Uygula
        var son = PeriodObservationRules.Latest([]);

        // Doğrula
        Assert.Null(son);
    }

    [Fact]
    public void Latest_GeriyeTarihliGirisSonraYapilsaDa_EnGecTarihliGozlemiDoner()
    {
        // Hazırla: 14 Eylül'ün bakiyesi girildi, iki gün sonra 10 Eylül'ün bakiyesi geriye tarihli eklendi.
        var ondortEylul = Gozlem(9, 14, 65_000m, kayitGunu: 14);
        var onEylul = Gozlem(9, 10, 80_000m, kayitGunu: 16);

        // Uygula
        var son = PeriodObservationRules.Latest([ondortEylul, onEylul]);

        // Doğrula
        Assert.Equal(ondortEylul, son);
    }

    private static PeriodObservation Gozlem(int ay, int gun, decimal bakiye, int? kayitGunu = null)
    {
        var kayitZamani = new DateTimeOffset(Yil, ay, kayitGunu ?? gun, 9, 0, 0, TimeSpan.Zero);
        return new PeriodObservation
        {
            PeriodPlanSnapshotId = PlanId,
            ObservedOn = new DateOnly(Yil, ay, gun),
            ObservedBalance = bakiye,
            RecordedAtUtc = kayitZamani
        };
    }
}
