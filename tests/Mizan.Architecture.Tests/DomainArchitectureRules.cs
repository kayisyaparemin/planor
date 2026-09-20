using System.Reflection;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Mizan.Domain projesinin mimari kuralını (K1) denetler.
/// Domain katmanı saf hesap çekirdeğidir: sıfır NuGet paketi, sıfır I/O, sıfır saat çağrısı.
/// </summary>
internal static class DomainArchitectureRules
{
    public static IReadOnlyList<string> CheckDependencies()
    {
        var violations = new List<string>();
        CheckProjectFile(violations);
        CheckAssemblyReferences(violations);
        CheckSourceCode(violations);
        return violations;
    }

    private static void CheckProjectFile(List<string> violations)
    {
        var csprojPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.Domain", "Mizan.Domain.csproj");
        if (!File.Exists(csprojPath))
        {
            return;
        }

        var content = File.ReadAllText(csprojPath);
        if (content.Contains("<PackageReference", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add("Mizan.Domain.csproj içinde PackageReference bulunamaz.");
        }
    }

    private static void CheckAssemblyReferences(List<string> violations)
    {
        var assembly = Assembly.Load("Mizan.Domain");
        var references = assembly.GetReferencedAssemblies();
        var allowedPrefixes = new[] { "System", "Microsoft.NET", "mscorlib", "netstandard" };

        foreach (var reference in references)
        {
            var name = reference.Name ?? string.Empty;
            var isAllowed = allowedPrefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            if (!isAllowed)
            {
                violations.Add($"Mizan.Domain izin verilmeyen referans içeriyor: {name}");
            }
        }
    }

    private static void CheckSourceCode(List<string> violations)
    {
        var domainDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.Domain");
        if (!Directory.Exists(domainDir))
        {
            return;
        }

        var files = Directory.GetFiles(domainDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("/obj/") &&
                        !f.Contains("\\bin\\") && !f.Contains("/bin/"));

        var forbiddenTokens = new[]
        {
            "using System.IO;",
            "DateTime.Now",
            "DateTime.Today",
            "DateTimeOffset.Now",
            "DateTimeOffset.UtcNow"
        };

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var token in forbiddenTokens)
                {
                    if (lines[i].Contains(token, StringComparison.Ordinal))
                    {
                        violations.Add($"{Path.GetFileName(file)} (Satır {i + 1}): '{token}' kullanımı yasaktır.");
                    }
                }
            }
        }
    }
}
