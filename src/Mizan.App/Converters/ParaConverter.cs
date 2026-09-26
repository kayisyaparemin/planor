using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu ham tutarı ekranda "41.723 ₺" biçimine çevirir. ViewModel metin üretmez;
/// para biçimi ve kültürü yalnız burada tanımlıdır (rules/03-mvvm.md).
/// </summary>
public sealed class ParaConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    // Tutar henüz bilinmiyorsa (örn. gözlem yok) sıfır yerine boşluğu dürüstçe gösterir.
    private const string Bilinmiyor = "—";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal amount ? amount.ToString("N0", Tr) + " ₺" : Bilinmiyor;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
