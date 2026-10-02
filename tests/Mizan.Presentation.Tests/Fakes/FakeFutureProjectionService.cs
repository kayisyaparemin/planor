using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde 12 dönemlik projeksiyonu ve erken kapama önerisini taklit eden test çiftidir.
/// </summary>
public sealed class FakeFutureProjectionService : IFutureProjectionService
{
    /// <summary>GetAsync çağrıldığında dönecek projeksiyon; <c>null</c> boş ekranı taklit eder.</summary>
    public FinancialProjectionResult? Result { get; set; }

    /// <summary>Doluysa GetAsync bu hatayı fırlatır; okuma ya da hesap hatasını taklit eder.</summary>
    public Exception? Failure { get; set; }

    /// <summary>Doluysa GetAsync bu görev tamamlanana kadar bekler; süren yüklemeyi taklit eder.</summary>
    public TaskCompletionSource<FinancialProjectionResult?>? Pending { get; set; }

    /// <summary>GetPayoffAdviceAsync çağrıldığında dönecek öneriler.</summary>
    public IReadOnlyList<LoanPayoffAdvice> PayoffAdvice { get; set; } = [];

    /// <summary>Doluysa GetPayoffAdviceAsync bu hatayı fırlatır; önerinin hesap hatasını taklit eder.</summary>
    public Exception? PayoffFailure { get; set; }

    /// <summary>GetPayoffAdviceAsync kaç kez çağrıldı; boş ekranın öneri istemediğini doğrulamak için.</summary>
    public int PayoffCallCount { get; private set; }

    /// <summary>Taklit edilen projeksiyonu döndürür.</summary>
    public Task<FinancialProjectionResult?> GetAsync(CancellationToken cancellationToken = default)
    {
        if (Failure is not null) { return Task.FromException<FinancialProjectionResult?>(Failure); }
        return Pending?.Task ?? Task.FromResult(Result);
    }

    /// <summary>Taklit edilen erken kapama önerilerini döndürür.</summary>
    public Task<IReadOnlyList<LoanPayoffAdvice>> GetPayoffAdviceAsync(CancellationToken cancellationToken = default)
    {
        PayoffCallCount++;
        return PayoffFailure is not null
            ? Task.FromException<IReadOnlyList<LoanPayoffAdvice>>(PayoffFailure)
            : Task.FromResult(PayoffAdvice);
    }
}
