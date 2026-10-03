using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Simülatörün sonucunu 12 Dönem'in zincirinden kurar (S76-1, 2): şu anki gidişat 12 Dönem'le aynıdır, açık döneme
/// düşen deneme senaryo zincirinin açılışına eklenir. Eskide simülatör zinciri planın çapasından kuruyordu ve
/// bugüne tarihli bir deneme hiç görünmüyordu. <see cref="SimulationWorkflowService"/> ve
/// <see cref="FutureProjectionService"/> 5 bağımlılıkta olduğu için (M3) sonuç ayrı bir okuma servisindedir.
/// </summary>
public sealed class SimulationResultService(
    IPeriodProgressService progressService,
    IPlanReader planReader,
    FinancialProjectionCalculator projectionCalculator,
    SimulationCalculator simulationCalculator,
    IClock clock) : ISimulationResultService
{
    private const int PeriodCount = 12;

    private readonly IPeriodProgressService _progressService =
        progressService ?? throw new ArgumentNullException(nameof(progressService));
    private readonly IPlanReader _planReader = planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly FinancialProjectionCalculator _projectionCalculator =
        projectionCalculator ?? throw new ArgumentNullException(nameof(projectionCalculator));
    private readonly SimulationCalculator _simulationCalculator =
        simulationCalculator ?? throw new ArgumentNullException(nameof(simulationCalculator));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public async Task<SimulationOutcome?> CalculateAsync(
        IReadOnlyList<SimulationDraftCondition> conditions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        var progress = await _progressService.GetAsync(cancellationToken);
        if (progress is null)
        {
            return null;
        }

        var today = _clock.Today;
        var query = await _planReader.GetProjectionPlanAsync(today, cancellationToken);
        if (ProjectionChainBuilder.Build(query.Plan, progress, today) is not { } chain)
        {
            return null;
        }

        // Geçerlilik her hesapta bugüne göre yeniden değerlendirilir: ekranın taşıdığı işaret dünden kalmış olabilir (S76-5).
        var requests = conditions
            .Where(x => x.IsEnabled && !SimulationConditionRules.IsDatePassed(x.Request, today))
            .Select(x => x.Request)
            .ToArray();

        return requests.Length == 0 ? BaselineOnly(chain) : WithScenario(chain, requests);
    }

    // Açık ve geçerli deneme yokken yalnız şu anki gidişat vardır: 12 Dönem'in zinciriyle kuruşu kuruşuna aynı (S76-6).
    private SimulationOutcome BaselineOnly(ProjectionChain chain) =>
        new(_projectionCalculator.Calculate(chain.Plan, chain.Today, PeriodCount, chain.FirstPeriodStart), null);

    // Açık döneme düşen etki senaryo zincirinin açılışına eklenir; baz zincir değişmez (S76-2).
    private SimulationOutcome WithScenario(ProjectionChain chain, IReadOnlyList<SimulationRequest> requests)
    {
        var openPeriodEffect = _simulationCalculator.CalculateOpenPeriodEffect(
            chain.OpenPeriodPlan, chain.Today, requests, chain.OpenPeriodStart, chain.OpenPeriodEndingBeforeDeficitInterest);
        var result = _simulationCalculator.Calculate(
            chain.Plan, chain.Today, requests, PeriodCount, chain.FirstPeriodStart, openPeriodEffect);
        var saving = result.LoanImpacts.Count > 0 ? result.LoanImpacts.Sum(x => x.InterestSaving) : (decimal?)null;
        var loanInterestSaving = saving > 0m ? saving : null;
        var financingCost = result.Risk.FinancingCost > 0m ? result.Risk.FinancingCost : null;
        return new SimulationOutcome(result.Baseline, result.Scenario, loanInterestSaving, financingCost);
    }
}
