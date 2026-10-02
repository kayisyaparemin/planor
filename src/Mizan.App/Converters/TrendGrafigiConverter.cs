using System.Globalization;
using Mizan.App.Charts;
using Mizan.Presentation.Charts;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu trend verisini (<see cref="ChartTrend"/>) bir <see cref="AreaTrend"/> çizimine çevirir ve
/// <c>GraphicsView.Drawable</c>'a bağlanır: katedilen yol düz, önümüzdeki yol kesikli, bakiye günleri nokta,
/// bugün ve plan seviyesi ince çizgi (GS23, GS24). Veri yoksa çizim de yoktur.
/// </summary>
public sealed class TrendGrafigiConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ChartTrend trend
            ? new AreaTrend
            {
                Series = trend.Series,
                ProjectionSeries = trend.Projection,
                Today = trend.Today,
                PlanLevel = trend.PlanLevel,
                MarkerSeries = trend.Markers,
                Threshold = trend.Threshold
            }
            : null;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
