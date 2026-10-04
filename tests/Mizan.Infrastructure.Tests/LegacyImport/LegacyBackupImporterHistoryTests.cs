namespace Mizan.Infrastructure.Tests.LegacyImport;

/// <summary>
/// Eski dönem tarihçesinin (dondurulmuş planlar, revizyonlar, kapanan dönemler, açık dönem gözlemi)
/// Planör şemasına çevrildiğini doğrulayan testler: S35 adları, S31 gelir satırları ve para korunumu (I28).
/// </summary>
public sealed class LegacyBackupImporterHistoryTests : IDisposable
{
    private const string KapananPlan = "00000000-0000-4000-8000-000000000f01";
    private const string AcikPlan = "00000000-0000-4000-8000-000000000f02";

    private readonly Eski17ImportKurulumu _kurulum = new();

    public void Dispose() => _kurulum.Dispose();

    /// <summary>
    /// Eski <c>OpeningSavings</c>, <c>PlannedLivingBudget</c> ve <c>ReviewAvailableFrom</c> yeni adlarıyla
    /// taşınır (S35); yetim plan (anlık görüntüsü olmayan) elenir.
    /// </summary>
    [Fact]
    public async Task ImportAsync_DondurulmusPlan_YeniAdlarlaTasinirYetimPlanElenir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(3, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM period_plan_snapshots;"));
        Assert.Equal(40000m, Plan<decimal>(profil, KapananPlan, "OpeningBalance"));
        Assert.Equal(12000m, Plan<decimal>(profil, KapananPlan, "PlannedVariableExpenseAllowance"));
        Assert.Equal(57000m, Plan<decimal>(profil, KapananPlan, "PlannedEndingBalance"));
        Assert.Equal("2026-09-15", Plan<string>(profil, KapananPlan, "SettlementAvailableFrom"));
    }

    /// <summary>
    /// Eski plan geliri yalnız toplam olarak saklıyordu (S31). Tek gelir akışından toplamı taşıyan tek satır
    /// türetilir: tarih strateji 0'da dönem başı, 1'de dönem sonu; geliri sıfır olan plana satır eklenmez.
    /// </summary>
    [Fact]
    public async Task ImportAsync_PlanGeliri_ToplamdanTekGelirSatiriTuretilir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(
            ["2026-08-15|40000.0|1", "2026-10-15|50000.0|1"],
            _kurulum.Satirlar<string>(
                profil,
                "SELECT PlannedDate || '|' || CAST(PlannedAmount AS REAL) || '|' || SourceType " +
                "FROM period_plan_income_lines ORDER BY PlannedDate;"));
        Assert.Equal(2, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM period_plan_income_lines WHERE RecurringIncomeId IN (SELECT Id FROM recurring_incomes);"));
    }

    /// <summary>
    /// Para korunumu (I28): her planın gelir satırlarının toplamı planın gelirine kuruşu kuruşuna eşittir.
    /// </summary>
    [Fact]
    public async Task ImportAsync_GelirSatirlari_PlanGeliriyleKurusKurusunaEsittir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        var uyusmayanPlan = _kurulum.Deger<int>(
            profil,
            "SELECT COUNT(*) FROM period_plan_snapshots p WHERE p.PlannedIncome <> " +
            "COALESCE((SELECT SUM(l.PlannedAmount) FROM period_plan_income_lines l WHERE l.PeriodPlanSnapshotId = p.Id), 0);");
        Assert.Equal(0, uyusmayanPlan);
    }

    [Fact]
    public async Task ImportAsync_Revizyon_GelirSatiriniDonemSonuStratejisineGoreAlir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(
            ["2026-10-15|50000.0"],
            _kurulum.Satirlar<string>(
                profil,
                "SELECT PlannedDate || '|' || CAST(PlannedAmount AS REAL) FROM period_plan_revision_income_lines;"));
        Assert.Equal(1, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM period_plan_revision_payment_lines;"));
    }

    [Fact]
    public async Task ImportAsync_KapananDonem_KapanisBakiyesiVeKalemleriyleTasinir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(57500m, _kurulum.Deger<decimal>(profil, "SELECT DerivedEndingBalance FROM period_actuals;"));
        Assert.Equal(57500m, _kurulum.Deger<decimal>(profil, "SELECT ConfirmedEndingBalance FROM period_actuals;"));
        Assert.Equal(1, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM actual_payments;"));
        Assert.Equal(1, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM actual_flows;"));
        Assert.Equal(1, _kurulum.Deger<int>(profil, "SELECT COUNT(*) FROM actual_living_breakdowns;"));
    }

    /// <summary>
    /// Açık dönemin gözlemi güncel şemanın şekline gelir (S68): bakiye ve tek kayıt zamanı gözlemde, ödeme
    /// işareti plana bağlı ayrı tabloda durur.
    /// </summary>
    [Fact]
    public async Task ImportAsync_AcikDonemGozlemi_BakiyeVeOdemeIsaretiGuncelSemadaDurur()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(48000m, _kurulum.Deger<decimal>(profil, "SELECT ObservedBalance FROM period_observations;"));
        Assert.Equal(
            AcikPlan,
            _kurulum.Deger<string>(profil, "SELECT PeriodPlanSnapshotId FROM period_payment_marks;"));
        Assert.Equal(5000m, _kurulum.Deger<decimal>(profil, "SELECT ActualAmount FROM period_payment_marks;"));
    }

    private T Plan<T>(Guid profil, string planId, string kolon) =>
        _kurulum.Deger<T>(profil, $"SELECT {kolon} FROM period_plan_snapshots WHERE Id = '{planId}';");
}
