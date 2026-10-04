using Mizan.Presentation.Charts;
using Xunit;

namespace Mizan.Presentation.Tests.Charts;

/// <summary>
/// <see cref="ChartColumns"/> rotayı sütun dilimlerine bölerken kuralları korur:
/// en fazla 10 sütun, bugünün dilimi işaretli, gelecek sütunlar tahmin (IsAhead).
/// </summary>
public sealed class ChartColumnsTests
{
    [Fact]
    public void From_BosRotada_BosDöner()
    {
        var trend = new ChartTrend(
            new ChartSeries("actual", []),
            null,
            null,
            null,
            null);

        var columns = ChartColumns.From(trend);

        Assert.Empty(columns);
    }

    [Fact]
    public void From_OtuzGunlukDonemi_EnFazlaOnSutunaBoler()
    {
        var start = new DateOnly(2026, 9, 10);
        var end = new DateOnly(2026, 10, 10);
        var today = new DateOnly(2026, 9, 30);

        var points = new List<ChartPoint>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            points.Add(new ChartPoint(d, 40000m + (d.DayNumber - start.DayNumber) * 100m));
        }

        var trend = new ChartTrend(
            new ChartSeries("actual", points.Where(p => p.Date <= today).ToList()),
            new ChartSeries("projection", points.Where(p => p.Date >= today).ToList()),
            today,
            new ChartThreshold(42000m),
            null);

        var columns = ChartColumns.From(trend, maxColumns: 10);

        Assert.Equal(10, columns.Count);
        Assert.Contains(columns, c => c.IsToday);
        Assert.True(columns.Count(c => c.IsToday) == 1);

        // Bugün öncesi tahmin değildir
        var todayIndex = -1;
        for (var i = 0; i < columns.Count; i++)
        {
            if (columns[i].IsToday) { todayIndex = i; break; }
        }

        for (var i = 0; i <= todayIndex; i++)
        {
            Assert.False(columns[i].IsAhead);
        }

        // Bugünden sonraki sütunlar tahmindir
        for (var i = todayIndex + 1; i < columns.Count; i++)
        {
            Assert.True(columns[i].IsAhead);
        }

        // Son sütun dönem sonu tutarını taşır
        Assert.Equal(end, columns[^1].Date);
        Assert.Equal(points[^1].Value, columns[^1].Value);
    }

    [Fact]
    public void From_GecersizParametrelerde_Firlatir()
    {
        Assert.Throws<ArgumentNullException>(() => ChartColumns.From(null!));
        var validTrend = new ChartTrend(new ChartSeries("actual", []), null, null, null, null);
        Assert.Throws<ArgumentOutOfRangeException>(() => ChartColumns.From(validTrend, 0));
    }
}
