using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class ScenarioPlanBuilderTests
{
    private readonly ScenarioPlanBuilder _builder = new(
        new InstallmentScheduleCalculator(),
        new LoanPrepaymentValidator(
            new LoanAmortizationCalculator(new LoanScheduleCalculator()),
            new LoanPaymentScheduleBuilder(
                new LoanScheduleCalculator(),
                new LoanAmortizationCalculator(new LoanScheduleCalculator()))));

    [Fact]
    public void Build_CashPurchase_AddsPlannedLargeExpense()
    {
        var plan = CreateBasicPlan();
        var request = new SimulationRequest(
            SimulationScenarioType.CashPurchase,
            "Tadilat Harcaması",
            150000m,
            new DateOnly(2026, 11, 15));

        var result = _builder.Build(plan, request);

        var expense = Assert.Single(result.PlannedLargeExpenses);
        Assert.Equal("Tadilat Harcaması", expense.Name);
        Assert.Equal(150000m, expense.Amount);
        Assert.Equal(new DateOnly(2026, 11, 15), expense.ExactDate);
        Assert.Equal(PlannedExpenseStatus.Planned, expense.Status);
    }

    [Fact]
    public void Build_CreditCardSinglePayment_AddsSingleChargeToTargetCard()
    {
        var cardId = Guid.NewGuid();
        var plan = CreateBasicPlanWithCard(cardId, limit: 100000m);
        var request = new SimulationRequest(
            SimulationScenarioType.CreditCardSinglePayment,
            "Telefon Alımı",
            35000m,
            new DateOnly(2026, 9, 20),
            1)
        {
            CreditCardId = cardId
        };

        var result = _builder.Build(plan, request);

        var card = Assert.Single(result.CreditCards);
        var charge = Assert.Single(card.Charges);
        Assert.Equal(35000m, charge.Amount);
        Assert.Equal(new DateOnly(2026, 9, 20), charge.PostingDate);
        Assert.Equal("Telefon Alımı", charge.Description);
    }

    [Fact]
    public void Build_CreditCardInstallmentPurchase_AddsSplitChargesToTargetCard()
    {
        var cardId = Guid.NewGuid();
        var plan = CreateBasicPlanWithCard(cardId, limit: 100000m);
        var request = new SimulationRequest(
            SimulationScenarioType.CreditCardInstallmentPurchase,
            "Beyaz Eşya",
            60000m,
            new DateOnly(2026, 9, 20),
            3)
        {
            CreditCardId = cardId
        };

        var result = _builder.Build(plan, request);

        var card = Assert.Single(result.CreditCards);
        Assert.Equal(3, card.Charges.Count);
        Assert.Equal(60000m, card.Charges.Sum(x => x.Amount));
        Assert.Equal(new DateOnly(2026, 9, 20), card.Charges[0].PostingDate);
        Assert.Equal(new DateOnly(2026, 10, 20), card.Charges[1].PostingDate);
        Assert.Equal(new DateOnly(2026, 11, 20), card.Charges[2].PostingDate);
    }

    [Fact]
    public void Build_CreditCardPurchase_ExceedingAvailableLimit_ThrowsInvalidOperationException()
    {
        var cardId = Guid.NewGuid();
        var plan = CreateBasicPlanWithCard(cardId, limit: 50000m);
        var request = new SimulationRequest(
            SimulationScenarioType.CreditCardSinglePayment,
            "Limit Aşımı",
            60000m,
            new DateOnly(2026, 9, 20))
        {
            CreditCardId = cardId
        };

        var ex = Assert.Throws<InvalidOperationException>(() => _builder.Build(plan, request));
        Assert.Contains("kullanılabilir limiti bu plan için yetersiz", ex.Message);
    }

    [Fact]
    public void Build_FinancingLoan_AddsInstallmentPlanAndAdHocIncome()
    {
        var plan = CreateBasicPlan();
        var request = new SimulationRequest(
            SimulationScenarioType.FinancingLoan,
            "İhtiyaç Finansmanı",
            100000m,
            new DateOnly(2026, 10, 1),
            5)
        {
            TotalRepaymentAmount = 125000m,
            FirstPaymentDate = new DateOnly(2026, 11, 1)
        };

        var result = _builder.Build(plan, request);

        var income = Assert.Single(result.AdHocIncomes);
        Assert.Equal(100000m, income.Amount);
        Assert.Equal(new DateOnly(2026, 10, 1), income.ExactDate);

        var paymentPlan = Assert.Single(result.PaymentPlans);
        Assert.Equal(PaymentPlanKind.Installment, paymentPlan.Kind);
        Assert.Equal(125000m, paymentPlan.TotalRepaymentAmount);
        Assert.Equal(5, paymentPlan.Installments.Count);
        Assert.Equal(new DateOnly(2026, 11, 1), paymentPlan.Installments[0].DueDate);
    }

    [Fact]
    public void Build_IncomeChange_AddsIncomeAmountHistoryToTargetStream()
    {
        var plan = CreateBasicPlan();
        var streamId = plan.RecurringIncomes[0].Id;
        var request = new SimulationRequest(
            SimulationScenarioType.IncomeChange,
            "Yıllık Zam",
            75000m,
            new DateOnly(2027, 1, 1))
        {
            RecurringIncomeId = streamId
        };

        var result = _builder.Build(plan, request);

        Assert.Equal(2, result.IncomeHistories.Count);
        var added = result.IncomeHistories.Single(x => x.EffectiveDate == new DateOnly(2027, 1, 1));
        Assert.Equal(75000m, added.Amount);
        Assert.Equal(streamId, added.RecurringIncomeId);
    }

    [Fact]
    public void Build_FutureIncome_AddsAdHocIncome()
    {
        var plan = CreateBasicPlan();
        var request = new SimulationRequest(
            SimulationScenarioType.FutureIncome,
            "Yıl Sonu Prim",
            30000m,
            new DateOnly(2026, 12, 25));

        var result = _builder.Build(plan, request);

        var income = Assert.Single(result.AdHocIncomes);
        Assert.Equal("Yıl Sonu Prim", income.Description);
        Assert.Equal(30000m, income.Amount);
        Assert.Equal(new DateOnly(2026, 12, 25), income.ExactDate);
    }

    [Fact]
    public void Build_CashDebt_AddsOtherScheduledPaymentPlan()
    {
        var plan = CreateBasicPlan();
        var request = new SimulationRequest(
            SimulationScenarioType.CashDebt,
            "Elden Borç",
            50000m,
            new DateOnly(2026, 9, 1),
            5);

        var result = _builder.Build(plan, request);

        var paymentPlan = Assert.Single(result.PaymentPlans);
        Assert.Equal(PaymentPlanKind.OtherScheduled, paymentPlan.Kind);
        Assert.Equal(50000m, paymentPlan.TotalRepaymentAmount);
        Assert.Equal(5, paymentPlan.Installments.Count);
    }

    [Fact]
    public void Build_FutureOneTimePayment_AddsSinglePaymentPlan()
    {
        var plan = CreateBasicPlan();
        var request = new SimulationRequest(
            SimulationScenarioType.FutureOneTimePayment,
            "Vergi Ödemesi",
            12000m,
            new DateOnly(2026, 11, 20));

        var result = _builder.Build(plan, request);

        var paymentPlan = Assert.Single(result.PaymentPlans);
        Assert.Equal(PaymentPlanKind.OtherScheduled, paymentPlan.Kind);
        Assert.Single(paymentPlan.Installments);
        Assert.Equal(12000m, paymentPlan.Installments[0].Amount);
        Assert.Equal(new DateOnly(2026, 11, 20), paymentPlan.Installments[0].DueDate);
    }

    [Fact]
    public void Build_RecurringPayment_AddsMonthlyScheduledInstallments()
    {
        var plan = CreateBasicPlan();
        var request = new SimulationRequest(
            SimulationScenarioType.RecurringPayment,
            "Kurs Ücreti",
            5000m,
            new DateOnly(2026, 9, 15),
            4);

        var result = _builder.Build(plan, request);

        var paymentPlan = Assert.Single(result.PaymentPlans);
        Assert.Equal(PaymentPlanKind.Recurring, paymentPlan.Kind);
        Assert.Equal(20000m, paymentPlan.TotalRepaymentAmount);
        Assert.Equal(4, paymentPlan.Installments.Count);
        Assert.Equal(new DateOnly(2026, 9, 15), paymentPlan.Installments[0].DueDate);
        Assert.Equal(new DateOnly(2026, 12, 15), paymentPlan.Installments[3].DueDate);
    }

    [Fact]
    public void Build_CardPaymentMode_DueDateOverride_AddsPaymentPlan()
    {
        var cardId = Guid.NewGuid();
        var plan = CreateBasicPlanWithCard(cardId, 50000m);
        var request = new SimulationRequest(
            SimulationScenarioType.CreditCardPaymentMode,
            "Asgari Ödeme",
            0m,
            new DateOnly(2026, 10, 5))
        {
            CreditCardId = cardId,
            CardPaymentType = CreditCardPaymentType.Minimum,
            AppliesToAllStatements = false
        };

        var result = _builder.Build(plan, request);

        var card = Assert.Single(result.CreditCards);
        var paymentPlan = Assert.Single(card.PaymentPlans);
        Assert.Equal(new DateOnly(2026, 10, 5), paymentPlan.DueDate);
        Assert.Equal(CreditCardPaymentType.Minimum, paymentPlan.PaymentType);
    }

    [Fact]
    public void Build_CardPaymentMode_AllStatements_UpdatesCardStrategy()
    {
        var cardId = Guid.NewGuid();
        var plan = CreateBasicPlanWithCard(cardId, 50000m);
        var request = new SimulationRequest(
            SimulationScenarioType.CreditCardPaymentMode,
            "Sürekli Tamamı",
            0m,
            new DateOnly(2026, 10, 5))
        {
            CreditCardId = cardId,
            CardPaymentType = CreditCardPaymentType.FullStatement,
            AppliesToAllStatements = true
        };

        var result = _builder.Build(plan, request);

        var card = Assert.Single(result.CreditCards);
        Assert.Equal(CreditCardPaymentStrategy.FullStatement, card.PaymentStrategy);
        Assert.Empty(card.PaymentPlans);
    }

    private static FinancialPlan CreateBasicPlan()
    {
        var streamId = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = new DateOnly(2026, 8, 20)
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = streamId,
                    Name = "Ana Gelir",
                    PaymentDay = 10,
                    IsActive = true
                }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = streamId,
                    Amount = 50000m,
                    EffectiveDate = new DateOnly(2026, 1, 1)
                }
            ]
        };
    }

    private static FinancialPlan CreateBasicPlanWithCard(Guid cardId, decimal limit)
    {
        var plan = CreateBasicPlan();
        return plan with
        {
            CreditCards =
            [
                new CreditCard
                {
                    Id = cardId,
                    Bank = "Garanti",
                    Name = "Bonus",
                    Limit = limit,
                    StatementClosingDay = 25,
                    PaymentDueDay = 5
                }
            ]
        };
    }
}
