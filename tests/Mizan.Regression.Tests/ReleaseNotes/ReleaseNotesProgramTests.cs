using Mizan.ReleaseNotes;

namespace Mizan.Regression.Tests.ReleaseNotes;

/// <summary>
/// Konsol aracının sözünü sınar: başarıda notu dosyaya yazar, ihlalde 1 döner ve eski bir notu
/// çıktı dosyasında bırakmaz (release.yml `--notes-file` ile yanlış sürümün notunu yayınlamasın diye).
/// </summary>
public sealed class ReleaseNotesProgramTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "mizan-notes-" + Guid.NewGuid().ToString("N"));

    public ReleaseNotesProgramTests() => Directory.CreateDirectory(directory);

    public void Dispose() => Directory.Delete(directory, recursive: true);

    [Fact]
    public void Main_SurumVarsa_SifirDonerVeNotuYazar()
    {
        var output = Path.Combine(directory, "notlar.md");

        var exitCode = Program.Main(Arguments(ChangelogFixtures.Valid, "0.1.0", output));

        Assert.Equal(0, exitCode);
        var written = File.ReadAllText(output);
        Assert.StartsWith("### Eklendi", written);
        Assert.Contains("- İlk sürümün düzeltmesi.", written);
        Assert.DoesNotContain("İkinci sürümün", written);
    }

    [Fact]
    public void Main_CiktiVerilmediyse_SifirDonerVeDosyaYazmaz()
    {
        var exitCode = Program.Main(Arguments(ChangelogFixtures.Valid, "0.1.0", output: null));

        Assert.Equal(0, exitCode);
        Assert.Equal(["CHANGELOG.md"], Directory.GetFiles(directory).Select(Path.GetFileName));
    }

    [Fact]
    public void Main_BolumYoksa_BirDonerVeCiktiDosyasiOlusmaz()
    {
        var output = Path.Combine(directory, "notlar.md");

        var exitCode = Program.Main(Arguments(ChangelogFixtures.Valid, "0.9.0", output));

        Assert.Equal(1, exitCode);
        Assert.False(File.Exists(output));
    }

    [Fact]
    public void Main_BolumYoksa_OncedenKalanCiktiyiSiler()
    {
        var output = Path.Combine(directory, "notlar.md");
        File.WriteAllText(output, "Eski sürümün notu.");

        var exitCode = Program.Main(Arguments(ChangelogFixtures.Valid, "0.9.0", output));

        Assert.Equal(1, exitCode);
        Assert.False(File.Exists(output));
    }

    [Fact]
    public void Main_ChangelogDosyasiYoksa_BirDoner()
    {
        var missing = Path.Combine(directory, "yok.md");

        var exitCode = Program.Main(["--changelog", missing, "--version", "0.1.0"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void Main_SurumArgumaniEksikse_BirDoner()
    {
        var changelog = WriteChangelog(ChangelogFixtures.Valid);

        var exitCode = Program.Main(["--changelog", changelog]);

        Assert.Equal(1, exitCode);
    }

    private string[] Arguments(string changelogText, string version, string? output)
    {
        var arguments = new List<string> { "--changelog", WriteChangelog(changelogText), "--version", version };
        if (output is not null)
        {
            arguments.AddRange(["--output", output]);
        }

        return [.. arguments];
    }

    private string WriteChangelog(string text)
    {
        var path = Path.Combine(directory, "CHANGELOG.md");
        File.WriteAllText(path, text);
        return path;
    }
}
