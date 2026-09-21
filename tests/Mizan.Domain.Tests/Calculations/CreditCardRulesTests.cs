using Mizan.Domain.Calculations;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CreditCardRulesTests
{
    [Theory]
    [InlineData(0, 0.20)]
    [InlineData(10_000, 0.20)]
    [InlineData(25_000, 0.20)]
    [InlineData(25_000.01, 0.40)]
    [InlineData(50_000, 0.40)]
    [InlineData(200_000, 0.40)]
    public void ResolveMinimumPaymentRate_LimitSinirinaGore_DogruBddkOraniniUretir(decimal limit, decimal expectedRate)
    {
        var rate = CreditCardRules.ResolveMinimumPaymentRate(limit);
        Assert.Equal(expectedRate, rate);
    }

    [Fact]
    public void ResolveMinimumPaymentRate_NegatifLimit_HataFirlatir()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreditCardRules.ResolveMinimumPaymentRate(-1m));
    }

    [Theory]
    [InlineData(1, 11)]
    [InlineData(5, 15)]
    [InlineData(15, 25)]
    [InlineData(20, 30)]
    [InlineData(21, 31)]
    [InlineData(22, 1)]
    [InlineData(25, 4)]
    [InlineData(31, 10)]
    public void ResolveDefaultPaymentDueDay_KesimGunundenOnGunSonrasini_AyIcindeGecerliGuneDonusturur(
        int closingDay,
        int expectedDueDay)
    {
        var dueDay = CreditCardRules.ResolveDefaultPaymentDueDay(closingDay);
        Assert.Equal(expectedDueDay, dueDay);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(-5)]
    public void ResolveDefaultPaymentDueDay_GecersizGun_HataFirlatir(int closingDay)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreditCardRules.ResolveDefaultPaymentDueDay(closingDay));
    }
}
