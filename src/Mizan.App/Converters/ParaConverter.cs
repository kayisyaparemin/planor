using System.Globalization;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu ham tutarı ekranda "41.723 ₺" biçimine çevirir. ViewModel metin üretmez;
/// para biçimi ve kültürü yalnız burada tanımlıdır (.claude/rules/03-mvvm.md). Parametre
/// <c>Isaretli</c> ise artı tutar "+480 ₺" yazılır: plana göre fark gibi yönü olan tutarlarda işaret,
/// renge bakmadan okunabilsin diye. Parametre <c>Eksi</c> ise tutar çıkış olarak yazılır ("−480 ₺"): dönem
/// ayrıntısının akışında ödemeler ve yaşam gideri ViewModel'de artı tutardır, eksiyi akışın yönü verir (EK-V9).
/// </summary>
public sealed class ParaConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private const string Isaretli = "Isaretli";
    private const string Eksi = "Eksi";

    // Tutar henüz bilinmiyorsa (örn. gözlem yok) sıfır yerine boşluğu dürüstçe gösterir.
    private const string Bilinmiyor = "—";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not decimal amount) { return Bilinmiyor; }
        if (Eksi.Equals(parameter as string, StringComparison.Ordinal)) { return Bicimle(-amount); }
        var sign = amount > 0m && Isaretli.Equals(parameter as string, StringComparison.Ordinal) ? "+" : string.Empty;
        return sign + Bicimle(amount);
    }

    /// <summary>Tutarı aynı biçimle yazar; başka bir çeviricinin cümlesine giren tutar ekranın geri kalanından ayrışmasın.</summary>
    internal static string Bicimle(decimal? amount) => amount is { } value ? value.ToString("N0", Tr) + "\u00A0₺" : Bilinmiyor;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
