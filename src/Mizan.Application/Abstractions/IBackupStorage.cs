using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Yedek dosyalarının uygulama dışındaki kalıcı fiziksel depolama portu.
/// Uygulama kaldırıldığında silinmeyen harici klasör erişimini soyutlamak için vardır.
/// </summary>
public interface IBackupStorage
{
    /// <summary>
    /// Kullanıcıya gösterilen klasör konumu açıklaması (örn. "Dahili depolama › Mizan").
    /// </summary>
    string LocationDescription { get; }

    /// <summary>
    /// Harici yedek klasörüne yazma ve okuma izninin verilip verilmediği.
    /// </summary>
    bool HasAccess { get; }

    /// <summary>
    /// Depolama klasörü erişim iznini kullanıcıdan talep eder ve iznin verilip verilmediğini döner.
    /// </summary>
    Task<bool> RequestAccessAsync();

    /// <summary>
    /// Belirtilen kaynak yoldaki geçici yedek dosyasını harici depoya kopyalar; aynı adlı dosyanın üzerine yazar.
    /// </summary>
    Task SaveAsync(
        string fileName,
        string sourcePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Depodaki tüm dosyaları listeler.
    /// </summary>
    Task<IReadOnlyList<StoredBackup>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen yedek dosyasını salt-okunur akış olarak açar.
    /// </summary>
    Task<Stream> OpenReadAsync(
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen yedek dosyasını depodan siler.
    /// </summary>
    Task DeleteAsync(
        string fileName,
        CancellationToken cancellationToken = default);
}
