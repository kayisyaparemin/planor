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
/// </summary>
public sealed class FutureProjectionService(
    IPeriodProgressService progressService,
    IPlanReader planReader,
    FinancialProjectionCalculator projectionCalculator,
    IClock clock) : IFutureProjectionService
{
    private const int PeriodCount = 12;

    private readonly IPeriodProgressService _progressService =
        progressService ?? throw new ArgumentNullException(nameof(progressService));
    private readonly IPlanReader _planReader = planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly FinancialProjectionCalculator _projectionCalculator =
        projectionCalculator ?? throw new ArgumentNullException(nameof(projectionCalculator));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public async Task<FinancialProjectionResult?> GetAsync(CancellationToken cancellationToken = default)
    {
        var progress = await _progressService.GetAsync(cancellationToken);
        if (progress is null)
        {
            return null;
        }

        var today = _clock.Today;
        var query = await _planReader.GetProjectionPlanAsync(today, cancellationToken);
        var plan = ChainFrom(query.Plan, progress);

        // Kural zincirin kendisine sorulur: gelir silinmiş ama ana sayfada bakiye varsa zincir erimeyi gösterir.
        return plan.CanBuildProjection
            ? _projectionCalculator.CalculatePlan(plan, today, PeriodCount, progress.PeriodEnd)
            : null;
    }

    // Planın çapası ve açılışı açık dönemin başıdır; ikisi de ana sayfanın dönem sonuyla değişir (S74-1).
    private static FinancialPlan ChainFrom(FinancialPlan plan, PeriodProgress progress) =>
        plan with
        {
            Settings = plan.Settings with
            {
                ProjectionAnchorDate = progress.PeriodEnd,
                ProjectionOpeningBalance = progress.ProjectedEndingBalance ?? progress.PlannedEndingBalance
            }
        };
}
