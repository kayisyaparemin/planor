using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi kullanıcı ayarları deposu implementasyonu.
/// </summary>
public sealed class InMemoryUserSettingsRepository : IUserSettingsRepository
{
    private UserSettings _settings = new();

    public Task<UserSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_settings);
    }

    public Task SaveSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _settings = settings;
        return Task.CompletedTask;
    }
}
