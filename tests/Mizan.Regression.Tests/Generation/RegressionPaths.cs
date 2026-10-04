namespace Mizan.Regression.Tests.Generation;

/// <summary>
/// Regresyon testleri ve E2E akış dosyalarının kök dizinini ve göreceli yollarını çözümleyen yardımcı sınıf.
/// </summary>
internal static class RegressionPaths
{
    private static readonly Lazy<string> RootLazy = new(ResolveSolutionRoot);

    public static string Root => RootLazy.Value;

    public static string MaestroFlowPath => Path.Combine(Root, ".maestro", "flows", "full_regression_flow.yaml");

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
