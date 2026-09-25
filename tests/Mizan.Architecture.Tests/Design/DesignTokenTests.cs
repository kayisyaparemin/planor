using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// GK1, GK2, GK3 ve GK8 kurallarını zorlayan tasarım token mimari testleri.
/// </summary>
public sealed class DesignTokenTests
{
    private static readonly HashSet<string> ForbiddenNamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "Black", "Blue", "Green", "Yellow", "Gray", "Grey", "Magenta", "Cyan",
        "Orange", "Purple", "Pink", "Brown", "Gold", "LightGray", "DarkGray"
    };

    [Fact]
    public void Renkler_SistemdekiTokenlarlaBirebir()
    {
        var expectedTokens = DesignSystemDocument.GetColorTokens();
        Assert.Equal(23, expectedTokens.Count);

        var darkDoc = XamlSources.FindDocument("DarkPalette.xaml");
        var lightDoc = XamlSources.FindDocument("LightPalette.xaml");

        Assert.True(darkDoc is not null, "DarkPalette.xaml bulunamadı.");
        Assert.True(lightDoc is not null, "LightPalette.xaml bulunamadı.");

        var darkActual = ParseColorsWithOrder(darkDoc.Content);
        var lightActual = ParseColorsWithOrder(lightDoc.Content);

        Assert.Equal(expectedTokens.Count, darkActual.Count);
        Assert.Equal(expectedTokens.Count, lightActual.Count);

        for (int i = 0; i < expectedTokens.Count; i++)
        {
            var exp = expectedTokens[i];
            Assert.Equal(exp.Token, darkActual[i].Key);
            Assert.Equal(exp.DarkHex, darkActual[i].Value);
            Assert.Equal(exp.Token, lightActual[i].Key);
            Assert.Equal(exp.LightHex, lightActual[i].Value);
        }
    }

    [Fact]
    public void Xaml_HamRenk_Iceremez()
    {
        var docs = XamlSources.GetAllDocuments();
        var failures = new List<string>();
        var hexRegex = new Regex(@"#(?:[0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\b");

        foreach (var doc in docs)
        {
            if (doc.FileName is "DarkPalette.xaml" or "LightPalette.xaml") { continue; }

            foreach (var line in doc.Content.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("<!--", StringComparison.Ordinal) && trimmed.EndsWith("-->", StringComparison.Ordinal)) { continue; }

                if (hexRegex.IsMatch(trimmed))
                {
                    failures.Add($"{doc.FileName}: Ham hex renk bulundu -> {trimmed}");
                }

                foreach (var named in ForbiddenNamedColors)
                {
                    if (Regex.IsMatch(trimmed, $@"\b{named}\b", RegexOptions.IgnoreCase))
                    {
                        failures.Add($"{doc.FileName}: Yasaklı adlandırılmış renk '{named}' bulundu -> {trimmed}");
                    }
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void TokenAdi_BoyaAdiOlamaz()
    {
        var forbidden = DesignSystemDocument.GetForbiddenTokenNames();
        var filesToScan = new[] { "DarkPalette.xaml", "LightPalette.xaml", "Styles.xaml" };
        var keyRegex = new Regex(@"x:Key=""(?<key>[A-Za-z0-9_]+)""");
        var failures = new List<string>();

        foreach (var fileName in filesToScan)
        {
            var doc = XamlSources.FindDocument(fileName);
            if (doc is null) { continue; }

            foreach (Match match in keyRegex.Matches(doc.Content))
            {
                var key = match.Groups["key"].Value;
                var words = Regex.Matches(key, @"([A-Z][a-z0-9]+|[A-Z]+(?=[A-Z][a-z0-9]|$))").Select(m => m.Value).ToList();
                if (words.Count == 0) { words.Add(key); }

                foreach (var word in words)
                {
                    if (forbidden.Contains(word))
                    {
                        failures.Add($"{fileName}: '{key}' anahtarında boya adı parçası '{word}' bulundu.");
                    }
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Xaml_FontSize_SkalaDisiOlamaz()
    {
        var validTokens = DesignSystemDocument.GetTypeScale().Select(t => t.Token).ToHashSet();
        validTokens.Add("IconSmall"); validTokens.Add("IconMedium"); validTokens.Add("IconLarge");

        var failures = new List<string>();
        var numRegex = new Regex(@"\bFontSize\s*=\s*""(?<val>[0-9]+)""");
        var resRegex = new Regex(@"\bFontSize\s*=\s*""{(?:StaticResource|DynamicResource)\s+(?<val>[A-Za-z0-9_]+)}""");
        var setterNumRegex = new Regex(@"<Setter\s+Property=""FontSize""\s+Value=""(?<val>[0-9]+)""");
        var setterResRegex = new Regex(@"<Setter\s+Property=""FontSize""\s+Value=""{(?:StaticResource|DynamicResource)\s+(?<val>[A-Za-z0-9_]+)}""");

        foreach (var doc in XamlSources.GetAllDocuments())
        {
            if (doc.FileName is "Tipografi.xaml") { continue; }

            foreach (Match m in numRegex.Matches(doc.Content).Concat(setterNumRegex.Matches(doc.Content)))
            {
                failures.Add($"{doc.FileName}: FontSize sayısal literal taşıyor -> {m.Groups["val"].Value}");
            }

            foreach (Match m in resRegex.Matches(doc.Content).Concat(setterResRegex.Matches(doc.Content)))
            {
                var token = m.Groups["val"].Value;
                if (!validTokens.Contains(token))
                {
                    failures.Add($"{doc.FileName}: Skala dışı FontSize token'ı -> {token}");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Xaml_OlcuLiterali_Yasak()
    {
        var props = new[] { "Margin", "Padding", "Spacing", "CornerRadius", "HeightRequest", "WidthRequest", "StrokeThickness" };
        var failures = new List<string>();
        var attrPattern = new Regex($@"\b(?<prop>{string.Join("|", props)})\s*=\s*""(?<val>[0-9.,\s]+)""");
        var setterPattern = new Regex($@"<Setter\s+Property=""(?<prop>{string.Join("|", props)})""\s+Value=""(?<val>[0-9.,\s]+)""");

        foreach (var doc in XamlSources.GetAllDocuments())
        {
            if (doc.FileName is "Olcu.xaml" or "DarkPalette.xaml" or "LightPalette.xaml" or "Tipografi.xaml") { continue; }

            foreach (Match m in attrPattern.Matches(doc.Content).Concat(setterPattern.Matches(doc.Content)))
            {
                failures.Add($"{doc.FileName}: {m.Groups["prop"].Value} özniteliğinde sayısal literal -> {m.Groups["val"].Value}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Xaml_AppThemeBinding_Yasak()
    {
        var failures = new List<string>();
        foreach (var doc in XamlSources.GetAllDocuments())
        {
            if (doc.Content.Contains("AppThemeBinding", StringComparison.Ordinal))
            {
                failures.Add($"{doc.FileName}: AppThemeBinding kullanımı yasak.");
            }
        }
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Xaml_RenkTokeni_DynamicResourceIle()
    {
        var colorTokens = DesignSystemDocument.GetColorTokens().Select(c => c.Token).ToHashSet();
        var failures = new List<string>();
        var staticRegex = new Regex(@"\{StaticResource\s+(?<name>[A-Za-z0-9_]+)\}");

        foreach (var doc in XamlSources.GetAllDocuments())
        {
            foreach (Match m in staticRegex.Matches(doc.Content))
            {
                var name = m.Groups["name"].Value;
                if (colorTokens.Contains(name))
                {
                    failures.Add($"{doc.FileName}: Renk token'ı '{name}' StaticResource ile bağlanamaz; DynamicResource kullanılmalı.");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static List<KeyValuePair<string, string>> ParseColorsWithOrder(string content)
    {
        var list = new List<KeyValuePair<string, string>>();
        var pattern = new Regex(@"<Color\s+x:Key=""(?<key>[A-Za-z0-9_]+)"">\s*(?<val>#[A-Fa-f0-9]{6,8})\s*</Color>");
        foreach (Match m in pattern.Matches(content))
        {
            list.Add(new KeyValuePair<string, string>(m.Groups["key"].Value, m.Groups["val"].Value.ToUpperInvariant()));
        }
        return list;
    }
}
