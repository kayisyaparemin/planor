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
}
