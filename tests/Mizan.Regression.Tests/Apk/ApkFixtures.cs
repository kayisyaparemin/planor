using Mizan.ApkVerifier;

namespace Mizan.Regression.Tests.Apk;

/// <summary>
/// Yerel Release APK'sının (2026-10-05) gerçek aapt2/apksigner çıktısından kısaltılmış kanıtlar.
/// Kırmızı senaryolar geçen kanıtın tek bir satırı değiştirilerek üretilir.
/// </summary>
internal static class ApkFixtures
{
    public const string Badging = """
        package: name='com.mizan.app' versionCode='1' versionName='0.1.0' platformBuildVersionName='14' platformBuildVersionCode='34' compileSdkVersion='34' compileSdkVersionCodename='14'
        targetSdkVersion:'34'
        uses-permission: name='android.permission.MANAGE_EXTERNAL_STORAGE'
        uses-permission: name='android.permission.WRITE_EXTERNAL_STORAGE' maxSdkVersion='29'
        uses-permission: name='android.permission.READ_EXTERNAL_STORAGE' maxSdkVersion='29'
        uses-permission: name='com.mizan.app.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION'
        application-label:'Planör'
        application-label-tr:'Planör'
        application: label='Planör' icon='res/mipmap-anydpi-v26/appicon.xml'
        launchable-activity: name='crc6481d3350ae0f0c8a0.MainActivity'  label='' icon=''
        """;

    public const string DebugCertificates = """
        V3.0 Signer: certificate DN: CN=Android Debug, O=Android, C=US
        V3.0 Signer: certificate SHA-256 digest: bbf5614f8f96d8d8fa870726df4109a63657b0a83de7d55ffcd302409ef4d428
        """;

    public const string ReleaseCertificates = """
        Signer #1 certificate DN: CN=Planor, O=Mizan, C=TR
        Signer #1 certificate SHA-256 digest: 0d6b7a3a2f1e9c8b7a6f5e4d3c2b1a09f8e7d6c5b4a39281706f5e4d3c2b1a09
        """;

    public const string Manifest = """
        N: android=http://schemas.android.com/apk/res/android (line=8)
          E: manifest (line=8)
            A: package="com.mizan.app" (Raw: "com.mizan.app")
              E: uses-permission (line=14)
                A: http://schemas.android.com/apk/res/android:name(0x01010003)="android.permission.MANAGE_EXTERNAL_STORAGE" (Raw: "android.permission.MANAGE_EXTERNAL_STORAGE")
              E: application (line=28)
                A: http://schemas.android.com/apk/res/android:label(0x01010001)="Planör" (Raw: "Planör")
                A: http://schemas.android.com/apk/res/android:allowBackup(0x01010280)=false
        """;

    public const string Csproj = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <ApplicationTitle>Planör</ApplicationTitle>
            <ApplicationId>com.mizan.app</ApplicationId>
            <ApplicationDisplayVersion>0.1.0</ApplicationDisplayVersion>
            <ApplicationVersion>1</ApplicationVersion>
          </PropertyGroup>
        </Project>
        """;

    public static ApkEvidence PrEvidence() => new(Badging, DebugCertificates, Manifest, SignatureVerified: true);

    public static ApkEvidence ReleaseEvidence() => PrEvidence() with { Certificates = ReleaseCertificates };

    public static ApkExpectation PrExpectation() =>
        new(new ApkIdentity("com.mizan.app", "Planör", "1", "0.1.0"), TagVersion: null, RequireReleaseSignature: false);

    public static ApkExpectation ReleaseExpectation() =>
        PrExpectation() with { TagVersion = "0.1.0", RequireReleaseSignature = true };
}
