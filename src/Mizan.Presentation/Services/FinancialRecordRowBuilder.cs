using Mizan.Application.Abstractions;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.ViewModels;

namespace Mizan.Presentation.Services;

/// <summary>
/// Finansal planı Finansal Yapı'nın dört grubuna çevirir. Yalnız plana giren kayıtları alır ve
/// kartın tutarını kart kontroldeki hero rakamla aynı hesaptan (sıradaki ödeme) çıkarır; böylece
/// iki ekran aynı kart için farklı sayı göstermez (EK-V6, S62-4, S62-5).
/// </summary>
public sealed class FinancialRecordRowBuilder
{
    // Kart kontroldeki ufukla aynı: sıfır tutarlı vadeler atlandığı için sıradaki ödemeye pay bırakır.
    private const int ProjectedStatementCount = 8;

    private readonly CreditCardStatementCalculator _calculator;
    private readonly IncomeResolver _incomeResolver;
    private readonly IClock _clock;

    /// <summary>Üreticiyi kart hesaplayıcısı, gelir çözümleyici ve saatle başlatır.</summary>
    public FinancialRecordRowBuilder(CreditCardStatementCalculator calculator, IncomeResolver incomeResolver, IClock clock)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _incomeResolver = incomeResolver ?? throw new ArgumentNullException(nameof(incomeResolver));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Plandaki kayıtları gruplara ayırır, süzer ve sıralar.</summary>
    public FinancialStructureRows Build(FinancialPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var today = _clock.Today;
        return new FinancialStructureRows(Incomes(plan, today), Cards(plan), Loans(plan), Payments(plan));
    }

    private FinancialRecordRow[] Incomes(FinancialPlan plan, DateOnly today)
    {
        var current = _incomeResolver.Resolve(today, plan.RecurringIncomes, plan.IncomeHistories)
            .ToDictionary(x => x.RecurringIncomeId, x => x.Amount);
        var recurring = plan.RecurringIncomes
            .Where(x => x.IsActive)
            .OrderBy(x => x.PaymentDay)
            .Select(x => new FinancialRecordRow(
                x.Id, FinancialRecordKind.RecurringIncome, x.Name,
                current.TryGetValue(x.Id, out var amount) ? amount : FirstFutureAmount(plan.IncomeHistories, x.Id),
                null)
            {
                DayOfMonth = x.PaymentDay
            });
        var adHoc = plan.AdHocIncomes
            .Where(x => x.ExactDate >= today)
            .OrderBy(x => x.ExactDate)
            .Select(x => new FinancialRecordRow(x.Id, FinancialRecordKind.AdHocIncome, x.Description, x.Amount, x.ExactDate));
        return [.. recurring, .. adHoc];
    }

    // Tutarı henüz yürürlüğe girmemiş gelir (ör. gelecek ay başlayan kira) ilk tutarıyla görünür.
    private static decimal? FirstFutureAmount(IEnumerable<IncomeAmountHistory> histories, Guid incomeId) =>
        histories.Where(x => x.RecurringIncomeId == incomeId).MinBy(x => x.EffectiveDate)?.Amount;

    private FinancialRecordRow[] Cards(FinancialPlan plan)
    {
        var rate = plan.Settings.CreditCardCarryInterestRate;
        return plan.CreditCards
            .Where(x => x.IsActive)
            .Select(card =>
            {
                var projections = _calculator.Project(card, ProjectedStatementCount, true, rate);
                var index = NextPaymentViewModel.FindPaymentIndex(projections);
                var next = index >= 0 ? projections[index] : null;
                return new FinancialRecordRow(
                    card.Id, FinancialRecordKind.CreditCard, NameOrBank(card.Name, card.Bank), next?.Payment, next?.PaymentDueDate);
            })
            .OrderBy(x => x.NextDate ?? DateOnly.MaxValue)
            .ToArray();
    }

    private static FinancialRecordRow[] Loans(FinancialPlan plan) =>
        plan.Loans
            .Where(x => x.IsActive && x.RemainingInstallmentCount > 0)
            .OrderBy(x => x.NextPaymentDate)
            .Select(x => new FinancialRecordRow(
                x.Id, FinancialRecordKind.Loan, NameOrBank(x.Name, x.Bank), x.MonthlyPayment, x.NextPaymentDate)
            {
                RemainingCount = x.RemainingInstallmentCount
            })
            .ToArray();

    private static FinancialRecordRow[] Payments(FinancialPlan plan)
    {
        var plans = plan.PaymentPlans
            .Where(x => !x.IsCompleted)
            .Select(x => new FinancialRecordRow(
                x.Id, FinancialRecordKind.PaymentPlan, x.Name, x.NextInstallment?.Amount, x.NextInstallment?.DueDate)
            {
                RemainingCount = x.RemainingInstallmentCount
            });
        var expenses = plan.PlannedLargeExpenses
            .Where(x => x.IsActive)
            .Select(x => new FinancialRecordRow(x.Id, FinancialRecordKind.LargeExpense, x.Name, x.Amount, x.ExactDate));
        return plans.Concat(expenses).OrderBy(x => x.NextDate ?? DateOnly.MaxValue).ToArray();
    }

    private static string NameOrBank(string name, string bank) =>
        string.IsNullOrWhiteSpace(name) ? bank : name;
}
