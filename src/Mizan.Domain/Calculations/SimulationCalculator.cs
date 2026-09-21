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

    /// <summary>Çoklu simülasyon istekleri için 12 dönemlik karşılaştırma projeksiyonu ve risk özetini hesaplar.</summary>
    public SimulationResult Calculate(
        FinancialPlan currentPlan, DateOnly asOf, IReadOnlyList<SimulationRequest> requests, int periodCount = 12, DateOnly? firstPeriodStartDate = null)
    {
        ArgumentNullException.ThrowIfNull(currentPlan);
        SimulationRequestValidator.Validate(requests);

        var baselineResult = _projectionCalculator.CalculatePlan(currentPlan, asOf, periodCount, firstPeriodStartDate);
        var scenarioPlan = _planBuilder.Build(currentPlan, requests);
        var scenarioResult = _projectionCalculator.CalculatePlan(scenarioPlan, asOf, periodCount, firstPeriodStartDate);

        var baseline = baselineResult.Periods;
        var scenario = scenarioResult.Periods;
        var rows = baseline.Zip(scenario, (b, s) => new SimulationImpactRow(b, s)).ToArray();
        var loanImpacts = BuildLoanImpacts(currentPlan, scenarioPlan);
        var risk = BuildRiskSummary(scenario, requests, loanImpacts);

        var interestDiff = scenarioResult.TotalInterestCost - baselineResult.TotalInterestCost;
        var endingDiff = scenario[^1].EndingBalance - baseline[^1].EndingBalance;
        var summary = BuildFriendlySummary(risk, interestDiff, endingDiff);

        return new SimulationResult(baseline, scenario, rows, risk, summary) { LoanImpacts = loanImpacts };
    }

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

    private static string BuildFriendlySummary(SimulationRiskSummary risk, decimal additionalInterest, decimal endingDiff)
    {
        var parts = new List<string>
        {
            endingDiff switch
            {
                > 0m => $"Bu plan 12 ay sonundaki tahmini finansal durumunu {FormatMoney(endingDiff)} artırıyor.",
                < 0m => $"Bu plan 12 ay sonundaki tahmini finansal durumunu {FormatMoney(Math.Abs(endingDiff))} azaltıyor.",
                _ => "Bu plan 12 ay sonundaki tahmini finansal durumu değiştirmiyor."
            }
        };

        if (risk.FirstNegativeProjectedBalancePeriod is { } negative)
        {
            parts.Add($"Bu plan {FormatPeriod(negative)} döneminde finansman açığı oluşturuyor.");
            parts.Add(risk.RecoveryPeriod is { } recovered
                ? $"Açık {FormatPeriod(recovered)} döneminde kapanıyor."
                : "Açık gösterilen dönemlerde kapanmıyor.");
        }
        else if (risk.MaximumCarryOverDeficit > 0m)
        {
            parts.Add(risk.RecoveryPeriod is { } openRecovery
                ? $"Devreden açık {FormatPeriod(openRecovery)} döneminde kapanıyor."
                : "Devreden açık gösterilen dönemlerde kapanmıyor.");
        }
        else
        {
            parts.Add("12 dönemlik görünümde finansman açığı oluşmuyor.");
        }

        parts.Add(additionalInterest switch
        {
            > 0m => $"Bu planın tahmini ek faiz yükü {FormatMoney(additionalInterest)}.",
            < 0m => $"Bu plan mevcut plana göre {FormatMoney(Math.Abs(additionalInterest))} daha düşük faiz yükü oluşturuyor.",
            _ => "Bu plan tahmini faiz yükünü değiştirmiyor."
        });

        return string.Join(" ", parts);
    }

    private static string FormatMoney(decimal value) => $"{value:N2} TL";

    private static string FormatPeriod(CashFlowPeriod p) => $"{p.Start:yyyy-MM}";
}
