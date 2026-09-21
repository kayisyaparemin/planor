using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Bankanın açıkladığı taksit tutarından örtük faiz oranını bisection ile geri çözen,
/// tarihli erken kapama bedelini ve yasal komisyonları hesaplayan saf Domain motoru.
/// </summary>
public sealed class LoanAmortizationCalculator(LoanScheduleCalculator scheduleCalculator)
{
    /// <summary>Makul kabul edilebilecek tavan aylık efektif faiz oranı (%8).</summary>
    public const decimal MaxPlausibleMonthlyRate = 0.08m;

    /// <summary>Krediyi analiz ederek örtük faizi ve itfa modelini türetir.</summary>
    public LoanAnalysis Analyze(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        if (!loan.IsActive || loan.RemainingInstallmentCount < 1)
        {
            return new LoanAnalysis(loan, null, LoanAnalysisIssue.Finished);
        }

        var previousDue = PreviousDueDate(loan);
        if (FromBankQuote(loan, previousDue) is { } quoted)
        {
            return new LoanAnalysis(loan, quoted, LoanAnalysisIssue.None);
        }

        return AnalyzeFromPrincipal(loan, previousDue);
    }

    /// <summary>Belirli sayıda taksit daha ödendikten sonra kalan anaparayı hesaplar.</summary>
    public static decimal PrincipalAfter(LoanAmortization amortization, int installmentsPaid)
    {
        ArgumentNullException.ThrowIfNull(amortization);
        if (installmentsPaid <= 0)
        {
            return amortization.Principal;
        }
        if (installmentsPaid >= amortization.RemainingInstallments)
        {
            return 0m;
        }

        var rate = amortization.MonthlyRate;
        var growth = Pow(1m + rate, installmentsPaid);
        var balance = (amortization.Principal * growth) - (amortization.MonthlyPayment * (growth - 1m) / rate);
        return MoneyRules.Round(Math.Max(0m, balance));
    }

    /// <summary>Tek taksit ödendiğinde yalnızca anapara payını düşerek yeni anaparayı bulur.</summary>
    public static decimal PrincipalAfterPayment(LoanAmortization amortization, decimal paidAmount)
    {
        ArgumentNullException.ThrowIfNull(amortization);
        var interest = amortization.Principal * amortization.MonthlyRate;
        return MoneyRules.Round(Math.Max(0m, amortization.Principal - (paidAmount - interest)));
    }

    /// <summary>Belirtilen takvim gününde krediyi kapatmanın detaylı maliyet teklifini üretir.</summary>
    public LoanPayoffQuote PayoffOn(Loan loan, LoanAmortization amortization, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(amortization);

        var dates = scheduleCalculator.GetPaymentDates(loan);
        var paidBefore = dates.Count(x => x <= date);
        var removed = Math.Max(0, amortization.RemainingInstallments - paidBefore);
        if (removed == 0)
        {
            return new LoanPayoffQuote { Date = date, InstallmentsPaidBefore = paidBefore };
        }

        var principal = PrincipalAfter(amortization, paidBefore);
        var lastPaid = paidBefore == 0 ? amortization.PreviousDueDate : dates[paidBefore - 1];
        var days = Math.Max(0, date.DayNumber - lastPaid.DayNumber);
        var accrued = MoneyRules.Round(principal * amortization.MonthlyRate * days / 30m);
        var fee = MoneyRules.Round(principal * PrepaymentFeeRate(loan.Kind, removed));

        return new LoanPayoffQuote
        {
            Date = date, InstallmentsPaidBefore = paidBefore, Principal = principal,
            AccruedInterest = accrued, Fee = fee, InstallmentsRemoved = removed,
            RemovedInstallmentTotal = (amortization.MonthlyPayment * (removed - 1)) + amortization.FinalPayment
        };
    }

    /// <summary>6502 sayılı Kanun md. 27 ve 37 erken ödeme ücreti tavan oranları.</summary>
    public static decimal PrepaymentFeeRate(LoanKind kind, int remainingInstallments) => kind switch
    {
        LoanKind.HousingFixed => remainingInstallments > 36 ? 0.02m : 0.01m,
        _ => 0m
    };

    /// <summary>Kredinin önceki taksit vade tarihini takvim kuralıyla hesaplar.</summary>
    public static DateOnly PreviousDueDate(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        return CalendarRules.AddMonthsKeepingDay(loan.NextPaymentDate, -1, loan.PaymentDay);
    }

