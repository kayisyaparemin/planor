using Mizan.ApkVerifier;

namespace Mizan.Regression.Tests.Apk;

public sealed class ApkRulesTests
{
    private static List<string> Failures(ApkEvidence evidence, ApkExpectation expectation) =>
        ApkRules.Check(evidence, expectation).Where(r => !r.Passed).Select(r => r.Name).ToList();

    [Fact]
    public void PrDerlemesi_DogruApk_HepsiGecer()
    {
        var results = ApkRules.Check(ApkFixtures.PrEvidence(), ApkFixtures.PrExpectation());

        Assert.Equal(8, results.Count);
        Assert.All(results, r => Assert.True(r.Passed, $"{r.Name}: {r.Detail}"));
    }

    [Fact]
    public void YayinDerlemesi_YayinImzasiVeEtiketTutarli_HepsiGecer()
    {
        var results = ApkRules.Check(ApkFixtures.ReleaseEvidence(), ApkFixtures.ReleaseExpectation());

        Assert.All(results, r => Assert.True(r.Passed, $"{r.Name}: {r.Detail}"));
    }

    [Fact]
    public void YayinDerlemesi_DebugSertifikasi_Reddedilir()
    {
        var failures = Failures(ApkFixtures.PrEvidence(), ApkFixtures.ReleaseExpectation());

        Assert.Equal(["Yayın imzası"], failures);
    }

    [Fact]
    public void YayinDerlemesi_SertifikaOkunamadi_Reddedilir()
    {
        var evidence = ApkFixtures.ReleaseEvidence() with { Certificates = string.Empty };

        Assert.Equal(["Yayın imzası"], Failures(evidence, ApkFixtures.ReleaseExpectation()));
    }

    [Fact]
    public void ImzaDogrulanamadi_Reddedilir()
    {
        var evidence = ApkFixtures.PrEvidence() with { SignatureVerified = false };

        Assert.Equal(["İmza geçerli"], Failures(evidence, ApkFixtures.PrExpectation()));
    }

    [Theory]
    [InlineData("name='com.mizan.app'", "name='com.coinflow.mobile'", "Paket kimliği")]
    [InlineData("versionCode='1'", "versionCode='2'", "versionCode")]
    [InlineData("versionName='0.1.0'", "versionName='0.2.0'", "versionName")]
    [InlineData("application-label:'Planör'", "application-label:'Mizan'", "Uygulama etiketi")]
    public void KimlikUyusmazsa_YalnizIlgiliKontrolKalir(string original, string replacement, string expectedFailure)
    {
        var evidence = ApkFixtures.PrEvidence() with { Badging = ApkFixtures.Badging.Replace(original, replacement, StringComparison.Ordinal) };

        Assert.Equal([expectedFailure], Failures(evidence, ApkFixtures.PrExpectation()));
    }

    [Theory]
    [InlineData("uses-permission: name='android.permission.INTERNET'")]
    [InlineData("uses-permission: name='android.permission.ACCESS_NETWORK_STATE'")]
    [InlineData("uses-permission-sdk-23: name='android.permission.INTERNET'")]
    public void BirlesmisManifesteAgIzniSizarsa_Reddedilir(string permissionLine)
    {
        var badging = ApkFixtures.Badging + $"\n{permissionLine}";
        var evidence = ApkFixtures.PrEvidence() with { Badging = badging };

        Assert.Equal(["Ağ izni yok"], Failures(evidence, ApkFixtures.PrExpectation()));
    }

    [Theory]
    [InlineData("allowBackup(0x01010280)=true")]
    [InlineData("label(0x01010001)=\"x\"")]
    public void AllowBackupKapaliDegilse_Reddedilir(string replacement)
    {
        var manifest = ApkFixtures.Manifest.Replace("allowBackup(0x01010280)=false", replacement, StringComparison.Ordinal);
        var evidence = ApkFixtures.PrEvidence() with { Manifest = manifest };

        Assert.Equal(["Bulut yedeği kapalı"], Failures(evidence, ApkFixtures.PrExpectation()));
    }

    [Fact]
    public void BadgingBos_KimlikVeIzinKontrolleriSessizceGecmez()
    {
        var evidence = ApkFixtures.PrEvidence() with { Badging = string.Empty };

        var failures = Failures(evidence, ApkFixtures.PrExpectation());

        Assert.Equal(["Paket kimliği", "versionCode", "versionName", "Uygulama etiketi", "Ağ izni yok"], failures);
    }

    [Fact]
    public void YayinEtiketi_CsprojSurumundenFarkliysa_Reddedilir()
    {
        var expectation = ApkFixtures.ReleaseExpectation() with { TagVersion = "0.2.0" };

        Assert.Equal(["Sürüm etiketi"], Failures(ApkFixtures.ReleaseEvidence(), expectation));
    }
}
