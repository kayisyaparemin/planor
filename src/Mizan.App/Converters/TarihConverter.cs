using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu ham <see cref="DateOnly"/> değerini Türkçe tarih metnine çevirir.
/// Biçim <c>ConverterParameter</c> ile verilir (örn. <c>'d MMMM yyyy'</c>); verilmezse "15 Ekim".
/// </summary>
public sealed class TarihConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private const string VarsayilanBicim = "d MMMM";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateOnly date ? date.ToString(parameter as string ?? VarsayilanBicim, Tr) : string.Empty;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
