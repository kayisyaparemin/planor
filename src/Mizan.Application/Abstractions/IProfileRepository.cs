using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcı profillerinin meta bilgilerini listeleyen, kaydeden ve silen
/// veri deposu portu. Çok kiracılı yapıda hangi profillerin mevcut olduğunu belirler.
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Kayıtlı tüm kullanıcı profillerini getirir.
    /// </summary>
    Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir profili kaydeder veya mevcut profilin meta bilgilerini günceller.
    /// </summary>
    Task SaveProfileAsync(UserProfile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen profile ait meta bilgisini ve fiziksel veritabanını kalıcı olarak siler.
    /// </summary>
    Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}
