using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kullanıcının simülasyon isteklerini (yeni kredi, taksitli kart harcaması, finansman,
/// erken borç kapama, gelir artışı) mevcut finansal plana uygulayarak izole bir hipotetik
/// senaryo planı (FinancialPlan) inşa eden saf hesaplayıcı.
/// </summary>
public sealed class ScenarioPlanBuilder(
    InstallmentScheduleCalculator installmentScheduleCalculator,
    LoanPrepaymentValidator loanPrepaymentValidator)
{
    private readonly InstallmentScheduleCalculator _installmentScheduleCalculator =
        installmentScheduleCalculator ?? throw new ArgumentNullException(nameof(installmentScheduleCalculator));
    private readonly LoanPrepaymentValidator _loanPrepaymentValidator =
        loanPrepaymentValidator ?? throw new ArgumentNullException(nameof(loanPrepaymentValidator));

    /// <summary>Tek bir simülasyon isteğini plana uygulayarak güncellenmiş senaryo planını üretir.</summary>
    public FinancialPlan Build(FinancialPlan plan, SimulationRequest request) =>
        Build(plan, [request ?? throw new ArgumentNullException(nameof(request))]);

    /// <summary>Çoklu simülasyon isteklerini kronolojik öncelik sırasıyla plana uygulayarak nihai senaryo planını üretir.</summary>
    public FinancialPlan Build(FinancialPlan plan, IReadOnlyList<SimulationRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(plan);
        SimulationRequestValidator.Validate(requests);

        var scenarioPlan = plan;
        foreach (var req in requests.OrderBy(SortKey))
        {
            var request = req.ScenarioId == Guid.Empty ? req with { ScenarioId = Guid.NewGuid() } : req;
            scenarioPlan = BuildCore(scenarioPlan, request);
        }
        return scenarioPlan;
    }

    private FinancialPlan BuildCore(FinancialPlan plan, SimulationRequest req) => req.Type switch
    {
        SimulationScenarioType.CashPurchase => AddLargeExpense(plan, req),
        SimulationScenarioType.CreditCardSinglePayment => AddCardPurchase(plan, req with { PaymentCount = 1 }),
        SimulationScenarioType.CreditCardInstallmentPurchase => AddCardPurchase(plan, req),
        SimulationScenarioType.FinancingLoan => AddFinancingLoan(plan, req),
        SimulationScenarioType.CashDebt => AddInstallmentPlan(plan, req, req.Amount, PaymentPlanKind.OtherScheduled),
        SimulationScenarioType.FutureOneTimePayment => AddSinglePayment(plan, req),
        SimulationScenarioType.RecurringPayment => AddRecurringPayment(plan, req),
        SimulationScenarioType.FutureIncome => AddFutureIncome(plan, req),
        SimulationScenarioType.IncomeChange => AddIncomeChange(plan, req),
        SimulationScenarioType.CreditCardPaymentMode => AddCardPaymentMode(plan, req),
        SimulationScenarioType.LoanEarlyClosure or SimulationScenarioType.LoanPartialPrepayment => AddLoanPrepayment(plan, req),
        _ => throw new ArgumentOutOfRangeException(nameof(req))
    };

    private FinancialPlan AddCardPurchase(FinancialPlan plan, SimulationRequest req)
    {
        var card = plan.CreditCards.SingleOrDefault(x => x.Id == req.CreditCardId)
            ?? throw new InvalidOperationException("Seçilen kredi kartı bulunamadı.");
        if (card.Limit > 0m && req.Amount > (card.Limit - card.KnownTotalDebt))
        {
            throw new InvalidOperationException("Kartın bilinen kullanılabilir limiti bu plan için yetersiz.");
        }

        var charges = _installmentScheduleCalculator.Split(req.Amount, req.PaymentCount, req.StartDate)
            .Select((x, i) => new CardCharge
            {
                Id = i == 0 ? req.ScenarioId : ChildId(req.ScenarioId, i), CreditCardId = card.Id,
                Description = req.PaymentCount == 1 ? req.Name.Trim() : $"{req.Name.Trim()} ({i + 1}/{req.PaymentCount})",
                PostingDate = x.Date, Amount = x.Amount
            }).ToArray();

        var updated = card with { Charges = card.Charges.Concat(charges).ToArray() };
        return plan with { CreditCards = plan.CreditCards.Select(x => x.Id == card.Id ? updated : x).ToArray() };
    }

    private static FinancialPlan AddCardPaymentMode(FinancialPlan plan, SimulationRequest req)
    {
        var card = plan.CreditCards.SingleOrDefault(x => x.Id == req.CreditCardId)
            ?? throw new InvalidOperationException("Seçilen kredi kartı bulunamadı.");
        var paymentType = req.CardPaymentType ?? CreditCardPaymentType.FullStatement;
        var updated = req.AppliesToAllStatements
            ? card with { PaymentStrategy = ToStrategy(paymentType) }
            : card with
            {
                PaymentPlans = card.PaymentPlans.Where(x => x.DueDate != req.StartDate)
                    .Append(new CreditCardPaymentPlan { Id = req.ScenarioId, CreditCardId = card.Id, DueDate = req.StartDate, PaymentType = paymentType })
                    .OrderBy(x => x.DueDate).ToArray()
            };
        return plan with { CreditCards = plan.CreditCards.Select(x => x.Id == card.Id ? updated : x).ToArray() };
    }

    private FinancialPlan AddLoanPrepayment(FinancialPlan plan, SimulationRequest req)
    {
        var loan = plan.Loans.SingleOrDefault(x => x.Id == req.LoanId);
        var prepayment = new LoanPrepayment
        {
            Id = req.ScenarioId, LoanId = req.LoanId.GetValueOrDefault(), Date = req.StartDate,
            Mode = req.Type == SimulationScenarioType.LoanEarlyClosure ? LoanPrepaymentMode.FullClosure : req.PrepaymentMode ?? LoanPrepaymentMode.ReduceTerm,
            PrincipalAmount = req.Type == SimulationScenarioType.LoanEarlyClosure ? null : req.Amount
        };
        _loanPrepaymentValidator.Validate(loan, plan.LoanPrepayments, prepayment);
        return plan with { LoanPrepayments = plan.LoanPrepayments.Append(prepayment).ToArray() };
    }

    private FinancialPlan AddFinancingLoan(FinancialPlan plan, SimulationRequest req)
    {
        var withRepayment = AddInstallmentPlan(plan, req, req.TotalRepaymentAmount ?? req.Amount, PaymentPlanKind.Installment);
        return AddFutureIncome(withRepayment, req);
    }

    private FinancialPlan AddInstallmentPlan(FinancialPlan plan, SimulationRequest req, decimal total, PaymentPlanKind kind)
    {
        var firstPayment = req.FirstPaymentDate ?? req.StartDate;
        var schedule = _installmentScheduleCalculator.Split(total, req.PaymentCount, firstPayment);
        return AddPaymentPlan(plan, req, kind, schedule, req.Amount, total);
    }

    private static FinancialPlan AddRecurringPayment(FinancialPlan plan, SimulationRequest req)
    {
        var firstPayment = req.FirstPaymentDate ?? req.StartDate;
        var schedule = Enumerable.Range(0, req.PaymentCount)
            .Select(i => new ScheduledAmount(CalendarRules.AddMonthsKeepingDay(firstPayment, i, firstPayment.Day), req.Amount)).ToArray();
        return AddPaymentPlan(plan, req, PaymentPlanKind.Recurring, schedule, req.Amount, req.Amount * req.PaymentCount);
    }

    private static FinancialPlan AddSinglePayment(FinancialPlan plan, SimulationRequest req) =>
        AddPaymentPlan(plan, req, PaymentPlanKind.OtherScheduled, [new ScheduledAmount(req.StartDate, req.Amount)], req.Amount, req.Amount);

    private static FinancialPlan AddFutureIncome(FinancialPlan plan, SimulationRequest req) => plan with
    {
        AdHocIncomes = plan.AdHocIncomes.Append(new AdHocIncome
        {
            Id = req.ScenarioId, Description = req.Name.Trim(), Amount = req.Amount, ExactDate = req.StartDate
        }).ToArray()
    };

    private static FinancialPlan AddIncomeChange(FinancialPlan plan, SimulationRequest req)
    {
        var streamId = req.RecurringIncomeId.GetValueOrDefault();
        var history = new IncomeAmountHistory
        {
            RecurringIncomeId = streamId, Amount = req.Amount, EffectiveDate = req.StartDate, Description = req.Name.Trim()
        };
        return plan with
        {
            IncomeHistories = plan.IncomeHistories
                .Where(x => !(x.RecurringIncomeId == streamId && x.EffectiveDate == req.StartDate))
                .Append(history).ToArray()
        };
    }

    private static FinancialPlan AddLargeExpense(FinancialPlan plan, SimulationRequest req) => plan with
    {
        PlannedLargeExpenses = plan.PlannedLargeExpenses.Append(new PlannedLargeExpense
        {
            Id = req.ScenarioId, Name = req.Name.Trim(), Amount = req.Amount, ExactDate = req.StartDate, Status = PlannedExpenseStatus.Planned
        }).ToArray()
    };

    private static FinancialPlan AddPaymentPlan(
        FinancialPlan plan, SimulationRequest req, PaymentPlanKind kind, IReadOnlyList<ScheduledAmount> schedule, decimal original, decimal total)
    {
        var paymentPlan = new TemporaryPaymentPlan
        {
            Id = req.ScenarioId, Name = req.Name.Trim(), Kind = kind, OriginalAmount = original, TotalRepaymentAmount = total,
            Installments = schedule.Select((x, i) => new TemporaryPaymentInstallment
            {
                Id = ChildId(req.ScenarioId, i), PlanId = req.ScenarioId, DueDate = x.Date, Amount = x.Amount
            }).ToArray()
        };
        return plan with { PaymentPlans = plan.PaymentPlans.Append(paymentPlan).ToArray() };
    }

    private static CreditCardPaymentStrategy ToStrategy(CreditCardPaymentType type) => type switch
    {
        CreditCardPaymentType.Minimum => CreditCardPaymentStrategy.Minimum,
        CreditCardPaymentType.FullStatement => CreditCardPaymentStrategy.FullStatement,
        _ => throw new InvalidOperationException("Kart ödeme şekli yalnızca asgari veya tamamı olabilir.")
    };

    private static (DateOnly Date, int Type, string Name, decimal Amount, Guid Id) SortKey(SimulationRequest req) =>
        (req.StartDate, (int)req.Type, req.Name.Trim(), req.Amount, req.ScenarioId);

    private static Guid ChildId(Guid parentId, int index)
    {
        var bytes = parentId.ToByteArray();
        var ordinal = BitConverter.GetBytes(index + 1);
        for (var offset = 0; offset < ordinal.Length; offset++)
        {
            bytes[12 + offset] ^= ordinal[offset];
        }
        return new Guid(bytes);
    }
}
