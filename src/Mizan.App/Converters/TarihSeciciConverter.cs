using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// <see cref="DatePicker"/> yalnız <see cref="DateTime"/> bağlar; ViewModel'ler tarihi
/// <see cref="DateOnly"/> olarak sunar (Kural 05). İki yönde dönüştürür.
/// </summary>
public sealed class TarihSeciciConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateOnly date ? date.ToDateTime(TimeOnly.MinValue) : Binding.DoNothing;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTime dateTime ? DateOnly.FromDateTime(dateTime) : Binding.DoNothing;
}
