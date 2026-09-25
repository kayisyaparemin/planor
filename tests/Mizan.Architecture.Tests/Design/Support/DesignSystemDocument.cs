using System.Globalization;
using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests.Design.Support;

/// <summary>
/// docs/TASARIM-SISTEMI.md belgesini makine olarak ayrıştıran test yardımcısı.
/// </summary>
internal static class DesignSystemDocument
{
    private static readonly Lazy<string> ContentLazy = new(() =>
        File.ReadAllText(Path.Combine(SolutionPaths.Root, "docs", "TASARIM-SISTEMI.md")));

    public static string Content => ContentLazy.Value;

    /// <summary>
    /// § Renk token'ları tablolarındaki 23 rengi döndürür.
    /// </summary>
    public static IReadOnlyList<ColorTokenRow> GetColorTokens()
    {
        var list = new List<ColorTokenRow>();
        var pattern = new Regex(@"^\|\s*`(?<token>[A-Za-z0-9_]+)`\s*\|\s*`(?<dark>#[A-Fa-f0-9]{6})`\s*\|\s*`(?<light>#[A-Fa-f0-9]{6})`", RegexOptions.Multiline);
        foreach (Match match in pattern.Matches(Content))
        {
            list.Add(new ColorTokenRow(
                match.Groups["token"].Value,
                match.Groups["dark"].Value.ToUpperInvariant(),
                match.Groups["light"].Value.ToUpperInvariant()));
        }
        return list;
    }

    /// <summary>
    /// § Yasaklı token adları tablosundaki terimleri döndürür.
    /// </summary>
    public static IReadOnlyList<string> GetForbiddenTokenNames()
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pattern = new Regex(@"^\|\s*(?<names>[^|]+)\|\s*[^|]+\|\s*[^|]+\|", RegexOptions.Multiline);
        bool inSection = false;

        foreach (var line in Content.Split('\n'))
        {
            if (line.StartsWith("## Yasaklı token adları", StringComparison.Ordinal))
            {
                inSection = true;
                continue;
            }
            if (inSection && line.StartsWith("## ", StringComparison.Ordinal))
            {
                break;
            }
            if (inSection && line.StartsWith('|') && !line.Contains("Yasaklı ad", StringComparison.Ordinal) && !line.Contains("---", StringComparison.Ordinal))
            {
                var match = pattern.Match(line);
                if (match.Success)
                {
                    var namesCell = match.Groups["names"].Value;
                    var tokenMatches = Regex.Matches(namesCell, @"`(?<token>[A-Za-z0-9]+)\*?`");
                    foreach (Match tm in tokenMatches)
                    {
                        terms.Add(tm.Groups["token"].Value);
                    }
                }
            }
        }
        return terms.ToList();
    }

    /// <summary>
    /// § Tip skalası tablosundaki 7 kademeyi döndürür.
    /// </summary>
    public static IReadOnlyList<TypeScaleRow> GetTypeScale()
    {
        var list = new List<TypeScaleRow>();
        var pattern = new Regex(@"^\|\s*`(?<token>Type[A-Za-z]+)`\s*\|\s*(?<size>\d+)\s*\|", RegexOptions.Multiline);
        foreach (Match match in pattern.Matches(Content))
        {
            list.Add(new TypeScaleRow(match.Groups["token"].Value, double.Parse(match.Groups["size"].Value, CultureInfo.InvariantCulture)));
        }
        return list;
    }

    /// <summary>
    /// § Kontrast çiftleri tablosundaki satırları döndürür.
    /// </summary>
    public static IReadOnlyList<ContrastPairRow> GetContrastPairs()
    {
        var list = new List<ContrastPairRow>();
        var pattern = new Regex(@"^\|\s*`(?<text>[A-Za-z0-9_]+)`\s*\|\s*`(?<surface>[A-Za-z0-9_]+)`\s*\|\s*(?<threshold>[0-9,]+)\s*\|\s*(?<dark>[0-9,]+)\s*\|\s*(?<light>[0-9,]+)\s*\|", RegexOptions.Multiline);
        foreach (Match match in pattern.Matches(Content))
        {
            var thrStr = match.Groups["threshold"].Value.Replace(',', '.');
            var thr = double.Parse(thrStr, CultureInfo.InvariantCulture);
            list.Add(new ContrastPairRow(match.Groups["text"].Value, match.Groups["surface"].Value, thr));
        }
        return list;
    }

    /// <summary>
    /// § Marka tablosundaki değerleri döndürür.
    /// </summary>
    public static BrandInfo GetBrandInfo()
    {
        var prodMatch = Regex.Match(Content, @"^\|\s*Ürün adı\s*\|\s*`(?<val>[^`]+)`\s*\|", RegexOptions.Multiline);
        var oldMatch = Regex.Match(Content, @"^\|\s*Eski ad\s*\|\s*`(?<val>[^`]+)`\s*\|", RegexOptions.Multiline);
        return new BrandInfo(prodMatch.Groups["val"].Value, oldMatch.Groups["val"].Value);
    }

    /// <summary>
    /// § İkonlar tablosundaki 12 ikonu döndürür.
    /// </summary>
    public static IReadOnlyList<string> GetIconNames()
    {
        var list = new List<string>();
        var pattern = new Regex(@"^\|\s*`(?<icon>[A-Za-z0-9]+)`\s*\|\s*[^|]+\|", RegexOptions.Multiline);
        bool inSection = false;

        foreach (var line in Content.Split('\n'))
        {
            if (line.StartsWith("## İkonlar", StringComparison.Ordinal))
            {
                inSection = true;
                continue;
            }
            if (inSection && line.StartsWith("## ", StringComparison.Ordinal))
            {
                break;
            }
            if (inSection && line.StartsWith('|') && !line.Contains("Ad", StringComparison.Ordinal) && !line.Contains("---", StringComparison.Ordinal))
            {
                var match = pattern.Match(line);
                if (match.Success)
                {
                    list.Add(match.Groups["icon"].Value);
                }
            }
        }
        return list;
    }
}

internal sealed record ColorTokenRow(string Token, string DarkHex, string LightHex);
internal sealed record TypeScaleRow(string Token, double Size);
internal sealed record ContrastPairRow(string TextToken, string SurfaceToken, double Threshold);
internal sealed record BrandInfo(string ProductName, string OldName);
