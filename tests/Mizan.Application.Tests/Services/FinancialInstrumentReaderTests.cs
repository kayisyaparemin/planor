using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class FinancialInstrumentReaderTests
{
    [Fact]
    public async Task ReadInstrumentsAsync_TumDepolardakiSozlesmeleriPaketler()
    {
        var loanRepo = new InMemoryLoanRepository();
        var paymentPlanRepo = new InMemoryTemporaryPaymentPlanRepository();
        var cardRepo = new InMemoryCreditCardRepository();
        var largeExpenseRepo = new InMemoryPlannedLargeExpenseRepository();

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            Name = "İhtiyaç Kredisi",
            MonthlyPayment = 5_000m,
            RemainingInstallmentCount = 12,
            NextPaymentDate = new DateOnly(2026, 10, 15)
        };
        var prepayment = new LoanPrepayment
        {
            LoanId = loan.Id,
            Date = new DateOnly(2026, 11, 15),
            PrincipalAmount = 10_000m,
            Mode = LoanPrepaymentMode.ReduceTerm
        };
        await loanRepo.UpsertLoanAsync(loan);
        await loanRepo.UpsertLoanPrepaymentAsync(prepayment);

        var plan = new TemporaryPaymentPlan
        {
            Id = Guid.NewGuid(),
            Name = "Beyaz Eşya",
            Installments = [new TemporaryPaymentInstallment { DueDate = new DateOnly(2026, 10, 20), Amount = 4_000m }]
        };
        await paymentPlanRepo.UpsertPaymentPlanAsync(plan);

        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            Name = "Bonus",
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 9, 25),
                DueDate = new DateOnly(2026, 10, 5),
                StatementAmount = 8_000m,
                MinimumPaymentAmount = 2_000m
            }
        };
        await cardRepo.UpsertCreditCardAsync(card);

        var expense = new PlannedLargeExpense
        {
            Id = Guid.NewGuid(),
            Name = "Yıllık Sigorta",
            Amount = 15_000m,
            ExactDate = new DateOnly(2026, 12, 1)
        };
        await largeExpenseRepo.UpsertPlannedLargeExpenseAsync(expense);

        var reader = new FinancialInstrumentReader(
            loanRepo,
            paymentPlanRepo,
            cardRepo,
            largeExpenseRepo);

        var bundle = await reader.ReadInstrumentsAsync();

        Assert.NotNull(bundle);
        Assert.Single(bundle.Loans);
        Assert.Equal("İhtiyaç Kredisi", bundle.Loans[0].Name);
        Assert.Single(bundle.LoanPrepayments);
        Assert.Equal(10_000m, bundle.LoanPrepayments[0].PrincipalAmount);
        Assert.Single(bundle.PaymentPlans);
        Assert.Equal("Beyaz Eşya", bundle.PaymentPlans[0].Name);
        Assert.Single(bundle.CreditCards);
        Assert.Equal("Bonus", bundle.CreditCards[0].Name);
        Assert.Single(bundle.PlannedLargeExpenses);
        Assert.Equal(15_000m, bundle.PlannedLargeExpenses[0].Amount);
    }
}
