using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Bakiye rotasını (S71) gidişatın içinden doğrular: rota ekrana gidişatla birlikte gider ve son noktası
/// gidişatın rakamına eşit olmak zorundadır.
/// Senaryo: dönem 1 Eylül – 1 Ekim (30 gün). Açılış 30.000; kira 5'inde 15.000; gelir 15'inde 40.000;
/// kart 20'sinde 12.000. Havuz 20.000 iken plan 23.000 ile kapanır; havuz 50.000 iken −7.000'e düşer,
/// %5 KMH faizi 350 olur ve plan −7.350 ile kapanır.
/// Nokta günün başındaki bakiyedir: 5'indeki kira 6'sının noktasında görünür.
/// </summary>
public sealed class PeriodBalancePathCalculatorTests
{
    private const decimal KmhOrani = 0.05m;
    private static readonly DateOnly DonemBasi = new(2026, 9, 1);
    private static readonly DateOnly DonemSonu = new(2026, 10, 1);
    private static readonly Guid KartId = Guid.NewGuid();

    private static readonly PeriodPlanPaymentLine Kira = Satir("Kira", Gun(5), 15_000m, PlanPaymentSourceType.OtherScheduledPayment, Guid.NewGuid());
    private static readonly PeriodPlanPaymentLine Kart = Satir("Kart", Gun(20), 12_000m, PlanPaymentSourceType.CreditCard, KartId);
    private static readonly PeriodPlanIncomeLine Gelir = new() { Name = "Gelir", PlannedDate = Gun(15), PlannedAmount = 40_000m };

    private static readonly PeriodPlanSnapshot Plan = PlanKur(yasamHavuzu: 20_000m, kmhFaizi: 0m, kapanis: 23_000m);
    private static readonly PeriodPlanSnapshot AcikliPlan = PlanKur(yasamHavuzu: 50_000m, kmhFaizi: 350m, kapanis: -7_350m);

    // ---------------------------------------------------------------
    // Gözlem yok: rota saf plandır (S71-6)
    // ---------------------------------------------------------------

    [Fact]
    public void FromPlan_GozlemYoksa_DonemBasindanBitisGununeHerGunBirNokta()
    {
        // Uygula
        var rota = Hesapla(Defter(Plan), Gun(12)).Path;

        // Doğrula — 30 günlük dönemde 31 nokta: dönem başı ve bitiş günü dahil
        Assert.Equal([new BalancePathPoint(DonemBasi, 30_000m)], rota.Travelled);
        Assert.Equal(Enumerable.Range(0, 31).Select(DonemBasi.AddDays), rota.Ahead.Select(x => x.Date));
        Assert.Equal(rota.Travelled[^1], rota.Ahead[0]);
    }

    [Fact]
    public void FromPlan_GozlemYoksa_HareketErtesiGunGorunurHavuzGunlereEsitYayilir()
    {
        // Uygula
        var rota = Hesapla(Defter(Plan), Gun(12)).Path;

        // Doğrula — havuz günde 666,67 iner; kira 5'inde, 6'sının noktasında düşer
        Assert.Equal(27_333.33m, Bakiye(rota.Ahead, Gun(5)));
        Assert.Equal(11_666.67m, Bakiye(rota.Ahead, Gun(6)));
        Assert.Equal(45_000m, Bakiye(rota.Ahead, Gun(16)));
    }

    [Fact]
    public void FromPlan_GozlemYoksa_SonNoktaPlanlananKapanistir()
    {
        // Uygula
        var gidisat = Hesapla(Defter(Plan), Gun(12));

        // Doğrula — 20.000 / 30 kuruşa bölünmüyor; artık birikseydi son nokta 23.000'den kayardı
        Assert.Equal(new BalancePathPoint(DonemSonu, 23_000m), gidisat.Path.Ahead[^1]);
        Assert.Equal(gidisat.PlannedEndingBalance, gidisat.Path.Ahead[^1].Balance);
    }

    [Fact]
    public void FromPlan_AcikliPlan_KmhFaiziYalnizSonNoktadaDusulur()
    {
        // Uygula
        var rota = Hesapla(Defter(AcikliPlan), Gun(12)).Path;

        // Doğrula — son günün başında 43.000 − 48.333,33; faiz ancak dönem sonunda işler
        Assert.Equal(-5_333.33m, Bakiye(rota.Ahead, Gun(30)));
        Assert.Equal(-7_350m, rota.Ahead[^1].Balance);
    }

