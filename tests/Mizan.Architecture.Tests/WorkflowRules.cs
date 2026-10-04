using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests;

/// <summary>
/// CI kalkanının (K2b, S80) sessizce gevşemesini engeller. İş akışı YAML'ı GitHub'da çalışana kadar
/// sınanamaz; bu yüzden burada yalnız kuralın metinde durup durmadığına bakılır: PR kapısı, workload,
/// release'de debug imza reddi, kapsam eşikleri. Eski projede bu satırlardan biri silinse kimse fark etmezdi.
/// </summary>
internal static class WorkflowRules
{
    public static IReadOnlyList<string> CheckCiWorkflow() =>
        CheckCiContent(File.ReadAllText(WorkflowPath("ci.yml")));

    public static IReadOnlyList<string> CheckReleaseWorkflow() =>
        CheckReleaseContent(File.ReadAllText(WorkflowPath("release.yml")));

    public static IReadOnlyList<string> CheckCoverageScript() =>
        CheckCoverageScriptContent(File.ReadAllText(Path.Combine(SolutionPaths.Root, "scripts", "verify-coverage.ps1")));

    public static IReadOnlyList<string> CheckCiContent(string yaml)
    {
        var violations = new List<string>();
        Require(violations, yaml, "ci.yml", "pull_request:", "PR'da çalışmalı: kural main'e girmeden önce denetlenir (S80-3)");
        Require(violations, yaml, "ci.yml", "workflow_call:", "release.yml onu çağırabilmeli, test adımları kopyalanmaz (S80-8)");
        Require(violations, yaml, "ci.yml", "maui-android", "workload kurulmalı: Mizan.App olmadan çözüm Ubuntu'da derlenmez (S80)");
        Require(violations, yaml, "ci.yml", "setup-java", "JDK kurulmalı: Android derlemesi onsuz çalışmaz");
        Require(violations, yaml, "ci.yml", "verify-coverage.ps1", "kapsam eşikleri CI'da zorlanmalı (F4, S80-4)");
        Require(violations, yaml, "ci.yml", "verify-apk.ps1", "APK doğrulanmalı (K2a, S80-1)");
        Forbid(violations, yaml, "ci.yml", "continue-on-error", "kapı adımı başarısızlıkla geçilemez");
        return violations;
    }

    public static IReadOnlyList<string> CheckReleaseContent(string yaml)
    {
        var violations = new List<string>();
        Require(violations, yaml, "release.yml", "v*.*.*", "yalnız vX.Y.Z etiketiyle çalışmalı");
        Require(violations, yaml, "release.yml", "./.github/workflows/ci.yml", "önce CI kapısından geçmeli (S80-8)");
        Require(violations, yaml, "release.yml", "-RequireReleaseSignature", "debug sertifikası reddedilmeli (S80-3)");
        Require(violations, yaml, "release.yml", "-Tag", "etiket csproj sürümüyle eşleşmeli (S80-5)");
        foreach (var secret in new[] { "MIZAN_KEYSTORE_BASE64", "MIZAN_KEYSTORE_PASSWORD", "MIZAN_KEY_ALIAS", "MIZAN_KEY_PASSWORD" })
        {
            Require(violations, yaml, "release.yml", $"secrets.{secret}", "imza secret'ı kullanılmalı");
        }

        Require(violations, yaml, "release.yml", "release-notes.ps1", "sürüm notu CHANGELOG.md'den okunmalı (S82-3)");
        Require(violations, yaml, "release.yml", "--notes-file", "GitHub Release notu okunan dosyadan gelmeli (S82-3)");
        Forbid(violations, yaml, "release.yml", "--generate-notes", "sürüm notu commit dökümü değil, CHANGELOG.md'dir (S82-3)");
        Forbid(violations, yaml, "release.yml", ">> release-notes", "sürüm notu iş akışında echo ile üretilmez (S82-3)");
        Forbid(violations, yaml, "release.yml", "continue-on-error", "kapı adımı başarısızlıkla geçilemez");
        return violations;
    }

    public static IReadOnlyList<string> CheckCoverageScriptContent(string script)
    {
        var violations = new List<string>();
        var thresholds = new (string Parameter, int Value)[]
        {
            ("DomainThreshold", 95), ("ApplicationThreshold", 95),
            ("PresentationThreshold", 90), ("InfrastructureThreshold", 80)
        };
        foreach (var (parameter, value) in thresholds)
        {
            if (!Regex.IsMatch(script, $@"\${parameter}\s*=\s*{value}(\.0+)?\b"))
            {
                violations.Add($"verify-coverage.ps1: ${parameter} varsayılanı {value} olmalı (S80-4).");
            }
        }

        Require(violations, script, "verify-coverage.ps1", "\"Mizan.Infrastructure\"", "Infrastructure katmanı ölçülmeli (S80-4)");
        Forbid(violations, script, "verify-coverage.ps1", "atlandı", "ölçülemeyen katman kapıyı düşürmeli, atlanmamalı (S80-7)");
        return violations;
    }

    private static void Require(List<string> violations, string content, string file, string token, string reason)
    {
        if (!content.Contains(token, StringComparison.Ordinal))
        {
            violations.Add($"{file}: '{token}' bulunamadı: {reason}.");
        }
    }

    private static void Forbid(List<string> violations, string content, string file, string token, string reason)
    {
        if (content.Contains(token, StringComparison.Ordinal))
        {
            violations.Add($"{file}: '{token}' geçemez: {reason}.");
        }
    }

    private static string WorkflowPath(string fileName) =>
        Path.Combine(SolutionPaths.Root, ".github", "workflows", fileName);
}
