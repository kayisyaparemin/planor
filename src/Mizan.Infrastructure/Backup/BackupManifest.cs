namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedek zip'inin başındaki içindekiler listesi. Geri yüklemede veritabanlarına dokunmadan
/// önce "bu dosya ne, hangi sürümden, içinde kimler var" sorularını cevaplamak için vardır.
/// </summary>
internal sealed record BackupManifest(
    int Format,
    DateTimeOffset CreatedAt,
    int SchemaVersion,
    IReadOnlyList<BackupManifestProfile> Profiles);
