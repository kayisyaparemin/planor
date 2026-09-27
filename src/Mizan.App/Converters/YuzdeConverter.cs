using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu ham oranı (0,0279) ekranda "%2,79" biçimine çevirir; Türkçede yüzde işareti
/// sayının önüne yazılır. ViewModel metin üretmez; biçim ve kültür yalnız burada tanımlıdır (EK-V6c).
/// </summary>
public sealed class YuzdeConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    // Oran çözülmediyse sıfır yerine boşluğu dürüstçe gösterir (ParaConverter ile aynı).
    private const string Bilinmiyor = "—";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal rate ? "%" + (rate * 100m).ToString("N2", Tr) : Bilinmiyor;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
