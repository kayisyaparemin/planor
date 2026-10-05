using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PeriodProgressServiceTests
{
    private static readonly DateOnly DonemBasi = new(2026, 9, 1);
    private static readonly DateOnly DonemSonu = new(2026, 10, 1);

    private readonly InMemoryPeriodHistoryRepository _tarihce = new();
    private readonly InMemoryPeriodObservationRepository _gozlemler = new();
    private readonly InMemoryPaymentReminderRepository _hatirlaticilar = new();
    private readonly InMemoryUserSettingsRepository _ayarlar = new();
    private readonly InMemoryCreditCardRepository _kartlar = new();

    [Fact]
    public async Task GetAsync_AcikDonemYoksa_NullDondurur()
    {
        // Hazırla
        var servis = Servis(DonemBasi);

        // Uygula
        var gidisat = await servis.GetAsync();

        // Doğrula
        Assert.Null(gidisat);
    }

    [Fact]
    public async Task GetAsync_KartOdemesi_VadesiDonemIlkGunuDusenGirerBitisGunuDusenGirmez()
    {
        // Hazırla — kesilmiş ekstrenin vadesi dönemin ilk günü (1 Eylül, 10.000); sonraki ekstrenin
        // vadesi dönemin bitiş günü (1 Ekim, 3.000) yani sonraki dönemin ilk günü (S30)
        var kart = Kart("Axess") with
        {
            StatementClosingDay = 20,
            PaymentDueDay = 1,
            BalanceAsOfDate = new DateOnly(2026, 8, 25),
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 20),
                DueDate = DonemBasi,
                StatementAmount = 10_000m,
                MinimumPaymentAmount = 4_000m
            },
            Charges = [new CardCharge { PostingDate = new DateOnly(2026, 8, 25), Amount = 3_000m }]
        };
        await _kartlar.UpsertCreditCardAsync(kart);
        await AcikDonemKurAsync(new PeriodPlanSnapshot { PaymentLines = [KartSatiri(kart, DonemBasi, 10_000m)] });

        // Uygula
        var gidisat = await Servis(new DateOnly(2026, 9, 12)).GetAsync();

        // Doğrula — eski (başlangıç, bitiş] penceresi burada 3.000 bulurdu
        Assert.NotNull(gidisat);
        Assert.Equal(10_000m, Assert.Single(gidisat.Cards).Current);
    }

    [Fact]
    public async Task GetAsync_PlansizKartHarcamasi_PlaniDegilKartinBugunkuOdemesiniDegistirir()
    {
        // Hazırla — plan kartın 8.000'lik ödemesini dondurdu; dönem içinde 2.095 plansız harcama geldi
        var kart = Kart("Bonus") with
        {
            StatementClosingDay = 5,
            PaymentDueDay = 15,
            BalanceAsOfDate = DonemBasi,
            UnbilledSpending = 8_000m,
            Charges = [new CardCharge { PostingDate = new DateOnly(2026, 9, 3), Amount = 2_095m }]
        };
        await _kartlar.UpsertCreditCardAsync(kart);
        var plan = await AcikDonemKurAsync(new PeriodPlanSnapshot
        {
            OpeningBalance = 20_000m,
            PlannedEndingBalance = 12_000m,
            PaymentLines = [KartSatiri(kart, new DateOnly(2026, 9, 15), 8_000m)]
        });
        await GozlemKaydetAsync(plan, new DateOnly(2026, 9, 4), 20_000m);

        // Uygula
        var gidisat = await Servis(new DateOnly(2026, 9, 4)).GetAsync();

        // Doğrula — plan kilitli (I23); gidişat 20.000 − 10.095
        Assert.NotNull(gidisat);
        var karsilastirma = Assert.Single(gidisat.Cards);
        Assert.Equal(8_000m, karsilastirma.Planned);
        Assert.Equal(10_095m, karsilastirma.Current);
        Assert.Equal(12_000m, gidisat.PlannedEndingBalance);
        Assert.Equal(9_905m, gidisat.ProjectedEndingBalance);
    }

    [Fact]
    public async Task GetAsync_VadesiGelmemisKartaHarcamaGirildiyse_KalanOdemelerKartinGuncelOdemesiniGosterir()
    {
        // Hazırla — plan Bonus'un 15 Eylül ödemesini 8.000 diye dondurdu; 3 Eylül'de 2.095 plansız harcama geldi
        var kart = Kart("Bonus") with
        {
            StatementClosingDay = 5,
            PaymentDueDay = 15,
            BalanceAsOfDate = DonemBasi,
            UnbilledSpending = 8_000m,
            Charges = [new CardCharge { PostingDate = new DateOnly(2026, 9, 3), Amount = 2_095m }]
        };
        await _kartlar.UpsertCreditCardAsync(kart);
        await AcikDonemKurAsync(new PeriodPlanSnapshot
        {
            PaymentLines = [KartSatiri(kart, new DateOnly(2026, 9, 15), 8_000m)]
        });

        // Uygula — vadeden önce, 4 Eylül
        var gidisat = await Servis(new DateOnly(2026, 9, 4)).GetAsync();

        // Doğrula — "Kalan ödemeler" satırı ve notundaki toplam, ana sayfanın "Şu an"ıyla aynı (I23)
        Assert.NotNull(gidisat);
        Assert.Equal(10_095m, Assert.Single(gidisat.RemainingPayments).Amount);
        Assert.Equal(10_095m, gidisat.RemainingTotal);
    }

    [Fact]
    public async Task GetAsync_KalanOdeme_HatirlaticininOdemeAnahtariniTasir()
    {
        // Hazırla — bildirimler bu anahtarla kurulur (PaymentDueCollector); listeden "Ödedim" denen ödemenin
        // bildirimi ancak anahtarlar aynıysa düşer (S88, I22)
        var kira = KiraSatiri();
        await AcikDonemKurAsync(new PeriodPlanSnapshot { PaymentLines = [kira] });

        // Uygula
        var gidisat = await Servis(new DateOnly(2026, 9, 10)).GetAsync();

        // Doğrula
        Assert.NotNull(gidisat);
        Assert.Equal(
            PaymentReminderPlanner.DueKey(kira.SourceEntityId, "Kira", new DateOnly(2026, 9, 20)),
            Assert.Single(gidisat.RemainingPayments).DueKey);
    }

    [Fact]
    public async Task GetAsync_KalanOdemeyeOdedimDenince_ListedenCikarDonemSonuTahminiDegismez()
    {
        // Hazırla — 20 Eylül vadeli 4.000'lik kira, bakiye 4 Eylül'de 20.000. Kullanıcı kirayı 10 Eylül'de erken
        // ödüyor ve listede satıra dokunup "Ödedim" diyor: cevap satırın taşıdığı anahtarla yazılır (S88)
        var plan = await AcikDonemKurAsync(new PeriodPlanSnapshot { OpeningBalance = 20_000m, PaymentLines = [KiraSatiri()] });
        await GozlemKaydetAsync(plan, new DateOnly(2026, 9, 4), 20_000m);
        var bugun = new DateOnly(2026, 9, 10);
        var satir = Assert.Single((await Servis(bugun).GetAsync())!.RemainingPayments);
        await _hatirlaticilar.UpsertResponsesAsync([new PaymentReminderResponse
        {
            DueKey = satir.DueKey,
            Name = satir.Name,
            DueDate = satir.DueDate,
            Amount = satir.Amount,
            Kind = PaymentReminderAnswerKind.Paid,
            AnsweredAt = new DateTime(2026, 9, 10, 10, 0, 0)
        }]);

        // Uygula
        var gidisat = await Servis(bugun).GetAsync();

        // Doğrula — ödeme listeden ve toplamdan çıkar; tahmin "Ödedim"den önceki gibi 20.000 − 4.000: ödeme aynı
        // tutarla kalandan yapılmışa geçti (I166)
        Assert.NotNull(gidisat);
        Assert.Empty(gidisat.RemainingPayments);
        Assert.Equal(0m, gidisat.RemainingTotal);
        Assert.Equal(16_000m, gidisat.ProjectedEndingBalance);
    }

    [Fact]
    public async Task GetAsync_KartinVadesiGeldiyse_DonemSonuTahminiKartinGuncelOdemesiyleKalir()
    {
        // Hazırla — plan Bonus'un 15 Eylül ödemesini 8.000 diye dondurdu, ödeme 10.095'e çıktı; bakiye 4 Eylül'de
        var kart = PlansizHarcamaliBonus();
        await _kartlar.UpsertCreditCardAsync(kart);
        var plan = await AcikDonemKurAsync(new PeriodPlanSnapshot
        {
            OpeningBalance = 20_000m,
            PaymentLines = [KartSatiri(kart, new DateOnly(2026, 9, 15), 8_000m)]
        });
        await GozlemKaydetAsync(plan, new DateOnly(2026, 9, 4), 20_000m);

        // Uygula — vade günü
        var gidisat = await Servis(new DateOnly(2026, 9, 15)).GetAsync();

        // Doğrula — vadeden önceki tahminle aynı (…_PlansizKartHarcamasi_…): ödeme kalandan ödenmişe geçince
        // tutarı değişmez, 20.000 − 10.095 (I23, I165)
        Assert.NotNull(gidisat);
        Assert.Equal(9_905m, gidisat.ProjectedEndingBalance);
    }

    [Fact]
    public async Task GetAsync_KartaOdedimDendiktenSonraBakiyeGirildiyse_YasamGideriKartinFarkiniIcermez()
    {
        // Hazırla — 10.095'lik kart 12 Eylül'de ödendi ve "Ödedim" dendi; kullanıcı 1.000 harcadı,
        // 13 Eylül'de bakiye 20.000 − 10.095 − 1.000 = 8.905
        var kart = PlansizHarcamaliBonus();
        var vade = new DateOnly(2026, 9, 15);
        await _kartlar.UpsertCreditCardAsync(kart);
        var plan = await AcikDonemKurAsync(new PeriodPlanSnapshot
        {
            OpeningBalance = 20_000m,
            PlannedVariableExpenseAllowance = 5_000m,
            PaymentLines = [KartSatiri(kart, vade, 8_000m)]
        });
        await _hatirlaticilar.UpsertResponsesAsync([new PaymentReminderResponse
        {
            DueKey = PaymentReminderPlanner.DueKey(kart.Id, kart.Name, vade),
            Name = kart.Name,
            DueDate = vade,
            Amount = 10_095m,
            Kind = PaymentReminderAnswerKind.Paid,
            AnsweredAt = new DateTime(2026, 9, 12, 10, 0, 0)
        }]);
        await GozlemKaydetAsync(plan, new DateOnly(2026, 9, 13), 8_905m);

        // Uygula
        var gidisat = await Servis(new DateOnly(2026, 9, 13)).GetAsync();

        // Doğrula — kartın planı aşan 2.095'i yaşam giderine yazılmaz; kalan havuz 4.000, dönem sonu 8.905 − 4.000
        Assert.NotNull(gidisat);
        Assert.Equal(1_000m, gidisat.ObservedLivingSpend);
        Assert.Equal(4_905m, gidisat.ProjectedEndingBalance);
    }

    [Fact]
    public async Task GetAsync_KmhOraniAyarlardanBugunSaattenOkunur()
    {
        // Hazırla — havuz 10.000, bakiye 0: dönem −10.000 kapanır; ayardaki oran %10
        await _ayarlar.SaveSettingsAsync(new UserSettings { DeficitFinancingInterestRate = 0.10m });
        var plan = await AcikDonemKurAsync(new PeriodPlanSnapshot { PlannedVariableExpenseAllowance = 10_000m });
        await GozlemKaydetAsync(plan, new DateOnly(2026, 9, 12), 0m);

        // Uygula
        var gidisat = await Servis(new DateOnly(2026, 9, 12)).GetAsync();

        // Doğrula
        Assert.NotNull(gidisat);
        Assert.Equal(new DateOnly(2026, 9, 12), gidisat.Today);
        Assert.Equal(1_000m, gidisat.ProjectedDeficitInterest);
        Assert.Equal(-11_000m, gidisat.ProjectedEndingBalance);
    }

    [Fact]
    public void Yapici_BagimlilikEksikse_ArgumentNullExceptionFirlatir()
    {
        // Hazırla — beş bağımlılığın her biri tek tek eksik bırakılır
        var okuyucu = new OpenPeriodLedgerReader(_tarihce, _gozlemler, _hatirlaticilar);
        var hesaplayici = new CreditCardStatementCalculator();
        var saat = new SabitSaat(DonemBasi);

        // Uygula
        Exception?[] hatalar =
        [
            Record.Exception(() => new PeriodProgressService(null!, _ayarlar, _kartlar, hesaplayici, saat)),
            Record.Exception(() => new PeriodProgressService(okuyucu, null!, _kartlar, hesaplayici, saat)),
            Record.Exception(() => new PeriodProgressService(okuyucu, _ayarlar, null!, hesaplayici, saat)),
            Record.Exception(() => new PeriodProgressService(okuyucu, _ayarlar, _kartlar, null!, saat)),
            Record.Exception(() => new PeriodProgressService(okuyucu, _ayarlar, _kartlar, hesaplayici, null!))
        ];

        // Doğrula
        Assert.All(hatalar, hata => Assert.IsType<ArgumentNullException>(hata));
    }

    private PeriodProgressService Servis(DateOnly bugun) => new(
        new OpenPeriodLedgerReader(_tarihce, _gozlemler, _hatirlaticilar),
        _ayarlar,
        _kartlar,
        new CreditCardStatementCalculator(),
        new SabitSaat(bugun));

    private async Task<PeriodPlanSnapshot> AcikDonemKurAsync(PeriodPlanSnapshot icerik)
    {
        var anlik = new DateTimeOffset(DonemBasi.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var durum = new FinancialSnapshot { Id = Guid.NewGuid(), SnapshotDate = DonemBasi, IsCurrent = true, CreatedAtUtc = anlik };
        var plan = icerik with
        {
            FinancialSnapshotId = durum.Id,
            PeriodStart = DonemBasi,
            PeriodEnd = DonemSonu,
            SettlementAvailableFrom = DonemSonu,
            CreatedAtUtc = anlik
        };
        await _tarihce.SaveCurrentFinancialSnapshotAsync(durum, plan);
        return plan;
    }

    private Task GozlemKaydetAsync(PeriodPlanSnapshot plan, DateOnly gun, decimal bakiye) =>
        _gozlemler.UpsertPeriodObservationAsync(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedOn = gun,
            ObservedBalance = bakiye,
            RecordedAtUtc = new DateTimeOffset(gun.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero)
        });

    private static CreditCard Kart(string ad) => new()
    {
        Name = ad,
        Limit = 50_000m,
        MinimumPaymentRate = 0.40m,
        PaymentStrategy = CreditCardPaymentStrategy.FullStatement
    };

    // Ekstresi 5 Eylül'de kesilen, 15 Eylül'de ödenen kart: dönem başında 8.000 borç, 3 Eylül'de 2.095 plansız harcama.
    private static CreditCard PlansizHarcamaliBonus() => Kart("Bonus") with
    {
        StatementClosingDay = 5,
        PaymentDueDay = 15,
        BalanceAsOfDate = DonemBasi,
        UnbilledSpending = 8_000m,
        Charges = [new CardCharge { PostingDate = new DateOnly(2026, 9, 3), Amount = 2_095m }]
    };

    // Kaynağı bir düzenli ödeme olan, 20 Eylül vadeli 4.000'lik kira satırı.
    private static PeriodPlanPaymentLine KiraSatiri() => new()
    {
        SourceEntityId = Guid.NewGuid(),
        SourceType = PlanPaymentSourceType.OtherScheduledPayment,
        Name = "Kira",
        PlannedDate = new DateOnly(2026, 9, 20),
        PlannedAmount = 4_000m
    };

    private static PeriodPlanPaymentLine KartSatiri(CreditCard kart, DateOnly vade, decimal tutar) => new()
    {
        SourceEntityId = kart.Id,
        SourceType = PlanPaymentSourceType.CreditCard,
        Name = kart.Name,
        PlannedDate = vade,
        PlannedAmount = tutar
    };

    private sealed class SabitSaat(DateOnly bugun) : IClock
    {
        public DateOnly Today { get; } = bugun;

        public DateTimeOffset UtcNow { get; } = new(bugun.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        public DateTimeOffset Now => UtcNow;
    }
}
