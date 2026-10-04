using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Dosya, tip ve metot boyutu ile partial kullanım kurallarını (K3, K4) denetler.
/// </summary>
internal static class CodeStructureRules
{
    private static readonly Regex PublicTypeRegex = new(
        @"^\s*public\s+(?:sealed\s+|abstract\s+|static\s+)?(?:class|interface|record|enum|struct)\s+(\w+)",
        RegexOptions.Compiled);

    private static readonly Regex PartialTypeRegex = new(
        @"^\s*(?:public|internal|private)?\s*(?:sealed|abstract|static)?\s*partial\s+(?:class|interface|record|struct)\s+(\w+)",
        RegexOptions.Compiled);

    public static IReadOnlyList<string> CheckFileAndMethodLimits()
    {
        var violations = new List<string>();
        var files = GetHandwrittenSourceFiles();

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            CheckFileLength(file, lines, violations);
            CheckPublicTypeCount(file, lines, violations);
            CheckMethodLengths(file, lines, violations);
        }

        return violations;
    }

    public static IReadOnlyList<string> CheckPartialUsage()
    {
        var violations = new List<string>();
        var files = GetHandwrittenSourceFiles();
        var partialTypesByProject = new Dictionary<string, Dictionary<string, List<string>>>();

        foreach (var file in files)
        {
            var projectDir = FindProjectDirectory(file);
            if (!partialTypesByProject.TryGetValue(projectDir, out var typesInProject))
            {
                typesInProject = [];
                partialTypesByProject[projectDir] = typesInProject;
            }

            foreach (var line in File.ReadAllLines(file))
            {
                var match = PartialTypeRegex.Match(line);
                if (match.Success)
                {
                    var typeName = match.Groups[1].Value;
                    if (!typesInProject.TryGetValue(typeName, out var declaringFiles))
                    {
                        declaringFiles = [];
                        typesInProject[typeName] = declaringFiles;
                    }
                    declaringFiles.Add(Path.GetFileName(file));
                }
            }
        }

        foreach (var (project, types) in partialTypesByProject)
        {
            foreach (var (typeName, declaringFiles) in types)
            {
                if (declaringFiles.Distinct().Count() > 1)
                {
                    violations.Add(
                        $"{Path.GetFileName(project)}: '{typeName}' tipi birden fazla dosyada partial ile bölünmüş: " +
                        string.Join(", ", declaringFiles.Distinct()));
                }
            }
        }

        return violations;
    }

    private static void CheckFileLength(string file, string[] lines, List<string> violations)
    {
        if (lines.Length > 200)
        {
            violations.Add($"{Path.GetFileName(file)}: {lines.Length} satır (Sınır: 200 satır).");
        }
    }

    private static void CheckPublicTypeCount(string file, string[] lines, List<string> violations)
    {
        var publicTypes = new List<string>();
        foreach (var line in lines)
        {
            var match = PublicTypeRegex.Match(line);
            if (match.Success)
            {
                publicTypes.Add(match.Groups[1].Value);
            }
        }

        if (publicTypes.Count > 1)
        {
            violations.Add(
                $"{Path.GetFileName(file)}: Dosyada birden fazla public tip var ({string.Join(", ", publicTypes)}).");
        }
    }

    private static void CheckMethodLengths(string file, string[] lines, List<string> violations)
    {
        var depth = 0;
        var blockStartLine = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            var opens = trimmed.Count(c => c == '{');
            var closes = trimmed.Count(c => c == '}');

            if (depth == 1 && opens > closes)
            {
                blockStartLine = i + 1;
            }

            depth += opens - closes;

            if (depth == 1 && blockStartLine > 0 && closes > 0)
            {
                var blockLength = (i + 1) - blockStartLine + 1;
                if (blockLength > 40)
                {
                    violations.Add(
                        $"{Path.GetFileName(file)} (Satır {blockStartLine}-{i + 1}): Metot gövdesi {blockLength} satır (Sınır: 40 satır).");
                }
                blockStartLine = -1;
            }
        }
    }

    internal static IEnumerable<string> GetHandwrittenSourceFiles()
    {
        // tools/ de taranır: CI'ın güvendiği doğrulayıcı, koruduğu koddan gevşek yazılamaz (K2a).
        return new[] { SolutionPaths.SourceDirectory, SolutionPaths.ToolsDirectory }
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("/obj/") &&
                        !f.Contains("\\bin\\") && !f.Contains("/bin/"));
    }

    private static string FindProjectDirectory(string filePath)
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(filePath)!);
        while (dir is not null && dir.GetFiles("*.csproj").Length == 0)
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? Path.GetDirectoryName(filePath)!;
    }
}
