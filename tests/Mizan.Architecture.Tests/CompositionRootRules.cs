using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests;

/// <summary>
/// DI kayıtlarının yalnız kompozisyon kökünde durduğunu denetler: <c>src/Mizan.App/MauiProgram.cs</c> ve
/// <c>src/Mizan.App/Composition/</c> altı. Bir kayıt başka bir yere sızarsa (kendini kaydeden bir uzantı metodu,
/// bir servisin içinde <c>services.AddX</c>) servis grafiği tek yerden okunamaz olur; eski projede gizli
/// <c>new</c> zincirleri böyle doğmuştu (S49). <c>MauiProgram.cs</c> 200 satır sınırına dayanınca ekran kayıtları
/// <c>Composition/</c>'a ayrıldı (V9) ve kural o gün testli hâle geldi.
/// </summary>
internal static class CompositionRootRules
{
    private const string MauiProgram = "Mizan.App/MauiProgram.cs";
    private const string CompositionFolder = "Mizan.App/Composition/";

    private static readonly Regex RegistrationRegex = new(
        @"\.\s*(AddSingleton|AddTransient|AddScoped)\s*[(<]",
        RegexOptions.Compiled);

    public static IReadOnlyList<string> CheckRegistrationsOnlyInCompositionRoot()
    {
        var files = Directory.GetFiles(SolutionPaths.SourceDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("/obj/") &&
                        !f.Contains("\\bin\\") && !f.Contains("/bin/"));

        return files
            .SelectMany(file => CheckSourceContent(RelativeToSource(file), File.ReadAllText(file)))
            .ToList();
    }

    /// <param name="relativePath"><c>src/</c>'ye göre yol, ayırıcı <c>/</c> (örn. <c>Mizan.App/MauiProgram.cs</c>).</param>
    public static IReadOnlyList<string> CheckSourceContent(string relativePath, string content)
    {
        if (relativePath == MauiProgram || relativePath.StartsWith(CompositionFolder, StringComparison.Ordinal))
        {
            return [];
        }

        var lines = content.Split('\n');
        var violations = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
            // Yorum satırı kuralın gerekçesini anlatabilir; yalnız çalışan kod denetlenir.
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (RegistrationRegex.IsMatch(lines[i]))
            {
                violations.Add($"{relativePath} (Satır {i + 1}): DI kaydı yalnız kompozisyon kökünde olur ({MauiProgram} ya da {CompositionFolder}).");
            }
        }

        return violations;
    }

    private static string RelativeToSource(string file) =>
        Path.GetRelativePath(SolutionPaths.SourceDirectory, file).Replace('\\', '/');
}
