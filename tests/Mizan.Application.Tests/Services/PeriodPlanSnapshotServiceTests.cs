using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PeriodPlanSnapshotServiceTests
{
    private readonly CashFlowPeriodCalculator _periodCalculator = new();
    private readonly PeriodPlanSnapshotService _service = new(CreateCalculator(), new CashFlowPeriodCalculator());

    [Fact]
    public void Freeze_GecerliPlanVeSnapshot_DondurulanPlaninTarihleriniVeAlanlariniKenetler()
    {
        var plan = CreateBasicPlan(50_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);
        var now = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

        var frozen = _service.Freeze(plan, snapshot, now);

        Assert.NotEqual(Guid.Empty, frozen.Id);
        Assert.Equal(snapshot.Id, frozen.FinancialSnapshotId);
        Assert.Equal(new DateOnly(2026, 10, 1), frozen.PeriodStart);
        Assert.Equal(new DateOnly(2026, 11, 1), frozen.PeriodEnd);
        Assert.Equal(new DateOnly(2026, 11, 1), frozen.SettlementAvailableFrom);
        Assert.Equal(now, frozen.CreatedAtUtc);
        Assert.Equal(20_000m, frozen.OpeningBalance);
        Assert.Equal(50_000m, frozen.PlannedIncome);
        Assert.Equal(15_000m, frozen.PlannedVariableExpenseAllowance);
        Assert.Equal(55_000m, frozen.PlannedEndingBalance);
    }

    [Fact]
    public void Freeze_DondurulmusPlanDegismez_SonradanYapilanPlanDegisikligiEtkilemez()
    {
        var plan = CreateBasicPlan(40_000m);
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 10_000m);
        var frozen = _service.Freeze(plan, snapshot, DateTimeOffset.UtcNow);

        // Plan sonradan değiştiriliyor / yeni harcama ekleniyor
        var mutatedPlan = plan with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Name = "Sonradan Eklenen Laptop",
                    Amount = 30_000m,
                    ExactDate = new DateOnly(2026, 10, 15),
                    Status = PlannedExpenseStatus.Planned
                }
            ]
        };

        Assert.Equal(0m, frozen.PlannedLargeExpenses);
        Assert.Empty(frozen.PaymentLines.Where(x => x.Name == "Sonradan Eklenen Laptop"));
    }

    [Fact]
    public void Freeze_VadesiDonemAraliginaDusenYukumulukleri_OdemeSatirlarinaEkler()
    {
        var planId = Guid.NewGuid();
        var plan = CreateBasicPlan(50_000m) with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Id = Guid.NewGuid(),
                    Name = "Beyaz Eşya",
                    Amount = 12_000m,
                    ExactDate = new DateOnly(2026, 10, 15),
                    Status = PlannedExpenseStatus.Planned
                }
            ],
            PaymentPlans =
            [
                new TemporaryPaymentPlan
                {
                    Id = planId,
                    Name = "Elden Borç",
                    Installments =
                    [
                        new TemporaryPaymentInstallment
                        {
                            PlanId = planId,
                            DueDate = new DateOnly(2026, 10, 20),
                            Amount = 5_000m
                        }
                    ]
                }
            ]
        };
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 30_000m);

        var frozen = _service.Freeze(plan, snapshot, DateTimeOffset.UtcNow);

        Assert.Equal(2, frozen.PaymentLines.Count);
        Assert.Equal("Beyaz Eşya", frozen.PaymentLines[0].Name);
        Assert.Equal(new DateOnly(2026, 10, 15), frozen.PaymentLines[0].PlannedDate);
        Assert.Equal(12_000m, frozen.PaymentLines[0].PlannedAmount);
        Assert.Equal("Elden Borç", frozen.PaymentLines[1].Name);
        Assert.Equal(new DateOnly(2026, 10, 20), frozen.PaymentLines[1].PlannedDate);
        Assert.Equal(5_000m, frozen.PaymentLines[1].PlannedAmount);
        Assert.Equal(17_000m, frozen.PlannedMandatoryPayments + frozen.PlannedLargeExpenses);
    }

    [Fact]
    public void Freeze_DonemDisindakiYukumulukleri_OdemeSatirlarinaDahilEtmez()
    {
        var plan = CreateBasicPlan(50_000m) with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Name = "Önceki Ay Harcaması",
                    Amount = 5_000m,
                    ExactDate = new DateOnly(2026, 9, 25),
                    Status = PlannedExpenseStatus.Planned
                },
                new PlannedLargeExpense
                {
                    Name = "Sonraki Ay Harcaması",
                    Amount = 8_000m,
                    ExactDate = new DateOnly(2026, 11, 5),
                    Status = PlannedExpenseStatus.Planned
                }
            ]
        };
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);

        var frozen = _service.Freeze(plan, snapshot, DateTimeOffset.UtcNow);

        Assert.Equal(0m, frozen.PlannedLargeExpenses);
        Assert.Empty(frozen.PaymentLines);
    }

    [Fact]
    public void Freeze_BelirlenmemisKrediKarti_OzelAciklamaylaSifirTutarliSatirOlarakEklenir()
    {
        var plan = CreateBasicPlan(50_000m) with
        {
            CreditCards =
            [
                new CreditCard
                {
                    Id = Guid.NewGuid(),
                    Bank = "Garanti",
                    Name = "Bonus",
                    Limit = 50_000m,
                    BalanceAsOfDate = new DateOnly(2026, 10, 1),
                    StatementClosingDay = 5,
                    PaymentDueDay = 15,
                    CurrentStatement = new CreditCardStatement
                    {
                        StatementDate = new DateOnly(2026, 10, 5),
                        DueDate = new DateOnly(2026, 10, 15),
                        StatementAmount = 10_000m,
                        MinimumPaymentAmount = 4_000m
                    },
                    PaymentStrategy = CreditCardPaymentStrategy.AskEachStatement,
                    ProjectionFallbackStrategy = ProjectionFallbackStrategy.None
                }
            ]
        };
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 10_000m);

        var frozen = _service.Freeze(plan, snapshot, DateTimeOffset.UtcNow);

        var cardLine = Assert.Single(frozen.PaymentLines);
        Assert.Equal("Garanti Bonus", cardLine.Name);
        Assert.Equal(0m, cardLine.PlannedAmount);
        Assert.Equal("Ödeme tutarı dönem başında belirlenmemişti.", cardLine.Detail);
    }

    [Fact]
    public void Freeze_GelirDonemOrtasindaYatiyorsa_GelirSatiriTarihiyleDondurulur()
    {
        // Çapa ayın 1'i, maaş ayın 15'i: gelir dönemin ortasında yatar (S31)
        var plan = CreateBasicPlan(40_000m, paymentDay: 15);
        var salaryId = plan.RecurringIncomes[0].Id;
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);

        var frozen = _service.Freeze(plan, snapshot, DateTimeOffset.UtcNow);

        var salary = Assert.Single(frozen.IncomeLines);
        Assert.Equal(frozen.Id, salary.PeriodPlanSnapshotId);
        Assert.Equal(IncomeSourceType.Recurring, salary.SourceType);
        Assert.Equal(salaryId, salary.RecurringIncomeId);
        Assert.Null(salary.AdHocIncomeId);
        Assert.Equal("Maaş", salary.Name);
        Assert.Equal(new DateOnly(2026, 10, 15), salary.PlannedDate);
        Assert.Equal(40_000m, salary.PlannedAmount);
    }

    [Fact]
    public void Freeze_DuzenliVeTekSeferlikGelir_SatirToplamiPlanlananGelireKurusuKurusunaEsittir()
    {
        var bonusId = Guid.NewGuid();
        var plan = CreateBasicPlan(38_750.25m, paymentDay: 15) with
        {
            AdHocIncomes =
            [
                new AdHocIncome { Id = bonusId, Description = "İkramiye", Amount = 1_249.75m, ExactDate = new DateOnly(2026, 10, 20) },
                new AdHocIncome { Description = "Sonraki Dönem İadesi", Amount = 900m, ExactDate = new DateOnly(2026, 11, 5) }
            ]
        };
        var snapshot = CreateSnapshot(new DateOnly(2026, 10, 1), 20_000m);

        var frozen = _service.Freeze(plan, snapshot, DateTimeOffset.UtcNow);

        Assert.Equal(40_000m, frozen.PlannedIncome);
        Assert.Equal(frozen.PlannedIncome, frozen.IncomeLines.Sum(x => x.PlannedAmount));
        Assert.Collection(
            frozen.IncomeLines,
            salary => Assert.Equal(new DateOnly(2026, 10, 15), salary.PlannedDate),
            bonus =>
            {
                Assert.Equal(IncomeSourceType.AdHoc, bonus.SourceType);
                Assert.Equal(bonusId, bonus.AdHocIncomeId);
                Assert.Equal("İkramiye", bonus.Name);
                Assert.Equal(new DateOnly(2026, 10, 20), bonus.PlannedDate);
            });
    }

    private static FinancialSnapshot CreateSnapshot(DateOnly date, decimal balance) => new()
    {
        Id = Guid.NewGuid(),
        SnapshotDate = date,
        ProjectionAnchorDate = date,
        ProjectionOpeningBalance = balance,
        Anchor = new PeriodAnchor(1),
        Source = FinancialSnapshotSource.MonthlyUpdate,
        IsCurrent = true
    };

    private static FinancialPlan CreateBasicPlan(decimal income, int paymentDay = 1)
    {
        var id = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 10, 1),
                PeriodVariableExpenseAllowance = 15_000m
            },
            RecurringIncomes = [new RecurringIncome { Id = id, Name = "Maaş", PaymentDay = paymentDay, IsActive = true }],
            IncomeHistories = [new IncomeAmountHistory { RecurringIncomeId = id, Amount = income, EffectiveDate = new DateOnly(2026, 10, 1) }]
        };
    }

    private static FinancialProjectionCalculator CreateCalculator()
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        return new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            new CreditCardStatementCalculator(),
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());
    }
}
