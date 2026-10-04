using System.Text;

namespace Mizan.Regression.Tests.Generation;

/// <summary>
/// C# otomasyon kimliklerinden derlenen Maestro E2E akışını üreten ve disk ile senkronize eden jeneratör.
/// </summary>
public static class MaestroFlowGenerator
{
    /// <summary>
    /// Tip-güvenli regresyon akış tanımından Maestro YAML metnini üretir.
    /// </summary>
    public static string GenerateYaml()
    {
        return RegressionFlowDefinition.Build().BuildYaml();
    }

    /// <summary>
    /// Üretilen akış YAML dosyasını hedeflenen dosya yoluna yazar.
    /// </summary>
    public static void WriteToDisk(string? targetPath = null)
    {
        var path = targetPath ?? RegressionPaths.MaestroFlowPath;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var yaml = GenerateYaml();
        File.WriteAllText(path, yaml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
