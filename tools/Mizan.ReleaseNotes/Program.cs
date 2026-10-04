using System.Text;

namespace Mizan.ReleaseNotes;

/// <summary>
/// Komut satırı girişi; <c>scripts/release-notes.ps1</c> tarafından çağrılır.
/// Kullanım: <c>--changelog F --version X.Y.Z [--output F]</c>.
/// </summary>
public static class Program
{
    /// <summary>Notu okur, doğrular ve <c>--output</c> verilmişse dosyaya yazar; ihlal ya da hata varsa 1 döner.</summary>
    /// <param name="args">Komut satırı argümanları.</param>
    /// <returns>Çıkış kodu.</returns>
    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        try
        {
            var options = ParseOptions(args);
            var output = options.ContainsKey("--output") ? Value(options, "--output") : null;

            // Eski bir not çıktıda kalırsa `--notes-file` onu yanlış sürümün notu olarak yayınlar;
            // bu yüzden okuma başarısız olsa bile önce silinir.
            if (output is not null)
            {
                File.Delete(output);
            }

            var changelog = File.ReadAllText(Value(options, "--changelog"), Encoding.UTF8);
            var note = ChangelogReader.Read(changelog, Value(options, "--version"));
            if (output is not null)
            {
                File.WriteAllText(output, note.Body + "\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            Console.WriteLine($"[OK] Sürüm notu: {note.Version} ({note.ReleaseDate:yyyy-MM-dd}), {note.Body.Split('\n').Length} satır.");
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"[HATA] Sürüm notu okunamadı: {ex.Message}");
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
}
