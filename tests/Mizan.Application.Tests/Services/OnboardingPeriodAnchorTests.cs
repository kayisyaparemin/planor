using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Xunit;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Kurulum sihirbazının (OnboardingService) çapa gününden sonraki bir tarihte çalıştırıldığında
/// ilk açık dönemi bir sonraki aya ötelemeyip bugünün içinde bulunduğu takvim döneminden başlattığını doğrular.
/// </summary>
public sealed class OnboardingPeriodAnchorTests
{
    private readonly InMemoryUserSettingsRepository _settingsRepo = new();
    private readonly InMemoryRecurringIncomeRepository _incomeRepo = new();
    private readonly InMemoryCreditCardRepository _cardRepo = new();
    private readonly InMemoryLoanRepository _loanRepo = new();
    private readonly InMemoryTemporaryPaymentPlanRepository _planRepo = new();
    private readonly InMemoryPlannedLargeExpenseRepository _expenseRepo = new();
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();
    private readonly FixedClock _clock = new(new DateOnly(2026, 10, 5), new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero));
    private readonly CashFlowPeriodCalculator _periodCalculator = new();

    [Fact]
    public async Task InitializeFromOnboardingAsync_BugunCapaGunundenSonraysa_IcindeBulunulanDoneminBaslangiciniAlir()
    {
        var instrumentWriter = new FinancialInstrumentWriter(_loanRepo, _planRepo, _cardRepo, _expenseRepo);
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var projectionCalc = new FinancialProjectionCalculator(
            _periodCalculator,
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
            _periodCalculator,
            _clock);

        var planId = Guid.NewGuid();
        var draft = new OnboardingDraft
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionOpeningBalance = 10000m,
                PeriodVariableExpenseAllowance = 5000m
            },
            PaymentPlans =
            [
                new TemporaryPaymentPlan
                {
                    Id = planId,
                    Name = "Gün",
                    Installments =
                    [
                        new TemporaryPaymentInstallment
                        {
                            PlanId = planId,
                            DueDate = new DateOnly(2026, 10, 5),
                            Amount = 7000m
                        }
                    ]
                }
            ]
        };

        await service.InitializeFromOnboardingAsync(draft);

        var history = await _historyRepo.GetFinancialHistoryAsync();
        var openPlan = history.FindOpenPlan();
        Assert.NotNull(openPlan);

        // Bugün 5 Ekim 2026, çapa günü 1 iken ilk açık dönem 1 Ekim 2026 - 1 Kasım 2026 olmalıdır.
        Assert.Equal(new DateOnly(2026, 10, 1), openPlan.PeriodStart);
        Assert.Equal(new DateOnly(2026, 11, 1), openPlan.PeriodEnd);

        // 5 Ekim vadeli taksit cari dönemin ödeme satırlarında yer almalıdır.
        var paymentLine = Assert.Single(openPlan.PaymentLines);
        Assert.Equal(7000m, paymentLine.PlannedAmount);
        Assert.Equal(new DateOnly(2026, 10, 5), paymentLine.PlannedDate);
    }

    [Fact]
    public async Task EnsureInitialSnapshotAsync_GelecekteKalmisIlkSnapshot_BugununDonemineOnarilir()
    {
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

        var planSnapshotService = new PeriodPlanSnapshotService(projectionCalc, periodCalc);
        var snapshotService = new FinancialSnapshotService(_historyRepo, _clock, planSnapshotService, periodCalc);

        // Hatalı oluşturulmuş durum: 1 Kasım 2026'da kalmış ilk snapshot
        var initialPlan = new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionOpeningBalance = 10000m,
                ProjectionAnchorDate = new DateOnly(2026, 11, 1),
                PeriodVariableExpenseAllowance = 5000m
            }
        };

        var bundle = snapshotService.Build(
            initialPlan, 10000m, new DateOnly(2026, 11, 1), FinancialSnapshotSource.Initial, "Eski", null);
        await _historyRepo.SaveCurrentFinancialSnapshotAsync(bundle.Snapshot, bundle.Plan);

        // Bugün 5 Ekim iken EnsureInitialSnapshotAsync çağrıldığında
        var repaired = await snapshotService.EnsureInitialSnapshotAsync(initialPlan);
        Assert.NotNull(repaired);

        Assert.Equal(new DateOnly(2026, 10, 1), repaired.SnapshotDate);
        Assert.Equal(new DateOnly(2026, 11, 1), repaired.NextSettlementDate);

        var history = await _historyRepo.GetFinancialHistoryAsync();
        var openPlan = history.FindOpenPlan();
        Assert.NotNull(openPlan);
        Assert.Equal(new DateOnly(2026, 10, 1), openPlan.PeriodStart);
    }

    private sealed class FixedClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => utcNow;
        public DateTimeOffset Now => UtcNow;
    }
}
