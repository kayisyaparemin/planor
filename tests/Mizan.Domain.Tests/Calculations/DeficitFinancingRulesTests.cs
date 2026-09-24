using Mizan.Domain.Calculations;

namespace Mizan.Domain.Tests.Calculations;

public sealed class DeficitFinancingRulesTests
{
    private const decimal DonemlikOran = 0.05m;

    [Fact]
    public void CalculateInterest_BakiyeEksiyse_AcigaDonemlikOranUygulanir()
    {
        // Hazırla
        const decimal faizOncesiDonemSonu = -10_000m;

        // Uygula
        var faiz = DeficitFinancingRules.CalculateInterest(faizOncesiDonemSonu, DonemlikOran);

        // Doğrula
        Assert.Equal(500m, faiz);
    }

    [Theory]
    [InlineData(1_000)]
    [InlineData(0.01)]
    [InlineData(0)]
    public void CalculateInterest_BakiyeSifirVeyaArtidaysa_AcikYoktur(double faizOncesiDonemSonu)
    {
        // Hazırla
        var bakiye = (decimal)faizOncesiDonemSonu;

        // Uygula
        var faiz = DeficitFinancingRules.CalculateInterest(bakiye, DonemlikOran);

        // Doğrula
        Assert.Equal(0m, faiz);
    }

    [Fact]
    public void CalculateInterest_TekKurusAcikVarsa_FaizKurusaYuvarlanir()
    {
        // Hazırla — 0,01 × 0,05 = 0,0005 → kuruşun altında kalır
        const decimal faizOncesiDonemSonu = -0.01m;

        // Uygula
        var faiz = DeficitFinancingRules.CalculateInterest(faizOncesiDonemSonu, DonemlikOran);

        // Doğrula
        Assert.Equal(0m, faiz);
    }

    [Fact]
    public void CalculateInterest_FaizKurusunTamOrtasindaysa_SifirdanUzagaYuvarlanir()
    {
        // Hazırla — 12,50 × 0,05 = 0,625; ToEven olsaydı 0,62 çıkardı
        const decimal faizOncesiDonemSonu = -12.50m;

        // Uygula
        var faiz = DeficitFinancingRules.CalculateInterest(faizOncesiDonemSonu, DonemlikOran);

        // Doğrula
        Assert.Equal(0.63m, faiz);
    }

    [Fact]
    public void CalculateInterest_OranSifirsa_DerinAciktaBileFaizDogmaz()
    {
        // Hazırla
        const decimal faizOncesiDonemSonu = -250_000m;

        // Uygula
        var faiz = DeficitFinancingRules.CalculateInterest(faizOncesiDonemSonu, 0m);

        // Doğrula
        Assert.Equal(0m, faiz);
    }
}
