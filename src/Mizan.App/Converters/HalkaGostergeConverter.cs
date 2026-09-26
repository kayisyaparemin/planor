using System.Globalization;
using Mizan.App.Charts;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu ham oranı (0–1) bir <see cref="RingGauge"/> çizimine çevirir ve
/// <c>GraphicsView.Drawable</c>'a bağlanır. Oran değişince yeni çizim atanır, görünüm kendiliğinden
/// yeniden çizilir; böylece sayfanın code-behind'ında yalnız <c>InitializeComponent()</c> kalır.
/// </summary>
public sealed class HalkaGostergeConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        new RingGauge { Ratio = value is double ratio && double.IsFinite(ratio) ? (decimal)ratio : 0m };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
