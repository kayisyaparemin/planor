using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class SimulationCalculatorTests
{
    private readonly SimulationCalculator _calculator;
    private readonly ScenarioPlanBuilder _planBuilder;
    private readonly FinancialProjectionCalculator _projectionCalculator;

    public SimulationCalculatorTests()
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var incomeCalc = new IncomeProjectionCalculator(new IncomeResolver());
        var cardCalc = new CreditCardStatementCalculator();
        var loanSchedule = new LoanScheduleCalculator();
        var loanAmortization = new LoanAmortizationCalculator(loanSchedule);
        var loanScheduleBuilder = new LoanPaymentScheduleBuilder(loanSchedule, loanAmortization);
        var loanValidator = new LoanPrepaymentValidator(loanAmortization, loanScheduleBuilder);
        var mandatoryCalc = new MandatoryPaymentCalculator(loanScheduleBuilder, new ScheduledPaymentCalculator());
        var grouper = new PeriodObligationGrouper();

        _projectionCalculator = new FinancialProjectionCalculator(
            periodCalc,
            incomeCalc,
            cardCalc,
            mandatoryCalc,
            grouper);

        var installments = new InstallmentScheduleCalculator();
        _planBuilder = new ScenarioPlanBuilder(installments, loanValidator);
        _calculator = new SimulationCalculator(_projectionCalculator, _planBuilder, loanScheduleBuilder);
    }

    [Fact]
    public void Calculate_CashPurchase_ReducesEndingBalanceAndCalculatesRowDelta()
    {
        var plan = CreateBasicPlan(openingBalance: 50000m, monthlyIncome: 60000m, livingAllowance: 30000m);
        var request = new SimulationRequest(
            SimulationScenarioType.CashPurchase,
            "Tek Seferlik Harcama",
            40000m,
            new DateOnly(2026, 10, 5));

        var result = _calculator.Calculate(plan, new DateOnly(2026, 8, 20), request, periodCount: 3);

        Assert.Equal(3, result.Baseline.Count);
        Assert.Equal(3, result.Scenario.Count);
        Assert.Equal(3, result.Rows.Count);

        var impactedRow = result.Rows.Single(x => x.Scenario.Period.Contains(new DateOnly(2026, 10, 5)));
        Assert.Equal(-40000m, impactedRow.ProjectedBalanceDifference);
        Assert.Equal(40000m, result.Risk.TotalScenarioCost);
    }

    [Fact]
    public void Calculate_FinancingLoan_CreditsPrincipalAndReportsFinancingCost()
    {
        var plan = CreateBasicPlan(openingBalance: 10000m, monthlyIncome: 50000m, livingAllowance: 25000m);
        var request = new SimulationRequest(
            SimulationScenarioType.FinancingLoan,
            "Nakit Finansman",
            60000m,
            new DateOnly(2026, 9, 15),
            3)
        {
            TotalRepaymentAmount = 75000m,
            FirstPaymentDate = new DateOnly(2026, 10, 1)
        };

        var result = _calculator.Calculate(plan, new DateOnly(2026, 8, 20), request, periodCount: 4);

        var drawdownRow = result.Rows.Single(x => x.Scenario.Period.Contains(new DateOnly(2026, 9, 15)));
        Assert.Equal(60000m, drawdownRow.Scenario.AdHocIncomeTotal);
        Assert.Equal(0m, drawdownRow.Baseline.AdHocIncomeTotal);

        Assert.Equal(75000m, result.Risk.TotalScenarioCost);
        Assert.Equal(15000m, result.Risk.FinancingCost);
    }

    [Fact]
    public void Calculate_IncomeChange_IncreasesRecurringIncomeFromEffectivePeriod()
    {
        var plan = CreateBasicPlan(openingBalance: 0m, monthlyIncome: 50000m, livingAllowance: 20000m);
        var streamId = plan.RecurringIncomes[0].Id;
        var request = new SimulationRequest(
            SimulationScenarioType.IncomeChange,
            "Zam",
            70000m,
            new DateOnly(2026, 11, 10))
        {
            RecurringIncomeId = streamId
        };

        var result = _calculator.Calculate(plan, new DateOnly(2026, 8, 20), request, periodCount: 4);

        var novPeriod = result.Scenario.Single(x => x.PeriodStart == new DateOnly(2026, 11, 10));
        Assert.Equal(70000m, novPeriod.RecurringIncomeTotal);

        var octPeriod = result.Scenario.Single(x => x.PeriodStart == new DateOnly(2026, 10, 10));
        Assert.Equal(50000m, octPeriod.RecurringIncomeTotal);
    }

    [Fact]
    public void Calculate_MultipleRequests_DoesNotMutateBaselinePlan()
    {
        var plan = CreateBasicPlan(openingBalance: 20000m, monthlyIncome: 50000m, livingAllowance: 25000m);
        var req1 = new SimulationRequest(
            SimulationScenarioType.CashPurchase, "Gider 1", 10000m, new DateOnly(2026, 9, 20));
        var req2 = new SimulationRequest(
            SimulationScenarioType.FutureIncome, "Gelir 2", 15000m, new DateOnly(2026, 10, 15));

        var result = _calculator.Calculate(plan, new DateOnly(2026, 8, 20), [req1, req2], periodCount: 3);

        Assert.Empty(plan.PlannedLargeExpenses);
        Assert.Empty(plan.AdHocIncomes);
        Assert.All(result.Baseline, x => Assert.Equal(0m, x.AdHocIncomeTotal));
        Assert.True(result.Scenario[^1].EndingBalance > result.Baseline[^1].EndingBalance);
    }

    [Fact]
    public void Calculate_CardPaymentMode_ReducesInterestAndCalculatesInterestSaving()
    {
        var cardId = Guid.NewGuid();
        var plan = CreateBasicPlanWithCard(cardId, carriedBalance: 20000m);
        var request = new SimulationRequest(
            SimulationScenarioType.CreditCardPaymentMode,
            "Borcu Tamamen Öde",
            0m,
            new DateOnly(2026, 10, 5))
        {
            CreditCardId = cardId,
            CardPaymentType = CreditCardPaymentType.FullStatement,
            AppliesToAllStatements = true
        };

        var result = _calculator.Calculate(plan, new DateOnly(2026, 8, 20), request, periodCount: 3);

        Assert.True(result.ScenarioInterest.CreditCardInterest <= result.BaselineInterest.CreditCardInterest);
    }

    private static FinancialPlan CreateBasicPlanWithCard(Guid cardId, decimal carriedBalance)
    {
        var plan = CreateBasicPlan(openingBalance: 50000m, monthlyIncome: 60000m, livingAllowance: 20000m);
        return plan with
        {
            CreditCards =
            [
                new CreditCard
                {
                    Id = cardId,
                    Bank = "Garanti",
                    Name = "Bonus",
                    Limit = 100000m,
                    StatementClosingDay = 25,
                    PaymentDueDay = 5,
                    BalanceAsOfDate = new DateOnly(2026, 8, 20),
                    CarriedBalance = carriedBalance,
                    PaymentStrategy = CreditCardPaymentStrategy.Minimum
                }
            ]
        };
    }

    private static FinancialPlan CreateBasicPlan(decimal openingBalance, decimal monthlyIncome, decimal livingAllowance)
    {
        var streamId = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = new DateOnly(2026, 8, 20),
                ProjectionOpeningBalance = openingBalance,
                PeriodVariableExpenseAllowance = livingAllowance,
                CreditCardCarryInterestRate = 0.04m,
                DeficitFinancingInterestRate = 0.05m
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = streamId,
                    Name = "Gelir",
                    PaymentDay = 10,
                    IsActive = true
                }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = streamId,
                    Amount = monthlyIncome,
                    EffectiveDate = new DateOnly(2026, 1, 1)
                }
            ]
        };
    }
}
