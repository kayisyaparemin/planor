using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Services;

/// <summary>
/// Finansal Yapı testlerinin ortak verisi. Bugün 27 Eylül 2026; kart kesim 15, son ödeme 25,
/// bakiye tarihi 1 Eylül 2026, asgari oran %20 (V7 kart kontrol düzeneğiyle aynı kart).
/// </summary>
internal static class FinancialStructureData
{
    public static readonly DateOnly Today = new(2026, 9, 27);

    public static (RecurringIncome Income, IncomeAmountHistory[] Histories) Income(
        string name, int day, bool active = true, params (decimal Amount, DateOnly From)[] amounts)
    {
        var income = new RecurringIncome { Name = name, PaymentDay = day, IsActive = active };
        var histories = amounts
            .Select(a => new IncomeAmountHistory { RecurringIncomeId = income.Id, Amount = a.Amount, EffectiveDate = a.From })
            .ToArray();
        return (income, histories);
    }

    public static AdHocIncome AdHoc(string name, decimal amount, DateOnly date) =>
        new() { Description = name, Amount = amount, ExactDate = date };

    public static CreditCard Card(string name, decimal carried = 0m, bool active = true, string bank = "Garanti BBVA") => new()
    {
        Name = name, Bank = bank, Limit = 50000m, CarriedBalance = carried, BalanceAsOfDate = new DateOnly(2026, 9, 1),
        StatementClosingDay = 15, PaymentDueDay = 25, MinimumPaymentRate = 0.20m,
        PaymentStrategy = CreditCardPaymentStrategy.FullStatement, ProjectionFallbackStrategy = ProjectionFallbackStrategy.Minimum,
        IsActive = active
    };

    public static Loan Loan(string name, DateOnly next, int remaining = 12, bool active = true, string bank = "Akbank") => new()
    {
        Name = name, Bank = bank, MonthlyPayment = 7500m, PaymentDay = next.Day, NextPaymentDate = next,
        RemainingInstallmentCount = remaining, IsActive = active
    };

    public static TemporaryPaymentPlan Plan(string name, params (DateOnly Due, decimal Amount, bool Paid)[] installments)
    {
        var id = Guid.NewGuid();
        return new TemporaryPaymentPlan
        {
            Id = id,
            Name = name,
            Installments = installments
                .Select(i => new TemporaryPaymentInstallment { PlanId = id, DueDate = i.Due, Amount = i.Amount, IsPaid = i.Paid })
                .ToArray()
        };
    }

    public static PlannedLargeExpense Expense(string name, decimal amount, DateOnly date, PlannedExpenseStatus status = PlannedExpenseStatus.Planned) =>
        new() { Name = name, Amount = amount, ExactDate = date, Status = status };
}
