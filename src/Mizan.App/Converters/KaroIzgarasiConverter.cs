using System.Collections;
using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// Tür seçici karolarını en fazla iki sütunlu ızgaraya yerleştirir (EK-V6f, GS29). Görünüm modeli karoları
/// sırayla verir ve ızgarayı bilmez (12 Dönem'in <see cref="IzgaraKonumuConverter"/> deseni); kaç sütun olduğu
/// sayfanın kararıdır. <c>ConverterParameter</c>: "Satir" karonun satırı, "Satirlar" karo listesinden gereken
/// satır tanımları, "Sutunlar" sütun tanımları (tek karo bölümün genişliğini doldurur), diğer hâlde karonun
/// sütunu. Satırlar sabit yazılsaydı tek satırlık bölümde boş ikinci satırın aralığı kalırdı.
/// </summary>
public sealed class KaroIzgarasiConverter : IValueConverter
{
    private const int SutunSayisi = 2;
    private const string Satir = "Satir";
    private const string Satirlar = "Satirlar";
    private const string Sutunlar = "Sutunlar";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => (value, parameter as string) switch
    {
        (ICollection karolar, Satirlar) => SatirTanimlari(karolar.Count),
        (ICollection karolar, Sutunlar) => SutunTanimlari(karolar.Count),
        (null, Satirlar) => new RowDefinitionCollection(),
        (null, Sutunlar) => new ColumnDefinitionCollection(),
        (int sira, Satir) => sira / SutunSayisi,
        (int sira, _) => sira % SutunSayisi,
        _ => 0
    };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static RowDefinitionCollection SatirTanimlari(int karoSayisi)
    {
        var satirSayisi = (karoSayisi + SutunSayisi - 1) / SutunSayisi;
        return new RowDefinitionCollection(Enumerable.Range(0, satirSayisi).Select(_ => new RowDefinition(GridLength.Auto)).ToArray());
    }

    private static ColumnDefinitionCollection SutunTanimlari(int karoSayisi)
    {
        var sutunSayisi = Math.Clamp(karoSayisi, 1, SutunSayisi);
        return new ColumnDefinitionCollection(Enumerable.Range(0, sutunSayisi).Select(_ => new ColumnDefinition(GridLength.Star)).ToArray());
    }
}
