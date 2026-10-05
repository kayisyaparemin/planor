namespace Mizan.Infrastructure.Tests.LegacyImport;

/// <summary>
/// Eski v17 verisinin Planör şemasına doğru çevrildiğini doğrulayan testler: ayarlar, gelir, kredi, kart ve
/// ödeme tarihçesi (S83, S2, S3, S35). Sonuç içe aktarılan profilin güncel şemadaki ham satırlarından okunur.
/// </summary>
public sealed class LegacyBackupImporterDataTests : IDisposable
{
    private readonly Eski17ImportKurulumu _kurulum = new();

    public void Dispose() => _kurulum.Dispose();

    [Fact]
    public async Task ImportAsync_Ayarlar_DonemCapasinaVeYasamGiderPayinaCevrilir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(15, _kurulum.Deger<int>(profil, "SELECT PeriodAnchorDay FROM settings;"));
        Assert.Equal(12000m, _kurulum.Deger<decimal>(profil, "SELECT PeriodVariableExpenseAllowance FROM settings;"));
        Assert.Equal(50000m, _kurulum.Deger<decimal>(profil, "SELECT ProjectionOpeningBalance FROM settings;"));
        Assert.Equal("2026-09-15", _kurulum.Deger<string>(profil, "SELECT ProjectionAnchorDate FROM settings;"));
        Assert.Equal(4.25m, _kurulum.Deger<decimal>(profil, "SELECT CreditCardCarryInterestRate FROM settings;"));
        Assert.Equal(2, _kurulum.Deger<int>(profil, "SELECT PaymentReminderMode FROM settings;"));
    }

    /// <summary>
    /// Eski <c>salary_schedule</c> tek gelirin etkin tarihli geçmişidir (S2); iki satır iki gelir değil,
    /// tek akışın iki tutarıdır. Ödeme günü eski global gündür (S3).
    /// </summary>
    [Fact]
    public async Task ImportAsync_GelirGecmisi_TekGelirAkisininTutarGecmisineDonusur()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(["Gelir"], _kurulum.Satirlar<string>(profil, "SELECT Name FROM recurring_incomes;"));
        Assert.Equal(15, _kurulum.Deger<int>(profil, "SELECT PaymentDay FROM recurring_incomes;"));
        Assert.Equal(
            ["2026-01-01|40000.0|Başlangıç Tutarı", "2026-07-01|50000.0|Zam"],
            _kurulum.Satirlar<string>(
                profil,
                "SELECT EffectiveDate || '|' || CAST(Amount AS REAL) || '|' || Description " +
                "FROM income_amount_histories ORDER BY EffectiveDate;"));
    }

    [Fact]
    public async Task ImportAsync_GelirGecmisiBossa_GelirAkisiKurulmaz()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin", "DELETE FROM salary_schedule;");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(0, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM recurring_incomes;"));
        Assert.Equal(0, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM period_plan_income_lines;"));
    }

    [Fact]
    public async Task ImportAsync_DigerGelirler_ArizGelirOlarakTasinir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(
            ["2026-10-02|7500.0|Ikramiye"],
            _kurulum.Satirlar<string>(
                profil,
                "SELECT ExactDate || '|' || CAST(Amount AS REAL) || '|' || Description FROM ad_hoc_incomes;"));
    }

    /// <summary>
    /// Eski <c>loans.StartDate</c> aslında sonraki ödeme tarihini taşıyordu; yeni şemada adı doğrudur.
    /// Karşı kredisi olmayan erken ödeme (yetim satır) taşınmaz.
    /// </summary>
    [Fact]
    public async Task ImportAsync_Kredi_EskiBaslangicTarihiniSonrakiOdemeTarihineKoyar()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal("2026-10-20", _kurulum.Deger<string>(profil, "SELECT NextPaymentDate FROM loans;"));
        Assert.Equal(12, _kurulum.Deger<int>(profil, "SELECT RemainingInstallmentCount FROM loans;"));
        Assert.Equal(5000m, _kurulum.Deger<decimal>(profil, "SELECT MonthlyPayment FROM loans;"));
        Assert.Equal(1, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM loan_prepayments;"));
    }

    /// <summary>
    /// Eski <c>card_installments.DueDate</c> aslında işlem (posting) tarihini taşıyordu; eski şemada
    /// kartın etkin/pasif bilgisi yoktu, her kart etkindi.
    /// </summary>
    [Fact]
    public async Task ImportAsync_KartHarcamasi_EskiVadeTarihiniIslemTarihineKoyar()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(["2026-09-10"], _kurulum.Satirlar<string>(profil, "SELECT PostingDate FROM card_installments;"));
        Assert.Equal(1, _kurulum.Deger<int>(profil, "SELECT IsActive FROM credit_cards;"));
    }

    [Fact]
    public async Task ImportAsync_Hatirlaticilar_YanitlariylaTasinir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(
            ["kredi-2026-09-20"],
            _kurulum.Satirlar<string>(profil, "SELECT DueKey FROM payment_reminder_responses;"));
    }

    /// <summary>
    /// Eski modelde dönem (kapanış, sonraki kapanış] idi; kapanış günü ve öncesi kapanmış döneme aitti ve eski
    /// pano o cevapları göstermiyordu. Yeni dönem [başlangıç, bitiş) olduğu için taşınsalar açık dönemin ilk
    /// gününe düşüp bayat bir "ertelendi" kartı olarak geri gelirlerdi (S86).
    /// </summary>
    [Fact]
    public async Task ImportAsync_EskiKapanisGunuVeOncesineVadeliCevaplar_Tasinmaz()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin", """
            INSERT INTO payment_reminder_responses (DueKey, Name, DueDate, Amount, Kind, AnsweredAt, SnoozedUntil) VALUES
                ('kredi-2026-09-15', 'Ihtiyac Kredisi', '2026-09-15', 5000, 1, '2026-09-15T09:00:00.0000000+00:00',
                 '2026-09-15T12:00:00.0000000+00:00'),
                ('kredi-2026-08-20', 'Ihtiyac Kredisi', '2026-08-20', 5000, 1, '2026-08-19T09:00:00.0000000+00:00', NULL);
            """);

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(
            ["kredi-2026-09-20"],
            _kurulum.Satirlar<string>(profil, "SELECT DueKey FROM payment_reminder_responses;"));
    }

    /// <summary>
    /// Simülasyon taslakları varsayımsal denemedir; eski koşul türlerinin bir kısmı yeni uygulamada yok ve
    /// tarihleri geçmiş olur (S76-3, S83-3). Çalışma listesi boş başlar.
    /// </summary>
    [Fact]
    public async Task ImportAsync_SimulasyonTaslaklari_Tasinmaz()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(0, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM simulation_drafts;"));
        Assert.Equal(0, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM simulation_draft_conditions;"));
    }
}
