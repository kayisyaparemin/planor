using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Bütün profilleri tek bir yedek dosyasına yazan ve geri açan arşiv portu.
/// Uygulama veritabanlarının fiziksel disk yerleşimini ve zip arşiv biçimini soyutlamak için vardır.
/// </summary>
public interface IProfileBackupArchive
{
    /// <summary>
    /// Profillerin verisi değişmediyse aynı kalan özet parmak izini hesaplar.
    /// Gece yedekleme görevinin değişiklik yoksa yeni dosya yazmama kararını desteklemek için vardır.
    /// </summary>
    Task<string> ComputeFingerprintAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bütün profilleri yedek arşiv biçiminde hedef akışa yazar ve arşiv özetini döner.
    /// </summary>
    Task<BackupSummary> WriteAsync(
        Stream destination,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yedek akışının başlığını ve içindeki profilleri okur; dosya geçerli bir Mizan yedeği değilse hata verir.
    /// </summary>
    Task<BackupSummary> ReadSummaryAsync(
        Stream source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Seçilen profilleri doğrular ve belirtilen hedef kimlik/ad ile sisteme geri yükler.
    /// </summary>
    Task ImportAsync(
        Stream source,
        IReadOnlyList<ProfileImport> profileImports,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydedilmiş en son yedek durumunu getirir.
    /// </summary>
    Task<BackupState?> GetStateAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Başarıyla tamamlanan yedeğin durumunu saklar.
    /// </summary>
    Task SaveStateAsync(
        BackupState state,
        CancellationToken cancellationToken = default);
}
