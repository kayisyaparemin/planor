using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// <see cref="InstallmentScheduleCalculator"/> sınıfının kuruş korunumlu taksit bölüştürme
/// ve takvim kenetlenmeli vade üretim davranışlarını doğrulayan testler.
/// </summary>
public sealed class InstallmentScheduleCalculatorTests
{
    private readonly InstallmentScheduleCalculator _calculator = new();

    [Fact]
    public void Split_GecerliDegerler_EsitBolumVeKurusKorunumunuSaglar()
    {
        // 100 TL / 3 taksit: 33.33 + 33.33 + 33.34 = 100.00 TL (kuruş artığı son taksite)
        var firstDate = new DateOnly(2026, 10, 15);

        var schedule = _calculator.Split(100m, 3, firstDate);

        Assert.Equal(3, schedule.Count);
        Assert.Equal(new ScheduledAmount(new DateOnly(2026, 10, 15), 33.33m), schedule[0]);
        Assert.Equal(new ScheduledAmount(new DateOnly(2026, 11, 15), 33.33m), schedule[1]);
        Assert.Equal(new ScheduledAmount(new DateOnly(2026, 12, 15), 33.34m), schedule[2]);
        Assert.Equal(100m, schedule.Sum(x => x.Amount));
    }

    [Fact]
    public void Split_TamBolunenTutar_TumTaksitlerEsitOlarakOlusturulur()
    {
        var firstDate = new DateOnly(2026, 5, 10);

        var schedule = _calculator.Split(120m, 4, firstDate);

        Assert.Equal(4, schedule.Count);
        Assert.All(schedule, item => Assert.Equal(30.00m, item.Amount));
        Assert.Equal(120m, schedule.Sum(x => x.Amount));
        Assert.Equal(new DateOnly(2026, 5, 10), schedule[0].Date);
        Assert.Equal(new DateOnly(2026, 8, 10), schedule[^1].Date);
    }

    [Fact]
    public void Split_TekTaksit_TekTutarVeAyniTarihleOlusturulur()
    {
        var firstDate = new DateOnly(2026, 6, 1);

        var schedule = _calculator.Split(500m, 1, firstDate);

        var single = Assert.Single(schedule);
        Assert.Equal(firstDate, single.Date);
        Assert.Equal(500m, single.Amount);
    }

    [Fact]
    public void Split_MaksimumTaksit120_SorunsuzHesaplanirVeToplamKorunur()
    {
        var firstDate = new DateOnly(2026, 1, 1);

        var schedule = _calculator.Split(120_000m, 120, firstDate);

        Assert.Equal(120, schedule.Count);
        Assert.Equal(120_000m, schedule.Sum(x => x.Amount));
        Assert.Equal(firstDate, schedule[0].Date);
    }

    [Fact]
    public void Split_AySonuTarihKenetlenmesi_SubatKisaAyVeGeriKazanimiKorur()
    {
        // 31 Ocak 2027 başlangıç (artık olmayan yıl):
        // 1. taksit: 31 Ocak 2027
        // 2. taksit: 28 Şubat 2027 (Şubat sonuna kırpılır)
        // 3. taksit: 31 Mart 2027 (tercih edilen 31 günü geri kazanılır)
        var firstDate = new DateOnly(2027, 1, 31);

        var schedule = _calculator.Split(300m, 3, firstDate);

        Assert.Equal(3, schedule.Count);
        Assert.Equal(new DateOnly(2027, 1, 31), schedule[0].Date);
        Assert.Equal(new DateOnly(2027, 2, 28), schedule[1].Date);
        Assert.Equal(new DateOnly(2027, 3, 31), schedule[2].Date);
    }

    [Fact]
    public void Split_ArtikYilSubat_29SubataKenetlenir()
    {
        // 31 Ocak 2028 başlangıç (2028 artık yıldır)
        var firstDate = new DateOnly(2028, 1, 31);

        var schedule = _calculator.Split(200m, 2, firstDate);

        Assert.Equal(2, schedule.Count);
        Assert.Equal(new DateOnly(2028, 1, 31), schedule[0].Date);
        Assert.Equal(new DateOnly(2028, 2, 29), schedule[1].Date);
    }

    [Fact]
    public void Split_OtuzCekenAyGecisi_30GuneKenetlenirVe31GeriKazanilir()
    {
        // 31 Mart 2026 başlangıç: Mart (31) -> Nisan (30) -> Mayıs (31)
        var firstDate = new DateOnly(2026, 3, 31);

        var schedule = _calculator.Split(300m, 3, firstDate);

        Assert.Equal(3, schedule.Count);
        Assert.Equal(new DateOnly(2026, 3, 31), schedule[0].Date);
        Assert.Equal(new DateOnly(2026, 4, 30), schedule[1].Date);
        Assert.Equal(new DateOnly(2026, 5, 31), schedule[2].Date);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Split_GecersizTutar_SifirVeyaNegatif_ArgumentOutOfRangeExceptionFirlatir(decimal invalidTotal)
    {
        var action = () => _calculator.Split(invalidTotal, 3, new DateOnly(2026, 1, 1));

        var ex = Assert.Throws<ArgumentOutOfRangeException>(action);
        Assert.Equal("total", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(121)]
    public void Split_GecersizTaksitSayisi_AralikDisinda_ArgumentOutOfRangeExceptionFirlatir(int invalidCount)
    {
        var action = () => _calculator.Split(100m, invalidCount, new DateOnly(2026, 1, 1));

        var ex = Assert.Throws<ArgumentOutOfRangeException>(action);
        Assert.Equal("count", ex.ParamName);
    }

    [Fact]
    public void Split_VarsayilanTarih_DefaultDateOnly_ArgumentExceptionFirlatir()
    {
        var action = () => _calculator.Split(100m, 3, default);

        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal("firstDate", ex.ParamName);
    }
}
