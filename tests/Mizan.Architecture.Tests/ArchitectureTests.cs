namespace Mizan.Architecture.Tests;

/// <summary>
/// Mizan projesinin 9 pazarlıksız mimari kuralını (K1-K9) ve mimari ilkelerini denetleyen
/// otomatik test kalkanı. Kuralları çiğneyen kod değişikliklerini derleme ve test aşamasında
/// derhal yakalayarak mimari bütünlüğü korur.
/// </summary>
public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_HicbirPakete_BagliOlamaz()
    {
        var violations = ArchitectureRules.VerifyDomainDependencies();
        Assert.Empty(violations);
    }

    [Fact]
    public void Dosyalar_SinirlariAsamaz()
    {
        var violations = ArchitectureRules.VerifyFileAndMethodLimits();
        Assert.Empty(violations);
    }

    [Fact]
    public void Partial_YalnizGeneratorIcin()
    {
        var violations = ArchitectureRules.VerifyPartialUsage();
        Assert.Empty(violations);
    }

    [Fact]
    public void AsyncVoid_Yasak()
    {
        var violations = ArchitectureRules.VerifyNoAsyncVoid();
        Assert.Empty(violations);
    }

    [Fact]
    public void ServiceLocator_Yasak()
    {
        var violations = ArchitectureRules.VerifyNoServiceLocator();
        Assert.Empty(violations);
    }

    [Fact]
    public void KaynakTarayanTest_YalnizBuradaOlabilir()
    {
        var violations = ArchitectureRules.VerifySourceScanningTestsOnlyHere();
        Assert.Empty(violations);
    }

    [Fact]
    public void SiniflarVeArayuzler_BoyutSinirlariniAsamaz()
    {
        var violations = ArchitectureRules.VerifyTypeSizeLimits();
        Assert.Empty(violations);
    }

    [Fact]
    public void KompozitDepoArayuzu_Ve_IMizanStore_Yasak()
    {
        var violations = ArchitectureRules.VerifyNoCompositeRepositories();
        Assert.Empty(violations);
    }

    [Fact]
    public void KompozitDepoKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        var violations = new List<string>();
        TypeSafetyRules.CheckTypeForCompositeRepositoryViolations(typeof(Fakes.IFakeCompositeStore), violations);
        Assert.NotEmpty(violations);
    }

    [Fact]
    public void GodFacade_Ve_MizanService_Yasak()
    {
        var violations = ArchitectureRules.VerifyNoGodFacade();
        Assert.Empty(violations);
    }

    [Fact]
    public void GodFacadeKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        var violations = new List<string>();
        TypeSafetyRules.CheckTypeForGodFacadeViolations(typeof(Fakes.FakeMizanService), violations);
        Assert.NotEmpty(violations);
    }

    [Fact]
    public void AndroidManifest_AgIzniVeBulutYedegiIstemez()
    {
        var violations = ArchitectureRules.VerifyOfflineManifest();
        Assert.Empty(violations);
    }

    [Fact]
    public void AndroidManifestKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        var manifest = System.Xml.Linq.XDocument.Parse(
            """
            <manifest xmlns:android="http://schemas.android.com/apk/res/android">
              <application android:supportsRtl="true" />
              <uses-permission android:name="android.permission.INTERNET" />
            </manifest>
            """);

        var violations = OfflineRules.CheckManifestContent(manifest);

        Assert.Equal(2, violations.Count);
    }

    [Fact]
    public void MerkeziPaketler_SentryIcermez()
    {
        var violations = ArchitectureRules.VerifyNoSentryPackage();
        Assert.Empty(violations);
    }

    [Fact]
    public void SentryKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        const string props = """
            <ItemGroup>
              <PackageVersion Include="Sentry.Maui" Version="4.13.0" />
            </ItemGroup>
            """;

        var violations = OfflineRules.CheckPackageContent("Directory.Packages.props", props);

        Assert.NotEmpty(violations);
    }
}
