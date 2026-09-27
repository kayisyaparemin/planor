using Mizan.Application.Abstractions;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>Plan revizyonu tetiklerini yalnız sayan sahte kaydedici.</summary>
internal sealed class FakePlanChangeRecorder : IPlanChangeRecorder
{
    public int ChangeCount { get; private set; }

    public Task RecordChangeAsync(string trigger, CancellationToken cancellationToken = default)
    {
        ChangeCount++;
        return Task.CompletedTask;
    }
}
