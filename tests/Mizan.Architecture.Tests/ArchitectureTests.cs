namespace Mizan.Architecture.Tests;

/// <summary>
/// Mizan projesinin 8 pazarlıksız mimari kuralını (K1-K8) ve mimari ilkelerini denetleyen
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
}
