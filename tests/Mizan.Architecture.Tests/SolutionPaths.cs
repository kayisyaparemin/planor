namespace Mizan.Architecture.Tests;

/// <summary>
/// Proje kök dizinini ve alt dizinleri test çalışma zamanında çözümleyen yardımcı sınıf.
/// Testler IDE, dotnet CLI veya CI ortamında farklı çalışma dizinlerinde koşabileceğinden
/// Mizan.sln dosyasını arayarak kök dizini kararlı şekilde tespit eder.
/// </summary>
internal static class SolutionPaths
{
    private static readonly Lazy<string> SolutionRootLazy = new(ResolveSolutionRoot);

    public static string Root => SolutionRootLazy.Value;

    public static string SourceDirectory => Path.Combine(Root, "src");

    public static string TestsDirectory => Path.Combine(Root, "tests");

    /// <summary>Derleme ve doğrulama araçları (K2a: APK doğrulayıcı); üretim kodu gibi K3–K6'ya tabidir.</summary>
    public static string ToolsDirectory => Path.Combine(Root, "tools");

    private static string ResolveSolutionRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Mizan.sln")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Mizan.sln çözümlenemedi.");
    }
}