    [Fact]
    public void FromPlan_RevizyonVarsa_SonRevizyonunSatirlarindanCizer()
    {
        // Hazırla — dönem içinde kira 10.000'e indi, plan 28.000 ile kapanıyor
        var revizyon = new PeriodPlanRevision
        {
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedEndingBalance = 28_000m,
            PaymentLines = [Kira with { PlannedAmount = 10_000m }, Kart],
            IncomeLines = [Gelir]
        };

        // Uygula
        var gidisat = Hesapla(Defter(Plan, [], revizyonlar: [revizyon]), Gun(12));

        // Doğrula
        Assert.Equal(28_000m, gidisat.Path.Ahead[^1].Balance);
        Assert.Equal(16_666.67m, Bakiye(gidisat.Path.Ahead, Gun(6)));
    }

    // ---------------------------------------------------------------
    // Katedilen yol: gözlemlerden geçer, aralar plandan (S71-3, 4)
    // ---------------------------------------------------------------

    [Fact]
    public void FromObservations_IkiGozlemArasi_PlanOlaylariKendiGunundeFarkGunlereYayilir()
    {
        // Hazırla — 3'ünde 29.000, 10'unda 7.000; arada kira 15.000, açıklanamayan 7.000 yedi güne: günde 1.000
        var defter = Defter(Plan, Gozlem(Gun(3), 29_000m), Gozlem(Gun(10), 7_000m));

        // Uygula
        var rota = Hesapla(defter, Gun(10)).Path;

        // Doğrula — açılıştan 3'üne 1.000 iki güne yayılır; kira 6'sının noktasında düşer
        decimal[] beklenen = [30_000m, 29_500m, 29_000m, 28_000m, 27_000m, 11_000m, 10_000m, 9_000m, 8_000m, 7_000m];
        Assert.Equal(Enumerable.Range(1, 10).Select(Gun), rota.Travelled.Select(x => x.Date));
        Assert.Equal(beklenen, rota.Travelled.Select(x => x.Balance));
    }

    [Fact]
    public void FromObservations_HerGozlemdenGirilenTutarlaGecer()
    {
        // Hazırla
        PeriodObservation[] gozlemler = [Gozlem(Gun(18), 40_000m), Gozlem(Gun(3), 29_000m), Gozlem(Gun(10), 7_000m)];

        // Uygula
        var rota = Hesapla(Defter(Plan, gozlemler), Gun(18)).Path;

        // Doğrula — giriş sırası değil tarih; son gözlem iki parçanın ortak noktası
        Assert.All(gozlemler, x => Assert.Equal(x.ObservedBalance, Bakiye(rota.Travelled, x.ObservedOn)));
        Assert.Equal(Enumerable.Range(1, 18).Select(Gun), rota.Travelled.Select(x => x.Date));
        Assert.Equal(new BalancePathPoint(Gun(18), 40_000m), rota.Ahead[0]);
    }

    [Fact]
    public void FromObservations_GelirGunuGirilenBakiye_GelirGozlemNoktasindaGorunur()
    {
        // Hazırla — 15'inde 48.000: açıklanamayan 7.000 on dört güne, günde 500
        var defter = Defter(Plan, Gozlem(Gelir.PlannedDate, 48_000m));

        // Uygula
        var rota = Hesapla(defter, Gelir.PlannedDate).Path;

        // Doğrula — 14'ünün başında gelir henüz yok; gözlem gelirle birlikte
        Assert.Equal(8_500m, Bakiye(rota.Travelled, Gun(14)));
        Assert.Equal(48_000m, Bakiye(rota.Travelled, Gun(15)));
    }

    [Fact]
    public void FromObservations_DonemBasindaGozlem_AcilisBakiyesininYerineGecer()
    {
        // Uygula
        var rota = Hesapla(Defter(Plan, Gozlem(DonemBasi, 31_000m)), DonemBasi).Path;

        // Doğrula
        Assert.Equal([new BalancePathPoint(DonemBasi, 31_000m)], rota.Travelled);
    }

