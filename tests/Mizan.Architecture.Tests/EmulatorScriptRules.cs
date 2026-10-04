using System.Text.RegularExpressions;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Emülatör regresyon betiğinin (K3, S81) eski betiğin açığına geri dönmesini engeller: yedek koordinatla
/// tıklayıp sonucu koşulsuz başarılı saymak ve akışın <c>clearState</c>'iyle emülatördeki profilleri silmek.
/// Betik gerçek emülatör ve Maestro ister, CI'da koşmaz; bu yüzden korunan şey kuralın metinde durmasıdır.
/// </summary>
internal static class EmulatorScriptRules
{
    private static readonly string[] ForbiddenTokens =
    [
        "input tap", "input swipe", "input keyevent", "uiautomator", "Record-Test"
    ];

    public static IReadOnlyList<string> CheckScript() =>
        CheckScriptContent(File.ReadAllText(Path.Combine(SolutionPaths.Root, "scripts", "emulator-regresyon.ps1")));

    public static IReadOnlyList<string> CheckScriptContent(string script)
    {
        var violations = new List<string>();
        RequirePattern(violations, script, @"maestro\s+--device\s+\$serial\s+test\s+\$Akis", "akışı Maestro koşar ve cihaz açıkça verilir (S81-1)");
        RequirePattern(violations, script, @"emulatorde-ac\.ps1""?\s+-Avd", "hazırlık ve yedek emulatorde-ac.ps1'e bırakılır (S81-3)");
        RequirePattern(violations, script, @"\$maestroKodu\s*=\s*\$LASTEXITCODE", "Maestro'nun çıkış kodu saklanmalı (S81-2)");
        RequirePattern(violations, script, @"exit\s+\$maestroKodu", "betik Maestro'nun çıkış koduyla bitmeli, sonuç yutulmaz (S81-2)");
        RequirePattern(violations, script, @"rm -rf files/profiles", "akışın açtığı test profili emülatörde kalmamalı (S81-3)");
        CheckRestoreOrder(violations, script);

        foreach (var token in ForbiddenTokens.Where(token => script.Contains(token, StringComparison.Ordinal)))
        {
            violations.Add($"emulator-regresyon.ps1: '{token}' geçemez: tıklayan ve doğrulayan yalnız Maestro'dur (S81-1).");
        }

        return violations;
    }

    // Geri yükleme akıştan sonra ve finally içinde olmalı: akış kırılsa da profiller dönmeli.
    private static void CheckRestoreOrder(List<string> violations, string script)
    {
        var flow = script.IndexOf("maestro", StringComparison.Ordinal);
        var finallyBlock = script.IndexOf("finally", StringComparison.Ordinal);
        var restore = script.LastIndexOf("-GeriYukle", StringComparison.Ordinal);
        if (flow < 0 || finallyBlock < flow || restore < finallyBlock)
        {
            violations.Add("emulator-regresyon.ps1: '-GeriYukle' akıştan sonra bir finally içinde çağrılmalı (S81-3).");
        }
    }

    private static void RequirePattern(List<string> violations, string script, string pattern, string reason)
    {
        if (!Regex.IsMatch(script, pattern))
        {
            violations.Add($"emulator-regresyon.ps1: '{pattern}' bulunamadı: {reason}.");
        }
    }
}
