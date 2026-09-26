using Mizan.Application.Models;

namespace Mizan.Presentation.Services;

/// <summary>
/// Profil seçimi ekranında yedekten geri yükleme ve içe aktarma kullanıcı akışını yöneten servis portu.
/// </summary>
public interface IProfileBackupHandler
{
    /// <summary>
    /// Kullanıcıya yedek seçtirerek tüm profilleri geri yükler ve sonucu bildirir; iptalde null döner.
    /// </summary>
    Task<BackupSummary?> RestoreAsync();

    /// <summary>
    /// Kullanıcıya yedek seçtirerek profilleri mevcutların yanına ekler ve sonucu bildirir; iptalde null döner.
    /// </summary>
    Task<BackupImportResult?> AddAsync();
}
