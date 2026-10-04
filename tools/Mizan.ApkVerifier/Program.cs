namespace Mizan.ApkVerifier;

/// <summary>
/// Komut satırı girişi; <c>scripts/verify-apk.ps1</c> tarafından çağrılır.
/// Kullanım: <c>--badging F --certs F --manifest F --signature-verified true|false --csproj F [--tag vX.Y.Z] [--require-release-signature]</c>.
/// </summary>
public static class Program
{
    /// <summary>Kanıt dosyalarını okur, kuralları çalıştırır; ihlal ya da hata varsa 1 döner.</summary>
    /// <param name="args">Komut satırı argümanları.</param>
    /// <returns>Çıkış kodu.</returns>
    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        try
        {
            var options = ParseOptions(args);
            var evidence = new ApkEvidence(
                ReadText(options, "--badging"),
                ReadText(options, "--certs"),
                ReadText(options, "--manifest"),
                bool.Parse(Value(options, "--signature-verified")));
            var expectation = ApkExpectationReader.FromCsproj(
                ReadText(options, "--csproj"),
                options.GetValueOrDefault("--tag"),
                options.ContainsKey("--require-release-signature"));

            var results = ApkRules.Check(evidence, expectation);
            foreach (var result in results)
            {
                Console.WriteLine($"[{(result.Passed ? "OK" : "HATA")}] {result.Name}: {result.Detail}");
            }

            return results.All(r => r.Passed) ? 0 : 1;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException or FormatException or System.Xml.XmlException)
        {
            Console.Error.WriteLine($"[HATA] APK doğrulanamadı: {ex.Message}");
            return 1;
        }
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < args.Length; i++)
        {
            var hasValue = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal);
            options[args[i]] = hasValue ? args[++i] : string.Empty;
        }

        return options;
    }

    private static string Value(Dictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && value.Length > 0
            ? value
            : throw new ArgumentException($"'{key}' argümanı eksik.");

    private static string ReadText(Dictionary<string, string> options, string key) =>
        File.ReadAllText(Value(options, key), System.Text.Encoding.UTF8);
}
