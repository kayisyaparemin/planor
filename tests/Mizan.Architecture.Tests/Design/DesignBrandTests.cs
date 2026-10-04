using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// GK11 kuralını (marka yüzeyleri ve eski ad taraması) zorlayan mimari testler.
/// </summary>
public sealed class DesignBrandTests
{
    private static readonly Regex VisibleTextAttrRegex = new(
        @"\b(?<attr>Text|Title|Placeholder|SemanticProperties\.Description|SemanticProperties\.Hint)\s*=\s*""(?<val>[^""]*)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Fact]
    public void UygulamaAdi_SistemdekiAdla()
    {
        var csprojPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Mizan.App.csproj");
        var content = File.ReadAllText(csprojPath);
        var match = Regex.Match(content, @"<ApplicationTitle>(?<title>[^<]+)</ApplicationTitle>");

        Assert.True(match.Success, "Mizan.App.csproj içinde ApplicationTitle bulunamadı.");
        var expected = DesignSystemDocument.GetBrandInfo().ProductName;
        Assert.Equal(expected, match.Groups["title"].Value.Trim());
    }

    [Fact]
    public void GorunenMetin_EskiAdiIceremez()
    {
        var oldName = DesignSystemDocument.GetBrandInfo().OldName;
        var failures = new List<string>();

        foreach (var doc in XamlSources.GetAllDocuments())
        {
            foreach (Match m in VisibleTextAttrRegex.Matches(doc.Content))
            {
                var val = m.Groups["val"].Value;
                if (ContainsName(val, oldName))
                {
                    failures.Add($"{doc.FileName}: '{m.Groups["attr"].Value}' içinde eski ad '{oldName}' bulundu -> {val}");
                }
            }
        }

        var stringsPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Resources", "Strings");
        if (Directory.Exists(stringsPath))
        {
            foreach (var file in Directory.GetFiles(stringsPath, "*.*", SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(file);
                if (ContainsName(text, oldName))
                {
                    failures.Add($"{Path.GetFileName(file)}: Eski ad '{oldName}' bulundu.");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void EskiAdTaramasi_KodAdiniYakalamaz()
    {
        var oldName = DesignSystemDocument.GetBrandInfo().OldName;

        var sampleCodeLine1 = @"x:Class=""Mizan.App.MainPage""";
        var sampleCodeLine2 = @"xmlns:vm=""clr-namespace:Mizan.Presentation""";
        Assert.False(ContainsOldNameInVisibleText(sampleCodeLine1, oldName));
        Assert.False(ContainsOldNameInVisibleText(sampleCodeLine2, oldName));

        var sampleUiLine1 = @"Text=""Mizan'a hoş geldin""";
        var sampleUiLine2 = @"Title=""MİZAN""";
        Assert.True(ContainsOldNameInVisibleText(sampleUiLine1, oldName));
        Assert.True(ContainsOldNameInVisibleText(sampleUiLine2, oldName));
    }

    [Fact]
    public void PlatformRenkleri_TokenlarlaAyni()
    {
        var colorTokens = DesignSystemDocument.GetColorTokens().ToDictionary(c => c.Token);
        var backdropDark = colorTokens["Backdrop"].DarkHex;
        var backdropLight = colorTokens["Backdrop"].LightHex;
        var indicatorDark = colorTokens["Indicator"].DarkHex;
        var indicatorLight = colorTokens["Indicator"].LightHex;

        var csprojPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Mizan.App.csproj");
        var csproj = File.ReadAllText(csprojPath);
        var iconMatch = Regex.Match(csproj, @"<MauiIcon\s+[^>]*Color=""(?<c>[^""]+)""");
        var splashMatch = Regex.Match(csproj, @"<MauiSplashScreen\s+[^>]*Color=""(?<c>[^""]+)""");

        Assert.True(iconMatch.Success && string.Equals(iconMatch.Groups["c"].Value, backdropDark, StringComparison.OrdinalIgnoreCase),
            $"MauiIcon Color ({iconMatch.Groups["c"].Value}) Backdrop koyu ({backdropDark}) ile eşleşmiyor.");
        Assert.True(splashMatch.Success && string.Equals(splashMatch.Groups["c"].Value, backdropDark, StringComparison.OrdinalIgnoreCase),
            $"MauiSplashScreen Color ({splashMatch.Groups["c"].Value}) Backdrop koyu ({backdropDark}) ile eşleşmiyor.");

        var appIconPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Resources", "AppIcon", "appicon.svg");
        var appIconSvg = File.ReadAllText(appIconPath);
        var rectMatch = Regex.Match(appIconSvg, @"fill=""(?<c>[^""]+)""");
        Assert.True(rectMatch.Success && string.Equals(rectMatch.Groups["c"].Value, backdropDark, StringComparison.OrdinalIgnoreCase),
            $"appicon.svg zemin rengi ({rectMatch.Groups["c"].Value}) Backdrop koyu ({backdropDark}) ile eşleşmiyor.");

        var lightColorsPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Platforms", "Android", "Resources", "values", "colors.xml");
        var nightColorsPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Platforms", "Android", "Resources", "values-night", "colors.xml");

        var lightXml = File.ReadAllText(lightColorsPath);
        var nightXml = File.ReadAllText(nightColorsPath);

        Assert.Contains($@"<color name=""colorPrimary"">{backdropLight}</color>", lightXml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($@"<color name=""colorPrimaryDark"">{backdropLight}</color>", lightXml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($@"<color name=""colorAccent"">{indicatorLight}</color>", lightXml, StringComparison.OrdinalIgnoreCase);

        Assert.Contains($@"<color name=""colorPrimary"">{backdropDark}</color>", nightXml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($@"<color name=""colorPrimaryDark"">{backdropDark}</color>", nightXml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($@"<color name=""colorAccent"">{indicatorDark}</color>", nightXml, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsOldNameInVisibleText(string snippet, string oldName)
    {
        foreach (Match m in VisibleTextAttrRegex.Matches(snippet))
        {
            if (ContainsName(m.Groups["val"].Value, oldName))
            {
                return true;
            }
        }
        return false;
    }

    // RegexOptions.IgnoreCase "İ"yi yalnız tr-TR kültüründe "i" sayar; CI'ın kültürsüz Linux
    // koşucusunda "MİZAN" kaçıyordu. "İ" önce "I"ya indirilir, eşleştirme kültürden bağımsız yapılır.
    private static bool ContainsName(string text, string name) =>
        Regex.IsMatch(
            text.Replace('İ', 'I'),
            $@"\b{Regex.Escape(name)}\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
