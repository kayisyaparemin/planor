using Mizan.ApkVerifier;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Uygulama kimliğini csproj'da sabitler (S80-2). APK doğrulayıcı APK'yı csproj ile karşılaştırır;
/// csproj'daki kimlik değişirse APK da onunla birlikte değişir ve doğrulama yine geçer.
/// Kimliğin kendisini sabitleyen tek yer bu testtir. Kaynak dosya okuduğu için burada durur (K7).
/// </summary>
public sealed class AppIdentityTests
{
    [Fact]
    public void GercekCsproj_BeklenenKimligiTasir()
    {
        var csproj = File.ReadAllText(Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Mizan.App.csproj"));

        var expectation = ApkExpectationReader.FromCsproj(csproj, tagVersion: null, requireReleaseSignature: false);

        Assert.Equal("com.mizan.app", expectation.Identity.PackageId);
        Assert.Equal("Planör", expectation.Identity.Label);
    }
}
