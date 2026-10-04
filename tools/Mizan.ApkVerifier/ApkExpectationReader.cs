using System.Xml.Linq;

namespace Mizan.ApkVerifier;

/// <summary>Beklenen APK kimliğini <c>Mizan.App.csproj</c>'dan okur (S80-5: sürüm otoritesi csproj'dur).</summary>
public static class ApkExpectationReader
{
    /// <summary>csproj içeriğinden beklentiyi kurar.</summary>
    /// <param name="csprojXml">csproj dosyasının metni.</param>
    /// <param name="tagVersion">Yayın etiketinden gelen sürüm (<c>v</c> öneki kabul edilir) ya da <see langword="null"/>.</param>
    /// <param name="requireReleaseSignature">Debug imza reddedilsin mi.</param>
    /// <returns>Beklenen kimlik.</returns>
    /// <exception cref="InvalidDataException">Zorunlu bir özellik csproj'da yoksa ya da boşsa.</exception>
    public static ApkExpectation FromCsproj(string csprojXml, string? tagVersion, bool requireReleaseSignature)
    {
        ArgumentNullException.ThrowIfNull(csprojXml);
        var document = XDocument.Parse(csprojXml);

        var identity = new ApkIdentity(
            Required(document, "ApplicationId"),
            Required(document, "ApplicationTitle"),
            Required(document, "ApplicationVersion"),
            Required(document, "ApplicationDisplayVersion"));

        return new ApkExpectation(identity, tagVersion?.TrimStart('v', 'V'), requireReleaseSignature);
    }

    private static string Required(XDocument document, string property)
    {
        var value = document.Descendants(property).Select(e => e.Value.Trim()).FirstOrDefault(v => v.Length > 0);
        return value ?? throw new InvalidDataException($"Mizan.App.csproj '{property}' özelliğini taşımıyor; beklenen APK kimliği kurulamaz.");
    }
}
