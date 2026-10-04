using System.Text.RegularExpressions;

namespace Mizan.ApkVerifier;

/// <summary>
/// APK'nın kimliğini, imzasını ve çevrimdışılığını denetleyen saf kurallar (S80, S60, I40).
/// Okunamayan ya da boş değer daima ihlaldir: eskide <c>sed</c> boş dönünce kontrol sessizce geçebilirdi.
/// </summary>
public static partial class ApkRules
{
    private static readonly string[] ForbiddenPermissions =
    [
        "android.permission.INTERNET",
        "android.permission.ACCESS_NETWORK_STATE"
    ];

    /// <summary>Bütün kontrolleri çalıştırır; ilk ihlalde durmaz.</summary>
    /// <param name="evidence">Araçlardan toplanan kanıt.</param>
    /// <param name="expectation">Beklenen kimlik.</param>
    /// <returns>Her kontrol için bir sonuç.</returns>
    public static IReadOnlyList<ApkCheckResult> Check(ApkEvidence evidence, ApkExpectation expectation)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(expectation);

        var package = PackageLine().Match(evidence.Badging);
        var identity = expectation.Identity;
        var results = new List<ApkCheckResult>
        {
            new("İmza geçerli", evidence.SignatureVerified,
                evidence.SignatureVerified ? "apksigner verify geçti" : "apksigner verify başarısız"),
            CheckReleaseSignature(evidence.Certificates, expectation.RequireReleaseSignature),
            Compare("Paket kimliği", Group(package, "id"), identity.PackageId),
            Compare("versionCode", Group(package, "code"), identity.VersionCode),
            Compare("versionName", Group(package, "name"), identity.VersionName),
            Compare("Uygulama etiketi", Group(LabelLine().Match(evidence.Badging), "label"), identity.Label),
            CheckNoNetworkPermission(evidence.Badging, package.Success),
            CheckBackupDisabled(evidence.Manifest)
        };

        if (expectation.TagVersion is not null)
        {
            results.Add(Compare("Sürüm etiketi", expectation.TagVersion, identity.VersionName));
        }

        return results;
    }

    private static ApkCheckResult CheckReleaseSignature(string certificates, bool required)
    {
        const string name = "Yayın imzası";
        if (!required)
        {
            return new(name, true, "istenmedi (PR derlemesi debug imzalıdır)");
        }

        var subject = CertificateSubject().Match(certificates);
        if (!subject.Success)
        {
            return new(name, false, "sertifika okunamadı");
        }

        var dn = subject.Groups["dn"].Value.Trim();
        var isDebug = dn.Contains("CN=Android Debug", StringComparison.OrdinalIgnoreCase);
        return new(name, !isDebug, isDebug ? $"debug sertifikası reddedildi ({dn})" : dn);
    }

    private static ApkCheckResult CheckNoNetworkPermission(string badging, bool badgingReadable)
    {
        const string name = "Ağ izni yok";
        if (!badgingReadable)
        {
            return new(name, false, "badging okunamadı; izinler doğrulanamadı");
        }

        var found = PermissionLine().Matches(badging)
            .Select(m => m.Groups["perm"].Value)
            .Where(p => ForbiddenPermissions.Contains(p, StringComparer.Ordinal))
            .ToList();

        return found.Count == 0
            ? new(name, true, "INTERNET ve ACCESS_NETWORK_STATE yok")
            : new(name, false, $"birleşmiş manifestte yasak izin: {string.Join(", ", found)} (S60, I40)");
    }

    private static ApkCheckResult CheckBackupDisabled(string manifest)
    {
        const string name = "Bulut yedeği kapalı";
        var match = AllowBackupAttribute().Match(manifest);
        if (!match.Success)
        {
            // Öznitelik yoksa Android varsayılanı "true"dur.
            return new(name, false, "allowBackup özniteliği yok; Android varsayılanı açıktır (I40)");
        }

        var value = match.Groups["value"].Value;
        var disabled = string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        return new(name, disabled, disabled ? "allowBackup=false" : $"allowBackup={value} (I40)");
    }

    private static ApkCheckResult Compare(string name, string actual, string expected)
    {
        if (string.IsNullOrWhiteSpace(actual))
        {
            return new(name, false, $"okunamadı (beklenen '{expected}')");
        }

        var equal = string.Equals(actual, expected, StringComparison.Ordinal);
        return new(name, equal, equal ? actual : $"'{actual}' bulundu, '{expected}' bekleniyordu");
    }

    private static string Group(Match match, string group) => match.Success ? match.Groups[group].Value : string.Empty;

    [GeneratedRegex(@"^package: name='(?<id>[^']*)' versionCode='(?<code>[^']*)' versionName='(?<name>[^']*)'", RegexOptions.Multiline)]
    private static partial Regex PackageLine();

    [GeneratedRegex(@"^application-label:'(?<label>[^']*)'", RegexOptions.Multiline)]
    private static partial Regex LabelLine();

    // <uses-permission-sdk-23> da aynı izni verir; bir kütüphane interneti o yoldan da getirebilir.
    [GeneratedRegex(@"^uses-permission(?:-sdk-23)?: name='(?<perm>[^']*)'", RegexOptions.Multiline)]
    private static partial Regex PermissionLine();

    [GeneratedRegex(@":allowBackup\(0x01010280\)=(?<value>\w+)")]
    private static partial Regex AllowBackupAttribute();

    [GeneratedRegex(@"certificate DN: (?<dn>.+)$", RegexOptions.Multiline)]
    private static partial Regex CertificateSubject();
}
