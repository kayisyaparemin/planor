using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// GS23: <c>AreaTrend</c>'in tarih eksenli rota çizimini (gerçek çizgi, kesikli devam, bugün, plan seviyesi)
/// ve <c>RingGauge</c>'in tempo işaretini koruyan test kalkanı. Çizim MAUI'ye bağlı olduğu için
/// (K7) davranış kaynak metninden denetlenir.
/// </summary>
public sealed class DesignChartRouteTests
{
    private static string Read(string fileName) =>
        File.ReadAllText(Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts", fileName));

    [Fact]
    public void AreaTrend_YeniAlanlariIsteBagliTasir()
    {
        var areaTrend = Read("AreaTrend.cs");

        Assert.Contains("ChartSeries? ProjectionSeries", areaTrend, StringComparison.Ordinal);
        Assert.Contains("DateOnly? Today", areaTrend, StringComparison.Ordinal);
        Assert.Contains("ChartThreshold? PlanLevel", areaTrend, StringComparison.Ordinal);
    }

    [Fact]
    public void AreaTrend_YatayEksenTarihtir_SiraNumarasiDegil()
    {
        var areaTrend = Read("AreaTrend.cs");
        var scale = Read("ChartScale.cs");

        Assert.Contains("ChartScale", areaTrend, StringComparison.Ordinal);
        Assert.Contains("DayNumber", scale, StringComparison.Ordinal);
        Assert.DoesNotContain("stepX", areaTrend, StringComparison.Ordinal);
        Assert.DoesNotContain("stepX", scale, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartScale_InternalYardimcidir_PrimitifDegil()
    {
        var scale = Read("ChartScale.cs");

        Assert.Contains("internal readonly struct ChartScale", scale, StringComparison.Ordinal);
        Assert.DoesNotContain(": IDrawable", scale, StringComparison.Ordinal);
    }

    [Fact]
    public void AreaTrend_TahminDevamiKesikliVeGostergeRengindedir()
    {
        var areaTrend = Read("AreaTrend.cs");
        var projection = areaTrend[areaTrend.IndexOf("private void DrawProjection", StringComparison.Ordinal)..];
        var projectionBody = projection[..projection.IndexOf("private ", 10, StringComparison.Ordinal)];

        Assert.Contains("ProjectionSeries", projectionBody, StringComparison.Ordinal);
        Assert.Contains("ResolveColor(\"Indicator\")", projectionBody, StringComparison.Ordinal);
        Assert.Contains("StrokeDashPattern", projectionBody, StringComparison.Ordinal);
    }

    [Fact]
    public void AreaTrend_BugunVePlanSeviyesiCizilir_IkisiDeTextSecondary()
    {
        var areaTrend = Read("AreaTrend.cs");

        Assert.Contains("DrawTodayLine", areaTrend, StringComparison.Ordinal);
        Assert.Contains("DrawPlanLevel", areaTrend, StringComparison.Ordinal);
        Assert.Contains("scale.Contains(", areaTrend, StringComparison.Ordinal);
    }

    [Fact]
    public void RingGauge_ZamanIsaretiTasir_TextSecondaryIleCizer()
    {
        var ringGauge = Read("RingGauge.cs");

        Assert.Contains("decimal? TimeRatio", ringGauge, StringComparison.Ordinal);
        Assert.Contains("DrawTimeMarker", ringGauge, StringComparison.Ordinal);
        Assert.Contains("ChartColorResolver.ResolveColor(\"TextSecondary\")", ringGauge, StringComparison.Ordinal);
    }

    [Fact]
    public void RingGauge_ZamanIsareti_OrandaNullIseCizilmez()
    {
        var ringGauge = Read("RingGauge.cs");

        Assert.Contains("TimeRatio is not { } ", ringGauge, StringComparison.Ordinal);
    }
}
