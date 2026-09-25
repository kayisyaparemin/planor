using System.Globalization;

namespace Mizan.Presentation.Models;

/// <summary>
/// Bir nakit akış döneminde devreden finansman açığının (KMH) seyrini ve telafi miktarını özetleyen sunum modeli.
/// </summary>
public sealed record DetailDeficitCallout(
    decimal OpeningDeficit,
    decimal CoveredThisPeriod,
    decimal RemainingDeficit,
    bool IsRecovered)
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>Açılış açığı biçimlendirilmiş metni.</summary>
    public string OpeningDeficitText => $"{OpeningDeficit.ToString("N2", TurkishCulture)} TL";

    /// <summary>Bu dönem kapatılan açık tutarı metni.</summary>
    public string CoveredText => $"Bu dönem karşılanan: {CoveredThisPeriod.ToString("N2", TurkishCulture)} TL";

    /// <summary>Dönem içinde karşılanan açık olup olmadığını belirtir.</summary>
    public bool HasCoveredAmount => CoveredThisPeriod > 0m;

    /// <summary>Açık telafi durumu mesajı.</summary>
    public string Message => IsRecovered
        ? "Bu dönemde açık tamamen kapanıyor."
        : $"Dönem sonuna devreden açık: {RemainingDeficit.ToString("N2", TurkishCulture)} TL";
}