    /// <summary>Bisection yöntemiyle aylık efektif faizi çözer.</summary>
    public static decimal SolveMonthlyRate(decimal principal, decimal payment, int count, decimal? finalPayment = null) =>
        Bisect((double)principal, r => Valuation(r, count, (double)payment, (double)(finalPayment ?? payment)));

    /// <summary>Kalan taksitlerin bugünkü değerini annüite formülüyle hesaplar.</summary>
    public static double Valuation(double rate, int count, double payment, double finalPayment) =>
        count < 1 ? 0d : (payment * Annuity(rate, count - 1)) + (finalPayment * Math.Pow(1d + rate, -count));

    private static LoanAnalysis AnalyzeFromPrincipal(Loan loan, DateOnly previousDue)
    {
        if (loan.RemainingDebt is not decimal principal || principal <= 0m)
        {
            return new LoanAnalysis(loan, null, LoanAnalysisIssue.MissingPrincipal);
        }
        if (principal >= loan.RemainingInstallmentTotal)
        {
            return new LoanAnalysis(loan, null, LoanAnalysisIssue.PrincipalNotBelowInstallments);
        }
        var rate = SolveMonthlyRate(principal, loan.MonthlyPayment, loan.RemainingInstallmentCount, loan.FinalPaymentAmount);
        if (rate > MaxPlausibleMonthlyRate)
        {
            return new LoanAnalysis(loan, null, LoanAnalysisIssue.ImplausibleRate);
        }
        return new LoanAnalysis(loan, CreateAmortization(loan, rate, principal, previousDue, LoanRateSource.RemainingPrincipal), LoanAnalysisIssue.None);
    }

    private LoanAmortization? FromBankQuote(Loan loan, DateOnly previousDue)
    {
        if (loan.EarlyClosureAmount is not decimal amount || amount <= 0m ||
            loan.EarlyClosureAmountAsOf is not DateOnly asOf || asOf < previousDue)
        {
            return null;
        }

        var dates = scheduleCalculator.GetPaymentDates(loan);
        var paidBefore = dates.Count(x => x <= asOf);
        var remaining = loan.RemainingInstallmentCount - paidBefore;
        if (remaining < 1 || amount >= (loan.MonthlyPayment * (remaining - 1)) + loan.LastInstallmentAmount)
        {
            return null;
        }

        var lastPaid = paidBefore == 0 ? previousDue : dates[paidBefore - 1];
        var days = Math.Max(0, asOf.DayNumber - lastPaid.DayNumber);
        var rate = Bisect((double)amount, r => Valuation(r, remaining, (double)loan.MonthlyPayment, (double)loan.LastInstallmentAmount) * (1d + (r * days / 30d)));
        if (rate > MaxPlausibleMonthlyRate)
        {
            return null;
        }

        var principal = MoneyRules.Round((decimal)Valuation((double)rate, loan.RemainingInstallmentCount, (double)loan.MonthlyPayment, (double)loan.LastInstallmentAmount));
        return CreateAmortization(loan, rate, principal, previousDue, LoanRateSource.BankQuote);
    }

    private static LoanAmortization CreateAmortization(Loan loan, decimal rate, decimal principal, DateOnly previousDue, LoanRateSource source) => new()
    {
        MonthlyRate = rate, Principal = principal, RemainingInstallments = loan.RemainingInstallmentCount,
        MonthlyPayment = loan.MonthlyPayment, FinalPayment = loan.LastInstallmentAmount,
        PreviousDueDate = previousDue, Source = source
    };

    private static decimal Bisect(double target, Func<double, double> value)
    {
        const double upper = 1d;
        if (value(upper) > target)
        {
            return (decimal)upper;
        }

        var low = 0d;
        var high = upper;
        for (var iteration = 0; iteration < 200; iteration++)
        {
            var middle = (low + high) / 2d;
            if (value(middle) > target)
            {
                low = middle;
            }
            else
            {
                high = middle;
            }
        }
        return decimal.Round((decimal)((low + high) / 2d), 10);
    }

    private static double Annuity(double rate, int count) =>
        rate <= 0d ? count : (1d - Math.Pow(1d + rate, -count)) / rate;

    private static decimal Pow(decimal value, int exponent)
    {
        var result = 1m;
        for (var index = 0; index < exponent; index++)
        {
            result *= value;
        }
        return result;
    }
}
