using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Infrastructure.Persistence;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Telefondaki bütün profilleri tek bir yedek zip'ine yazan arşiv. Uygulama kaldırılınca
/// profillerin veritabanları da silindiği için vardır. Yedek dosyası bir zip'tir:
/// <c>mizan-backup.json</c> (biçim, tarih, şema sürümü, profiller) ve verisi olan her profil için
/// <c>profiles/{id}/mizan.db3</c>. Disk yerleşimini somut depodan değil
/// <see cref="IProfileFileLayout"/>'tan öğrenir (S55).
/// </summary>
/// <remarks>
/// Bu adımda (I4a) yalnız yedek alma yarısı vardır. <see cref="IProfileBackupArchive"/> sözleşmesini
/// geri yükleme yarısı (I4b) tamamlandığında üstlenir.
/// </remarks>
public sealed class ProfileBackupArchive(
    IProfileRepository profiles,
    IProfileFileLayout layout,
    IClock clock)
{
    private readonly IProfileRepository _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
    private readonly IProfileFileLayout _layout = layout ?? throw new ArgumentNullException(nameof(layout));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// Bütün profillerin içeriğinden, veri değişmedikçe aynı kalan bir SHA-256 parmak izi (64 onaltılık
    /// karakter) hesaplar. Profil adı ve veritabanı içeriği girer; son açılış tarihi ve dosya zamanı girmez.
    /// </summary>
    public async Task<string> ComputeFingerprintAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _profiles.GetProfilesAsync(cancellationToken);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var profile in profiles.OrderBy(profile => profile.Id))
        {
            cancellationToken.ThrowIfCancellationRequested();
            DatabaseContentFingerprint.AppendProfile(hash, profile);
            var databasePath = _layout.GetDatabasePath(profile.Id);
            if (File.Exists(databasePath))
            {
                DatabaseContentFingerprint.AppendDatabase(hash, databasePath);
            }
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>
    /// Bütün profilleri oluşturulma sırasıyla hedef akışa yedek zip'i olarak yazar ve arşivin özetini döner.
    /// Açık bir profil o anda yazıyor olsa da her veritabanının tutarlı anlık görüntüsü alınır.
    /// </summary>
    public async Task<BackupSummary> WriteAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var profiles = await _profiles.GetProfilesAsync(cancellationToken);

        // Anlık görüntüler uygulama klasöründe hazırlanır; yarıda kalsa da geride iz bırakmaz.
        var workDirectory = Path.Combine(_layout.RootDirectory, $".backup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workDirectory);
        try
        {
            using var zip = new ZipArchive(destination, ZipArchiveMode.Create, leaveOpen: true);
            var manifestProfiles = new List<BackupManifestProfile>();
            foreach (var profile in profiles.OrderBy(profile => profile.CreatedAt))
            {
                cancellationToken.ThrowIfCancellationRequested();
                manifestProfiles.Add(WriteProfileEntry(zip, profile, workDirectory));
            }

            var manifest = new BackupManifest(
                BackupArchiveFormat.Version,
                _clock.UtcNow,
                DatabaseConstants.CurrentSchemaVersion,
                manifestProfiles);
            await WriteManifestAsync(zip, manifest, cancellationToken);
            return Summary(manifest);
        }
        finally
        {
            TryDeleteDirectory(workDirectory);
        }
    }

    /// <summary>
    /// Son başarılı yedeğin kaydını getirir; kayıt yoksa ya da bozuksa <see langword="null"/> döner.
    /// </summary>
    public Task<BackupState?> GetStateAsync(CancellationToken cancellationToken = default) =>
        BackupStateFile.ReadAsync(StatePath(), cancellationToken);

    /// <summary>
    /// Başarıyla tamamlanan yedeğin kaydını uygulama veri kökünde saklar.
    /// </summary>
    public Task SaveStateAsync(BackupState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        return BackupStateFile.WriteAsync(StatePath(), state, cancellationToken);
    }

    private BackupManifestProfile WriteProfileEntry(ZipArchive zip, UserProfile profile, string workDirectory)
    {
        // Hiç açılmamış profilin veritabanı yoktur; manifeste verisiz girer.
        var databasePath = _layout.GetDatabasePath(profile.Id);
        var hasData = File.Exists(databasePath);
        if (hasData)
        {
            var snapshotPath = Path.Combine(workDirectory, $"{profile.Id:N}.db3");
            SqliteDatabaseSnapshot.Create(databasePath, snapshotPath);
            zip.CreateEntryFromFile(
                snapshotPath,
                BackupArchiveFormat.DatabaseEntryName(profile.Id),
                CompressionLevel.Optimal);
        }

        return new BackupManifestProfile(profile.Id, profile.Name, profile.CreatedAt, profile.LastOpenedAt, hasData);
    }

    private static async Task WriteManifestAsync(
        ZipArchive zip,
        BackupManifest manifest,
        CancellationToken cancellationToken)
    {
        var entry = zip.CreateEntry(BackupArchiveFormat.ManifestEntryName, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await JsonSerializer.SerializeAsync(
            stream,
            manifest,
            BackupJsonContext.Default.BackupManifest,
            cancellationToken);
    }

    private static BackupSummary Summary(BackupManifest manifest) =>
        new(
            manifest.CreatedAt,
            manifest.Profiles
                .Select(profile => new BackupProfile(profile.Id, profile.Name, profile.CreatedAt, profile.LastOpenedAt))
                .ToArray());

    private string StatePath() =>
        Path.Combine(_layout.RootDirectory, BackupStateFile.FileName);

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Temizlik en iyi çabadır; yedeğin sonucunu değiştirmez.
        }
        catch (UnauthorizedAccessException)
        {
            // Temizlik en iyi çabadır; yedeğin sonucunu değiştirmez.
        }
    }
}
