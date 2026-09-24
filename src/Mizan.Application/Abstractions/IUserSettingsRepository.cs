using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Dönem çapa günü, serbest harcama havuzu ve varsayılan faiz oranları gibi kullanıcı bazlı
/// finansal planlama parametrelerinin kalıcı veri deposuna erişimini sağlayan dar port arayüzü.
/// </summary>
public interface IUserSettingsRepository
{
    /// <summary>
    /// Kullanıcının mevcut finansal planlama ayarlarını getirir.
    /// </summary>
    Task<UserSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının finansal planlama ayarlarını kalıcı olarak kaydeder veya günceller.
    /// </summary>
    Task SaveSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
