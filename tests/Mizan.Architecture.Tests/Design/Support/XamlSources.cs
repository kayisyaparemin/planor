using System.Xml.Linq;

namespace Mizan.Architecture.Tests.Design.Support;

/// <summary>
/// Mizan.App altındaki XAML kaynak dosyalarını okuyan ve sunan test yardımcısı.
/// </summary>
internal static class XamlSources
{
    private static readonly Lazy<IReadOnlyList<XamlDocumentInfo>> DocumentsLazy = new(LoadDocuments);

    /// <summary>
    /// src/Mizan.App altındaki tüm XAML belgelerini döndürür.
    /// </summary>
    public static IReadOnlyList<XamlDocumentInfo> GetAllDocuments() => DocumentsLazy.Value;

    /// <summary>
    /// Belirtilen dosya adıyla eşleşen XAML belgesini döndürür. Bulunamazsa null döner.
    /// </summary>
    public static XamlDocumentInfo? FindDocument(string fileName)
    {
        return GetAllDocuments().FirstOrDefault(d => string.Equals(d.FileName, fileName, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<XamlDocumentInfo> LoadDocuments()
    {
        var appPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App");
        if (!Directory.Exists(appPath))
        {
            return Array.Empty<XamlDocumentInfo>();
        }

        var xamlFiles = Directory.GetFiles(appPath, "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToList();

        var list = new List<XamlDocumentInfo>();
        foreach (var file in xamlFiles)
        {
            var content = File.ReadAllText(file);
            list.Add(new XamlDocumentInfo(file, Path.GetFileName(file), content));
        }

        return list;
    }

    /// <summary>
    /// XAML belgesindeki Color öğelerini anahtar ve değer sırasıyla döndürür.
    /// </summary>
    public static List<KeyValuePair<string, string>> ParseColorsWithOrder(string content)
    {
        var list = new List<KeyValuePair<string, string>>();
        var pattern = new System.Text.RegularExpressions.Regex(@"<Color\s+x:Key=""(?<key>[A-Za-z0-9_]+)"">\s*(?<val>#[A-Fa-f0-9]{6,8})\s*</Color>");
        foreach (System.Text.RegularExpressions.Match m in pattern.Matches(content))
        {
            list.Add(new KeyValuePair<string, string>(m.Groups["key"].Value, m.Groups["val"].Value.ToUpperInvariant()));
        }
        return list;
    }
}

/// <summary>
/// Bir XAML dosyasının yolunu, adını ve ham içeriğini temsil eden veri kaydı.
/// </summary>
internal sealed record XamlDocumentInfo(string FullPath, string FileName, string Content);
