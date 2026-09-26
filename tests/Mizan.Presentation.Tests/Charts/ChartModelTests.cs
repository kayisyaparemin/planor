using Mizan.Presentation.Charts;
using Xunit;

namespace Mizan.Presentation.Tests.Charts;

/// <summary>
/// Grafik ham veri modellerinin (ChartPoint, ChartSeries, ChartThreshold, ChartCategory)
/// sözleşme ve invaryant doğrulamalarını gerçekleştiren testler.
/// </summary>
public sealed class ChartModelTests
{
    [Fact]
    public void ChartPoint_VeriAlanlarini_DogruTasir()
    {
        var date = new DateOnly(2026, 9, 26);
        var point = new ChartPoint(date, 12500.50m);

        Assert.Equal(date, point.Date);
        Assert.Equal(12500.50m, point.Value);
    }

    [Fact]
    public void ChartPoint_NegatifVeSifirDegerleri_Destekler()
    {
        var date = new DateOnly(2026, 10, 15);
        var zeroPoint = new ChartPoint(date, 0m);
        var negativePoint = new ChartPoint(date, -4500.75m);

        Assert.Equal(0m, zeroPoint.Value);
        Assert.Equal(-4500.75m, negativePoint.Value);
    }

    [Fact]
    public void ChartSeries_RolVeNoktalari_DogruTasir()
    {
        var points = new List<ChartPoint>
        {
            new(new DateOnly(2026, 9, 1), 10000m),
            new(new DateOnly(2026, 10, 1), 8500m),
            new(new DateOnly(2026, 11, 1), -1200m)
        };

        var series = new ChartSeries("actual", points);

        Assert.Equal("actual", series.Key);
        Assert.Equal(3, series.Points.Count);
        Assert.Equal(-1200m, series.Points[2].Value);
    }

    [Fact]
    public void ChartThreshold_EsiDegerini_DogruTasir()
    {
        var zeroThreshold = new ChartThreshold(0m);
        var creditThreshold = new ChartThreshold(-15000m);

        Assert.Equal(0m, zeroThreshold.Value);
        Assert.Equal(-15000m, creditThreshold.Value);
    }

    [Fact]
    public void ChartCategory_RolEtiketVeTutari_DogruTasir()
    {
        var category = new ChartCategory("loan", "Kredi Taksitleri", 24500m);

        Assert.Equal("loan", category.Key);
        Assert.Equal("Kredi Taksitleri", category.Label);
        Assert.Equal(24500m, category.Value);
    }
}
