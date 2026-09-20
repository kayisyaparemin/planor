using Mizan.Domain.Calculations;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CalendarRulesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(32)]
    [InlineData(100)]
    public void ValidateDay_GecersizGun_HataFirlatir(int gecersizGun)
    {
        // Hazırla & Uygula & Doğrula
        Assert.Throws<ArgumentOutOfRangeException>(() => CalendarRules.ValidateDay(gecersizGun));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(28)]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    public void ValidateDay_GecerliGun_HataFirlatmaz(int gecerliGun)
    {
        // Hazırla & Uygula
        var exception = Record.Exception(() => CalendarRules.ValidateDay(gecerliGun));

        // Doğrula
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(2027, 1, 31, 31)] // Ocak (31 gün)
    [InlineData(2027, 2, 31, 28)] // Şubat normal yıl (28 gün)
    [InlineData(2028, 2, 31, 29)] // Şubat artık yıl (29 gün)
    [InlineData(2027, 4, 31, 30)] // Nisan (30 gün)
    [InlineData(2027, 2, 15, 15)] // Tercih edilen gün ay sınırları içinde
    public void ResolveDay_AySonuKisaAylar_AyinSonGununeKenetlenir(
        int yil,
        int ay,
        int tercihEdilenGun,
        int beklenenGun)
    {
        // Hazırla & Uygula
        var sonuc = CalendarRules.ResolveDay(yil, ay, tercihEdilenGun);

        // Doğrula
        Assert.Equal(new DateOnly(yil, ay, beklenenGun), sonuc);
    }

    [Fact]
    public void AddMonthsKeepingDay_KisaAydanSonraUzunAyaGecildiginde_TercihEdilenGunuGeriKazanir()
    {
        // Hazırla
        var baslangic = new DateOnly(2027, 1, 31);
        const int tercihEdilenGun = 31;

        // Uygula & Doğrula
        // +1 ay -> 28 Şubat 2027 (Kısa ay kelepçesi)
        Assert.Equal(new DateOnly(2027, 2, 28), CalendarRules.AddMonthsKeepingDay(baslangic, 1, tercihEdilenGun));

        // +2 ay -> 31 Mart 2027 (Tercih edilen gün geri kazanıldı)
        Assert.Equal(new DateOnly(2027, 3, 31), CalendarRules.AddMonthsKeepingDay(baslangic, 2, tercihEdilenGun));

        // +3 ay -> 30 Nisan 2027 (30 gün kelepçesi)
        Assert.Equal(new DateOnly(2027, 4, 30), CalendarRules.AddMonthsKeepingDay(baslangic, 3, tercihEdilenGun));

        // +4 ay -> 31 Mayıs 2027 (Tercih edilen gün geri kazanıldı)
        Assert.Equal(new DateOnly(2027, 5, 31), CalendarRules.AddMonthsKeepingDay(baslangic, 4, tercihEdilenGun));
    }
}
