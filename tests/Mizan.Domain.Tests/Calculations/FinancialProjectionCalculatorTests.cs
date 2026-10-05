using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class FinancialProjectionCalculatorTests
{
    private readonly FinancialProjectionCalculator _calculator = CreateCalculator();

    [Fact]
    public void CalculatePlan_NullPlan_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _calculator.CalculatePlan(null!, new DateOnly(2026, 8, 20)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(61)]
    public void CalculatePlan_GecersizDonemSayisi_ArgumentOutOfRangeExceptionFirlatir(int count)
    {
        var plan = CreateBasicPlan(50000m);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), count));
    }

    [Fact]
    public void CalculatePlan_PlanlamaCapasindanOncekiBaslangic_InvalidOperationExceptionFirlatir()
    {
        var plan = CreateBasicPlan(50000m);
        // Çapa 2026-08-20, ilk dönem başlangıcı olarak 2026-08-10 verilirse çapanın gerisinde kalır.
        Assert.Throws<InvalidOperationException>(() =>
            _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 12, new DateOnly(2026, 8, 10)));
    }

    [Fact]
    public void CalculatePlan_TemelAkis_GelirVeZorunluOdemeleriDogruHesaplar()
    {
        var plan = CreateBasicPlan(100000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = new DateOnly(2026, 8, 20),
                PeriodVariableExpenseAllowance = 30000m,
                ProjectionOpeningBalance = 10000m
            },
            Loans =
            [
                new Loan
                {
                    Name = "İhtiyaç Kredisi",
                    Bank = "İş Bankası",
                    MonthlyPayment = 20000m,
                    PaymentDay = 15,
                    NextPaymentDate = new DateOnly(2026, 9, 15),
                    RemainingInstallmentCount = 6
                }
            ]
        };

        var result = _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 2);

        Assert.Equal(2, result.Periods.Count);
        var p0 = result.Periods[0];
        Assert.Equal(new DateOnly(2026, 9, 10), p0.PeriodStart);
        Assert.Equal(10000m, p0.OpeningBalance);
        Assert.Equal(100000m, p0.TotalIncome);
        Assert.Equal(20000m, p0.LoanPayments);
        Assert.Equal(20000m, p0.MandatoryOutflow);
        Assert.Equal(80000m, p0.AvailableAfterMandatory);
        Assert.Equal(30000m, p0.VariableExpenseAllowance);
        Assert.Equal(50000m, p0.EstimatedSurplus);
        Assert.Equal(60000m, p0.EndingBalance);
        Assert.False(p0.HasDeficit);

        var p1 = result.Periods[1];
        Assert.Equal(60000m, p1.OpeningBalance);
        Assert.Equal(110000m, p1.EndingBalance);
    }

    [Fact]
    public void CalculatePlan_EksiBakiyeVeFinansmanAcigiFaizi_DogruHesaplanirVeSonrakiDonemeDevreder()
    {
        var plan = CreateBasicPlan(50000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = new DateOnly(2026, 8, 20),
                PeriodVariableExpenseAllowance = 30000m,
                ProjectionOpeningBalance = -25000m,
                DeficitFinancingInterestRate = 0.05m
            }
        };

        var result = _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 2);
        var p0 = result.Periods[0];

        Assert.Equal(-25000m, p0.OpeningBalance);
        Assert.Equal(25000m, p0.CarryOverDeficit);
        Assert.Equal(20000m, p0.EstimatedSurplus);
        // EndingBeforeDeficit = -25000 + 20000 = -5000
        Assert.Equal(-5000m, p0.EndingBalanceBeforeDeficitInterest);
        Assert.Equal(5000m, p0.DeficitPrincipal);
        // Faiz = 5000 * 0.05 = 250
        Assert.Equal(250m, p0.DeficitFinancingInterest);
        Assert.Equal(-5250m, p0.EndingBalance);
        Assert.True(p0.HasDeficit);

        var p1 = result.Periods[1];
        Assert.Equal(-5250m, p1.OpeningBalance);
        Assert.Equal(5250m, p1.CarryOverDeficit);
    }

    [Fact]
    public void CalculatePlan_AcikTelafisi_PozitifBakiyeyleAcikKapatilirVeTelafiIsaretlenir()
    {
        var plan = CreateBasicPlan(70000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = new DateOnly(2026, 8, 20),
                PeriodVariableExpenseAllowance = 30000m,
                ProjectionOpeningBalance = -25000m,
                DeficitFinancingInterestRate = 0.05m
            }
        };

        var result = _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 1);
        var p0 = result.Periods[0];

        Assert.Equal(25000m, p0.CarryOverDeficit);
        Assert.Equal(40000m, p0.CurrentPeriodNetContribution);
        // -25000 + 40000 = 15000
        Assert.Equal(15000m, p0.EndingBalanceBeforeDeficitInterest);
        Assert.Equal(0m, p0.DeficitFinancingInterest);
        Assert.Equal(15000m, p0.EndingBalance);
        Assert.True(p0.RecoveredCarryOverDeficit);
        Assert.Equal(25000m, p0.DeficitCoveredThisPeriod);
        Assert.Equal(0m, p0.RemainingCarryOverDeficit);
    }

    [Fact]
    public void CalculatePlan_CapadanOncekiVeIlkDonemArasindakiAriziGelir_IlkDonemeDahilEdilir()
    {
        // Çapa 20.08.2026, ilk dönem 10.09.2026. Aradaki 01.09.2026 tarihli gelir ilk döneme dahil edilmelidir (BR-INCOME-01).
        var plan = CreateBasicPlan(100000m) with
        {
            AdHocIncomes =
            [
                new AdHocIncome
                {
                    Amount = 150000m,
                    ExactDate = new DateOnly(2026, 9, 1),
                    Description = "Çapa arası ikramiye"
                }
            ]
        };

        var result = _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 2);
        var p0 = result.Periods[0];

        Assert.Equal(150000m, p0.AdHocIncomeTotal);
        Assert.Equal(250000m, p0.TotalIncome);
        Assert.Contains(p0.IncomeItems, x => x.Name == "Çapa arası ikramiye");
        Assert.Equal(0m, result.Periods[1].AdHocIncomeTotal);
    }

    [Fact]
    public void CalculatePlan_CapadanOncekiAriziGelir_ProjeksiyonaDahilEdilmez()
    {
        var plan = CreateBasicPlan(100000m) with
        {
            AdHocIncomes =
            [
                new AdHocIncome
                {
                    Amount = 50000m,
                    ExactDate = new DateOnly(2026, 8, 19),
                    Description = "Çapa öncesi para"
                }
            ]
        };

        var result = _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 2);

        Assert.Equal(0m, result.Periods[0].AdHocIncomeTotal);
        Assert.Equal(100000m, result.Periods[0].TotalIncome);
    }

    [Fact]
    public void CalculatePlan_PlanliBuyukHarcama_DonemFazlasiniDusururVeAcikFaiziUretir()
    {
        var plan = CreateBasicPlan(50000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = new DateOnly(2026, 8, 20),
                PeriodVariableExpenseAllowance = 30000m,
                DeficitFinancingInterestRate = 0.05m
            },
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Name = "Tadilat",
                    Amount = 350000m,
                    ExactDate = new DateOnly(2026, 9, 15)
                }
            ]
        };

        var result = _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 1);
        var p0 = result.Periods[0];

        Assert.Equal(350000m, p0.PlannedLargeCashExpenses);
        // EstimatedSurplus = 50000 - 30000 - 350000 = -330000
        Assert.Equal(-330000m, p0.EstimatedSurplus);
        Assert.Equal(-330000m, p0.EndingBalanceBeforeDeficitInterest);
        // 330000 * 0.05 = 16500
        Assert.Equal(16500m, p0.DeficitFinancingInterest);
        Assert.Equal(-346500m, p0.EndingBalance);
    }

    [Fact]
    public void CalculatePlan_KrediKartiEkstreProjeksiyonuVeCarryFaizi_KartaVeDonemeDogruYansir()
    {
        var card = new CreditCard
        {
            Name = "Bonus",
            Bank = "Garanti BBVA",
            CarriedBalance = 100000m,
            BalanceAsOfDate = new DateOnly(2026, 8, 26),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            MinimumPaymentRate = 0.40m,
            PaymentStrategy = CreditCardPaymentStrategy.Minimum
        };

        var streamId = Guid.NewGuid();
        var plan = new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = new DateOnly(2026, 8, 20),
                PeriodVariableExpenseAllowance = 30000m,
                CreditCardCarryInterestRate = 0.05m,
                DeficitFinancingInterestRate = 0.05m
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = streamId,
                    Name = "Gelir Akışı",
                    PaymentDay = 10,
                    IsActive = true
                }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = streamId,
                    Amount = 45000m,
                    EffectiveDate = new DateOnly(2026, 1, 1)
                },
                new IncomeAmountHistory
                {
                    RecurringIncomeId = streamId,
                    Amount = 65200m,
                    EffectiveDate = new DateOnly(2026, 10, 10)
                }
            ],
            CreditCards = [card]
        };

        var result = _calculator.CalculatePlan(plan, new DateOnly(2026, 8, 20), 2);

        Assert.Equal(42000m, result.Periods[0].MandatoryOutflow);
        Assert.Equal(5000m, result.Periods[0].CardInterestGenerated);
        Assert.Equal(1350m, result.Periods[0].DeficitFinancingInterest);
        Assert.Equal(6350m, result.Periods[0].TotalInterestGenerated);
        Assert.Equal(8150m, result.TotalCreditCardInterest);
        Assert.Equal(2330.50m, result.TotalDeficitFinancingInterest);
        Assert.Equal(10480.50m, result.TotalInterestCost);
    }

    [Fact]
    public void Calculate_Metodu_CalculatePlanPeriodsDondurur()
    {
        var plan = CreateBasicPlan(50000m);
        var periods = _calculator.Calculate(plan, new DateOnly(2026, 8, 20), 3);

        Assert.Equal(3, periods.Count);
        Assert.Equal(new DateOnly(2026, 9, 10), periods[0].PeriodStart);
    }

    private static FinancialPlan CreateBasicPlan(decimal recurringIncomeAmount)
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
                    Name = "Düzenli Gelir",
                    PaymentDay = 10,
                    IsActive = true
                }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = streamId,
                    Amount = recurringIncomeAmount,
                    EffectiveDate = new DateOnly(2026, 1, 1)
                }
            ]
        };
    }

    private static FinancialProjectionCalculator CreateCalculator()
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var incomeCalc = new IncomeProjectionCalculator(new IncomeResolver());
        var cardCalc = new CreditCardStatementCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var mandatoryCalc = new MandatoryPaymentCalculator(loanBuilder, scheduledCalc);
        var grouper = new PeriodObligationGrouper();

        return new FinancialProjectionCalculator(
            periodCalc,
            incomeCalc,
            cardCalc,
            mandatoryCalc,
            grouper);
    }
}
