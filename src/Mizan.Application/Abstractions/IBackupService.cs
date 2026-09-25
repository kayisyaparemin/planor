using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Yedekleme, geri yükleme ve yedekten profil içe aktarma kullanım senaryolarını yöneten servis portu.
/// Ayarlar ekranının ve arka plan gece yedekleme görevinin somut servise bağımlı olmadan çalışabilmesi için vardır.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Harici yedek klasörüne yazma ve okuma izninin verilip verilmediği.
    /// </summary>
    bool HasAccess { get; }

    /// <summary>
    /// Kullanıcıya gösterilen klasör konumu açıklaması.
    /// </summary>
    string LocationDescription { get; }

    /// <summary>
    /// Depolama klasörü erişim iznini kullanıcıdan talep eder.
    /// </summary>
    Task<bool> RequestAccessAsync();

    /// <summary>
    /// En son alınan yedek durumunu döner.
    /// </summary>
    Task<BackupState?> GetLastBackupAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gece görevi: Son yedekten beri hiçbir profil değişmediyse dosya yazmaz; değişiklik varsa bugünün yedeğini alır.
    /// </summary>
    Task<BackupResult> BackUpIfChangedAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının açık talebiyle ("Şimdi Yedekle") bugünün yedeğini hemen alır veya üzerine yazar.
    /// </summary>
    Task<BackupResult> BackUpNowAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Depodaki Mizan yedek dosyalarını en yenisi en başta olacak şekilde listeler.
    /// </summary>
    Task<IReadOnlyList<StoredBackup>> ListBackupsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yedek akışının başlığını ve profil özetini doğrulamak üzere okur.
    /// </summary>
    Task<BackupSummary> ReadBackupAsync(
        Stream source,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Verilen akıştan hiç profil yokken tüm profilleri orijinal kimlik ve adlarıyla geri yükler.
    /// </summary>
    Task<BackupSummary> RestoreAsync(
        Stream source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut profiller korunarak yedekten seçilen profilleri sisteme ekler; kimlik çakışmasında kopya oluşturur.
    /// </summary>
    Task<BackupImportResult> AddFromBackupAsync(
        Stream source,
        IReadOnlyCollection<Guid>? profileIds,
        CancellationToken cancellationToken = default);
}
