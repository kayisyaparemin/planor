using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// Bir bayrağın tersini verir: "bakiye girilmediyse görün" gibi bir öğe, ViewModel'e ikinci bir bayrak
/// eklemeden ve öğe başına beş satırlık tetikleyici yazmadan aynı bayrağa bağlanır.
/// </summary>
public sealed class TersConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;
}
