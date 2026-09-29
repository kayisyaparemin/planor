using System.IO.Compression;
using SQLite;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedekteki bir profil veritabanını hazırlık klasörüne çıkarır ve geri yüklenebilir olduğunu denetler:
/// boyut, bütünlük ve şema sürümü. Sürümü 0 olan bir veritabanı açılışta "yeni" sanılıp boş profil
/// gibi kurulacağı için reddedilir (S58).
/// </summary>
internal static class BackupDatabaseValidator
{
    /// <summary>
    /// Tek bir profil veritabanının açılmış hâlinin üst sınırı; zip'in bildirdiği boyut bunu aşarsa
    /// dosya diske hiç açılmaz.
    /// </summary>
    private const long MaxDatabaseBytes = 512L * 1024 * 1024;

    /// <summary>
    /// Profilin veritabanı girdisini <paramref name="targetPath"/>'e çıkarır ve denetler; geri yüklenemiyorsa
    /// kullanıcıya gidecek mesajla <see cref="InvalidOperationException"/> fırlatır. Sürümü
    /// <paramref name="currentVersion"/>'dan eski olan veritabanı kabul edilir; onu çağıran yükseltir (S69).
    /// </summary>
    public static void ExtractAndValidate(
        ZipArchive zip,
        BackupManifestProfile profile,
        string targetPath,
        int currentVersion)
    {
        Extract(zip, profile, targetPath);
        var version = ReadVerifiedVersion(targetPath, profile.Name);
        if (version > currentVersion)
        {
            throw BackupRestoreErrors.NewerVersion();
        }

        if (version < 1)
        {
            throw BackupRestoreErrors.Corrupt($"\"{profile.Name}\" profilinin verisi tanınmıyor.");
        }
    }

    private static void Extract(ZipArchive zip, BackupManifestProfile profile, string targetPath)
    {
        var entry = zip.GetEntry(BackupArchiveFormat.DatabaseEntryName(profile.Id)) ??
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

    /// <summary>
    /// Bütünlük denetiminden geçen veritabanının <c>user_version</c>'ını döner.
    /// </summary>
    private static int ReadVerifiedVersion(string path, string profileName)
    {
        try
        {
            using var connection = new SQLiteConnection(path, SQLiteOpenFlags.ReadOnly);
            var check = connection.ExecuteScalar<string>("PRAGMA quick_check;");
            if (!string.Equals(check, "ok", StringComparison.OrdinalIgnoreCase))
            {
                throw BackupRestoreErrors.Corrupt($"\"{profileName}\" profilinin verisi bozuk.");
            }

            return connection.ExecuteScalar<int>("PRAGMA user_version;");
        }
        catch (SQLiteException exception)
        {
            throw BackupRestoreErrors.Corrupt($"\"{profileName}\" profilinin verisi okunamadı.", exception);
        }
    }
}
