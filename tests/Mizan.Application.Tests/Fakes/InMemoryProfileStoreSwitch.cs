using Mizan.Application.Abstractions;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Application katmanı birim testlerinde profil veritabanı açma/kapatma durumunu bellek içinde taklit eden sahte test çifti.
/// </summary>
public sealed class InMemoryProfileStoreSwitch : IProfileStoreSwitch
{
    public Guid? OpenProfileId { get; private set; }
    public bool IsOpen => OpenProfileId.HasValue;
    public int OpenCalls { get; private set; }
    public int CloseCalls { get; private set; }

    public Task OpenAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        OpenCalls++;
        OpenProfileId = profileId;
        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        CloseCalls++;
        OpenProfileId = null;
        return Task.CompletedTask;
    }
}
