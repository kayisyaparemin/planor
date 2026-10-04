using System.IO.Compression;
using Mizan.Application.Models;
using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Persistence;

namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski yedekten seçilen profilleri telefondaki hiçbir şeye dokunmadan hazırlık klasöründe hazırlar: her
/// profilin eski veritabanını zip'ten çıkarır ve güncel şemada yeni bir veritabanına çevirir. Hepsi
/// hazırlanmadan hiçbiri yerine taşınmaz; taşıma <see cref="ProfileImportTransaction"/>'ın ortak hep-ya-hiç
/// yolundan geçer (S58).
/// </summary>
internal static class LegacyProfileStager
{
    /// <summary>
    /// Bir profilin eski veritabanının açılmış hâlinin üst sınırı; zip'in bildirdiği boyut bunu aşarsa dosya
    /// diske hiç açılmaz.
    /// </summary>
    private const long MaxDatabaseBytes = 512L * 1024 * 1024;

    public static async Task<IReadOnlyList<ProfileImportTransaction.StagedProfile>> StageAsync(
        Stream source,
        IReadOnlyList<ProfileImport> imports,
        string staging,
        DatabaseSchema schema,
        CancellationToken cancellationToken)
    {
        using var zip = BackupManifestReader.Open(source);
        var manifest = await BackupManifestReader.ReadLegacyAsync(zip, cancellationToken);
        var staged = new List<ProfileImportTransaction.StagedProfile>(imports.Count);
        foreach (var import in imports)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var profile = manifest.Profiles.FirstOrDefault(candidate => candidate.Id == import.SourceId) ??
                          throw BackupRestoreErrors.Corrupt("Seçilen profil yedekte yok.");
            var database = profile.HasData
                ? await ConvertAsync(zip, profile, import.Target.Id, staging, schema, cancellationToken)
                : null;
            staged.Add(new ProfileImportTransaction.StagedProfile(import.Target, database));
        }

        return staged;
    }

    private static async Task<string> ConvertAsync(
        ZipArchive zip,
        BackupManifestProfile profile,
        Guid targetId,
        string staging,
        DatabaseSchema schema,
        CancellationToken cancellationToken)
    {
        var legacyPath = Path.Combine(staging, $"{targetId:N}.eski.db3");
        var targetPath = Path.Combine(staging, $"{targetId:N}.db3");
        Extract(zip, profile, legacyPath);
        await LegacyDatabaseConverter.ConvertAsync(legacyPath, targetPath, schema, profile.Name, cancellationToken);
        return targetPath;
    }

    private static void Extract(ZipArchive zip, BackupManifestProfile profile, string targetPath)
    {
        var entry = zip.GetEntry(LegacySchemaV17.DatabaseEntryName(profile.Id)) ??
                    throw BackupRestoreErrors.Corrupt($"\"{profile.Name}\" profilinin verisi yedekte yok.");
        if (entry.Length > MaxDatabaseBytes)
        {
            throw BackupRestoreErrors.Corrupt($"\"{profile.Name}\" profilinin verisi beklenenden büyük.");
        }

        try
        {
            entry.ExtractToFile(targetPath);
        }
        catch (InvalidDataException exception)
        {
            throw BackupRestoreErrors.Corrupt($"\"{profile.Name}\" profilinin verisi okunamadı.", exception);
        }
    }
}
