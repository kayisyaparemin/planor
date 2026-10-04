using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Tip güvenliği, Service Locator yasağı ve asenkron metot kurallarını (K5, K6, M3, M5) denetler.
/// </summary>
internal static class TypeSafetyRules
{
    internal static readonly string[] ProductionAssemblies =
    [
        "Mizan.Domain",
        "Mizan.Application",
        "Mizan.Infrastructure",
        "Mizan.Presentation",
        "Mizan.ApkVerifier"
    ];

    public static IReadOnlyList<string> CheckNoAsyncVoid()
    {
        var violations = new List<string>();

        foreach (var assemblyName in ProductionAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            foreach (var type in assembly.GetTypes())
            {
                var methods = type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    if (method.ReturnType == typeof(void) &&
                        method.GetCustomAttribute<AsyncStateMachineAttribute>() != null)
                    {
                        violations.Add($"{type.FullName}.{method.Name}: async void metot yasaktır.");
                    }
                }
            }
        }

        CheckAppAsyncVoid(violations);
        return violations;
    }

    public static IReadOnlyList<string> CheckNoServiceLocator()
    {
        var violations = new List<string>();

        foreach (var assemblyName in ProductionAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            foreach (var type in assembly.GetTypes())
            {
                foreach (var ctor in type.GetConstructors())
                {
                    if (ctor.GetParameters().Any(p => typeof(IServiceProvider).IsAssignableFrom(p.ParameterType)))
                    {
                        violations.Add($"{type.FullName}: Yapıcıda IServiceProvider enjekte edilemez.");
                    }
                }
            }
        }

        return violations;
    }

    public static IReadOnlyList<string> CheckTypeSizeLimits()
    {
        var violations = new List<string>();

        foreach (var assemblyName in ProductionAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsInterface)
                {
                    var methodCount = type.GetMethods().Length;
                    if (methodCount > 10)
                    {
                        violations.Add($"{type.FullName}: Arayüz 10'dan fazla metot içeremez ({methodCount} metot).");
                    }
                }
                else
                {
                    foreach (var ctor in type.GetConstructors())
                    {
                        var paramCount = ctor.GetParameters().Length;
                        if (paramCount > 5)
                        {
                            violations.Add($"{type.FullName}: Yapıcı 5'ten fazla bağımlılık içeremez ({paramCount} parametre).");
                        }
                    }
                }
            }
        }

        return violations;
    }

    public static IReadOnlyList<string> CheckNoCompositeRepositories()
    {
        var violations = new List<string>();

        foreach (var assemblyName in ProductionAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            foreach (var type in assembly.GetTypes())
            {
                CheckTypeForCompositeRepositoryViolations(type, violations);
            }
        }

        return violations;
    }

    public static void CheckTypeForCompositeRepositoryViolations(Type type, List<string> violations)
    {
        if (type.Name == "IMizanStore" || type.Name.EndsWith("MizanStore", StringComparison.Ordinal))
        {
            violations.Add($"{type.FullName}: IMizanStore veya türevleri tanrı arayüzdür, var olamaz (Kural M5, Düğüm T10).");
        }

        if (type.IsInterface)
        {
            var inheritedRepoInterfaces = type.GetInterfaces()
                .Where(i => i.Name.EndsWith("Repository", StringComparison.Ordinal))
                .ToList();

            if (inheritedRepoInterfaces.Count > 1 ||
                (inheritedRepoInterfaces.Count == 1 && type.Name.EndsWith("Repository", StringComparison.Ordinal) && type != inheritedRepoInterfaces[0]))
            {
                violations.Add($"{type.FullName}: Depo arayüzleri başka depo arayüzlerini miras alamaz. Kompozit arayüz yasaktır (Kural M5, Düğüm T10). Miras alınan: {string.Join(", ", inheritedRepoInterfaces.Select(i => i.Name))}");
            }
        }
    }

    public static IReadOnlyList<string> CheckNoGodFacade()
    {
        var violations = new List<string>();

        foreach (var assemblyName in ProductionAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            foreach (var type in assembly.GetTypes())
            {
                CheckTypeForGodFacadeViolations(type, violations);
            }
        }

        return violations;
    }

    public static void CheckTypeForGodFacadeViolations(Type type, List<string> violations)
    {
        if (type.Name == "MizanService" || type.Name.EndsWith("MizanService", StringComparison.Ordinal))
        {
            violations.Add($"{type.FullName}: MizanService ve türevleri tanrı cephedir (god facade), var olamaz (Kural M3, Düğüm T7).");
        }
    }

    private static void CheckAppAsyncVoid(List<string> violations)
    {
        var appDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App");
        if (!Directory.Exists(appDir))
        {
            return;
        }

        var files = Directory.GetFiles(appDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("/obj/") &&
                        !f.Contains("\\bin\\") && !f.Contains("/bin/"));

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("async void", StringComparison.Ordinal))
                {
                    var content = string.Join("\n", lines);
                    var hasTryCatch = content.Contains("try", StringComparison.Ordinal) &&
                                      content.Contains("catch", StringComparison.Ordinal);
                    if (!hasTryCatch)
                    {
                        violations.Add($"{Path.GetFileName(file)} (Satır {i + 1}): async void metot try/catch ile sarılmalıdır.");
                    }
                }
            }
        }
    }
}
