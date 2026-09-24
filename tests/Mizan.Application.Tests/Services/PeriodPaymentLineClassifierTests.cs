using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PeriodPaymentLineClassifierTests
{
    private static readonly DateOnly DonemBasi = new(2026, 9, 1);
    private static readonly DateOnly DonemSonu = new(2026, 10, 1);

    private static readonly PeriodPlanPaymentLine Kira = Satir("Kira", new DateOnly(2026, 9, 5), 15_000m);
    private static readonly PeriodPlanPaymentLine Kredi = Satir("Kredi", new DateOnly(2026, 9, 10), 5_000m);
    private static readonly PeriodPlanPaymentLine Kart = Satir("Kart", new DateOnly(2026, 9, 20), 12_000m);

    // ---------------------------------------------------------------
    // Vade: kullanıcı hiçbir şey demediyse planın kendi varsayımı
    // ---------------------------------------------------------------

    [Fact]
    public void Classify_VadesiGelmemisSatir_KalanlardaDururVeOdenmisSayilmaz()
    {
        // Hazırla
        var defter = Defter([Kira]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, Kira.PlannedDate.AddDays(-1));

        // Doğrula
        Assert.Equal([Kira.Id], sonuc.RemainingLines.Select(x => x.Id));
        Assert.Equal(0m, sonuc.SettledBeforeObservation);
        Assert.Equal(0m, sonuc.SettledAfterObservation);
    }

    [Fact]
    public void Classify_VadesiBugunGelenSatir_OdenmisSayilir()
    {
        // Hazırla
        var defter = Defter([Kira]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, Kira.PlannedDate);

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Equal(15_000m, sonuc.SettledBeforeObservation);
    }

    [Fact]
    public void Classify_VadesiGozlemGunundenOnceGecenSatir_BakiyeyeYansimisSayilir()
    {
        // Hazırla — kira 5 Eylül, bakiye 6 Eylül'de girildi
        var defter = Defter([Kira], gozlem: Gozlem(new DateOnly(2026, 9, 6)));

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 8));

        // Doğrula
        Assert.Equal(15_000m, sonuc.SettledBeforeObservation);
        Assert.Equal(0m, sonuc.SettledAfterObservation);
    }

    [Fact]
    public void Classify_VadesiGozlemGunuDusenSatir_GozlemdenSonraOdenmisSayilir()
    {
        // Hazırla — kira ve bakiye aynı gün: ödeme bakiyeye henüz yansımamış sayılır
        var defter = Defter([Kira], gozlem: Gozlem(Kira.PlannedDate));

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 6));

        // Doğrula
        Assert.Equal(0m, sonuc.SettledBeforeObservation);
        Assert.Equal(15_000m, sonuc.SettledAfterObservation);
    }

    [Fact]
    public void Classify_GozlemYokken_YapilanOdemelerinTamamiBakiyeyeYansimisSayilir()
    {
        // Hazırla — biri vadesiyle, biri "Ödedim" cevabıyla yapılmış
        var defter = Defter([Kira, Kart], cevaplar: [Cevap(Kart, PaymentReminderAnswerKind.Paid, Utc(9, 9, 9))]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 10));

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Equal(27_000m, sonuc.SettledBeforeObservation);
        Assert.Equal(0m, sonuc.SettledAfterObservation);
    }

    [Fact]
    public void Classify_TutariBelirsizSatir_OdendigindeSifirSayilir()
    {
        // Hazırla
        var aidat = Satir("Aidat", new DateOnly(2026, 9, 6), null);
        var defter = Defter([Kira, aidat]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 10));

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Equal(15_000m, sonuc.SettledBeforeObservation);
    }

    [Fact]
    public void Classify_KalanSatirlar_OnceVadeyeSonraAdaGoreSiralanir()
    {
        // Hazırla
        var su = Satir("Su", new DateOnly(2026, 9, 15), 400m);
        var elektrik = Satir("Elektrik", new DateOnly(2026, 9, 15), 900m);
        var defter = Defter([Kart, su, elektrik]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, DonemBasi);

        // Doğrula
        Assert.Equal([elektrik.Id, su.Id, Kart.Id], sonuc.RemainingLines.Select(x => x.Id));
    }

    // ---------------------------------------------------------------
    // Gözlem defterindeki açık işaret
    // ---------------------------------------------------------------

    [Fact]
    public void Classify_AcikOdendiIsareti_GercekTutarlaBakiyeyeYansimisSayilir()
    {
        // Hazırla — vadesi gelmemiş kart farklı tutarla erken ödendi
        var gozlem = Gozlem(new DateOnly(2026, 9, 10), Isaret(Kart, ActualPaymentStatus.DifferentAmount, 11_500m));
        var defter = Defter([Kart], gozlem: gozlem);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 10));

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Equal(11_500m, sonuc.SettledBeforeObservation);
        Assert.Equal(0m, sonuc.SettledAfterObservation);
    }

    [Fact]
    public void Classify_AcikOdenmediIsareti_VadesiGecseDeKalanlardaDurur()
    {
        // Hazırla
        var gozlem = Gozlem(new DateOnly(2026, 9, 10), Isaret(Kira, ActualPaymentStatus.Unpaid, 0m));
        var defter = Defter([Kira], gozlem: gozlem);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 10));

        // Doğrula
        Assert.Equal([Kira.Id], sonuc.RemainingLines.Select(x => x.Id));
        Assert.Equal(0m, sonuc.SettledBeforeObservation);
        Assert.Empty(sonuc.SnoozedLineIds);
    }

    [Fact]
    public void Classify_AcikIsaret_HatirlaticiCevabinaUstunGelir()
    {
        // Hazırla
        var gozlem = Gozlem(new DateOnly(2026, 9, 10), Isaret(Kira, ActualPaymentStatus.Unpaid, 0m));
        var defter = Defter([Kira], gozlem: gozlem, cevaplar: [Cevap(Kira, PaymentReminderAnswerKind.Paid, Utc(9, 5, 9))]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 10));

        // Doğrula
        Assert.Equal([Kira.Id], sonuc.RemainingLines.Select(x => x.Id));
        Assert.Equal(0m, sonuc.SettledBeforeObservation);
    }

    // ---------------------------------------------------------------
    // Hatırlatıcı cevabı
    // ---------------------------------------------------------------

    [Fact]
    public void Classify_OdedimCevabiGozlemdenOnceVerildiyse_BakiyeyeYansimisSayilir()
    {
        // Hazırla — gözlem 12 Eylül 10:00, cevap bir gün önce
        var defter = Defter(
            [Kart],
            gozlem: Gozlem(new DateOnly(2026, 9, 12)),
            cevaplar: [Cevap(Kart, PaymentReminderAnswerKind.Paid, Utc(9, 11, 9))]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 12));

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Equal(12_000m, sonuc.SettledBeforeObservation);
        Assert.Equal(0m, sonuc.SettledAfterObservation);
    }

    [Fact]
    public void Classify_OdedimCevabiGozlemdenSonraVerildiyse_GozlemdenSonraOdenmisSayilir()
    {
        // Hazırla — gözlem 12 Eylül 10:00, cevap aynı gün 11:00
        var defter = Defter(
            [Kart],
            gozlem: Gozlem(new DateOnly(2026, 9, 12)),
            cevaplar: [Cevap(Kart, PaymentReminderAnswerKind.Paid, Utc(9, 12, 11))]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 12));

        // Doğrula
        Assert.Equal(0m, sonuc.SettledBeforeObservation);
        Assert.Equal(12_000m, sonuc.SettledAfterObservation);
    }

    [Fact]
    public void Classify_ErteleCevabi_VadesiGecseDeKalanlardaVeErtelenmisIsaretlidir()
    {
        // Hazırla
        var defter = Defter([Kira, Kredi], cevaplar: [Cevap(Kira, PaymentReminderAnswerKind.Snoozed, Utc(9, 5, 9))]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 10));

        // Doğrula — kredi vadesiyle ödenmiş sayılır, kira ertelendiği için kalır
        Assert.Equal([Kira.Id], sonuc.RemainingLines.Select(x => x.Id));
        Assert.Equal([Kira.Id], sonuc.SnoozedLineIds);
        Assert.Equal(5_000m, sonuc.SettledBeforeObservation);
    }

    [Fact]
    public void Classify_AyniOdemeyeIkiCevapVarsa_EnSonVerilenGecerlidir()
    {
        // Hazırla — önce ertelendi, ertesi gün "Ödedim" dendi; liste sırası bilerek ters
        var defter = Defter(
            [Kira],
            cevaplar:
            [
                Cevap(Kira, PaymentReminderAnswerKind.Paid, Utc(9, 6, 9)),
                Cevap(Kira, PaymentReminderAnswerKind.Snoozed, Utc(9, 5, 9))
            ]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 10));

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Empty(sonuc.SnoozedLineIds);
        Assert.Equal(15_000m, sonuc.SettledBeforeObservation);
    }

    [Fact]
    public void Classify_CevapSaatiUtcDegilse_YerelSaatOlarakGozlemleKarsilastirilir()
    {
        // Hazırla — cevap saatleri yerel (biri açıkça, biri türü belirsiz); gözlem yarım saat sonra
        var yerelGozlemAni = new DateTimeOffset(new DateTime(2026, 9, 11, 23, 30, 0, DateTimeKind.Local));
        var defter = Defter(
            [Kart, Kredi],
            gozlem: Gozlem(new DateOnly(2026, 9, 11)) with { UpdatedAtUtc = yerelGozlemAni.ToUniversalTime() },
            cevaplar:
            [
                Cevap(Kart, PaymentReminderAnswerKind.Paid, new DateTime(2026, 9, 11, 22, 0, 0, DateTimeKind.Local)),
                Cevap(Kredi, PaymentReminderAnswerKind.Paid, new DateTime(2026, 9, 11, 23, 0, 0, DateTimeKind.Unspecified))
            ]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 12));

        // Doğrula
        Assert.Equal(17_000m, sonuc.SettledBeforeObservation);
        Assert.Equal(0m, sonuc.SettledAfterObservation);
    }

    // ---------------------------------------------------------------
    // Plan revizyonu: satırlar yeni kimlik alır, işaret ve cevap kaybolmaz
    // ---------------------------------------------------------------

    [Fact]
    public void Classify_RevizyondanOnceVerilenOdedimCevabi_YeniKimlikliSatirdaGecerlidir()
    {
        // Hazırla
        var revizeKira = YeniKimlik(Kira);
        var defter = Defter(
            [Kira],
            revizyonlar: [Revizyon(revizeKira)],
            cevaplar: [Cevap(Kira, PaymentReminderAnswerKind.Paid, Utc(9, 2, 9))]);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 3));

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Equal(15_000m, sonuc.SettledBeforeObservation);
    }

    [Fact]
    public void Classify_RevizyondanOnceKonanAcikIsaret_RevizyondanSonraDaGecerlidir()
    {
        // Hazırla — kira erken ödendi işaretlendi, sonra plan revize edildi (S33)
        var revizeKira = YeniKimlik(Kira);
        var defter = Defter(
            [Kira],
            revizyonlar: [Revizyon(revizeKira)],
            gozlem: Gozlem(new DateOnly(2026, 9, 2), Isaret(Kira, ActualPaymentStatus.Paid, 15_000m)));

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 3));

        // Doğrula
        Assert.Empty(sonuc.RemainingLines);
        Assert.Equal(15_000m, sonuc.SettledBeforeObservation);
    }

    [Fact]
    public void Classify_AyniOdemeyeIkiSurumdeIsaretVarsa_EnYeniSurumdekiGecerlidir()
    {
        // Hazırla — dondurulan satır "ödendi", revizyondaki satır "ödenmedi"; liste sırası bilerek ters
        var revizeKira = YeniKimlik(Kira);
        var gozlem = Gozlem(
            new DateOnly(2026, 9, 2),
            Isaret(revizeKira, ActualPaymentStatus.Unpaid, 0m),
            Isaret(Kira, ActualPaymentStatus.Paid, 15_000m));
        var defter = Defter([Kira], revizyonlar: [Revizyon(revizeKira)], gozlem: gozlem);

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 3));

        // Doğrula
        Assert.Equal([revizeKira.Id], sonuc.RemainingLines.Select(x => x.Id));
        Assert.Equal(0m, sonuc.SettledBeforeObservation);
    }

    [Fact]
    public void Classify_RevizyonOdemeninVadesiniDegistirdiyse_EskiIsaretYokSayilir()
    {
        // Hazırla — anahtar kaynak + vade; vade değişince aynı ödeme sayılmaz
        var ertelenmisKira = YeniKimlik(Kira) with { PlannedDate = new DateOnly(2026, 9, 7) };
        var defter = Defter(
            [Kira],
            revizyonlar: [Revizyon(ertelenmisKira)],
            gozlem: Gozlem(new DateOnly(2026, 9, 2), Isaret(Kira, ActualPaymentStatus.Paid, 15_000m)));

        // Uygula
        var sonuc = PeriodPaymentLineClassifier.Classify(defter, new DateOnly(2026, 9, 3));

        // Doğrula
        Assert.Equal([ertelenmisKira.Id], sonuc.RemainingLines.Select(x => x.Id));
        Assert.Equal(0m, sonuc.SettledBeforeObservation);
    }

    private static PeriodPlanPaymentLine Satir(string ad, DateOnly vade, decimal? tutar) => new()
    {
        SourceEntityId = Guid.NewGuid(),
        SourceType = PlanPaymentSourceType.OtherScheduledPayment,
        Name = ad,
        PlannedDate = vade,
        PlannedAmount = tutar
    };

    private static PeriodPlanPaymentLine YeniKimlik(PeriodPlanPaymentLine satir) => satir with { Id = Guid.NewGuid() };

    private static PeriodPlanRevision Revizyon(params PeriodPlanPaymentLine[] satirlar) =>
        new() { RevisionNumber = 1, PaymentLines = satirlar };

    private static OpenPeriodLedger Defter(
        IReadOnlyList<PeriodPlanPaymentLine> planSatirlari,
        IReadOnlyList<PeriodPlanRevision>? revizyonlar = null,
        PeriodObservation? gozlem = null,
        IReadOnlyList<PaymentReminderResponse>? cevaplar = null) =>
        new(
            new PeriodPlanSnapshot { PeriodStart = DonemBasi, PeriodEnd = DonemSonu, PaymentLines = planSatirlari },
            revizyonlar ?? [],
            gozlem,
            cevaplar ?? []);

    /// <summary>Bakiye o gün saat 10:00'da (UTC) girilmiş gözlem.</summary>
    private static PeriodObservation Gozlem(DateOnly gun, params PeriodObservationPayment[] isaretler) => new()
    {
        ObservedOn = gun,
        ObservedBalance = 10_000m,
        UpdatedAtUtc = new DateTimeOffset(gun.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero),
        Payments = isaretler
    };

    private static PeriodObservationPayment Isaret(PeriodPlanPaymentLine satir, ActualPaymentStatus durum, decimal tutar) =>
        new() { PeriodPlanPaymentLineId = satir.Id, Status = durum, ActualAmount = tutar };

    private static PaymentReminderResponse Cevap(PeriodPlanPaymentLine satir, PaymentReminderAnswerKind tur, DateTime verilme) => new()
    {
        DueKey = PaymentReminderPlanner.DueKey(satir.SourceEntityId, satir.Name, satir.PlannedDate),
        Name = satir.Name,
        DueDate = satir.PlannedDate,
        Amount = satir.PlannedAmount,
        Kind = tur,
        AnsweredAt = verilme
    };

    private static DateTime Utc(int ay, int gun, int saat) => new(2026, ay, gun, saat, 0, 0, DateTimeKind.Utc);
}
