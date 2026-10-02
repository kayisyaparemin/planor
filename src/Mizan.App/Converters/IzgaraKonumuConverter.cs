using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// Sıra numarasını üç sütunlu ızgaradaki satıra ya da sütuna çevirir (12 dönem ızgarası, GS26-1). Görünüm modeli
/// karoları sırayla verir ve ızgarayı bilmez; kaç sütun olduğu sayfanın yerleşim kararıdır.
/// <c>ConverterParameter</c> "Satir" ise satır, değilse sütun döner.
/// </summary>
public sealed class IzgaraKonumuConverter : IValueConverter
{
    private const int SutunSayisi = 3;
    private const string Satir = "Satir";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int sira)
        {
            return 0;
        }

        return parameter as string == Satir ? sira / SutunSayisi : sira % SutunSayisi;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
