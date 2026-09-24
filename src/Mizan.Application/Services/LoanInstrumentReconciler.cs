using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Dönem kapanış mutabakatında kredilere ait taksit ödemelerini ve erken ödeme olaylarını
/// işleyerek kalan anaparayı (I17), yeni taksit tarihlerini (I4) ve erken ödeme tüketimini güncelleyen servistir.
/// </summary>
public sealed class LoanInstrumentReconciler(
    LoanAmortizationCalculator loanCalculator,
    LoanPaymentScheduleBuilder loanScheduleBuilder)
{
    private readonly LoanAmortizationCalculator _loanCalculator =
        loanCalculator ?? throw new ArgumentNullException(nameof(loanCalculator));
    private readonly LoanPaymentScheduleBuilder _loanScheduleBuilder =
        loanScheduleBuilder ?? throw new ArgumentNullException(nameof(loanScheduleBuilder));

    /// <summary>
    /// Krediye veya erken ödemeye ait tek bir ödeme satırını işler.
    /// </summary>
    public void ProcessPayment(
        PeriodPlanPaymentLine line,
        ActualPayment actual,
        bool paid,
        Dictionary<Guid, Loan> loans,
        Dictionary<Guid, LoanPrepayment> prepayments,
        HashSet<Guid> removedPrepaymentIds,
        HashSet<Guid> unpaidLoanIds,
        DateOnly newAnchor,
        DateOnly carryDate)
    {
        if (prepayments.TryGetValue(line.SourceEntityId, out var prepayment))
        {
            ApplyPrepayment(prepayment, paid, loans, removedPrepaymentIds);
            return;
        }

        if (!loans.TryGetValue(line.SourceEntityId, out var loan))
        {
            return;
        }

        if (paid)
        {
            ApplyPaidLoan(loan, line, actual, loans, newAnchor, carryDate);
        }
        else
        {
            ApplyUnpaidLoan(loan, loans, unpaidLoanIds, newAnchor, carryDate);
        }
    }

    /// <summary>
    /// Ödenmeyen kredilerin sonraki vade tarihini yeni dönemin başlangıcına taşır.
    /// </summary>
    public void FinalizeUnpaidLoans(
        Dictionary<Guid, Loan> loans,
        HashSet<Guid> unpaidLoanIds,
        DateOnly carryDate)
    {
        foreach (var loanId in unpaidLoanIds)
        {
            loans[loanId] = loans[loanId] with { NextPaymentDate = carryDate };
        }
    }

    /// <summary>
    /// Dönem kapanış tarihinden önce kalmış ancak plan satırına girmemiş erken ödemeleri tüketir.
    /// </summary>
    public void FinalizeStalePrepayments(
        IEnumerable<LoanPrepayment> prepayments,
        DateOnly newAnchor,
        HashSet<Guid> removedPrepaymentIds)
    {
        foreach (var stale in prepayments.Where(x => x.Date <= newAnchor))
        {
            removedPrepaymentIds.Add(stale.Id);
        }
    }

    private void ApplyPrepayment(
        LoanPrepayment prepayment,
        bool paid,
        Dictionary<Guid, Loan> loans,
        HashSet<Guid> removedPrepaymentIds)
    {
        removedPrepaymentIds.Add(prepayment.Id);
        if (paid && loans.TryGetValue(prepayment.LoanId, out var owner) &&
            _loanScheduleBuilder.Replay(owner, [prepayment]).StateAfterEvent.TryGetValue(prepayment.Id, out var closed))
        {
            loans[owner.Id] = closed;
        }
    }

    private void ApplyPaidLoan(
        Loan loan,
        PeriodPlanPaymentLine line,
        ActualPayment actual,
        Dictionary<Guid, Loan> loans,
        DateOnly newAnchor,
        DateOnly carryDate)
    {
        var remaining = Math.Max(0, loan.RemainingInstallmentCount - 1);
        var nextDate = ResolveOutstandingDate(CalendarRules.AddMonthsKeepingDay(line.PlannedDate, 1, loan.PaymentDay), newAnchor, carryDate);
        var paidLoan = loan with
        {
            NextPaymentDate = nextDate,
            RemainingInstallmentCount = remaining,
            RemainingDebt = RemainingPrincipalAfter(loan, actual.ActualAmount, remaining),
            IsActive = remaining > 0
        };
        loans[loan.Id] = DropStaleClosureQuote(paidLoan);
    }

    private static void ApplyUnpaidLoan(
        Loan loan,
        Dictionary<Guid, Loan> loans,
        HashSet<Guid> unpaidLoanIds,
        DateOnly newAnchor,
        DateOnly carryDate)
    {
        if (loan.NextPaymentDate <= newAnchor)
        {
            loans[loan.Id] = DropStaleClosureQuote(loan with { NextPaymentDate = carryDate });
        }
        unpaidLoanIds.Add(loan.Id);
    }

    private decimal? RemainingPrincipalAfter(Loan loan, decimal paidAmount, int remainingInstallments)
    {
        if (remainingInstallments == 0)
        {
            return loan.RemainingDebt is null ? null : 0m;
        }

        var analysis = _loanCalculator.Analyze(loan);
        return analysis.Amortization is { } amortization
            ? LoanAmortizationCalculator.PrincipalAfterPayment(amortization, paidAmount)
            : loan.RemainingDebt;
    }

    private static Loan DropStaleClosureQuote(Loan loan) =>
        loan.EarlyClosureAmountAsOf is DateOnly asOf &&
        asOf < LoanAmortizationCalculator.PreviousDueDate(loan)
            ? loan with { EarlyClosureAmount = null, EarlyClosureAmountAsOf = null }
            : loan;

    private static DateOnly ResolveOutstandingDate(DateOnly date, DateOnly newAnchor, DateOnly carryDate) =>
        date <= newAnchor ? carryDate : date;
}
