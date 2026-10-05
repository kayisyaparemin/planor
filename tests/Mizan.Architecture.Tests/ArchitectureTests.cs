namespace Mizan.Architecture.Tests;

/// <summary>
/// Mizan projesinin 9 pazarlıksız mimari kuralını (K1-K9) ve mimari ilkelerini denetleyen
/// otomatik test kalkanı. Kuralları çiğneyen kod değişikliklerini derleme ve test aşamasında
/// derhal yakalayarak mimari bütünlüğü korur.
/// </summary>
public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_HicbirPakete_BagliOlamaz() =>
        Assert.Empty(ArchitectureRules.VerifyDomainDependencies());

    [Fact]
    public void Dosyalar_SinirlariAsamaz() =>
        Assert.Empty(ArchitectureRules.VerifyFileAndMethodLimits());

    [Fact]
    public void Partial_YalnizGeneratorIcin() =>
        Assert.Empty(ArchitectureRules.VerifyPartialUsage());

    [Fact]
    public void AsyncVoid_Yasak() =>
        Assert.Empty(ArchitectureRules.VerifyNoAsyncVoid());

    [Fact]
    public void ServiceLocator_Yasak() =>
        Assert.Empty(ArchitectureRules.VerifyNoServiceLocator());

    [Fact]
    public void MimariTarama_ToolsKlasorunuKapsar()
    {
        var scannedFiles = CodeStructureRules.GetHandwrittenSourceFiles();
        Assert.Contains(scannedFiles, f => f.StartsWith(SolutionPaths.ToolsDirectory, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Mizan.ApkVerifier", TypeSafetyRules.ProductionAssemblies);
    }

    [Fact]
    public void KaynakTarayanTest_YalnizBuradaOlabilir() =>
        Assert.Empty(ArchitectureRules.VerifySourceScanningTestsOnlyHere());

    [Fact]
    public void SiniflarVeArayuzler_BoyutSinirlariniAsamaz() =>
        Assert.Empty(ArchitectureRules.VerifyTypeSizeLimits());

    [Fact]
    public void KompozitDepoArayuzu_Ve_IMizanStore_Yasak() =>
        Assert.Empty(ArchitectureRules.VerifyNoCompositeRepositories());

    [Fact]
    public void KompozitDepoKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        var violations = new List<string>();
        TypeSafetyRules.CheckTypeForCompositeRepositoryViolations(typeof(Fakes.IFakeCompositeStore), violations);
        Assert.NotEmpty(violations);
    }

    [Fact]
    public void GodFacade_Ve_MizanService_Yasak() =>
        Assert.Empty(ArchitectureRules.VerifyNoGodFacade());

    [Fact]
    public void GodFacadeKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        var violations = new List<string>();
        TypeSafetyRules.CheckTypeForGodFacadeViolations(typeof(Fakes.FakeMizanService), violations);
        Assert.NotEmpty(violations);
    }

    [Fact]
    public void AndroidManifest_AgIzniVeBulutYedegiIstemez() =>
        Assert.Empty(ArchitectureRules.VerifyOfflineManifest());

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
    public void MerkeziPaketler_SentryIcermez() =>
        Assert.Empty(ArchitectureRules.VerifyNoSentryPackage());

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

    [Fact]
    public void InsertOrReplace_Yasak() =>
        Assert.Empty(ArchitectureRules.VerifyNoInsertOrReplace());

    [Fact]
    public void InsertOrReplaceKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        const string source = """
            // INSERT OR REPLACE alt kayıtları götürür.
            await _connection.InsertOrReplaceAsync(entity);
            conn.Execute("INSERT OR REPLACE INTO loans (Id) VALUES (?)", id);
            conn.Insert(entity, "OR REPLACE");
            """;

        var violations = PersistenceRules.CheckSourceContent("SqliteLoanRepository.cs", source);
        Assert.Equal(3, violations.Count);
    }

    [Fact]
    public void DiKaydi_YalnizKompozisyonKokundeOlabilir() =>
        Assert.Empty(ArchitectureRules.VerifyRegistrationsOnlyInCompositionRoot());

    [Fact]
    public void KompozisyonKokuKurali_IhlalGordugunde_Yakalayabilmelidir()
    {
        const string source = """
            // services.AddSingleton<IFoo, Foo>(); yorumda geçebilir.
            services.AddSingleton<IFutureProjectionService, FutureProjectionService>();
            services.AddTransient(sp => new PeriodDetailViewModel(sp.GetRequiredService<IFutureProjectionService>(), nav));
            """;

        var outside = CompositionRootRules.CheckSourceContent("Mizan.Application/Services/FutureProjectionService.cs", source);
        var inside = CompositionRootRules.CheckSourceContent("Mizan.App/Composition/ScreenRegistrations.cs", source);

        Assert.Equal(2, outside.Count);
        Assert.Empty(inside);
    }

    [Fact]
    public void YasakliTerimler_KaynaktaGecemez()
    {
        var violations = ArchitectureRules.VerifyForbiddenTerms();
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

    [Fact]
    public void YasakliTerimRegex_YanlisPozitifUretmez()
    {
        Assert.True(ForbiddenTermRules.IsIdentifierAllowed("MigratePeriodPlanRevisionSchemaAsync", "maas"));
        Assert.True(ForbiddenTermRules.IsIdentifierAllowed("PreviewSettlementAsync", "Review"));
        Assert.True(ForbiddenTermRules.IsIdentifierAllowed("SavingsGoal", "Savings"));
        Assert.True(ForbiddenTermRules.IsIdentifierAllowed("SavingsTarget", "Savings"));
        Assert.False(ForbiddenTermRules.IsIdentifierAllowed("TotalSavings", "Savings"));
        Assert.False(ForbiddenTermRules.IsIdentifierAllowed("EmployeeSalary", "Salary"));
    }
}
