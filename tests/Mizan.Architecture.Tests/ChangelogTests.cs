using Mizan.ApkVerifier;
using Mizan.ReleaseNotes;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Sürümü artıran PR'ın notu da getirmesini zorlar (S82-4): csproj'daki sürümün CHANGELOG.md'de dolu
/// bir bölümü yoksa bu test kırmızıdır ve etiket günü yayın sürprizle durmaz. Kaynak dosya okuduğu
/// için burada durur (K7).
/// </summary>
public sealed class ChangelogTests
{
    [Fact]
    public void GercekChangelog_CsprojSurumununNotunuTasir()
    {
        var csproj = File.ReadAllText(Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Mizan.App.csproj"));
        var version = ApkExpectationReader.FromCsproj(csproj, tagVersion: null, requireReleaseSignature: false).Identity.VersionName;
        var changelog = File.ReadAllText(Path.Combine(SolutionPaths.Root, "CHANGELOG.md"));

        var note = ChangelogReader.Read(changelog, version);

        Assert.Equal(version, note.Version);
        Assert.NotEmpty(note.Body);
    }
}
