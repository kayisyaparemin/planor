using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// GK10 kuralını (kontrast kalkanı) denetleyen mimari testler.
/// </summary>
public sealed class DesignContrastTests
{
    [Fact]
    public void KontrastHesabi_BilinenDegerleriUretir()
    {
        var bw = ContrastRatio.Calculate("#000000", "#FFFFFF");
        Assert.Equal(21.0, bw);

        var same = ContrastRatio.Calculate("#1E232A", "#1E232A");
        Assert.Equal(1.0, same);
    }

    [Fact]
    public void KontrastCiftleri_EsigiGecer()
    {
        var darkDoc = XamlSources.FindDocument("DarkPalette.xaml");
        var lightDoc = XamlSources.FindDocument("LightPalette.xaml");

        Assert.True(darkDoc is not null, "DarkPalette.xaml bulunamadı.");
        Assert.True(lightDoc is not null, "LightPalette.xaml bulunamadı.");

        var darkColors = ParsePaletteColors(darkDoc.Content);
        var lightColors = ParsePaletteColors(lightDoc.Content);

        var pairs = DesignSystemDocument.GetContrastPairs();
        Assert.NotEmpty(pairs);

        var failures = new List<string>();

        foreach (var pair in pairs)
        {
            if (!darkColors.TryGetValue(pair.TextToken, out var darkText) ||
                !darkColors.TryGetValue(pair.SurfaceToken, out var darkSurface))
            {
                failures.Add($"Koyu temada '{pair.TextToken}' veya '{pair.SurfaceToken}' token'ı eksik.");
            }
            else
            {
                var ratio = ContrastRatio.Calculate(darkText, darkSurface);
                if (ratio < pair.Threshold)
                {
                    failures.Add($"Koyu temada '{pair.TextToken}' / '{pair.SurfaceToken}' oranı {ratio:F2} < eşik {pair.Threshold:F2} (metin: {darkText}, zemin: {darkSurface})");
                }
            }

            if (!lightColors.TryGetValue(pair.TextToken, out var lightText) ||
                !lightColors.TryGetValue(pair.SurfaceToken, out var lightSurface))
            {
                failures.Add($"Açık temada '{pair.TextToken}' veya '{pair.SurfaceToken}' token'ı eksik.");
            }
            else
            {
                var ratio = ContrastRatio.Calculate(lightText, lightSurface);
                if (ratio < pair.Threshold)
                {
                    failures.Add($"Açık temada '{pair.TextToken}' / '{pair.SurfaceToken}' oranı {ratio:F2} < eşik {pair.Threshold:F2} (metin: {lightText}, zemin: {lightSurface})");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static Dictionary<string, string> ParsePaletteColors(string xamlContent)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        var pattern = new Regex(@"<Color\s+x:Key=""(?<key>[A-Za-z0-9_]+)"">\s*(?<val>#[A-Fa-f0-9]{6,8})\s*</Color>");
        foreach (Match match in pattern.Matches(xamlContent))
        {
            dict[match.Groups["key"].Value] = match.Groups["val"].Value.ToUpperInvariant();
        }
        return dict;
    }
}
