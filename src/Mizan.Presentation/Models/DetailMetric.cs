using System.Globalization;

namespace Mizan.Presentation.Models;

/// <summary>
/// Dönem ayrıntısı ekranındaki özet ve akış metriklerini, sayısal tutarını ve sunum formatını temsil eder.
/// </summary>
public sealed record DetailMetric(
    string Label,
    decimal Amount,
    DetailSemanticType Semantic)
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>Pozitif değerlerin başında artı işareti gösterilip gösterilmeyeceği.</summary>
    public bool ShowPositiveSign { get; init; }

    /// <summary>Metriğin bir ara toplam veya genel toplam satırı olup olmadığı.</summary>
    public bool IsTotal { get; init; }

    /// <summary>Kuruş basamak sayısı (varsayılan: 2, tam sayı özetler için: 0).</summary>
    public int DecimalPlaces { get; init; } = 2;

    /// <summary>
    /// Kullanıcı arayüzünde gösterilecek biçimlendirilmiş Türkçe para metni.
    /// </summary>
    public string AmountText
    {
        get
        {
            var formatted = Amount.ToString($"N{DecimalPlaces}", TurkishCulture);
            if (ShowPositiveSign && Amount > 0m)
            {
                return $"+{formatted} TL";
            }

            return $"{formatted} TL";
        }
    }
}