    [Fact]
    public void FromObservations_DonemBasindanOnceTarihliEskiGozlem_DonemBasinaCekilir()
    {
        // Hazırla — A28'den önceki kurulum gözlemi çapa gününden önceye düşebiliyordu (S71 açık not a)
        var defter = Defter(Plan, Gozlem(DonemBasi.AddDays(-4), 29_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula
        Assert.Equal([new BalancePathPoint(DonemBasi, 29_000m)], gidisat.Path.Travelled);
        Assert.Equal(31, gidisat.Path.Ahead.Count);
        Assert.Equal(gidisat.ProjectedEndingBalance, gidisat.Path.Ahead[^1].Balance);
    }

    [Fact]
    public void FromObservations_DonemSonundanSonraTarihliEskiGozlem_SonGuneCekilir()
    {
        // Hazırla — A28'den önce ertelenmiş kapanışta giriş bugünün tarihiyle dönem sonrasına yazılıyordu
        var gun = DonemSonu.AddDays(2);
        var defter = Defter(Plan, Gozlem(gun, 20_000m));

        // Uygula
        var gidisat = Hesapla(defter, gun);

        // Doğrula
        Assert.Equal(new BalancePathPoint(DonemSonu.AddDays(-1), 20_000m), gidisat.Path.Travelled[^1]);
        Assert.Equal(new BalancePathPoint(DonemSonu, 20_000m), gidisat.Path.Ahead[^1]);
        Assert.Equal(gidisat.ProjectedEndingBalance, gidisat.Path.Ahead[^1].Balance);
    }

    // ---------------------------------------------------------------
    // Önümüzdeki yol: gidişatın terimleri günlere dağılır (S71-5)
    // ---------------------------------------------------------------

    [Fact]
    public void FromObservations_SonNoktaDonemSonuTahminidir()
    {
        // Hazırla — 12'sinde 8.000: 7.000 harcanmış, kalan havuz 13.000 on dokuz güne
        var defter = Defter(Plan, Gozlem(Gun(12), 8_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula — gelir 16'sında, kart 21'inde görünür
        var ileri = gidisat.Path.Ahead;
        Assert.Equal(new BalancePathPoint(Gun(12), 8_000m), ileri[0]);
        Assert.Equal(5_947.37m, Bakiye(ileri, Gun(15)));
        Assert.Equal(45_263.16m, Bakiye(ileri, Gun(16)));
        Assert.Equal(29_842.11m, Bakiye(ileri, Gun(21)));
        Assert.Equal(new BalancePathPoint(DonemSonu, 23_000m), ileri[^1]);
        Assert.Equal(gidisat.ProjectedEndingBalance, ileri[^1].Balance);
    }

    [Fact]
    public void FromObservations_KartinBugunkuTutariFarkliysa_SonNoktaGidisatlaAyniKalir()
    {
        // Hazırla — kart dönem içinde büyüdü: 12.000 yerine 14.000 (I23)
        var defter = Defter(Plan, Gozlem(Gun(12), 8_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12), new Dictionary<Guid, decimal> { [KartId] = 14_000m });

        // Doğrula
        Assert.Equal(21_000m, gidisat.ProjectedEndingBalance);
        Assert.Equal(21_000m, gidisat.Path.Ahead[^1].Balance);
    }

    [Fact]
    public void FromObservations_GozlemdenOncekiKartinBugunkuTutariFarkliysa_KatedilenYolGuncelTutarlaCizilirVeAciklanamayanFarkDogrudur()
    {
        // Hazırla — kart 20'sinde plan 12.000; dönem içi harcamayla kart 14.000'e çıktı (I23).
        // 25'inde bakiye gözlemi 36.000:
        // Gerçek: Açılış 30.000 − Kira (5'inde) 15.000 + Gelir (15'inde) 40.000 − Kart (20'sinde) 14.000 − Yaşam 5.000 = 36.000.
        // Açıklanamayan yaşam harcaması 5.000 olmalı; 24 güne günde 208,33 düşer.
        // Kart 20'sinde ödenir, 21'inin noktasında görünür:
        // 20'sinde (kart henüz düşmedi, 19 gün harcama): 30.000 − 15.000 + 40.000 − 5.000 * 19 / 24 = 55.000 − 3.958,33 = 51.041,67.
        // 21'inde (kart 14.000 düştü, 20 gün harcama): 30.000 − 15.000 + 40.000 − 14.000 − 5.000 * 20 / 24 = 41.000 − 4.166,67 = 36.833,33.
        var defter = Defter(Plan, Gozlem(Gun(25), 36_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(25), new Dictionary<Guid, decimal> { [KartId] = 14_000m });

        // Doğrula — katedilen yol kartın güncel tutarını düşer ve yaşam harcamasını şişirmez
        var gecmis = gidisat.Path.Travelled;
        Assert.Equal(51_041.67m, Bakiye(gecmis, Gun(20)));
        Assert.Equal(36_833.33m, Bakiye(gecmis, Gun(21)));
        Assert.Equal(36_000m, Bakiye(gecmis, Gun(25)));
    }

    [Fact]
    public void FromObservations_ErtelenenOdeme_AradaDusulmezGozlemdenSonraDuser()
    {
        // Hazırla — kira ertelendi; 12'sinde 23.000: yalnız 7.000 yaşam harcaması
        var ertele = new PaymentReminderResponse
        {
            DueKey = PaymentReminderPlanner.DueKey(Kira.SourceEntityId, Kira.Name, Kira.PlannedDate),
            Name = Kira.Name,
            DueDate = Kira.PlannedDate,
            Kind = PaymentReminderAnswerKind.Snoozed,
            AnsweredAt = new DateTime(2026, 9, 5, 9, 0, 0, DateTimeKind.Utc)
        };
        var defter = Defter(Plan, [Gozlem(Gun(12), 23_000m)], cevaplar: [ertele]);

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula — 6'sında kira düşmüş görünmez; ödenmemiş kira gözlemden sonraki ilk noktada düşer
        Assert.Equal(26_818.18m, Bakiye(gidisat.Path.Travelled, Gun(6)));
        Assert.Equal(7_315.79m, Bakiye(gidisat.Path.Ahead, Gun(13)));
        Assert.Equal(gidisat.ProjectedEndingBalance, gidisat.Path.Ahead[^1].Balance);
    }

    private static decimal Bakiye(IReadOnlyList<BalancePathPoint> yol, DateOnly gun) =>
        yol.Single(x => x.Date == gun).Balance;

    private static PeriodProgress Hesapla(
        OpenPeriodLedger defter,
        DateOnly bugun,
        IReadOnlyDictionary<Guid, decimal>? kartOdemeleri = null) =>
        PeriodProgressCalculator.Calculate(defter, kartOdemeleri ?? new Dictionary<Guid, decimal>(), KmhOrani, bugun);

    private static OpenPeriodLedger Defter(PeriodPlanSnapshot plan, params PeriodObservation[] gozlemler) =>
        Defter(plan, gozlemler, revizyonlar: null);

    private static OpenPeriodLedger Defter(
        PeriodPlanSnapshot plan,
        IReadOnlyList<PeriodObservation> gozlemler,
        IReadOnlyList<PeriodPlanRevision>? revizyonlar = null,
        IReadOnlyList<PaymentReminderResponse>? cevaplar = null) =>
        new(plan, revizyonlar ?? [], gozlemler, [], cevaplar ?? []);

    private static PeriodPlanSnapshot PlanKur(decimal yasamHavuzu, decimal kmhFaizi, decimal kapanis) => new()
    {
        PeriodStart = DonemBasi,
        PeriodEnd = DonemSonu,
        SettlementAvailableFrom = DonemSonu,
        OpeningBalance = 30_000m,
        PlannedIncome = 40_000m,
        PlannedMandatoryPayments = 27_000m,
        PlannedVariableExpenseAllowance = yasamHavuzu,
        PlannedDeficitInterest = kmhFaizi,
        PlannedEndingBalance = kapanis,
        PaymentLines = [Kira, Kart],
        IncomeLines = [Gelir]
    };

    private static PeriodObservation Gozlem(DateOnly gun, decimal bakiye) => new()
    {
        ObservedOn = gun,
        ObservedBalance = bakiye,
        RecordedAtUtc = new DateTimeOffset(gun.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero)
    };

    private static PeriodPlanPaymentLine Satir(string ad, DateOnly vade, decimal tutar, PlanPaymentSourceType tur, Guid kaynak) => new()
    {
        SourceEntityId = kaynak,
        SourceType = tur,
        Name = ad,
        PlannedDate = vade,
        PlannedAmount = tutar
    };

    private static DateOnly Gun(int gun) => new(2026, 9, gun);
}
