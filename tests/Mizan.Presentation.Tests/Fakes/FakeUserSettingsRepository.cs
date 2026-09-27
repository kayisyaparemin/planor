using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

internal sealed class FakeUserSettingsRepository : IUserSettingsRepository
{
    public UserSettings Settings { get; set; } = new();

    public Task<UserSettings> GetSettingsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Settings);

    public Task SaveSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        Settings = settings;
        return Task.CompletedTask;
    }
}
