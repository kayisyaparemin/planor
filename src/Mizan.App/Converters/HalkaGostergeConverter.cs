using System.Globalization;
using Mizan.App.Charts;
using Mizan.Presentation.Charts;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu halka verisini (<see cref="ChartGauge"/>: doluluk ve geçen süre) bir <see cref="RingGauge"/>
/// çizimine çevirir ve <c>GraphicsView.Drawable</c>'a bağlanır. Veri değişince yeni çizim atanır, görünüm
/// kendiliğinden yeniden çizilir; böylece sayfanın code-behind'ında yalnız <c>InitializeComponent()</c> kalır.
/// </summary>
public sealed class HalkaGostergeConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ChartGauge gauge
            ? new RingGauge { Ratio = gauge.Ratio, TimeRatio = gauge.TimeRatio }
            : new RingGauge { Ratio = 0m };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
