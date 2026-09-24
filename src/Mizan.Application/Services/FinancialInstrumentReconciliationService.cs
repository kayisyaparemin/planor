using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Dönem mutabakatı ve kapanışında gerçekleşen fiili ödemeleri borç ve harcama enstrümanlarına uygulayarak
/// bir sonraki döneme devreden kanonik finansal sözleşme durumlarını güncelleyen uygulama servisidir.
/// </summary>
public sealed class FinancialInstrumentReconciliationService(
    CreditCardActualPaymentReconciler cardReconciler,
    CreditCardStatementCalculator cardStatementCalculator,
    LoanInstrumentReconciler loanReconciler)
{
    private readonly CreditCardActualPaymentReconciler _cardReconciler =
        cardReconciler ?? throw new ArgumentNullException(nameof(cardReconciler));
    private readonly CreditCardStatementCalculator _cardStatementCalculator =
        cardStatementCalculator ?? throw new ArgumentNullException(nameof(cardStatementCalculator));
    private readonly LoanInstrumentReconciler _loanReconciler =
        loanReconciler ?? throw new ArgumentNullException(nameof(loanReconciler));

    /// <summary>
    /// Kapanan dönemin dondurulmuş plan satırları ve kullanıcının fiili ödeme bildirimlerini
    /// finansal plana uygulayarak bir sonraki döneme devreden sözleşme durumlarını üretir.
    /// </summary>
    public ReconciledFinancialInstruments Apply(
        FinancialPlan data,
        IReadOnlyList<PeriodPlanPaymentLine> paymentLines,
        IReadOnlyList<ActualPayment> actualPayments,
        DateOnly newAnchor)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(paymentLines);
        ArgumentNullException.ThrowIfNull(actualPayments);

        var lines = paymentLines.ToDictionary(x => x.Id);
        var loans = data.Loans.ToDictionary(x => x.Id);
        var paymentPlans = data.PaymentPlans.ToDictionary(x => x.Id);
        var cards = data.CreditCards.ToDictionary(x => x.Id);
        var largeExpenses = data.PlannedLargeExpenses.ToDictionary(x => x.Id);
        var prepayments = data.LoanPrepayments.ToDictionary(x => x.Id);

        var carryDate = newAnchor;
        var unpaidLoanIds = new HashSet<Guid>();
        var removedPrepaymentIds = new HashSet<Guid>();

        ProcessActualPayments(
            actualPayments, lines, loans, paymentPlans, cards, largeExpenses, prepayments,
            data.Settings.CreditCardCarryInterestRate, newAnchor, carryDate, unpaidLoanIds, removedPrepaymentIds);

        _loanReconciler.FinalizeUnpaidLoans(loans, unpaidLoanIds, carryDate);
        FinalizeUnpaidPaymentPlans(paymentPlans, newAnchor, carryDate);
        FinalizeUnpaidLargeExpenses(largeExpenses, newAnchor, carryDate);
        _loanReconciler.FinalizeStalePrepayments(data.LoanPrepayments, newAnchor, removedPrepaymentIds);

        return new ReconciledFinancialInstruments
        {
            Loans = loans.Values.OrderBy(x => x.NextPaymentDate).ToArray(),
            PaymentPlans = paymentPlans.Values.OrderBy(x => x.Name).ToArray(),
            CreditCards = cards.Values.OrderBy(x => x.Name).ToArray(),
            LargeExpenses = largeExpenses.Values.OrderBy(x => x.ExactDate).ToArray(),
            RemovedLoanPrepaymentIds = removedPrepaymentIds.ToArray()
        };
    }

    private void ProcessActualPayments(
        IReadOnlyList<ActualPayment> actualPayments,
        Dictionary<Guid, PeriodPlanPaymentLine> lines,
        Dictionary<Guid, Loan> loans,
        Dictionary<Guid, TemporaryPaymentPlan> paymentPlans,
        Dictionary<Guid, CreditCard> cards,
        Dictionary<Guid, PlannedLargeExpense> largeExpenses,
        Dictionary<Guid, LoanPrepayment> prepayments,
        decimal carryInterestRate,
        DateOnly newAnchor,
        DateOnly carryDate,
        HashSet<Guid> unpaidLoanIds,
        HashSet<Guid> removedPrepaymentIds)
    {
        var ordered = actualPayments
            .OrderBy(x => lines.TryGetValue(x.PeriodPlanPaymentLineId, out var candidate) ? candidate.PlannedDate : DateOnly.MinValue)
            .ThenBy(x => lines.TryGetValue(x.PeriodPlanPaymentLineId, out var c) && prepayments.ContainsKey(c.SourceEntityId));

        foreach (var actual in ordered)
        {
            if (!lines.TryGetValue(actual.PeriodPlanPaymentLineId, out var line))
            {
                throw new InvalidOperationException("Planlanan ödeme satırı bulunamadı.");
            }

            var paid = actual.Status != ActualPaymentStatus.Unpaid && actual.ActualAmount > 0m;
            switch (actual.SourceType)
            {
                case PlanPaymentSourceType.Loan:
                    _loanReconciler.ProcessPayment(line, actual, paid, loans, prepayments, removedPrepaymentIds, unpaidLoanIds, newAnchor, carryDate);
                    break;
                case PlanPaymentSourceType.TemporaryPayment:
                case PlanPaymentSourceType.InstallmentPayment:
                case PlanPaymentSourceType.OtherScheduledPayment:
                    ProcessPaymentPlanPayment(line, paid, paymentPlans, newAnchor, carryDate);
                    break;
                case PlanPaymentSourceType.CreditCard:
                    ProcessCreditCardPayment(line, actual, paid, cards, carryInterestRate);
                    break;
                case PlanPaymentSourceType.PlannedLargeExpense:
                    ProcessLargeExpensePayment(line, paid, largeExpenses, newAnchor, carryDate);
                    break;
            }
        }
    }

    private static void ProcessPaymentPlanPayment(
        PeriodPlanPaymentLine line, bool paid, Dictionary<Guid, TemporaryPaymentPlan> paymentPlans, DateOnly newAnchor, DateOnly carryDate)
    {
        var parent = paymentPlans.Values.SingleOrDefault(x => x.Installments.Any(i => i.Id == line.SourceEntityId))
            ?? throw new InvalidOperationException("Planlı ödeme kaydı bulunamadı.");

        paymentPlans[parent.Id] = parent with
        {
            Installments = parent.Installments.Select(item =>
                item.Id != line.SourceEntityId ? item :
                paid ? item with { IsPaid = true } :
                item.DueDate <= newAnchor ? item with { DueDate = carryDate } : item).ToArray()
        };
    }

    private void ProcessCreditCardPayment(
        PeriodPlanPaymentLine line, ActualPayment actual, bool paid, Dictionary<Guid, CreditCard> cards, decimal carryInterestRate)
    {
        var card = cards.GetValueOrDefault(line.SourceEntityId)
            ?? throw new InvalidOperationException("Kredi kartı bulunamadı.");

        var projections = _cardStatementCalculator.Project(card, 12, useProjectionFallback: true, carryInterestRate: carryInterestRate);
        cards[card.Id] = _cardReconciler.Apply(card, projections, line.PlannedDate, paid ? actual.ActualAmount : 0m);
    }

    private static void ProcessLargeExpensePayment(
        PeriodPlanPaymentLine line, bool paid, Dictionary<Guid, PlannedLargeExpense> largeExpenses, DateOnly newAnchor, DateOnly carryDate)
    {
        var expense = largeExpenses.GetValueOrDefault(line.SourceEntityId)
            ?? throw new InvalidOperationException("Planlı büyük ödeme bulunamadı.");

        largeExpenses[expense.Id] = paid
            ? expense with { Status = PlannedExpenseStatus.Completed }
            : expense.ExactDate <= newAnchor ? expense with { ExactDate = carryDate } : expense;
    }

    private static void FinalizeUnpaidPaymentPlans(
        Dictionary<Guid, TemporaryPaymentPlan> paymentPlans, DateOnly newAnchor, DateOnly carryDate)
    {
        foreach (var key in paymentPlans.Keys.ToArray())
        {
            var plan = paymentPlans[key];
            paymentPlans[key] = plan with
            {
                Installments = plan.Installments.Select(item =>
                    !item.IsPaid && item.DueDate <= newAnchor ? item with { DueDate = carryDate } : item).ToArray()
            };
        }
    }

    private static void FinalizeUnpaidLargeExpenses(
        Dictionary<Guid, PlannedLargeExpense> largeExpenses, DateOnly newAnchor, DateOnly carryDate)
    {
        foreach (var key in largeExpenses.Keys.ToArray())
        {
            var expense = largeExpenses[key];
            if (expense.Status == PlannedExpenseStatus.Planned && expense.ExactDate <= newAnchor)
            {
                largeExpenses[key] = expense with { ExactDate = carryDate };
            }
        }
    }
}
