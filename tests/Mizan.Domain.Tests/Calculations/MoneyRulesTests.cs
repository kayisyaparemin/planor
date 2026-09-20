using Mizan.Domain.Calculations;

namespace Mizan.Domain.Tests.Calculations;

public sealed class MoneyRulesTests
{
    [Theory]
    [InlineData(10.123, 10.12)]
    [InlineData(10.126, 10.13)]
    [InlineData(2.125, 2.13)] // AwayFromZero testi: ToEven olsaydı 2.12 olurdu
    [InlineData(2.135, 2.14)]
    [InlineData(-2.125, -2.13)] // Negatif sayılarda da sıfırdan uzağa yuvarlama
    [InlineData(0, 0)]
    public void Round_OndalikliTutar_AwayFromZeroVeIkiHaneyeYuvarlar(double giris, double beklenen)
    {
        // Hazırla
        var hamTutar = (decimal)giris;
        var beklenenTutar = (decimal)beklenen;

        // Uygula
        var sonuc = MoneyRules.Round(hamTutar);

        // Doğrula
        Assert.Equal(beklenenTutar, sonuc);
    }

    [Fact]
    public void Distribute_TamBolunmeyenTutar_KurusArtiginiSonTaksiteEkler()
    {
        // Hazırla
        const decimal toplam = 100m;
        const int taksitSayisi = 3;

        // Uygula
        var taksitler = MoneyRules.Distribute(toplam, taksitSayisi);

        // Doğrula
        Assert.Equal(3, taksitler.Count);
        Assert.Equal(33.33m, taksitler[0]);
        Assert.Equal(33.33m, taksitler[1]);
        Assert.Equal(33.34m, taksitler[2]); // 1 kuruşluk artık son taksite eklenmeli
        Assert.Equal(toplam, taksitler.Sum());
    }

    [Fact]
    public void Distribute_TamBolunenTutar_TumTaksitlerEsittir()
    {
        // Hazırla
        const decimal toplam = 100m;
        const int taksitSayisi = 4;

        // Uygula
        var taksitler = MoneyRules.Distribute(toplam, taksitSayisi);

        // Doğrula
        Assert.Equal(4, taksitler.Count);
        Assert.All(taksitler, t => Assert.Equal(25.00m, t));
        Assert.Equal(toplam, taksitler.Sum());
    }

    [Fact]
    public void Distribute_TekTaksit_ToplamTutarAynenDoner()
    {
        // Hazırla
        const decimal toplam = 1500.75m;
        const int taksitSayisi = 1;

        // Uygula
        var taksitler = MoneyRules.Distribute(toplam, taksitSayisi);

        // Doğrula
        var tekTaksit = Assert.Single(taksitler);
        Assert.Equal(toplam, tekTaksit);
    }

    [Theory]
    [InlineData(1234.56, 12)]
    [InlineData(50.05, 3)]
    [InlineData(0.05, 2)]
    [InlineData(999.99, 9)]
    [InlineData(1000, 7)]
    public void Distribute_FarkliTutarVeTaksitler_ParaKorunumuKurusKurusunaSaglanir(double toplamDouble, int taksitSayisi)
    {
        // Hazırla
        var toplam = (decimal)toplamDouble;

        // Uygula
        var taksitler = MoneyRules.Distribute(toplam, taksitSayisi);

        // Doğrula
        Assert.Equal(taksitSayisi, taksitler.Count);
        Assert.Equal(toplam, taksitler.Sum());
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(-100, 3)]
    [InlineData(100, 0)]
    [InlineData(100, -1)]
    [InlineData(100, 121)]
    public void Distribute_GecersizParametreler_ArgumentOutOfRangeExceptionFirlatir(double toplamDouble, int taksitSayisi)
    {
        // Hazırla
        var toplam = (decimal)toplamDouble;

        // Uygula & Doğrula
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyRules.Distribute(toplam, taksitSayisi));
    }
}
