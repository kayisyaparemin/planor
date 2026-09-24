using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PlanChangeRecorderTests
{
    private readonly InMemoryUserSettingsRepository _userSettingsRepo = new();
    private readonly InMemoryRecurringIncomeRepository _recurringIncomeRepo = new();
    private readonly InMemoryAdHocIncomeRepository _adHocIncomeRepo = new();
    private readonly InMemoryLoanRepository _loanRepo = new();
    private readonly InMemoryTemporaryPaymentPlanRepository _paymentPlanRepo = new();
    private readonly InMemoryCreditCardRepository _creditCardRepo = new();
    private readonly InMemoryPlannedLargeExpenseRepository _largeExpenseRepo = new();
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();
    private readonly CashFlowPeriodCalculator _periodCalculator = new();

    private sealed class FixedClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => utcNow;
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

    private (PlanChangeRecorder sut, IPlanReader planReader) CreateSut()
    {
        var instrumentReader = new FinancialInstrumentReader(
            _loanRepo,
            _paymentPlanRepo,
            _creditCardRepo,
            _largeExpenseRepo);

        var incomeReader = new IncomePlanReader(
            _recurringIncomeRepo,
            _adHocIncomeRepo);

        var boundaryResolver = new ProjectionBoundaryResolver(_periodCalculator);

        var planReader = new PlanReader(
            _userSettingsRepo,
            incomeReader,
            instrumentReader,
            _historyRepo,
            boundaryResolver);

        var calculator = CreateCalculator();
        var snapshotCalc = new PeriodPlanSnapshotService(calculator, _periodCalculator);
        var clock = new FixedClock(new DateOnly(2026, 10, 15), DateTimeOffset.UtcNow);

        var snapshotService = new FinancialSnapshotService(
            _historyRepo,
            clock,
            snapshotCalc,
            _periodCalculator);

        var revisionService = new HistoricalPlanRevisionService(
            _historyRepo,
            clock,
            snapshotCalc);

        var sut = new PlanChangeRecorder(
            planReader,
            snapshotService,
            revisionService);

        return (sut, planReader);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RecordChangeAsync_GecersizTetikleyicideHataFirlatir(string? invalidTrigger)
    {
        var (sut, _) = CreateSut();

        await Assert.ThrowsAnyAsync<ArgumentException>(() => sut.RecordChangeAsync(invalidTrigger!));
    }

    [Fact]
    public async Task RecordChangeAsync_IlkSnapshotYoksa_SnapshotVeAcikPlaniOlusturur()
    {
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            ProjectionOpeningBalance = 20_000m,
            ProjectionAnchorDate = new DateOnly(2026, 10, 15)
        };
        await _userSettingsRepo.SaveSettingsAsync(settings);
        var recurringIncome = new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", IsActive = true, PaymentDay = 15 };
        await _recurringIncomeRepo.UpsertRecurringIncomeAsync(recurringIncome);
        await _recurringIncomeRepo.UpsertIncomeAmountHistoryAsync(new IncomeAmountHistory
        {
            RecurringIncomeId = recurringIncome.Id,
            EffectiveDate = new DateOnly(2026, 1, 1),
            Amount = 40_000m
        });

        var (sut, _) = CreateSut();

        await sut.RecordChangeAsync("Yeni kredi eklendi");

        var history = await _historyRepo.GetFinancialHistoryAsync();
        Assert.Single(history.Snapshots);
        Assert.Single(history.Plans);
    }

    [Fact]
    public async Task RecordChangeAsync_AcikPlanVarsa_RevizyonKaydeder_I24()
    {
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            ProjectionOpeningBalance = 20_000m,
            ProjectionAnchorDate = new DateOnly(2026, 10, 15)
        };
        await _userSettingsRepo.SaveSettingsAsync(settings);
        var recurringIncome = new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", IsActive = true, PaymentDay = 15 };
        await _recurringIncomeRepo.UpsertRecurringIncomeAsync(recurringIncome);
        await _recurringIncomeRepo.UpsertIncomeAmountHistoryAsync(new IncomeAmountHistory
        {
            RecurringIncomeId = recurringIncome.Id,
            EffectiveDate = new DateOnly(2026, 1, 1),
            Amount = 40_000m
        });

        var (sut, _) = CreateSut();

        // İlk kayıt snapshot ve açık planı oluşturur
        await sut.RecordChangeAsync("Kurulum");

        // İkinci kayıt: yeni bir kredi eklenmiş olsun (fark yaratan değişiklik)
        await _loanRepo.UpsertLoanAsync(new Loan
        {
            Id = Guid.NewGuid(),
            Name = "Yeni Taşıt Kredisi",
            MonthlyPayment = 5_000m,
            PaymentDay = 20,
            RemainingInstallmentCount = 12,
            NextPaymentDate = new DateOnly(2026, 10, 20)
        });

        await sut.RecordChangeAsync("Yeni taşıt kredisi eklendi");

        var history = await _historyRepo.GetFinancialHistoryAsync();
        Assert.Single(history.Snapshots);
        Assert.Single(history.Plans);
        Assert.Single(history.Revisions);
        Assert.Equal("Yeni taşıt kredisi eklendi", history.Revisions[0].Trigger);
    }
}
