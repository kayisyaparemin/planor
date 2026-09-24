using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class FinancialProjectionServiceTests
{
    private readonly FinancialProjectionService _service = new(CreateCalculator());

    [Fact]
    public void BuildDashboard_NullPlan_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => _service.BuildDashboard(null!, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void BuildFuturePeriods_NullPlan_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => _service.BuildFuturePeriods(null!, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void BuildDashboard_GecerliPlanVerildiginde_AktifDonemVe12DonemBitisBakiyesiniDondurur()
    {
        var plan = CreateBasicPlan(50000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 1, 1),
                ProjectionOpeningBalance = 25000m,
                PeriodVariableExpenseAllowance = 20000m
            }
        };

        var asOf = new DateOnly(2026, 1, 1);
        var snapshot = _service.BuildDashboard(plan, asOf);

        Assert.NotNull(snapshot);
        Assert.Equal(new DateOnly(2026, 1, 1), snapshot.CurrentPeriod.Period.Start);
        Assert.Equal(new DateOnly(2026, 1, 1), snapshot.ProjectionAnchorDate);
        Assert.Equal(25000m, snapshot.ProjectionOpeningBalance);
        Assert.False(snapshot.HasUndeterminedCardPayments);
        // Her dönem: Gelir 50.000, Yaşam 20.000 -> Fazla +30.000. 12 dönemde: 25.000 + 12 * 30.000 = 385.000
        Assert.Equal(385000m, snapshot.TwelvePeriodEndingBalance);
        Assert.Equal(snapshot.CurrentPeriod.Period.Start, snapshot.TightestPeriod.Period.Start);
        Assert.Equal(55000m, snapshot.TightestPeriod.EndingBalance);
    }

    [Fact]
    public void BuildDashboard_EnKritikDonemiVeSikisikBakiyeyiTespitEder()
    {
        var plan = CreateBasicPlan(40000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 1, 1),
                ProjectionOpeningBalance = 10000m,
                PeriodVariableExpenseAllowance = 30000m
            },
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Id = Guid.NewGuid(),
                    Name = "Büyük Tadilat",
                    Amount = 150000m,
                    ExactDate = new DateOnly(2026, 4, 15),
                    Status = PlannedExpenseStatus.Planned
                }
            ]
        };

        var asOf = new DateOnly(2026, 1, 1);
        var snapshot = _service.BuildDashboard(plan, asOf);

        // 4. dönemde (Nisan 2026) 150.000 harcama bakiyeyi çukura düşürür
        Assert.Equal(new DateOnly(2026, 4, 1), snapshot.TightestPeriod.Period.Start);
        Assert.True(snapshot.TightestPeriod.EndingBalance < 0m);
    }

    [Fact]
    public void BuildDashboard_CapaOncesiAcikOdemelerVeDonemIciOdemeleri_BirlestiripIlkBesYaklasanOdemeyiSiralar()
    {
        var planId = Guid.NewGuid();
        var plan = CreateBasicPlan(60000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(15),
                ProjectionAnchorDate = new DateOnly(2026, 1, 10),
                ProjectionOpeningBalance = 50000m
            },
            PaymentPlans =
            [
                new TemporaryPaymentPlan
                {
                    Id = planId,
                    Name = "Taksitler",
                    Installments =
                    [
                        // Çapa öncesi (10 Ocak ile 15 Ocak arası)
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 1, 12), Amount = 5000m },
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 1, 14), Amount = 3000m },
                        // 1. dönem içi [15 Ocak, 15 Şubat)
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 1, 20), Amount = 7000m },
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 1, 25), Amount = 2000m },
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 2, 5), Amount = 4000m },
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 2, 10), Amount = 8000m }
                    ]
                }.Normalize()
            ]
        };

        var asOf = new DateOnly(2026, 1, 10);
        var snapshot = _service.BuildDashboard(plan, asOf);

        Assert.Equal(2, snapshot.PreFirstPeriodObligations.Count);
        Assert.Equal(5, snapshot.UpcomingPayments.Count);
        // İlk yaklaşan ödeme 12 Ocak olmalı
        Assert.Equal(new DateOnly(2026, 1, 12), snapshot.UpcomingPayments[0].DueDate);
        Assert.Equal(new DateOnly(2026, 1, 14), snapshot.UpcomingPayments[1].DueDate);
        Assert.Equal(new DateOnly(2026, 1, 20), snapshot.UpcomingPayments[2].DueDate);
    }

    [Fact]
    public void BuildDashboard_BelirsizKartOdemesiVarsa_BayragiIsaretler()
    {
        var plan = CreateBasicPlan(50000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 1, 1),
                ProjectionOpeningBalance = 20000m
            },
            CreditCards =
            [
                new CreditCard
                {
                    Id = Guid.NewGuid(),
                    Name = "Belirsiz Kart",
                    Bank = "Banka",
                    Limit = 50000m,
                    BalanceAsOfDate = new DateOnly(2026, 1, 1),
                    StatementClosingDay = 10,
                    PaymentDueDay = 20,
                    PaymentStrategy = CreditCardPaymentStrategy.AskEachStatement,
                    ProjectionFallbackStrategy = ProjectionFallbackStrategy.None
                }
            ]
        };

        var snapshot = _service.BuildDashboard(plan, new DateOnly(2026, 1, 1));
        Assert.True(snapshot.HasUndeterminedCardPayments);
    }

    [Fact]
    public void BuildDashboard_ToplamFaizMaliyetlerini_DogruAktarir()
    {
        var plan = CreateBasicPlan(30000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 1, 1),
                ProjectionOpeningBalance = 0m,
                PeriodVariableExpenseAllowance = 40000m, // Her ay 10.000 açık
                DeficitFinancingInterestRate = 0.05m // %5 KMH faizi
            }
        };

        var snapshot = _service.BuildDashboard(plan, new DateOnly(2026, 1, 1));

        Assert.True(snapshot.TwelvePeriodDeficitFinancingInterest > 0m);
        Assert.Equal(snapshot.TwelvePeriodDeficitFinancingInterest + snapshot.TwelvePeriodCreditCardInterest, snapshot.TwelvePeriodTotalInterest);
    }

    [Fact]
    public void BuildFuturePeriods_IstenenDonemSayisinda_ProjeksiyonDizisiDondurur()
    {
        var plan = CreateBasicPlan(50000m) with
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 1, 1)
            }
        };

        var periods = _service.BuildFuturePeriods(plan, new DateOnly(2026, 1, 1), 6);

        Assert.Equal(6, periods.Count);
        Assert.Equal(new DateOnly(2026, 1, 1), periods[0].Period.Start);
        Assert.Equal(new DateOnly(2026, 6, 1), periods[^1].Period.Start);
    }

    private static FinancialPlan CreateBasicPlan(decimal recurringIncomeAmount)
    {
        var streamId = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 1, 1)
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = streamId,
                    Name = "Temel Gelir",
                    PaymentDay = 1,
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
