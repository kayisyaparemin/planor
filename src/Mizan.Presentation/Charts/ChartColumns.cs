namespace Mizan.Presentation.Charts;

/// <summary>
/// Bakiye rotasını (<see cref="ChartTrend"/>) sütunlu grafiğin ham verisine çevirir. Dönem eşit dilimlere bölünür
/// ve her dilim tek sütun olur. Günlük rotanın 30 ince sütunu tek bakışta okunmuyordu, konsept de 10 sütun
/// gösteriyor. Ana sayfa ile "Bakiye gir" önizlemesi aynı dilimleri görsün diye hesap tek yerde durur (M8).
/// </summary>
public static class ChartColumns
{
    /// <summary>Bir dönemin en fazla kaç sütuna bölüneceği.</summary>
    public const int MaxColumns = 10;

    /// <summary>
    /// Rotayı dilimlere böler. Sütunun tutarı dilimin son günündeki bakiyedir; bugünün diliminde bugünkü bakiye.
    /// Bugünden sonraki sütunlar tahmindir. Son sütun rotanın son noktasıdır (dönem sonu tahmini).
    /// </summary>
    /// <param name="trend">Bakiye rotası: katedilen yol ve kesikli devam.</param>
    /// <param name="maxColumns">En fazla sütun sayısı; dönemin gün sayısı daha azsa gün başına bir sütun.</param>
    /// <returns>Soldan sağa sütunlar; rota boşsa boş liste.</returns>
    public static IReadOnlyList<ChartColumn> From(ChartTrend trend, int maxColumns = MaxColumns)
    {
        ArgumentNullException.ThrowIfNull(trend);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxColumns, 1);

        var points = Route(trend);
        if (points.Count == 0) { return []; }

        var start = points[0].Date;
        var end = points[^1].Date;
        var span = end.DayNumber - start.DayNumber;
        var today = trend.Today ?? (trend.Series.Points.Count > 0 ? trend.Series.Points[^1].Date : end);
        var count = Math.Max(1, Math.Min(maxColumns, span));

        var columns = new List<ChartColumn>(count);
        var todayPlaced = false;
        for (var i = 0; i < count; i++)
        {
            var sliceEnd = start.AddDays(CeilDiv(span * (i + 1), count));
            var isToday = !todayPlaced && today >= start && today <= sliceEnd;
            todayPlaced |= isToday;

            var date = isToday ? today : sliceEnd;
            columns.Add(new ChartColumn(date, ValueAt(points, date), date > today, isToday));
        }

        return columns;
    }

    // Katedilen yol ve kesikli devam tek sırada; devamın ilk noktası yolun son noktasıdır, aynı gün bir kez kalır.
    private static List<ChartPoint> Route(ChartTrend trend) =>
        trend.Series.Points
            .Concat(trend.Projection?.Points ?? [])
            .GroupBy(point => point.Date)
            .Select(day => day.Last())
            .OrderBy(point => point.Date)
            .ToList();

    // Rota günlüktür; arada boş gün kalırsa o güne kadarki son bilinen bakiye geçerlidir.
    private static decimal ValueAt(List<ChartPoint> points, DateOnly date) =>
        points.LastOrDefault(point => point.Date <= date)?.Value ?? points[0].Value;

    private static int CeilDiv(int value, int divisor) => (value + divisor - 1) / divisor;
}
