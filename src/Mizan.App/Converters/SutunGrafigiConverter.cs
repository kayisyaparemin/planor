using System.Globalization;
using Mizan.App.Charts;
using Mizan.App.Resources;
using Mizan.Presentation.Charts;

namespace Mizan.App.Converters;

/// <summary>
/// Bakiye rotasını (<see cref="ChartTrend"/>) bir <see cref="ColumnTrend"/> çizimine çevirir ve
/// <c>GraphicsView.Drawable</c>'a bağlanır: dönem eşit dilimlere bölünür (<see cref="ChartColumns"/>), planın dönem
/// sonu ince çizgi, bugünün sütunu etiketli. Ana sayfa ile "Bakiye gir" önizlemesi aynı çevirmeni kullanır (M8).
/// Veri yoksa çizim de yoktur.
/// </summary>
public sealed class SutunGrafigiConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ChartTrend trend
            ? new ColumnTrend
            {
                Columns = ChartColumns.From(trend),
                PlanLevel = trend.PlanLevel,
                TodayLabel = Strings.Etiket_Bugun
            }
            : null;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
