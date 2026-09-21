using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodAnchorTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(31)]
    public void Constructor_GecerliGunlerIle_BasariylaOlusturur(int dayOfMonth)
    {
        var anchor = new PeriodAnchor(dayOfMonth);

        Assert.Equal(dayOfMonth, anchor.DayOfMonth);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(32)]
    public void Constructor_GecersizGunVerildiginde_HataFirlatir(int invalidDay)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PeriodAnchor(invalidDay));
        Assert.Equal("day", ex.ParamName);
    }

    [Fact]
    public void Equals_AyniGunDegerineSahipIkiCapa_EsitKabulEdilir()
    {
        var anchor1 = new PeriodAnchor(15);
        var anchor2 = new PeriodAnchor(15);

        Assert.Equal(anchor1, anchor2);
        Assert.True(anchor1 == anchor2);
    }
}
