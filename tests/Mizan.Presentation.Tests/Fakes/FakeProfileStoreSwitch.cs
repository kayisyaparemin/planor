using Mizan.Application.Abstractions;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde veritabanı profil anahtarlamasını taklit eden sahte servis.
/// </summary>
public sealed class FakeProfileStoreSwitch : IProfileStoreSwitch
{
    public Guid? ActiveProfileId { get; private set; }

    public Task OpenAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        ActiveProfileId = profileId;
        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        ActiveProfileId = null;
        return Task.CompletedTask;
    }
}
