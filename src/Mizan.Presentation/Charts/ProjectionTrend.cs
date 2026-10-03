using Mizan.Domain.Models;

namespace Mizan.Presentation.Charts;

/// <summary>
/// 12 dönemlik zincirden <see cref="ChartTrend"/> kuran ortak yardımcı: 12 Dönem tek seri çizer, simülatör denemeli
/// seriyi şu anki gidişatın yanına koyar (GS26-3, 4, GS28-2). Aynı kurucuyu paylaşmaları, simülatörde deneme yokken
/// grafiğin 12 Dönem'inkiyle aynı olmasını garanti eder (M8). Seri zincirin başından (açılış) dönem sonlarına gider,
/// sıfır eşiği her zaman ölçeğe girer: sıfıra uzaklık güvenlik payıdır.
/// </summary>
public static class ProjectionTrend
{
    // Rol anahtarları (TASARIM-SISTEMI § Rol → token): ana seri düz çizgi, karşılaştırma kesikli.
    private const string ActualKey = "actual";
    private const string PlannedKey = "planned";

    /// <summary>
    /// Trendi kurar. <paramref name="comparison"/> verilirse ikinci, kesikli ve dolgusuz seri olarak yanına çizilir;
    /// verilmezse tek çizgi vardır.
    /// </summary>
    /// <param name="periods">Ana seri: zincirin 12 dönemi.</param>
    /// <param name="comparison">Karşılaştırma serisi; yoksa <c>null</c>.</param>
    public static ChartTrend Build(
        IReadOnlyList<CashFlowPeriodProjection> periods,
        IReadOnlyList<CashFlowPeriodProjection>? comparison = null) =>
        new(SeriesOf(ActualKey, periods), null, null, null, null)
        {
            Threshold = new ChartThreshold(0m),
            Comparison = comparison is null ? null : SeriesOf(PlannedKey, comparison)
        };

    private static ChartSeries SeriesOf(string key, IReadOnlyList<CashFlowPeriodProjection> periods)
    {
        var points = new List<ChartPoint> { new(periods[0].PeriodStart, periods[0].OpeningBalance) };
        points.AddRange(periods.Select(period => new ChartPoint(period.PeriodEnd, period.EndingBalance)));
        return new ChartSeries(key, points);
    }
}
