using System.Xml.Linq;

namespace Mizan.Architecture.Tests;

/// <summary>
/// Mizan'ın çevrimdışılık sözünü (I40, S60) denetler: uygulama ağ izni istemez, verisini
/// Android'in bulut yedeğine vermez ve internet iznini gizlice getiren Sentry paketine başvurmaz.
/// Eski uygulamada internet izni manifestte yazılı değildi; Sentry'nin Android kütüphanesi onu
/// derlemede kendiliğinden ekliyordu. Bu yüzden hem manifest hem paket listesi denetlenir.
/// </summary>
internal static class OfflineRules
{
    private static readonly XNamespace AndroidNamespace = "http://schemas.android.com/apk/res/android";

    private static readonly string[] ForbiddenPermissions =
    [
        "android.permission.INTERNET",
        "android.permission.ACCESS_NETWORK_STATE"
    ];

    public static IReadOnlyList<string> CheckAndroidManifest()
    {
        var manifestPath = Path.Combine(
            SolutionPaths.SourceDirectory, "Mizan.App", "Platforms", "Android", "AndroidManifest.xml");

        return CheckManifestContent(XDocument.Load(manifestPath));
    }

    public static IReadOnlyList<string> CheckManifestContent(XDocument manifest)
    {
        var violations = new List<string>();
        var permissions = manifest.Descendants("uses-permission")
            .Select(element => (string?)element.Attribute(AndroidNamespace + "name"));

        foreach (var permission in permissions.Where(name => ForbiddenPermissions.Contains(name)))
        {
            violations.Add($"AndroidManifest.xml '{permission}' izni isteyemez: Mizan çevrimdışıdır (I40).");
        }

        // Öznitelik yoksa Android varsayılanı "true"dur; bu yüzden yalnız açık "false" kabul edilir.
        var application = manifest.Descendants("application").SingleOrDefault();
        var allowBackup = (string?)application?.Attribute(AndroidNamespace + "allowBackup");
        if (!string.Equals(allowBackup, "false", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add("AndroidManifest.xml android:allowBackup=\"false\" olmalı: profil veritabanları bulut yedeğine gidemez (I40).");
        }

        return violations;
    }

    public static IReadOnlyList<string> CheckNoSentryPackage()
    {
        var projectFiles = Directory.GetFiles(SolutionPaths.SourceDirectory, "*.csproj", SearchOption.AllDirectories)
            .Append(Path.Combine(SolutionPaths.Root, "Directory.Packages.props"));

        return projectFiles
            .SelectMany(file => CheckPackageContent(Path.GetFileName(file), File.ReadAllText(file)))
            .ToList();
    }

    public static IReadOnlyList<string> CheckPackageContent(string fileName, string content)
    {
        var lines = content.Split('\n');
        var violations = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Include=\"Sentry", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add($"{fileName} (Satır {i + 1}): Sentry paketine başvurulamaz (S60).");
            }
        }

        return violations;
    }
}
