namespace Mizan.Application.Abstractions;

/// <summary>
/// Uygulamanın veri deposunu o an açık olan profile bağlayan veya profil kapatıldığında
/// erişimi kesen port. Veri izolasyonunu fiziksel veritabanı düzeyinde garanti eder.
/// </summary>
public interface IProfileStoreSwitch
{
    /// <summary>
    /// Belirtilen profile ait veritabanını açar ve uygulama depolarını bu profile yönlendirir.
    /// </summary>
    Task OpenAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Açık profilin veritabanı bağlantısını kapatır. Kapalıyken depolara yapılan çağrılar hata üretir.
    /// </summary>
    Task CloseAsync();
}
