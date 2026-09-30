using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu ham oranı (0,0279) ekranda "%2,79" biçimine çevirir; Türkçede yüzde işareti
/// sayının önüne yazılır. ViewModel metin üretmez; biçim ve kültür yalnız burada tanımlıdır (EK-V6c).
/// Parametre sayı biçimidir: faiz iki basamakla ("N2", varsayılan), harcama temposu tam sayıyla ("N0") okunur.
/// </summary>
public sealed class YuzdeConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private const string VarsayilanBicim = "N2";

    // Oran çözülmediyse sıfır yerine boşluğu dürüstçe gösterir (ParaConverter ile aynı).
    private const string Bilinmiyor = "—";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal rate ? "%" + (rate * 100m).ToString(parameter as string ?? VarsayilanBicim, Tr) : Bilinmiyor;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
