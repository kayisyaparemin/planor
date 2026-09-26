using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde profil verilerini bellek içinde saklayan sahte profil deposu.
/// </summary>
public sealed class FakeProfileRepository : IProfileRepository
{
    private readonly List<UserProfile> _profiles = [];

    public IReadOnlyList<UserProfile> Items => _profiles.AsReadOnly();

    public Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<UserProfile>>(_profiles.ToList());
    }

    public Task SaveProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        var index = _profiles.FindIndex(p => p.Id == profile.Id);
        if (index >= 0)
        {
            _profiles[index] = profile;
        }
        else
        {
            _profiles.Add(profile);
        }

        return Task.CompletedTask;
    }

    public Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        _profiles.RemoveAll(p => p.Id == profileId);
        return Task.CompletedTask;
    }
}
