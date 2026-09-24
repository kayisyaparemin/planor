using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Doğrulanmış simülasyon isteklerini izole senaryo planına dönüştüren, yeni ve güncellenen varlıkları
/// odaklı yazıcılar üzerinden dar depolara kaydeden ve plan değişikliğini taahhüt eden uygulama servisidir.
/// </summary>
public sealed class SimulationPlanApplier(
    ScenarioPlanBuilder planBuilder,
    IncomePlanWriter incomeWriter,
    FinancialInstrumentWriter instrumentWriter,
    IPlanChangeRecorder planChangeRecorder) : ISimulationPlanApplier
{
    private readonly ScenarioPlanBuilder _planBuilder =
        planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
    private readonly IncomePlanWriter _incomeWriter =
        incomeWriter ?? throw new ArgumentNullException(nameof(incomeWriter));
    private readonly FinancialInstrumentWriter _instrumentWriter =
        instrumentWriter ?? throw new ArgumentNullException(nameof(instrumentWriter));
    private readonly IPlanChangeRecorder _planChangeRecorder =
        planChangeRecorder ?? throw new ArgumentNullException(nameof(planChangeRecorder));

    /// <inheritdoc />
    public async Task<SimulationApplyResult> ApplyAsync(
        FinancialPlan currentPlan,
        IReadOnlyList<SimulationRequest> requests,
        string trigger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentPlan);
        ArgumentNullException.ThrowIfNull(requests);
        SimulationRequestValidator.Validate(requests);

        if (requests.Any(x => x.ScenarioId == Guid.Empty))
        {
            throw new InvalidOperationException("Uygulanacak simülasyon kimliği bulunamadı. Planı yeniden simüle edin.");
        }

        var existingCheck = CheckAlreadyApplied(currentPlan, requests);
        if (existingCheck is not null)
        {
            return existingCheck;
        }

        ValidateConflicts(currentPlan, requests);

        var scenario = _planBuilder.Build(currentPlan, requests);
        await PersistEntitiesAsync(scenario, requests, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(trigger, cancellationToken);

        return BuildAppliedResult(requests, scenario);
    }

    private static SimulationApplyResult? CheckAlreadyApplied(
        FinancialPlan currentPlan,
        IReadOnlyList<SimulationRequest> requests)
    {
        var existingResults = requests
            .Select(request => FindAppliedSimulation(currentPlan, request))
            .ToArray();

        if (existingResults.All(x => x is not null))
        {
            var first = existingResults[0]!;
            return first with
            {
                AlreadyApplied = true,
                Message = requests.Count == 1 ? first.Message : "Bu simülasyon planı daha önce finans planına eklendi."
            };
        }

        if (existingResults.Any(x => x is not null))
        {
            throw new InvalidOperationException(
                "Bu simülasyon planının bir kısmı daha önce uygulanmış. Tekrar kaydı önlemek için planı temizleyip yeniden oluştur.");
        }

        return null;
    }

    private static void ValidateConflicts(FinancialPlan current, IReadOnlyList<SimulationRequest> requests)
    {
        var conflictingIncome = requests.FirstOrDefault(request =>
            request.Type == SimulationScenarioType.IncomeChange &&
            current.IncomeHistories.Any(x =>
                x.RecurringIncomeId == request.RecurringIncomeId &&
                x.EffectiveDate == request.StartDate));

        if (conflictingIncome is not null)
        {
            throw new InvalidOperationException(
                "Bu tarihte zaten bir gelir değişikliği kaydı var. Geçmişi korumak için farklı bir geçerlilik tarihi seçin.");
        }
    }

    private async Task PersistEntitiesAsync(
        FinancialPlan scenario,
        IReadOnlyList<SimulationRequest> requests,
        CancellationToken cancellationToken)
    {
        var requestIds = requests.Select(x => x.ScenarioId).ToHashSet();
        var cardIds = requests.Where(x => x.CreditCardId.HasValue).Select(x => x.CreditCardId!.Value).ToHashSet();

        await _instrumentWriter.WriteLargeExpensesAsync(
            scenario.PlannedLargeExpenses.Where(x => requestIds.Contains(x.Id)), cancellationToken);
        await _instrumentWriter.WritePaymentPlansAsync(
            scenario.PaymentPlans.Where(x => requestIds.Contains(x.Id)), cancellationToken);
        await _instrumentWriter.WriteCreditCardsAsync(
            scenario.CreditCards.Where(x => cardIds.Contains(x.Id)), cancellationToken);
        await _instrumentWriter.WriteLoanPrepaymentsAsync(
            scenario.LoanPrepayments.Where(x => requestIds.Contains(x.Id)), cancellationToken);
        await _incomeWriter.WriteAdHocIncomesAsync(
            scenario.AdHocIncomes.Where(x => requestIds.Contains(x.Id)), cancellationToken);
        await _incomeWriter.WriteIncomeHistoriesAsync(
            scenario.IncomeHistories.Where(x => requests.Any(r =>
                r.Type == SimulationScenarioType.IncomeChange &&
                r.RecurringIncomeId == x.RecurringIncomeId &&
                r.StartDate == x.EffectiveDate)), cancellationToken);
    }

    private static SimulationApplyResult BuildAppliedResult(
        IReadOnlyList<SimulationRequest> requests,
        FinancialPlan scenario)
    {
        if (requests.Count == 1)
        {
            var req = requests[0];
            return req.Type switch
            {
                SimulationScenarioType.CashPurchase =>
                    new(req.ScenarioId, req.ScenarioId, SimulationApplyDestination.Payments, false, "Plan finans planına eklendi."),
                SimulationScenarioType.CreditCardSinglePayment or
                SimulationScenarioType.CreditCardInstallmentPurchase or
                SimulationScenarioType.CreditCardPaymentMode =>
                    new(req.ScenarioId, req.CreditCardId!.Value, SimulationApplyDestination.CreditCard, false,
                        $"Plan {scenario.CreditCards.Single(c => c.Id == req.CreditCardId).Bank} {scenario.CreditCards.Single(c => c.Id == req.CreditCardId).Name} kartına eklendi."),
                SimulationScenarioType.FinancingLoan or
                SimulationScenarioType.CashDebt or
                SimulationScenarioType.FutureOneTimePayment or
                SimulationScenarioType.RecurringPayment =>
                    new(req.ScenarioId, req.ScenarioId, SimulationApplyDestination.Payments, false, "Plan finans planına eklendi."),
                SimulationScenarioType.FutureIncome =>
                    new(req.ScenarioId, req.ScenarioId, SimulationApplyDestination.Income, false, "Gelir finans planına eklendi."),
                SimulationScenarioType.IncomeChange =>
                    new(req.ScenarioId, req.ScenarioId, SimulationApplyDestination.IncomeHistory, false, "Gelir değişikliği kaydedildi."),
                SimulationScenarioType.LoanEarlyClosure or
                SimulationScenarioType.LoanPartialPrepayment =>
                    new(req.ScenarioId, req.ScenarioId, SimulationApplyDestination.Payments, false, "Erken ödeme kredinin planına eklendi."),
                _ => throw new ArgumentOutOfRangeException(nameof(requests))
            };
        }

        return new SimulationApplyResult(
            requests[0].ScenarioId,
            Guid.Empty,
            SimulationApplyDestination.Payments,
            AlreadyApplied: false,
            $"{requests.Count} koşul finans planına eklendi.");
    }

    private static SimulationApplyResult? FindAppliedSimulation(FinancialPlan plan, SimulationRequest request)
    {
        var id = request.ScenarioId;
        return request.Type switch
        {
            SimulationScenarioType.CashPurchase when plan.PlannedLargeExpenses.Any(x => x.Id == id) =>
                new(id, id, SimulationApplyDestination.Payments, true, "Plan daha önce finans planına eklendi."),
            SimulationScenarioType.CreditCardSinglePayment or SimulationScenarioType.CreditCardInstallmentPurchase
                when plan.CreditCards.Any(c => c.Id == request.CreditCardId && c.Charges.Any(ch => ch.Id == id)) =>
                new(id, request.CreditCardId!.Value, SimulationApplyDestination.CreditCard, true, "Plan daha önce kredi kartına eklendi."),
            SimulationScenarioType.CreditCardPaymentMode
                when plan.CreditCards.Any(c => c.Id == request.CreditCardId && c.PaymentPlans.Any(p => p.Id == id)) =>
                new(id, request.CreditCardId!.Value, SimulationApplyDestination.CreditCard, true, "Tam ödeme planı daha önce kredi kartına eklendi."),
            SimulationScenarioType.FinancingLoan or SimulationScenarioType.CashDebt or
            SimulationScenarioType.FutureOneTimePayment or SimulationScenarioType.RecurringPayment
                when plan.PaymentPlans.Any(x => x.Id == id) =>
                new(id, id, SimulationApplyDestination.Payments, true, "Plan daha önce finans planına eklendi."),
            SimulationScenarioType.FutureIncome when plan.AdHocIncomes.Any(x => x.Id == id) =>
                new(id, id, SimulationApplyDestination.Income, true, "Gelir daha önce finans planına eklendi."),
            SimulationScenarioType.IncomeChange when plan.IncomeHistories.Any(x =>
                x.RecurringIncomeId == request.RecurringIncomeId &&
                x.EffectiveDate == request.StartDate &&
                x.Amount == request.Amount) =>
                new(id, id, SimulationApplyDestination.IncomeHistory, true, "Gelir değişikliği daha önce kaydedildi."),
            SimulationScenarioType.LoanEarlyClosure or SimulationScenarioType.LoanPartialPrepayment
                when plan.LoanPrepayments.Any(x => x.Id == id) =>
                new(id, id, SimulationApplyDestination.Payments, true, "Erken ödeme daha önce kredinin planına eklendi."),
            _ => null
        };
    }
}
