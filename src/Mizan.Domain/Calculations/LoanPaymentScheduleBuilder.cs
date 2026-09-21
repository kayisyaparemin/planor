using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredinin takvim taksitleri üzerine tanımlanmış kısmi ara ödeme ve erken kapama olaylarını
/// kronolojik sırayla oynatarak (replay) güncel ödeme takvimini ve ara durumları hesaplayan saf Domain motoru.
/// </summary>
public sealed class LoanPaymentScheduleBuilder(
    LoanScheduleCalculator scheduleCalculator,
    LoanAmortizationCalculator amortizationCalculator)
{
    /// <summary>Krediye ait erken ödeme olaylarını kronolojik olarak takvim üzerinde oynatır.</summary>
    public LoanReplay Replay(Loan loan, IEnumerable<LoanPrepayment> prepayments)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(prepayments);

        var events = prepayments.Where(x => x.LoanId == loan.Id)
            .OrderBy(x => x.Date).ThenBy(x => x.Mode == LoanPrepaymentMode.FullClosure).ThenBy(x => x.Id).ToArray();

        if (!loan.IsActive || loan.RemainingInstallmentCount < 1)
        {
            return Empty(loan);
        }
        if (events.Length == 0)
        {
            return Plain(loan, ignored: false);
        }
        if (amortizationCalculator.Analyze(loan).Amortization is not { } amortization)
        {
            return Plain(loan, ignored: true);
        }

        return ExecuteReplay(loan, amortization, events);
    }

    private LoanReplay ExecuteReplay(Loan loan, LoanAmortization amortization, LoanPrepayment[] events)
    {
        var dates = scheduleCalculator.GetPaymentDates(loan);
        var context = new ReplayContext(amortization);
        var payments = new List<LoanScheduledPayment>();
        var states = new Dictionary<Guid, Loan>();
        var principals = new Dictionary<Guid, decimal>();

        foreach (var prepayment in events)
        {
            if (prepayment.Date < amortization.PreviousDueDate)
            {
                continue;
            }

            AdvanceInstallments(loan, dates, prepayment.Date, context, payments);
            if (context.Index >= context.Count || context.Principal <= 0m)
            {
                break;
            }

            principals[prepayment.Id] = context.Principal;
            if (ProcessPrepayment(loan, prepayment, dates, context, payments, states))
            {
                return new LoanReplay(loan, payments, states, principals, false);
            }
        }

        AppendRemainingInstallments(loan, dates, context, payments);
        return new LoanReplay(loan, payments, states, principals, false);
    }

    private static void AdvanceInstallments(
        Loan loan, IReadOnlyList<DateOnly> dates, DateOnly targetDate, ReplayContext context, List<LoanScheduledPayment> payments)
    {
        while (context.Index < context.Count && dates[context.Index] <= targetDate)
        {
            var isLast = context.Index == context.Count - 1;
            var amount = isLast ? context.FinalPayment : context.MonthlyPayment;
            payments.Add(new LoanScheduledPayment(dates[context.Index], amount, LoanPaymentKind.Installment, loan.Id, isLast));

            context.Principal = isLast ? 0m : MoneyRules.Round((context.Principal * (1m + context.Rate)) - context.MonthlyPayment);
            context.LastPaidDate = dates[context.Index++];
        }
    }

    private static bool ProcessPrepayment(
        Loan loan, LoanPrepayment prepayment, IReadOnlyList<DateOnly> dates,
        ReplayContext context, List<LoanScheduledPayment> payments, Dictionary<Guid, Loan> states)
    {
        var remaining = context.Count - context.Index;
        var days = Math.Max(0, prepayment.Date.DayNumber - context.LastPaidDate.DayNumber);
        var closes = prepayment.Mode == LoanPrepaymentMode.FullClosure ||
                     prepayment.PrincipalAmount is null || prepayment.PrincipalAmount >= context.Principal;

        var reduction = closes ? context.Principal : Math.Max(0m, prepayment.PrincipalAmount!.Value);
        var fee = MoneyRules.Round(reduction * LoanAmortizationCalculator.PrepaymentFeeRate(loan.Kind, remaining));
        var amount = MoneyRules.Round(reduction * (1m + (context.Rate * days / 30m))) + fee;

        if (closes)
        {
            payments.Add(new LoanScheduledPayment(prepayment.Date, amount, LoanPaymentKind.EarlyClosure, prepayment.Id, true));
            states[prepayment.Id] = CreateClosedLoanState(loan);
            return true;
        }

        payments.Add(new LoanScheduledPayment(prepayment.Date, amount, LoanPaymentKind.PartialPrepayment, prepayment.Id, false));
        context.Principal -= reduction;
        ApplyPrepaymentRestructuring(prepayment.Mode, context, ref remaining);
        context.Count = context.Index + remaining;
        states[prepayment.Id] = CreateRestructuredLoanState(loan, dates[context.Index], remaining, context);
        return false;
    }

    private static void ApplyPrepaymentRestructuring(LoanPrepaymentMode mode, ReplayContext context, ref int remaining)
    {
        if (mode == LoanPrepaymentMode.ReduceTerm)
        {
            (remaining, context.FinalPayment) = ShortenTerm(context.Principal, context.Rate, context.MonthlyPayment);
            return;
        }

        var annuity = 1m - (decimal)Math.Pow(1d + (double)context.Rate, -remaining);
        context.MonthlyPayment = MoneyRules.Round(context.Principal * context.Rate / annuity);
        context.FinalPayment = LastInstallment(context.Principal, context.Rate, context.MonthlyPayment, remaining);
    }

    private static void AppendRemainingInstallments(
        Loan loan, IReadOnlyList<DateOnly> dates, ReplayContext context, List<LoanScheduledPayment> payments)
    {
        for (; context.Index < context.Count; context.Index++)
        {
            var isLast = context.Index == context.Count - 1;
            var amount = isLast ? context.FinalPayment : context.MonthlyPayment;
            payments.Add(new LoanScheduledPayment(dates[context.Index], amount, LoanPaymentKind.Installment, loan.Id, isLast));
        }
    }

    private LoanReplay Plain(Loan loan, bool ignored)
    {
        var dates = scheduleCalculator.GetPaymentDates(loan);
        var payments = dates.Select((date, index) => new LoanScheduledPayment(
            date, index == dates.Count - 1 ? loan.LastInstallmentAmount : loan.MonthlyPayment,
            LoanPaymentKind.Installment, loan.Id, index == dates.Count - 1)).ToArray();

        return new LoanReplay(loan, payments, new Dictionary<Guid, Loan>(), new Dictionary<Guid, decimal>(), ignored);
    }

    private static LoanReplay Empty(Loan loan) =>
        new(loan, [], new Dictionary<Guid, Loan>(), new Dictionary<Guid, decimal>(), false);

    private static (int Count, decimal Final) ShortenTerm(decimal principal, decimal rate, decimal payment)
    {
        var balance = principal;
        for (var count = 1; count <= 1_200; count++)
        {
            var due = balance * (1m + rate);
            if (due <= payment)
            {
                return (count, MoneyRules.Round(due));
            }
            balance = due - payment;
        }
        throw new InvalidOperationException("Taksit, kalan anaparanın faizini karşılamıyor.");
    }

    private static decimal LastInstallment(decimal principal, decimal rate, decimal payment, int count)
    {
        var balance = principal;
        for (var index = 1; index < count; index++)
        {
            balance = (balance * (1m + rate)) - payment;
        }
        return MoneyRules.Round(Math.Max(0m, balance * (1m + rate)));
    }

    private static Loan CreateClosedLoanState(Loan loan) => loan with
    {
        RemainingInstallmentCount = 0, RemainingDebt = loan.RemainingDebt is null ? null : 0m,
        FinalPaymentAmount = null, EarlyClosureAmount = null, EarlyClosureAmountAsOf = null, IsActive = false
    };

    private static Loan CreateRestructuredLoanState(Loan loan, DateOnly nextDate, int remaining, ReplayContext context) => loan with
    {
        NextPaymentDate = nextDate, RemainingInstallmentCount = remaining, MonthlyPayment = context.MonthlyPayment,
        FinalPaymentAmount = context.FinalPayment == context.MonthlyPayment ? null : context.FinalPayment,
        RemainingDebt = context.Principal, EarlyClosureAmount = null, EarlyClosureAmountAsOf = null
    };

    private sealed class ReplayContext(LoanAmortization amortization)
    {
        public decimal Rate { get; } = amortization.MonthlyRate;
        public decimal Principal { get; set; } = amortization.Principal;
        public decimal MonthlyPayment { get; set; } = amortization.MonthlyPayment;
        public decimal FinalPayment { get; set; } = amortization.FinalPayment;
        public int Count { get; set; } = amortization.RemainingInstallments;
        public int Index { get; set; }
        public DateOnly LastPaidDate { get; set; } = amortization.PreviousDueDate;
    }
}
