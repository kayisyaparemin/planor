using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

public sealed class InMemoryRepositoryTests
{
    [Fact]
    public async Task LoanRepository_UpsertGetDelete_DogruCalismalidir()
    {
        var repo = new InMemoryLoanRepository();
        var loanId = Guid.NewGuid();
        var loan = new Loan
        {
            Id = loanId,
            Name = "Konut Kredisi",
            Bank = "Ziraat",
            MonthlyPayment = 15_000m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 120,
            RemainingDebt = 1_800_000m
        };

        await repo.UpsertLoanAsync(loan);
        var loans = await repo.GetLoansAsync();

        Assert.Single(loans);
        Assert.Equal("Konut Kredisi", loans[0].Name);

        var prepaymentId = Guid.NewGuid();
        var prepayment = new LoanPrepayment
        {
            Id = prepaymentId,
            LoanId = loanId,
            Date = new DateOnly(2026, 11, 1),
            PrincipalAmount = 100_000m,
            Mode = LoanPrepaymentMode.ReduceTerm
        };

        await repo.UpsertLoanPrepaymentAsync(prepayment);
        var prepayments = await repo.GetLoanPrepaymentsAsync();

        Assert.Single(prepayments);
        Assert.Equal(100_000m, prepayments[0].PrincipalAmount);

        await repo.DeleteLoanPrepaymentAsync(prepaymentId);
        var prepaymentsAfterDelete = await repo.GetLoanPrepaymentsAsync();
        Assert.Empty(prepaymentsAfterDelete);

        await repo.DeleteLoanAsync(loanId);
        var loansAfterDelete = await repo.GetLoansAsync();
        Assert.Empty(loansAfterDelete);
    }

    [Fact]
    public async Task CreditCardRepository_UpsertGetDelete_DogruCalismalidir()
    {
        var repo = new InMemoryCreditCardRepository();
        var cardId = Guid.NewGuid();
        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            Bank = "Garanti",
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            Limit = 100_000m
        };

        await repo.UpsertCreditCardAsync(card);
        var cards = await repo.GetCreditCardsAsync();

        Assert.Single(cards);
        Assert.Equal("Bonus", cards[0].Name);

        await repo.DeleteCreditCardAsync(cardId);
        var cardsAfterDelete = await repo.GetCreditCardsAsync();
        Assert.Empty(cardsAfterDelete);
    }

    [Fact]
    public async Task TemporaryPaymentPlanRepository_UpsertGetDelete_DogruCalismalidir()
    {
        var repo = new InMemoryTemporaryPaymentPlanRepository();
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Beyaz Eşya",
            Kind = PaymentPlanKind.Temporary,
            OriginalAmount = 30_000m,
            TotalRepaymentAmount = 30_000m,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 10, 1),
                    Amount = 10_000m,
                    IsPaid = false
                }
            ]
        };

        await repo.UpsertPaymentPlanAsync(plan);
        var plans = await repo.GetPaymentPlansAsync();

        Assert.Single(plans);
        Assert.Equal("Beyaz Eşya", plans[0].Name);

        await repo.DeletePaymentPlanAsync(planId);
        var plansAfterDelete = await repo.GetPaymentPlansAsync();
        Assert.Empty(plansAfterDelete);
    }

    [Fact]
    public async Task PlannedLargeExpenseRepository_UpsertGetDelete_DogruCalismalidir()
    {
        var repo = new InMemoryPlannedLargeExpenseRepository();
        var expenseId = Guid.NewGuid();
        var expense = new PlannedLargeExpense
        {
            Id = expenseId,
            Name = "Yıllık Sigorta",
            Amount = 25_000m,
            ExactDate = new DateOnly(2026, 12, 10),
            Status = PlannedExpenseStatus.Planned
        };

        await repo.UpsertPlannedLargeExpenseAsync(expense);
        var expenses = await repo.GetPlannedLargeExpensesAsync();

        Assert.Single(expenses);
        Assert.Equal("Yıllık Sigorta", expenses[0].Name);

        await repo.DeletePlannedLargeExpenseAsync(expenseId);
        var expensesAfterDelete = await repo.GetPlannedLargeExpensesAsync();
        Assert.Empty(expensesAfterDelete);
    }

    [Fact]
    public async Task RecurringIncomeRepository_UpsertGetDelete_DogruCalismalidir()
    {
        var repo = new InMemoryRecurringIncomeRepository();
        var incomeId = Guid.NewGuid();
        var income = new RecurringIncome
        {
            Id = incomeId,
            Name = "Maaş",
            PaymentDay = 15,
            IsActive = true
        };

        await repo.UpsertRecurringIncomeAsync(income);
        var incomes = await repo.GetRecurringIncomesAsync();

        Assert.Single(incomes);
        Assert.Equal("Maaş", incomes[0].Name);

        await repo.DeleteRecurringIncomeAsync(incomeId);
        var incomesAfterDelete = await repo.GetRecurringIncomesAsync();
        Assert.Empty(incomesAfterDelete);
    }

    [Fact]
    public async Task AdHocIncomeRepository_UpsertGetDelete_DogruCalismalidir()
    {
        var repo = new InMemoryAdHocIncomeRepository();
        var incomeId = Guid.NewGuid();
        var income = new AdHocIncome
        {
            Id = incomeId,
            Description = "Yıl Sonu Primi",
            Amount = 40_000m,
            ExactDate = new DateOnly(2026, 12, 31)
        };

        await repo.UpsertAdHocIncomeAsync(income);
        var incomes = await repo.GetAdHocIncomesAsync();

        Assert.Single(incomes);
        Assert.Equal("Yıl Sonu Primi", incomes[0].Description);

        await repo.DeleteAdHocIncomeAsync(incomeId);
        var incomesAfterDelete = await repo.GetAdHocIncomesAsync();
        Assert.Empty(incomesAfterDelete);
    }

    [Fact]
    public async Task UserSettingsRepository_SaveAndGet_DogruCalismalidir()
    {
        var repo = new InMemoryUserSettingsRepository();
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            PeriodVariableExpenseAllowance = 20_000m,
            ProjectionOpeningBalance = 50_000m,
            CreditCardCarryInterestRate = 0.0425m,
            DeficitFinancingInterestRate = 0.05m
        };

        await repo.SaveSettingsAsync(settings);
        var loaded = await repo.GetSettingsAsync();

        Assert.NotNull(loaded);
        Assert.Equal(15, loaded.PeriodAnchor.DayOfMonth);
        Assert.Equal(20_000m, loaded.PeriodVariableExpenseAllowance);
    }
}
