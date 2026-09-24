using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kullanıcının tüm finansal planını (düzenli/arızi gelirler, krediler, kredi kartları, vadeli borçlar, büyük harcamalar)
/// 12 nakit akış dönemi boyunca ileriye doğru simüle eden, dönem bazlı nakit dengesini ve finansman açığı faizini
/// hesaplayan ve kümülatif faiz maliyetlerini bir araya getiren ana projeksiyon motorudur.
/// </summary>
public sealed class FinancialProjectionCalculator(
    CashFlowPeriodCalculator periodCalculator,
    IncomeProjectionCalculator incomeProjectionCalculator,
    CreditCardStatementCalculator cardStatementCalculator,
    MandatoryPaymentCalculator mandatoryPaymentCalculator,
    PeriodObligationGrouper periodObligationGrouper)
{
    private readonly CashFlowPeriodCalculator _periodCalculator =
        periodCalculator ?? throw new ArgumentNullException(nameof(periodCalculator));
    private readonly IncomeProjectionCalculator _incomeProjectionCalculator =
        incomeProjectionCalculator ?? throw new ArgumentNullException(nameof(incomeProjectionCalculator));
    private readonly CreditCardStatementCalculator _cardStatementCalculator =
        cardStatementCalculator ?? throw new ArgumentNullException(nameof(cardStatementCalculator));
    private readonly MandatoryPaymentCalculator _mandatoryPaymentCalculator =
        mandatoryPaymentCalculator ?? throw new ArgumentNullException(nameof(mandatoryPaymentCalculator));
    private readonly PeriodObligationGrouper _periodObligationGrouper =
        periodObligationGrouper ?? throw new ArgumentNullException(nameof(periodObligationGrouper));

    /// <summary>
    /// Finansal plan için belirtilen ufukta nakit akış dönem projeksiyonları listesini hesaplar.
    /// </summary>
    public IReadOnlyList<CashFlowPeriodProjection> Calculate(
        FinancialPlan plan, DateOnly asOf, int periodCount = 12, DateOnly? firstPeriodStartDate = null) =>
        CalculatePlan(plan, asOf, periodCount, firstPeriodStartDate).Periods;

    /// <summary>
    /// Finansal plan için belirtilen ufukta dönem projeksiyonlarını, yükümlülük planını ve kart durumlarını içeren
    /// bütüncül projeksiyon sonucunu hesaplar.
    /// </summary>
    public FinancialProjectionResult CalculatePlan(
        FinancialPlan plan, DateOnly asOf, int periodCount = 12, DateOnly? firstPeriodStartDate = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (periodCount is < 1 or > 60)
        {
            throw new ArgumentOutOfRangeException(nameof(periodCount), "Dönem sayısı 1 ile 60 arasında olmalıdır.");
        }
        FinancialPlanValidator.Validate(plan);

        var anchor = plan.Settings.ProjectionAnchorDate == default ? asOf : plan.Settings.ProjectionAnchorDate;
        var firstPeriodStart = firstPeriodStartDate ?? _periodCalculator.GetFirstPeriodStartOnOrAfter(anchor, plan.Settings.PeriodAnchor);
        ValidateProjectionBoundary(anchor, firstPeriodStart, plan.Settings.PeriodAnchor);

        var periods = _periodCalculator.GetPeriods(firstPeriodStart, plan.Settings.PeriodAnchor, periodCount);
        var cardBundle = BuildCardPayments(plan.CreditCards, periods[^1].End, plan.Settings.CreditCardCarryInterestRate);

        var obligations = _mandatoryPaymentCalculator
            .BuildObligations(plan.Loans, plan.LoanPrepayments, plan.PaymentPlans, cardBundle.Obligations)
            .Concat(BuildLargeExpenseObligations(plan.PlannedLargeExpenses))
            .ToArray();

        var obligationPlan = _periodObligationGrouper.Group(periods, obligations);
        var statuses = AssignCardStatuses(cardBundle.Statuses, periods);
        var projections = new List<CashFlowPeriodProjection>(periods.Count);
        var openingBalance = plan.Settings.ProjectionOpeningBalance;

        foreach (var period in periods)
        {
            var isFirst = period.Start == firstPeriodStart;
            var projection = BuildPeriodProjection(plan, period, isFirst ? anchor : null, obligationPlan, statuses, ref openingBalance);
            projections.Add(projection);
        }

        return new FinancialProjectionResult(projections, obligationPlan, statuses);
    }

    private CashFlowPeriodProjection BuildPeriodProjection(
        FinancialPlan plan, CashFlowPeriod period, DateOnly? prePeriodIncomeStart,
        PeriodObligationPlan obligationPlan, IReadOnlyList<CreditCardPaymentProjectionStatus> statuses,
        ref decimal openingBalance)
    {
        var income = _incomeProjectionCalculator.Calculate(
            period, plan.RecurringIncomes, plan.IncomeHistories, plan.AdHocIncomes, prePeriodIncomeStart);
        var periodGroup = obligationPlan[period];
        var mandatory = _mandatoryPaymentCalculator.Summarize(periodGroup.Items);
        var available = income.TotalIncome - mandatory.Total;
        var largeExpenses = periodGroup.LargeExpenseItems
            .Select(item => plan.PlannedLargeExpenses.Single(x => x.Id == item.PaymentId))
            .OrderBy(x => x.ExactDate).ThenBy(x => x.Name).ToArray();
        var largeTotal = largeExpenses.Sum(x => x.Amount);
        var surplus = available - plan.Settings.PeriodVariableExpenseAllowance - largeTotal;
        var cardStatuses = statuses.Where(x => x.AssignedPeriodDate == period.Start).ToArray();

        var endingBeforeDeficit = openingBalance + surplus;
        var deficitInterest = DeficitFinancingRules.CalculateInterest(endingBeforeDeficit, plan.Settings.DeficitFinancingInterestRate);
        var ending = endingBeforeDeficit - deficitInterest;

        var projection = new CashFlowPeriodProjection
        {
            Period = period, RecurringIncomeTotal = income.RecurringTotal, AdHocIncomeTotal = income.AdHocTotal,
            TotalIncome = income.TotalIncome, LoanPayments = mandatory.LoanPayments, CreditCardPayments = mandatory.CreditCardPayments,
            TemporaryPayments = mandatory.TemporaryPayments, InstallmentPayments = mandatory.InstallmentPayments,
            OtherScheduledPayments = mandatory.OtherScheduledPayments, MandatoryOutflow = mandatory.Total,
            AvailableAfterMandatory = available, VariableExpenseAllowance = plan.Settings.PeriodVariableExpenseAllowance,
            EstimatedSurplus = surplus, PlannedLargeCashExpenses = largeTotal, OpeningBalance = openingBalance,
            EndingBalanceBeforeDeficitInterest = endingBeforeDeficit, DeficitFinancingInterest = deficitInterest,
            EndingBalance = ending, CardInterestGenerated = cardStatuses.Sum(x => x.CarryInterest),
            AppliedDeficitInterestRate = plan.Settings.DeficitFinancingInterestRate, ProjectionAnchorDate = plan.Settings.ProjectionAnchorDate,
            IsEstimatedCardPayment = cardStatuses.Any(x => x.Resolution == CreditCardPaymentResolution.ProjectionFallback),
            HasUndeterminedCardPayment = cardStatuses.Any(x => x.Resolution == CreditCardPaymentResolution.Undetermined),
            HasDeficit = available < 0m || surplus < 0m || ending < 0m,
            IncomeItems = income.Items, MandatoryItems = mandatory.Items, LargeExpenseItems = largeExpenses, CardPaymentStatuses = cardStatuses
        };

        openingBalance = ending;
        return projection;
    }

    private CardPaymentBundle BuildCardPayments(IEnumerable<CreditCard> cards, DateOnly horizonEnd, decimal carryRate)
    {
        var obligations = new List<ObligationItem>();
        var statuses = new List<CreditCardPaymentProjectionStatus>();

        foreach (var card in cards.Where(x => x.IsActive))
        {
            var firstClose = _cardStatementCalculator.ResolveStatementCloseOnOrAfter(card.BalanceAsOfDate, card.StatementClosingDay);
            var count = Math.Max(2, MonthDistance(firstClose, horizonEnd) + 3);
            var name = $"{card.Bank} {card.Name}".Trim();

            foreach (var stmt in _cardStatementCalculator.Project(card, count, true, carryRate).Where(x => x.PaymentDueDate < horizonEnd))
            {
                statuses.Add(MapCardStatus(card.Id, name, stmt));
                if (stmt.Payment is decimal payment)
                {
                    obligations.Add(MapCardObligation(card.Id, name, stmt, payment));
                }
            }
        }

        return new CardPaymentBundle(obligations, statuses);
    }

    private static CreditCardPaymentProjectionStatus MapCardStatus(Guid id, string name, CreditCardStatementProjection s) => new()
    {
        CardId = id, CardName = name, StatementCloseDate = s.StatementCloseDate, PaymentDueDate = s.PaymentDueDate,
        StatementBalance = s.StatementBalance, MinimumPayment = s.MinimumPayment, Payment = s.Payment,
        OpeningCarriedBalance = s.OpeningCarriedBalance, NewCharges = s.NewCharges,
        CarriedPrincipalAfterPayment = s.CarriedAfterPayment, CarryInterest = s.CarryInterest,
        NextCarriedBalance = s.NextCarriedBalance, AppliedInterestRate = s.AppliedInterestRate,
        Resolution = s.PaymentResolution, PaymentType = s.AppliedPaymentType
    };

    private static ObligationItem MapCardObligation(Guid id, string name, CreditCardStatementProjection s, decimal payment) => new(
        name, ObligationType.CreditCard, s.PaymentDueDate, payment)
    {
        IsEstimate = s.PaymentResolution == CreditCardPaymentResolution.ProjectionFallback,
        Detail = s.PaymentResolution == CreditCardPaymentResolution.DueDateOverride ? "Ekstre ödeme planı" : "Kart ödeme tercihi",
        PaymentId = id
    };

    private static CreditCardPaymentProjectionStatus[] AssignCardStatuses(
        IReadOnlyList<CreditCardPaymentProjectionStatus> statuses, IReadOnlyList<CashFlowPeriod> periods) =>
        statuses.Select(s => s with { AssignedPeriodDate = periods.FirstOrDefault(p => p.Contains(s.PaymentDueDate))?.Start ?? default }).ToArray();

    private static IEnumerable<ObligationItem> BuildLargeExpenseObligations(IEnumerable<PlannedLargeExpense> expenses) =>
        expenses.Where(x => x.Status == PlannedExpenseStatus.Planned)
            .Select(x => new ObligationItem(x.Name, ObligationType.PlannedLargeExpense, x.ExactDate, x.Amount)
            {
                Detail = x.Note,
                PaymentId = x.Id
            });

    private void ValidateProjectionBoundary(DateOnly anchor, DateOnly firstPeriodStart, PeriodAnchor periodAnchor)
    {
        if (_periodCalculator.GetPeriod(firstPeriodStart, periodAnchor).Start != firstPeriodStart)
        {
            throw new InvalidOperationException("Projeksiyon başlangıç dönemi geçerli bir dönem tarihi olmalıdır.");
        }

        if (firstPeriodStart < _periodCalculator.GetFirstPeriodStartOnOrAfter(anchor, periodAnchor))
        {
            throw new InvalidOperationException("Projeksiyon başlangıç dönemi planlama çapa tarihinden önce olamaz.");
        }
    }

    private static int MonthDistance(DateOnly from, DateOnly to) => ((to.Year - from.Year) * 12) + to.Month - from.Month;

    private sealed record CardPaymentBundle(IReadOnlyList<ObligationItem> Obligations, IReadOnlyList<CreditCardPaymentProjectionStatus> Statuses);
}
