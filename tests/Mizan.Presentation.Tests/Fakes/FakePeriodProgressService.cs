using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde açık dönemin gidişat verilerini taklit eden test çiftidir.
/// </summary>
public sealed class FakePeriodProgressService : IPeriodProgressService
{
    /// <summary>GetAsync çağrıldığında dönecek gidişat modeli.</summary>
    public PeriodProgress? CurrentProgress { get; set; }

    /// <summary>Doluysa GetAsync bu hatayı fırlatır; okuma hatasını taklit eder.</summary>
    public Exception? Failure { get; set; }

    /// <summary>Doluysa GetAsync bu görev tamamlanana kadar bekler; süren yüklemeyi taklit eder.</summary>
    public TaskCompletionSource<PeriodProgress?>? Pending { get; set; }

    /// <summary>Taklit edilen gidişatı döndürür.</summary>
    public Task<PeriodProgress?> GetAsync(CancellationToken cancellationToken = default)
    {
        if (Failure is not null) { return Task.FromException<PeriodProgress?>(Failure); }
        return Pending?.Task ?? Task.FromResult(CurrentProgress);
    }

    /// <summary>PreviewAsync'e sırayla gelen bakiye ve gün istekleri.</summary>
    public List<(decimal Balance, DateOnly ObservedOn)> PreviewRequests { get; } = [];

    /// <summary>Önizlemenin cevabı: istenen bakiye ve günden taslak gidişatı kurar.</summary>
    public Func<decimal, DateOnly, PeriodProgress>? Preview { get; set; }

    /// <summary>Doluysa PreviewAsync bu hatayı fırlatır; reddedilen günü ya da hesap hatasını taklit eder.</summary>
    public Exception? PreviewFailure { get; set; }

    /// <summary>Doluysa sıradaki önizleme bu görev tamamlanana kadar bekler; birbirini geçen istekleri taklit eder.</summary>
    public Queue<TaskCompletionSource<PeriodProgress>> PendingPreviews { get; } = new();

    /// <summary>Önizlemeyi taklit eder; çağrılan bakiye ve gün kaydedilir.</summary>
    public Task<PeriodProgress> PreviewAsync(
        decimal balance,
        DateOnly observedOn,
        CancellationToken cancellationToken = default)
    {
        PreviewRequests.Add((balance, observedOn));
        if (PreviewFailure is not null) { return Task.FromException<PeriodProgress>(PreviewFailure); }
        if (PendingPreviews.Count > 0) { return PendingPreviews.Dequeue().Task; }
        return Task.FromResult(Preview?.Invoke(balance, observedOn) ??
            throw new InvalidOperationException("Testte önizleme cevabı ayarlanmadı."));
    }
}
