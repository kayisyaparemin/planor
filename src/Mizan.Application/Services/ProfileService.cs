using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Profil yaşam döngüsünü (oluşturma, ad değiştirme, silme, oturum açma ve kapatma)
/// yöneten kullanım senaryosu servisi.
/// </summary>
public sealed class ProfileService(
    IProfileRepository repository,
    IProfileStoreSwitch storeSwitch,
    IClock clock) : IDisposable
{
    private readonly IProfileRepository _repository = repository;
    private readonly IProfileStoreSwitch _storeSwitch = storeSwitch;
    private readonly IClock _clock = clock;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// O an açık olan profil; hiçbir profil açılmadıysa veya kapatıldıysa <c>null</c>.
    /// </summary>
    public UserProfile? ActiveProfile { get; private set; }

    /// <summary>
    /// Profil her açıldığında yenilenen oturum kimliği.
    /// </summary>
    public Guid SessionId { get; private set; }

    /// <summary>
    /// Profilleri oluşturulma sırasıyla döner.
    /// </summary>
    public async Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var profiles = await _repository.GetProfilesAsync(cancellationToken);
            return Ordered(profiles);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Belirtilen adla yeni bir profil oluşturur ve kaydeder.
    /// </summary>
    public async Task<UserProfile> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var profiles = await _repository.GetProfilesAsync(cancellationToken);
            var validatedName = ProfileNameValidator.Validate(name, profiles, exceptId: null);
            var profile = new UserProfile
            {
                Name = validatedName,
                CreatedAt = _clock.UtcNow
            };

            await _repository.SaveProfileAsync(profile, cancellationToken);
            return profile;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Mevcut bir profilin adını günceller.
    /// </summary>
    public async Task<UserProfile> RenameAsync(Guid profileId, string name, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var profiles = await _repository.GetProfilesAsync(cancellationToken);
            var existing = Find(profiles, profileId);
            var validatedName = ProfileNameValidator.Validate(name, profiles, exceptId: profileId);
            var updated = existing with { Name = validatedName };

            await _repository.SaveProfileAsync(updated, cancellationToken);
            if (ActiveProfile?.Id == profileId)
            {
                ActiveProfile = updated;
            }

            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Belirtilen profili kalıcı olarak siler; açık olan veya son kalan profil silinemez.
    /// </summary>
    public async Task DeleteAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var profiles = await _repository.GetProfilesAsync(cancellationToken);
            Find(profiles, profileId);

            if (ActiveProfile?.Id == profileId)
            {
                throw new InvalidOperationException("Açık olan profil silinemez. Önce profil seçim ekranına dön.");
            }

            if (profiles.Count == 1)
            {
                throw new InvalidOperationException("Son kalan profil silinemez.");
            }

            await _repository.DeleteProfileAsync(profileId, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Belirtilen profili açar, son açılış zamanını günceller ve yeni bir oturum başlatır.
    /// </summary>
    public async Task<UserProfile> OpenAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var profiles = await _repository.GetProfilesAsync(cancellationToken);
            var existing = Find(profiles, profileId);
            var updated = existing with { LastOpenedAt = _clock.UtcNow };

            ActiveProfile = null;
            await _storeSwitch.OpenAsync(profileId, cancellationToken);
            await _repository.SaveProfileAsync(updated, cancellationToken);
            ActiveProfile = updated;
            SessionId = Guid.NewGuid();

            return updated;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Açık olan profili kapatır ve veri deposu bağlantısını keser.
    /// </summary>
    public async Task CloseAsync()
    {
        await _lock.WaitAsync();
        try
        {
            ActiveProfile = null;
            await _storeSwitch.CloseAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Kilit kaynağını serbest bırakır.
    /// </summary>
    public void Dispose()
    {
        _lock.Dispose();
    }

    private static UserProfile[] Ordered(IEnumerable<UserProfile> profiles) =>
        profiles
            .OrderBy(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .ToArray();

    private static UserProfile Find(IEnumerable<UserProfile> profiles, Guid profileId) =>
        profiles.FirstOrDefault(p => p.Id == profileId) ??
        throw new InvalidOperationException("Profil bulunamadı.");
}
