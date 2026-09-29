using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Senaryo: çapa ayın 1'i. Açılış 30.000; kira 5'inde 15.000; gelir 15'inde 40.000; kart 20'sinde 12.000.
/// Yaşam havuzu 20.000 iken plan 23.000 ile kapanır; havuz 50.000 iken dönem −7.000'e düşer,
/// %5 KMH faizi 350 olur ve plan −7.350 ile kapanır.
/// </summary>
public sealed class PeriodProgressCalculatorTests
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
    // Gidişat plana sadıktır (I31) ve gelir kendi gününde yatar (S31)
    // ---------------------------------------------------------------

    [Fact]
    public void Calculate_GelirYatmadanHavuzIcindeHarcama_DonemSonuPlanlananKapanisaEsittir()
    {
        // Hazırla — 12'sinde 7.000 harcanmış: 30.000 − 15.000 − 7.000 = 8.000; gelir henüz yatmadı
        var defter = Defter(Plan, Gozlem(Gun(12), 8_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula — eski formül burada "47.000 harcadın" ve −4.200 diyordu
        Assert.Equal(7_000m, gidisat.ObservedLivingSpend);
        Assert.Equal(13_000m, gidisat.RemainingVariableExpenseAllowance);
        Assert.Equal(23_000m, gidisat.ProjectedEndingBalance);
        Assert.Equal(0m, gidisat.EndingDeviation);
        Assert.Equal(0m, gidisat.ProjectedDeficitInterest);
        Assert.Null(gidisat.LivingOverspend);
    }

    [Fact]
    public void Calculate_AcikliPlandaHicHarcamaYoksa_GidisatVeKmhFaiziPlaninAynisidir()
    {
        // Hazırla
        var defter = Defter(AcikliPlan, Gozlem(Gun(12), 15_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula
        Assert.Equal(0m, gidisat.ObservedLivingSpend);
        Assert.Equal(50_000m, gidisat.RemainingVariableExpenseAllowance);
        Assert.Equal(350m, gidisat.ProjectedDeficitInterest);
        Assert.Equal(-7_350m, gidisat.ProjectedEndingBalance);
        Assert.Equal(0m, gidisat.EndingDeviation);
        Assert.Equal(0m, gidisat.DeficitInterestDeviation);
    }

    [Fact]
    public void Calculate_GozlemGunuYatanGelir_BakiyeninIcindeSayilir()
    {
        // Hazırla — gelir günü girilen bakiye: 30.000 − 15.000 + 40.000 − 7.000 = 48.000
        var defter = Defter(Plan, Gozlem(Gelir.PlannedDate, 48_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gelir.PlannedDate);

        // Doğrula — gelir iki kez sayılsaydı dönem sonu 56.000 çıkardı
        Assert.Equal(7_000m, gidisat.ObservedLivingSpend);
        Assert.Equal(23_000m, gidisat.ProjectedEndingBalance);
    }

    [Fact]
    public void Calculate_GozlemdenSonraYapilanOdeme_BakiyedenAyricaDusulur()
    {
        // Hazırla — bakiye kira günü girildi; kira o gün bakiyeye henüz yansımamış sayılır
        var defter = Defter(Plan, Gozlem(Kira.PlannedDate, 30_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(7));

        // Doğrula
        Assert.Equal(0m, gidisat.ObservedLivingSpend);
        Assert.Equal(23_000m, gidisat.ProjectedEndingBalance);
    }

    // ---------------------------------------------------------------
    // Havuz ve KMH
    // ---------------------------------------------------------------

    [Fact]
    public void Calculate_HavuzAsilinca_AsimDonemSonunaYansirVeKmhFaiziDogar()
    {
        // Hazırla — gelir yattıktan sonra 50.000 harcanmış: 30.000 − 15.000 + 40.000 − 50.000 = 5.000
        var defter = Defter(Plan, Gozlem(Gun(16), 5_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(16));

        // Doğrula — 5.000 − 12.000 = −7.000; %5 faiz 350
        Assert.Equal(50_000m, gidisat.ObservedLivingSpend);
        Assert.Equal(0m, gidisat.RemainingVariableExpenseAllowance);
        Assert.Equal(30_000m, gidisat.LivingOverspend);
        Assert.Equal(350m, gidisat.ProjectedDeficitInterest);
        Assert.Equal(-7_350m, gidisat.ProjectedEndingBalance);
        Assert.Equal(-30_350m, gidisat.EndingDeviation);
        Assert.Equal(350m, gidisat.DeficitInterestDeviation);
    }

    [Fact]
    public void Calculate_BakiyeBeklenendenYuksekse_HarcamaSifiraKenetlenir()
    {
        // Hazırla — beklenen 15.000, girilen 20.000
        var defter = Defter(Plan, Gozlem(Gun(12), 20_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula
        Assert.Equal(0m, gidisat.ObservedLivingSpend);
        Assert.Equal(20_000m, gidisat.RemainingVariableExpenseAllowance);
        Assert.Equal(28_000m, gidisat.ProjectedEndingBalance);
    }

    [Fact]
    public void Calculate_PozitifeDonenPozisyon_KmhFaiziniSifirlar()
    {
        // Hazırla
        var defter = Defter(AcikliPlan, Gozlem(Gun(12), 60_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula — 60.000 − 12.000 − 50.000 + 40.000 = 38.000
        Assert.Equal(0m, gidisat.ProjectedDeficitInterest);
        Assert.Equal(38_000m, gidisat.ProjectedEndingBalance);
        Assert.Equal(-350m, gidisat.DeficitInterestDeviation);
    }

    // ---------------------------------------------------------------
    // Kart: plan kilitli, gidişat kartın bugünkü hâliyle
    // ---------------------------------------------------------------

    [Fact]
    public void Calculate_KalanKartSatiri_KartinBugunkuOdemesiyleSayilirPlanlananDegismez()
    {
        // Hazırla — dönem içi plansız harcamayla kart ödemesi 14.095'e çıktı
        var defter = Defter(Plan, Gozlem(Gun(12), 8_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12), new Dictionary<Guid, decimal> { [KartId] = 14_095m });

        // Doğrula — 8.000 − 14.095 − 13.000 + 40.000
        var kart = Assert.Single(gidisat.Cards);
        Assert.Equal(new PeriodCardComparison(KartId, "Kart", Kart.PlannedDate, 12_000m, 14_095m), kart);
        Assert.Equal(20_905m, gidisat.ProjectedEndingBalance);
        Assert.Equal(12_000m, gidisat.RemainingPlannedTotal);
        Assert.Equal(23_000m, gidisat.PlannedEndingBalance);
    }

    [Fact]
    public void Calculate_KartinBugunkuOdemesiBilinmiyorsa_PlanlananTutarSayilir()
    {
        // Hazırla
        var defter = Defter(Plan, Gozlem(Gun(12), 8_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula
        Assert.Null(Assert.Single(gidisat.Cards).Current);
        Assert.Equal(23_000m, gidisat.ProjectedEndingBalance);
    }

    // ---------------------------------------------------------------
    // Gözlem yoksa gidişat yok
    // ---------------------------------------------------------------

    [Fact]
    public void Calculate_GozlemYoksa_GidisatRakamlariUretilmezPlanGosterilir()
    {
        // Hazırla
        var defter = Defter(Plan);

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula
        Assert.Null(gidisat.ObservedLivingSpend);
        Assert.Null(gidisat.RemainingVariableExpenseAllowance);
        Assert.Null(gidisat.ProjectedDeficitInterest);
        Assert.Null(gidisat.ProjectedEndingBalance);
        Assert.Null(gidisat.EndingDeviation);
        Assert.Null(gidisat.DeficitInterestDeviation);
        Assert.Null(gidisat.LivingOverspend);
        Assert.Equal(Plan.Id, gidisat.PeriodPlanSnapshotId);
        Assert.Equal((DonemBasi, DonemSonu, Gun(12)), (gidisat.PeriodStart, gidisat.PeriodEnd, gidisat.Today));
        Assert.Equal((40_000m, 27_000m, 20_000m, 0m, 23_000m, 0), (
            gidisat.PlannedIncome,
            gidisat.PlannedMandatoryPayments,
            gidisat.PlannedVariableExpenseAllowance,
            gidisat.PlannedDeficitInterest,
            gidisat.PlannedEndingBalance,
            gidisat.RevisionCount));
    }

    [Fact]
    public void Calculate_GozlemdeBakiyeGirilmemisse_GidisatRakamlariUretilmez()
    {
        // Hazırla — kullanıcı yalnız kirayı erken ödendi işaretledi, bakiye girmedi
        var isaret = new PeriodPaymentMark { PeriodPlanPaymentLineId = Kira.Id, Status = ActualPaymentStatus.Paid, ActualAmount = 15_000m };
        var defter = Defter(Plan, isaretler: [isaret]);

        // Uygula
        var gidisat = Hesapla(defter, Gun(3));

        // Doğrula
        Assert.Null(gidisat.ObservedLivingSpend);
        Assert.Null(gidisat.ProjectedEndingBalance);
        Assert.Null(gidisat.Observation);
        Assert.Equal([Kart.Id], gidisat.RemainingLines.Select(x => x.Id));
    }

    // ---------------------------------------------------------------
    // Revizyon: planım şu an ne (I24)
    // ---------------------------------------------------------------

    [Fact]
    public void Calculate_RevizyonVarsa_PlanlananTutarlarVeSatirlarSonRevizyondanGelir()
    {
        // Hazırla — revizyon kart ödemesini 14.000'e, havuzu 16.000'e çekti: 30 + 40 − 15 − 14 − 16 = 25.000
        var revizeKart = Kart with { Id = Guid.NewGuid(), PlannedAmount = 14_000m };
        var revizyon = new PeriodPlanRevision
        {
            RevisionNumber = 1,
            PlannedIncome = 40_000m,
            PlannedMandatoryPayments = 29_000m,
            PlannedVariableExpenseAllowance = 16_000m,
            PlannedEndingBalance = 25_000m,
            PaymentLines = [Kira with { Id = Guid.NewGuid() }, revizeKart],
            IncomeLines = [Gelir with { Id = Guid.NewGuid() }]
        };
        var defter = Defter(Plan, Gozlem(Gun(12), 8_000m), revizyonlar: [revizyon]);

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula — 8.000 − 14.000 − (16.000 − 7.000) + 40.000 = 25.000
        Assert.Equal(1, gidisat.RevisionCount);
        Assert.Equal(29_000m, gidisat.PlannedMandatoryPayments);
        Assert.Equal(16_000m, gidisat.PlannedVariableExpenseAllowance);
        Assert.Equal(25_000m, gidisat.PlannedEndingBalance);
        Assert.Equal(14_000m, Assert.Single(gidisat.Cards).Planned);
        Assert.Equal(25_000m, gidisat.ProjectedEndingBalance);
        Assert.Equal(0m, gidisat.EndingDeviation);
    }

    [Fact]
    public void Calculate_RevizyonGelirGununuDegistirdiyse_GidisatYeniGunuKullanir()
    {
        // Hazırla — gelir 15'inden 10'una alındı; 12'sinde girilen bakiye geliri içeriyor
        var revizyon = new PeriodPlanRevision
        {
            RevisionNumber = 1,
            PlannedIncome = 40_000m,
            PlannedMandatoryPayments = 27_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedEndingBalance = 23_000m,
            PaymentLines = Plan.PaymentLines,
            IncomeLines = [Gelir with { Id = Guid.NewGuid(), PlannedDate = Gun(10) }]
        };
        var defter = Defter(Plan, Gozlem(Gun(12), 48_000m), revizyonlar: [revizyon]);

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula — dondurulan tarih kullanılsaydı gelir iki kez sayılır, dönem sonu 56.000 çıkardı
        Assert.Equal(7_000m, gidisat.ObservedLivingSpend);
        Assert.Equal(23_000m, gidisat.ProjectedEndingBalance);
    }

    // ---------------------------------------------------------------
    // Kalan satırlar, gün sayacı, kapanış
    // ---------------------------------------------------------------

    [Fact]
    public void Calculate_ErtelenenSatir_KalanlardaVeErtelenmisIsaretlidir()
    {
        // Hazırla
        var ertele = new PaymentReminderResponse
        {
            DueKey = PaymentReminderPlanner.DueKey(Kira.SourceEntityId, Kira.Name, Kira.PlannedDate),
            Name = Kira.Name,
            DueDate = Kira.PlannedDate,
            Kind = PaymentReminderAnswerKind.Snoozed,
            AnsweredAt = new DateTime(2026, 9, 5, 9, 0, 0, DateTimeKind.Utc)
        };
        var defter = Defter(Plan, cevaplar: [ertele]);

        // Uygula
        var gidisat = Hesapla(defter, Gun(12));

        // Doğrula
        Assert.Equal([Kira.Id, Kart.Id], gidisat.RemainingLines.Select(x => x.Id));
        Assert.Equal(27_000m, gidisat.RemainingPlannedTotal);
        Assert.True(gidisat.IsSnoozed(Kira.Id));
        Assert.False(gidisat.IsSnoozed(Kart.Id));
    }

    [Theory]
    [InlineData(-3, 0)]
    [InlineData(11, 11)]
    [InlineData(45, 30)]
    public void Calculate_GecenGun_SifirIleDonemUzunluguArasinaKenetlenir(int donemBasindanGun, int beklenenGun)
    {
        // Hazırla
        var defter = Defter(Plan);

        // Uygula
        var gidisat = Hesapla(defter, DonemBasi.AddDays(donemBasindanGun));

        // Doğrula
        Assert.Equal(beklenenGun, gidisat.ElapsedDays);
        Assert.Equal(30, gidisat.TotalDays);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    public void Calculate_MutabakatGunuGeldiginde_DonemKapatilabilirOlur(int mutabakatGunundenGun, bool beklenen)
    {
        // Hazırla
        var defter = Defter(Plan);

        // Uygula
        var gidisat = Hesapla(defter, Plan.SettlementAvailableFrom.AddDays(mutabakatGunundenGun));

        // Doğrula
        Assert.Equal(beklenen, gidisat.IsClosable);
    }

    private static PeriodProgress Hesapla(
        OpenPeriodLedger defter,
        DateOnly bugun,
        IReadOnlyDictionary<Guid, decimal>? kartOdemeleri = null) =>
        PeriodProgressCalculator.Calculate(defter, kartOdemeleri ?? new Dictionary<Guid, decimal>(), KmhOrani, bugun);

    private static OpenPeriodLedger Defter(
        PeriodPlanSnapshot plan,
        PeriodObservation? gozlem = null,
        IReadOnlyList<PeriodPlanRevision>? revizyonlar = null,
        IReadOnlyList<PeriodPaymentMark>? isaretler = null,
        IReadOnlyList<PaymentReminderResponse>? cevaplar = null) =>
        new(plan, revizyonlar ?? [], gozlem, isaretler ?? [], cevaplar ?? []);

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
