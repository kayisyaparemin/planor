namespace Mizan.Architecture.Tests;

/// <summary>
/// Mimari ve kod kalitesi kurallarını denetleyen yardımcı metodlar sınıfı.
/// Kuralları ilgili uzman kontrol sınıflarına yönlendirir.
/// </summary>
internal static class ArchitectureRules
{
    public static IReadOnlyList<string> VerifyDomainDependencies() =>
        DomainArchitectureRules.CheckDependencies();

    public static IReadOnlyList<string> VerifyFileAndMethodLimits() =>
        CodeStructureRules.CheckFileAndMethodLimits();

    public static IReadOnlyList<string> VerifyPartialUsage() =>
        CodeStructureRules.CheckPartialUsage();

    public static IReadOnlyList<string> VerifyNoAsyncVoid() =>
        TypeSafetyRules.CheckNoAsyncVoid();

    public static IReadOnlyList<string> VerifyNoServiceLocator() =>
        TypeSafetyRules.CheckNoServiceLocator();

    public static IReadOnlyList<string> VerifySourceScanningTestsOnlyHere() =>
        TestIntegrityRules.CheckSourceScanningTests();

    public static IReadOnlyList<string> VerifyTypeSizeLimits() =>
        TypeSafetyRules.CheckTypeSizeLimits();

    public static IReadOnlyList<string> VerifyNoCompositeRepositories() =>
        TypeSafetyRules.CheckNoCompositeRepositories();
}
