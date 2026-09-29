using System.IO.Compression;
using System.Text.Json;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedek zip'ini açar ve manifestini okuyup denetler. Geri yüklemede veritabanlarına dokunmadan önce
/// "bu dosya ne, hangi sürümden, içinde kimler var" sorularının cevabı buradan çıkar; özet okuma ile
/// geri yükleme aynı denetimden geçsin diye tek yerde durur.
/// </summary>
internal static class BackupManifestReader
{
    /// <summary>
    /// Kaynak akışı okuma kipinde zip olarak açar; akış kapatılmaz, çağıran onu başa sarıp yeniden okuyabilir.
    /// </summary>
    public static ZipArchive Open(Stream source)
    {
        try
        {
            return new ZipArchive(source, ZipArchiveMode.Read, leaveOpen: true);
        }
        catch (InvalidDataException exception)
        {
            throw BackupRestoreErrors.NotABackup(exception);
        }
    }

    /// <summary>
    /// Manifesti okur ve denetler: biçim (eski uygulamanın yedeği şema sürümünden önce tanınır),
    /// şema sürümü (<paramref name="currentVersion"/>'dan yeni olamaz) ve profil listesi.
    /// </summary>
    public static async Task<BackupManifest> ReadAsync(
        ZipArchive zip,
        int currentVersion,
        CancellationToken cancellationToken)
    {
        var manifest = await DeserializeAsync(zip, cancellationToken);
        EnsureKnownVersion(manifest, currentVersion);
        EnsureValidProfiles(manifest.Profiles);
        return manifest;
    }

    private static async Task<BackupManifest> DeserializeAsync(ZipArchive zip, CancellationToken cancellationToken)
    {
        var entry = zip.GetEntry(BackupArchiveFormat.ManifestEntryName) ?? throw BackupRestoreErrors.NotABackup();
        BackupManifest? manifest;
        try
        {
            await using var stream = entry.Open();
            manifest = await JsonSerializer.DeserializeAsync(
                stream,
                BackupJsonContext.Default.BackupManifest,
                cancellationToken);
        }
        catch (Exception exception) when (exception is JsonException or InvalidDataException)
        {
            throw BackupRestoreErrors.NotABackup(exception);
        }

        // Biçim alanı olmayan (0) ya da profil listesi olmayan bir JSON, Mizan'ın yazdığı bir manifest değildir.
        return manifest is { Format: > 0, Profiles: not null } ? manifest : throw BackupRestoreErrors.NotABackup();
    }

    private static void EnsureKnownVersion(BackupManifest manifest, int currentVersion)
    {
        // Eski uygulamanın yedeği şema v17 taşır; biçime önce bakılmazsa "daha yeni sürüm" denirdi (S57).
        if (manifest.Format == BackupArchiveFormat.LegacyVersion)
        {
            throw BackupRestoreErrors.LegacyBackup();
        }

        if (manifest.Format > BackupArchiveFormat.Version ||
            manifest.SchemaVersion > currentVersion)
        {
            throw BackupRestoreErrors.NewerVersion();
        }
    }

    private static void EnsureValidProfiles(IReadOnlyList<BackupManifestProfile> profiles)
    {
        if (profiles.Count == 0)
        {
            throw BackupRestoreErrors.Corrupt("Yedekte hiç profil yok.");
        }

        var broken = profiles.Any(profile => profile.Id == Guid.Empty || string.IsNullOrWhiteSpace(profile.Name));
        if (broken || profiles.Select(profile => profile.Id).Distinct().Count() != profiles.Count)
        {
            throw BackupRestoreErrors.Corrupt("Yedekteki profil listesi bozuk.");
        }
    }
}
