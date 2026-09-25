using System.Globalization;

namespace Mizan.Presentation.Models;

/// <summary>
/// Baz durum ile What-If simülasyon senaryosu arasındaki net finansal farkı ve olumsuzluk yönünü temsil eder.
/// </summary>
public sealed record DetailComparisonRow(
    string Label,
    decimal Difference,
    bool IsUnfavorable = false)
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>Kullanıcı arayüzünde gösterilecek biçimlendirilmiş fark metni.</summary>
    public string DifferenceText => Difference switch
    {
        0m => "Değişmiyor",
        > 0m => $"+{Difference.ToString("N2", TurkishCulture)} TL",
        _ => $"{Difference.ToString("N2", TurkishCulture)} TL"
    };
}
