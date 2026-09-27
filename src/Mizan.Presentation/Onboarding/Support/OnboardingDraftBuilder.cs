using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Onboarding.Models;

namespace Mizan.Presentation.Onboarding.Support;

/// <summary>
/// Kurulum sihirbazında toplanan arayüz girdilerini ve taslak listelerini
/// dondurulmaya hazır <see cref="OnboardingDraft"/> paketine dönüştüren ve özet hesaplayan saf yardımcıdır.
/// </summary>
internal static class OnboardingDraftBuilder
{
    /// <summary>Girdilerden tam bir kurulum taslağı üretir.</summary>
    public static OnboardingDraft Build(
        int periodDay,
        decimal livingAllowance,
        decimal openingBalance,
        IReadOnlyList<RecurringIncome> incomes,
        IReadOnlyList<OnboardingDraftItem> draftIncomes,
        IReadOnlyList<CreditCard> cards,
        IReadOnlyList<Loan> loans,
        IReadOnlyList<TemporaryPaymentPlan> plans,
        IReadOnlyList<PlannedLargeExpense> expenses,
        DateOnly today)
    {
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(periodDay),
            PeriodVariableExpenseAllowance = livingAllowance,
            ProjectionOpeningBalance = openingBalance
        };

        var histories = incomes.Select(inc =>
        {
            var match = draftIncomes.FirstOrDefault(d => d.Id == inc.Id);
            return new IncomeAmountHistory
            {
                RecurringIncomeId = inc.Id,
                Amount = match?.Amount ?? 0m,
                EffectiveDate = today,
                Description = "Başlangıç Tutarı"
            };
        }).ToArray();

        var validatedCards = cards.Select(c =>
            c.BalanceAsOfDate == default ? c with { BalanceAsOfDate = today } : c
        ).ToArray();

        return new OnboardingDraft
        {
            Settings = settings,
            RecurringIncomes = incomes.ToArray(),
            IncomeAmountHistories = histories,
            CreditCards = validatedCards,
            Loans = loans.ToArray(),
            PaymentPlans = plans.ToArray(),
            PlannedLargeExpenses = expenses.ToArray()
        };
    }

    /// <summary>Adım başlığını sıra numarasına göre çözer.</summary>
    public static string ResolveTitle(int stepIndex) => stepIndex switch
    {
        1 => "Dönemini Ayarlayalım",
        2 => "Düzenli Gelirlerin",
        3 => "Kredi Kartların",
        4 => "Kredilerin",
        5 => "Yaklaşan Ödemelerin",
        6 => "Yaşam Giderin",
        7 => "Mevcut Nakit Paran",
        _ => "İlk Planın Hazır"
    };

    public static (RecurringIncome Model, OnboardingDraftItem Draft) CreateIncome(string name, decimal amount, int day)
    {
        var m = new RecurringIncome { Name = name.Trim(), PaymentDay = day };
        return (m, new OnboardingDraftItem(m.Id, m.Name, $"Her ayın {m.PaymentDay}'i", amount, OnboardingRecordKind.Income));
    }

    public static (CreditCard Model, OnboardingDraftItem Draft) CreateCard(string name, string bank, decimal limit, int closeDay, int dueDay, decimal? currentDebt, DateOnly today)
    {
        var debt = currentDebt ?? 0m;
        var m = new CreditCard
        {
            Name = name.Trim(), Bank = bank.Trim(), Limit = limit,
            StatementClosingDay = closeDay, PaymentDueDay = dueDay,
            CarriedBalance = debt, PaymentStrategy = CreditCardPaymentStrategy.FullStatement,
            BalanceAsOfDate = today
        };
        return (m, new OnboardingDraftItem(m.Id, m.Name, $"Kesim: {m.StatementClosingDay} · Vade: {m.PaymentDueDay}", debt, OnboardingRecordKind.Card));
    }

    public static (Loan Model, OnboardingDraftItem Draft) CreateLoan(string name, string bank, decimal payment, int day, int count, decimal? debt, DateOnly today)
    {
        var m = new Loan { Name = name.Trim(), Bank = bank.Trim(), MonthlyPayment = payment, PaymentDay = day, RemainingInstallmentCount = count, RemainingDebt = debt, NextPaymentDate = today };
        return (m, new OnboardingDraftItem(m.Id, m.Name, $"{m.RemainingInstallmentCount} taksit · Gün: {m.PaymentDay}", m.MonthlyPayment, OnboardingRecordKind.Loan));
    }

    public static (TemporaryPaymentPlan? Plan, PlannedLargeExpense? Expense, OnboardingDraftItem Draft) CreateExpense(string name, decimal amount, DateOnly date, int? installments)
    {
        if (installments is > 1)
        {
            var p = new TemporaryPaymentPlan { Name = name.Trim(), Kind = PaymentPlanKind.Temporary, OriginalAmount = amount, TotalRepaymentAmount = amount * installments.Value };
            return (p, null, new OnboardingDraftItem(p.Id, p.Name, $"{installments.Value} taksit · {date:dd.MM.yyyy}", amount, OnboardingRecordKind.Expense));
        }
        var e = new PlannedLargeExpense { Name = name.Trim(), Amount = amount, ExactDate = date };
        return (null, e, new OnboardingDraftItem(e.Id, e.Name, e.ExactDate.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture), e.Amount, OnboardingRecordKind.Expense));
    }
}
