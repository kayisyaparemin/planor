using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kullanıcının canlı finansal planı (baseline) ile varsayımsal senaryo koşullarını (scenario)
/// 12 dönem boyunca koşturup karşılaştıran, likidite farklarını, finansman açığı sürelerini,
/// ek faiz maliyeti ve tasarruflarını hesaplayan ana simülasyon motorudur.
/// </summary>
public sealed class SimulationCalculator(
    FinancialProjectionCalculator projectionCalculator,
    ScenarioPlanBuilder planBuilder,
    LoanPaymentScheduleBuilder loanScheduleBuilder)
{
    private readonly FinancialProjectionCalculator _projectionCalculator =
        projectionCalculator ?? throw new ArgumentNullException(nameof(projectionCalculator));
    private readonly ScenarioPlanBuilder _planBuilder =
        planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
    private readonly LoanPaymentScheduleBuilder _loanScheduleBuilder =
        loanScheduleBuilder ?? throw new ArgumentNullException(nameof(loanScheduleBuilder));

    /// <summary>Tek bir simülasyon isteği için 12 dönemlik karşılaştırma projeksiyonu ve risk özetini hesaplar.</summary>
    public SimulationResult Calculate(
        FinancialPlan currentPlan, DateOnly asOf, SimulationRequest request, int periodCount = 12, DateOnly? firstPeriodStartDate = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Calculate(currentPlan, asOf, [request], periodCount, firstPeriodStartDate);
    }

    /// <summary>
    /// Çoklu simülasyon istekleri için 12 dönemlik karşılaştırma projeksiyonu ve risk özetini hesaplar.
    /// <paramref name="scenarioOpeningShift"/> yalnız senaryo zincirinin açılışına eklenir: zincirden önce düşen
    /// (açık dönemdeki) denemenin etkisi buradan zincire girer, baz zincir değişmez (S76-2).
    /// </summary>
    public SimulationResult Calculate(
        FinancialPlan currentPlan, DateOnly asOf, IReadOnlyList<SimulationRequest> requests, int periodCount = 12,
        DateOnly? firstPeriodStartDate = null, decimal scenarioOpeningShift = 0m)
    {
        ArgumentNullException.ThrowIfNull(currentPlan);
        SimulationRequestValidator.Validate(requests);

        var baselineResult = _projectionCalculator.CalculatePlan(currentPlan, asOf, periodCount, firstPeriodStartDate);
        var builtPlan = _planBuilder.Build(currentPlan, requests);
        var scenarioPlan = ShiftOpening(builtPlan, scenarioOpeningShift);
        var scenarioResult = _projectionCalculator.CalculatePlan(scenarioPlan, asOf, periodCount, firstPeriodStartDate);

        var baseline = baselineResult.Periods;
        var scenario = scenarioResult.Periods;
        var rows = baseline.Zip(scenario, (b, s) => new SimulationImpactRow(b, s)).ToArray();
        var loanImpacts = BuildLoanImpacts(currentPlan, builtPlan);
        var risk = BuildRiskSummary(scenario, requests, loanImpacts);

        return new SimulationResult(baseline, scenario, rows, risk) { LoanImpacts = loanImpacts };
    }

    private static FinancialPlan ShiftOpening(FinancialPlan plan, decimal shift) =>
        shift == 0m
            ? plan
            : plan with { Settings = plan.Settings with { ProjectionOpeningBalance = plan.Settings.ProjectionOpeningBalance + shift } };

    /// <summary>
    /// Bugün ile açık dönemin bitişi arasına düşen denemelerin açık dönemin sonuna etkisini bulur (S76-2). Deneme
    /// zincirden önce düştüğü için zincirin kendisinde görünmez; bu fark senaryo zincirinin açılışına eklenir.
    /// Etki, denemeli ve denemesiz açık dönem akışının farkıdır ve plandaki açılışa bağlı değildir. Faiz ise bu
    /// akış farkı ana sayfadaki gerçek dönem sonuna eklendikten sonra yeniden işletilir (V10b notları b).
    /// </summary>
    /// <param name="openPeriodPlan">Açık dönemi içeren plan; çapası açık dönemin başına çekilmek üzere alınır.</param>
    /// <param name="asOf">Hesabın günü.</param>
    /// <param name="requests">Hesaba giren açık ve geçerli denemeler.</param>
    /// <param name="openPeriodStart">Açık dönemin ilk günü.</param>
    /// <param name="endingBeforeDeficitInterest">Ana sayfadaki dönem sonunun faiz öncesi hâli (dönem sonu + KMH faizi).</param>
    /// <returns>Denemeli dönem sonu eksi denemesiz dönem sonu; deneme açık dönemi etkilemiyorsa tam sıfır.</returns>
    public decimal CalculateOpenPeriodEffect(
        FinancialPlan openPeriodPlan, DateOnly asOf, IReadOnlyList<SimulationRequest> requests,
        DateOnly openPeriodStart, decimal endingBeforeDeficitInterest)
    {
        ArgumentNullException.ThrowIfNull(openPeriodPlan);
        SimulationRequestValidator.Validate(requests);

        var plan = openPeriodPlan with { Settings = openPeriodPlan.Settings with { ProjectionAnchorDate = openPeriodStart } };
        var baseSurplus = OpenPeriodSurplus(plan, asOf, openPeriodStart);
        var scenarioSurplus = OpenPeriodSurplus(_planBuilder.Build(plan, requests), asOf, openPeriodStart);

        var rate = plan.Settings.DeficitFinancingInterestRate;
        var baseEnding = EndingAfterInterest(endingBeforeDeficitInterest, rate);
        var scenarioEnding = EndingAfterInterest(endingBeforeDeficitInterest + scenarioSurplus - baseSurplus, rate);
        return scenarioEnding - baseEnding;
    }

    // Akış açılıştan bağımsızdır: yalnız gelir, zorunlu ödeme, yaşam havuzu ve büyük harcama girer.
    private decimal OpenPeriodSurplus(FinancialPlan plan, DateOnly asOf, DateOnly openPeriodStart) =>
        _projectionCalculator.CalculatePlan(plan, asOf, 1, openPeriodStart).Periods[0].EstimatedSurplus;

    private static decimal EndingAfterInterest(decimal endingBeforeInterest, decimal rate) =>
        endingBeforeInterest - DeficitFinancingRules.CalculateInterest(endingBeforeInterest, rate);

    private static SimulationRiskSummary BuildRiskSummary(
        IReadOnlyList<CashFlowPeriodProjection> scenario, IReadOnlyList<SimulationRequest> requests, IReadOnlyList<LoanPrepaymentImpact> impacts)
    {
        var lowest = scenario.OrderBy(x => x.EndingBalance).ThenBy(x => x.PeriodStart).First();
        var firstNegativeSurplus = scenario.FirstOrDefault(x => x.EstimatedSurplus < 0m);
        var firstNegativeBalance = scenario.FirstOrDefault(x => x.EndingBalance < 0m);
        var maxCarry = scenario.Select(x => x.OpeningBalance < 0m ? Math.Abs(x.OpeningBalance) : 0m).DefaultIfEmpty(0m).Max();
        var recovery = scenario.FirstOrDefault(x => x.OpeningBalance < 0m && x.EndingBalance >= 0m);

        var totalCost = requests.Sum(ResolveTotalCost) + impacts.Sum(x => x.PrepaidAmount);
        var financingCosts = requests.Where(x => x.Type == SimulationScenarioType.FinancingLoan)
            .Select(x => (x.TotalRepaymentAmount ?? x.Amount) - x.Amount).ToArray();
        decimal? financingCost = financingCosts.Length > 0 ? financingCosts.Sum() : null;

        return new SimulationRiskSummary
        {
            LowestAvailableAfterMandatory = scenario.Min(x => x.AvailableAfterMandatory),
            LowestSurplus = scenario.Min(x => x.EstimatedSurplus),
            LowestProjectedBalance = scenario.Min(x => x.EndingBalance),
            LowestPeriod = lowest.Period,
            FirstNegativeSurplusPeriod = firstNegativeSurplus?.Period,
            FirstNegativeProjectedBalancePeriod = firstNegativeBalance?.Period,
            MaximumCarryOverDeficit = maxCarry,
            RecoveryPeriod = recovery?.Period,
            EndingProjectedBalance = scenario[^1].EndingBalance,
            TotalScenarioCost = totalCost,
            FinancingCost = financingCost
        };
    }

    private LoanPrepaymentImpact[] BuildLoanImpacts(FinancialPlan baselinePlan, FinancialPlan scenarioPlan)
    {
        var existing = baselinePlan.LoanPrepayments.Select(x => x.Id).ToHashSet();
        var added = scenarioPlan.LoanPrepayments.Where(x => !existing.Contains(x.Id)).ToArray();

        return scenarioPlan.Loans.Where(loan => added.Any(x => x.LoanId == loan.Id)).Select(loan =>
        {
            var baseline = _loanScheduleBuilder.Replay(loan, baselinePlan.LoanPrepayments);
            var scenario = _loanScheduleBuilder.Replay(loan, scenarioPlan.LoanPrepayments);
            var addedIds = added.Where(x => x.LoanId == loan.Id).Select(x => x.Id).ToHashSet();
            var lastAdded = added.Where(x => x.LoanId == loan.Id).OrderBy(x => x.Date).Last();

            decimal? newPayment = scenario.StateAfterEvent.TryGetValue(lastAdded.Id, out var state) && state.IsActive
                ? state.MonthlyPayment
                : null;

            return new LoanPrepaymentImpact
            {
                LoanId = loan.Id,
                LoanName = $"{loan.Bank} {loan.Name}".Trim(),
                PrepaidAmount = scenario.Payments.Where(x => addedIds.Contains(x.SourceId)).Sum(x => x.Amount),
                InterestSaving = baseline.Total - scenario.Total,
                BaselineEndDate = baseline.LastPaymentDate,
                ScenarioEndDate = scenario.LastPaymentDate,
                BaselineMonthlyPayment = loan.MonthlyPayment,
                ScenarioMonthlyPayment = newPayment
            };
        }).ToArray();
    }

    private static decimal ResolveTotalCost(SimulationRequest req) => req.Type switch
    {
        SimulationScenarioType.FutureIncome or SimulationScenarioType.IncomeChange => 0m,
        SimulationScenarioType.CreditCardPaymentMode => 0m,
        SimulationScenarioType.LoanEarlyClosure or SimulationScenarioType.LoanPartialPrepayment => 0m,
        SimulationScenarioType.FinancingLoan => req.TotalRepaymentAmount ?? req.Amount,
        SimulationScenarioType.RecurringPayment => req.Amount * req.PaymentCount,
        _ => req.Amount
    };
}
