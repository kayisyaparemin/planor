using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Xunit;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Kurulum sihirbazının (OnboardingService) ilk başlangıç ayarlarını, gelirlerini,
/// enstrümanlarını kaydetmesini ve ilk açık döneme ait canlı gözlem kaydını (PeriodObservation)
/// oluşturmasını doğrulayan birim testleridir.
/// </summary>
public sealed class OnboardingServiceTests
{
    private readonly InMemoryUserSettingsRepository _settingsRepo = new();
    private readonly InMemoryRecurringIncomeRepository _incomeRepo = new();
    private readonly InMemoryCreditCardRepository _cardRepo = new();
    private readonly InMemoryLoanRepository _loanRepo = new();
    private readonly InMemoryTemporaryPaymentPlanRepository _planRepo = new();
    private readonly InMemoryPlannedLargeExpenseRepository _expenseRepo = new();
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();
    private readonly InMemoryPeriodObservationRepository _observationRepo = new();
    private readonly FixedClock _clock = new(new DateOnly(2026, 9, 27), new DateTimeOffset(2026, 9, 27, 10, 0, 0, TimeSpan.Zero));
    private readonly CashFlowPeriodCalculator _periodCalculator = new();

    [Fact]
    public async Task InitializeFromOnboardingAsync_BaslangicBakiyesini_IlkAcikDonemGozlemiOlarakKaydeder()
    {
        var instrumentWriter = new FinancialInstrumentWriter(_loanRepo, _planRepo, _cardRepo, _expenseRepo);
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var projectionCalc = new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            new CreditCardStatementCalculator(),
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());

        var planSnapshotService = new PeriodPlanSnapshotService(projectionCalc, _periodCalculator);
        var snapshotService = new FinancialSnapshotService(_historyRepo, _clock, planSnapshotService, _periodCalculator);
        var planWriter = new OnboardingPlanWriter(_settingsRepo, _incomeRepo, instrumentWriter);

        var service = new OnboardingService(
            planWriter,
            snapshotService,
            _historyRepo,
            _observationRepo,
            _clock);

        var draft = new OnboardingDraft
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(15),
                ProjectionOpeningBalance = 25000m,
                PeriodVariableExpenseAllowance = 10000m
            },
            RecurringIncomes =
            [
                new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", PaymentDay = 15 }
            ],
            IncomeAmountHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = Guid.NewGuid(),
                    Amount = 50000m,
                    EffectiveDate = new DateOnly(2026, 9, 27)
                }
            ]
        };

        await service.InitializeFromOnboardingAsync(draft);

        var history = await _historyRepo.GetFinancialHistoryAsync();
        var openPlan = history.FindOpenPlan();
        Assert.NotNull(openPlan);

        var observation = await _observationRepo.GetPeriodObservationAsync(openPlan.Id);
        Assert.NotNull(observation);
        Assert.Equal(25000m, observation.ObservedBalance);
    }

    [Fact]
    public async Task InitializeFromOnboardingAsync_KartGuncelBorcuVarsa_IlkDonemOdemeSatirinaYansir()
    {
        var instrumentWriter = new FinancialInstrumentWriter(_loanRepo, _planRepo, _cardRepo, _expenseRepo);
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var projectionCalc = new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            new CreditCardStatementCalculator(),
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());

        var planSnapshotService = new PeriodPlanSnapshotService(projectionCalc, _periodCalculator);
        var snapshotService = new FinancialSnapshotService(_historyRepo, _clock, planSnapshotService, _periodCalculator);
        var planWriter = new OnboardingPlanWriter(_settingsRepo, _incomeRepo, instrumentWriter);

        var service = new OnboardingService(
            planWriter,
            snapshotService,
            _historyRepo,
            _observationRepo,
            _clock);

        var card = new CreditCard
        {
            Name = "Akbank Axess",
            Bank = "Akbank",
            Limit = 50000m,
            StatementClosingDay = 28,
            PaymentDueDay = 8,
            CarriedBalance = 15000m,
            PaymentStrategy = CreditCardPaymentStrategy.FullStatement,
            BalanceAsOfDate = new DateOnly(2026, 9, 27)
        };

        var draft = new OnboardingDraft
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionOpeningBalance = 25000m,
                PeriodVariableExpenseAllowance = 10000m
            },
            CreditCards = [card],
            RecurringIncomes =
            [
                new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", PaymentDay = 10 }
            ],
            IncomeAmountHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = Guid.NewGuid(),
                    Amount = 50000m,
                    EffectiveDate = new DateOnly(2026, 9, 27)
                }
            ]
        };

        await service.InitializeFromOnboardingAsync(draft);

        var history = await _historyRepo.GetFinancialHistoryAsync();
        var openPlan = history.FindOpenPlan();
        Assert.NotNull(openPlan);

        var cardPaymentLine = Assert.Single(openPlan.PaymentLines, x => x.SourceType == PlanPaymentSourceType.CreditCard);
        Assert.Equal(15000m, cardPaymentLine.PlannedAmount);
    }

    private sealed class FixedClock(DateOnly today, DateTimeOffset utcNow) : Mizan.Application.Abstractions.IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => utcNow;
    }
}
