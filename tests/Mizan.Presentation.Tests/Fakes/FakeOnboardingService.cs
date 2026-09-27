using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Kurulum sihirbazı testleri için kurulum taslağını hafızada yakalayan sahte servis.
/// </summary>
public sealed class FakeOnboardingService : IOnboardingService
{
    public OnboardingDraft? LastDraft { get; private set; }
    public int CallCount { get; private set; }
    public bool ShouldThrow { get; set; }

    public Task InitializeFromOnboardingAsync(OnboardingDraft draft, CancellationToken cancellationToken = default)
    {
        CallCount++;
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Kurulum kaydedilemedi.");
        }

        LastDraft = draft;
        return Task.CompletedTask;
    }
}
