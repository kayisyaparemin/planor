using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// 12 dönemlik zinciri ana sayfanın dönem sonuna bağlar (S74). Eskide zincir planın çapasından kuruluyor,
/// açık dönem planla hesaplanıyordu; kullanıcı bakiye girince ana sayfa tahmini, 12 dönem planı gösteriyor ve
/// ekran bu çelişkiyi bir uyarı cümlesiyle açıklamak zorunda kalıyordu. Burada açılış bakiyesi ana sayfadaki
/// rakamdır (bakiye girildiyse tahmin, girilmediyse plan; S72-1) ve ilk dönem açık dönemin bittiği gündür.
/// Erken kapama önerisi de aynı zincirde koşar; iki cevap aynı rakamdan başlar (S74-7).
/// </summary>
public sealed class FutureProjectionService(
    IPeriodProgressService progressService,
    IPlanReader planReader,
    FinancialProjectionCalculator projectionCalculator,
    LoanPayoffAdvisor payoffAdvisor,
    IClock clock) : IFutureProjectionService
{
    private const int PeriodCount = 12;

    private readonly IPeriodProgressService _progressService =
        progressService ?? throw new ArgumentNullException(nameof(progressService));
    private readonly IPlanReader _planReader = planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly FinancialProjectionCalculator _projectionCalculator =
        projectionCalculator ?? throw new ArgumentNullException(nameof(projectionCalculator));
    private readonly LoanPayoffAdvisor _payoffAdvisor = payoffAdvisor ?? throw new ArgumentNullException(nameof(payoffAdvisor));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public async Task<FinancialProjectionResult?> GetAsync(CancellationToken cancellationToken = default)
    {
        var chain = await LoadChainAsync(cancellationToken);
        return chain is null
            ? null
            : _projectionCalculator.CalculatePlan(chain.Plan, chain.Today, PeriodCount, chain.FirstPeriodStart);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LoanPayoffAdvice>> GetPayoffAdviceAsync(CancellationToken cancellationToken = default)
    {
        var chain = await LoadChainAsync(cancellationToken);
        if (chain is null)
        {
            return [];
        }

        // Her kredi için ufuktaki her taksit günü denenir; birkaç düzine projeksiyon ekranı dondurmasın diye arka planda.
        return await Task.Run(
            () => _payoffAdvisor.Advise(chain.Plan, chain.Today, chain.FirstPeriodStart, PeriodCount, cancellationToken),
            cancellationToken);
    }

    // Açık dönem yoksa zincir yoktur (S74-2). Zincirin kendisi ortak kurucudadır: simülatör de aynısını kullanır (S76-1).
    // Zincirin ilk dönemi açık dönemin bittiği gündür; öneri bu yüzden açık dönemin taksitlerini denemez (S74-7).
    private async Task<ProjectionChain?> LoadChainAsync(CancellationToken cancellationToken)
    {
        var progress = await _progressService.GetAsync(cancellationToken);
        if (progress is null)
        {
            return null;
        }

        var today = _clock.Today;
        var query = await _planReader.GetProjectionPlanAsync(today, cancellationToken);
        return ProjectionChainBuilder.Build(query.Plan, progress, today);
    }
}
