using System.Globalization;

namespace Mizan.Architecture.Tests.Design.Support;

/// <summary>
/// WCAG 2.1 bağıl parlaklık ve kontrast oranı hesabı yapan yardımcı sınıf.
/// </summary>
internal static class ContrastRatio
{
    /// <summary>
    /// Verilen iki hex renk arasındaki kontrast oranını hesaplar.
    /// </summary>
    /// <param name="hex1">İlk hex renk (örn. #1E232A).</param>
    /// <param name="hex2">İkinci hex renk (örn. #F3F4F6).</param>
    /// <returns>1.0 ile 21.0 arasında kontrast oranı.</returns>
    public static double Calculate(string hex1, string hex2)
    {
        var l1 = CalculateLuminance(hex1);
        var l2 = CalculateLuminance(hex2);

        var lighter = Math.Max(l1, l2);
        var darker = Math.Min(l1, l2);

        return Math.Round((lighter + 0.05) / (darker + 0.05), 2);
    }

    /// <summary>
    /// Verilen hex rengin WCAG 2.1 bağıl parlaklık (luminance) değerini hesaplar.
    /// </summary>
    /// <param name="hex">Hex renk kodu.</param>
    /// <returns>0.0 ile 1.0 arasında parlaklık.</returns>
    public static double CalculateLuminance(string hex)
    {
        var cleanHex = hex.Trim().TrimStart('#');
        if (cleanHex.Length == 8)
        {
            cleanHex = cleanHex[2..];
        }

        var r = int.Parse(cleanHex.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
        var g = int.Parse(cleanHex.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
        var b = int.Parse(cleanHex.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;

        var rLinear = r <= 0.04045 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        var gLinear = g <= 0.04045 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
        var bLinear = b <= 0.04045 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

        return (0.2126 * rLinear) + (0.7152 * gLinear) + (0.0722 * bLinear);
    }
}
