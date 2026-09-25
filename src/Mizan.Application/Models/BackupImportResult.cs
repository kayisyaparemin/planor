namespace Mizan.Application.Models;

/// <summary>
/// Yedekten profil ekleme operasyonunun sonucunu bildiren kayıt.
/// Arşiv özeti ve sisteme yeni eklenen profilleri çağıran tarafa sunmak için vardır.
/// </summary>
public sealed record BackupImportResult(
    BackupSummary Backup,
    IReadOnlyList<ImportedProfile> Added);
