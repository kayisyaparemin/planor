namespace Mizan.Architecture.Tests;

/// <summary>
/// Test projelerinin izolasyonunu ve sahte kaynak tarama testlerinin yasağını (K7) denetler.
/// Kaynak dosyaları metin olarak okuyan testler yalnızca Mizan.Architecture.Tests içinde olabilir.
/// </summary>
internal static class TestIntegrityRules
{
    public static IReadOnlyList<string> CheckSourceScanningTests()
    {
        var violations = new List<string>();
        var testsDir = SolutionPaths.TestsDirectory;

        if (!Directory.Exists(testsDir))
        {
            return violations;
        }

        var testFiles = Directory.GetFiles(testsDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("/obj/") &&
                        !f.Contains("\\bin\\") && !f.Contains("/bin/") &&
                        !f.Contains("Mizan.Architecture.Tests"))
            .ToArray();

        var forbiddenFilePattern = "SourceTests.cs";
        var forbiddenTokens = new[]
        {
            "File.ReadAllText",
            "File.ReadAllLines",
            "File.ReadLines",
            "File.OpenText",
            "File.OpenRead"
        };

        foreach (var file in testFiles)
        {
            var fileName = Path.GetFileName(file);
            if (fileName.EndsWith(forbiddenFilePattern, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add($"{fileName}: *SourceTests.cs dosyası oluşturulamaz. Yalnızca gerçek davranış testleri yazılmalıdır.");
            }

            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var hasSourceReading = forbiddenTokens.Any(t => line.Contains(t, StringComparison.Ordinal)) &&
                                       (line.Contains(".cs", StringComparison.OrdinalIgnoreCase) ||
                                        line.Contains(".xaml", StringComparison.OrdinalIgnoreCase));

                if (hasSourceReading)
                {
                    violations.Add($"{fileName} (Satır {i + 1}): Test içinde kaynak kod veya XAML metni taranamaz.");
                }
            }
        }

        return violations;
    }
}
