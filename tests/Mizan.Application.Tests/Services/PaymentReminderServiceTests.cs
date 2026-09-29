using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PaymentReminderServiceTests
{
    private static readonly DateOnly InitialDate = new(2026, 9, 1);
    private static readonly DateOnly SettlementDate = new(2026, 10, 1);
    private static readonly DateTimeOffset InitialNowUtc = new(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly MutableClock _clock = new(InitialDate, InitialNowUtc);
    private readonly InMemoryPaymentReminderRepository _reminderRepo = new();
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();
    private readonly InMemoryPeriodObservationRepository _observationRepo = new();

    private PaymentReminderService CreateService(out FinancialProjectionService projectionService, out FakePlanReader planReader)
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var cardStatementCalc = new CreditCardStatementCalculator();

        var projectionCalc = new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            cardStatementCalc,
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());

        projectionService = new FinancialProjectionService(projectionCalc);
        planReader = new FakePlanReader();

        return new PaymentReminderService(
            _reminderRepo,
            _historyRepo,
            _observationRepo,
            planReader,
            projectionService);
    }

    [Fact]
    public void Yapici_NullParametreleri_Reddeder()
    {
        CreateService(out var projectionService, out var planReader);

        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(null!, _historyRepo, _observationRepo, planReader, projectionService));
        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(_reminderRepo, null!, _observationRepo, planReader, projectionService));
        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(_reminderRepo, _historyRepo, null!, planReader, projectionService));
        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(_reminderRepo, _historyRepo, _observationRepo, null!, projectionService));
        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(_reminderRepo, _historyRepo, _observationRepo, planReader, null!));
    }

    [Fact]
    public async Task GetModeAsync_Ve_SaveModeAsync_ModuKaydederVeOkur()
    {
        var service = CreateService(out _, out _);

        Assert.Equal(PaymentReminderMode.Off, await service.GetModeAsync());

        await service.SaveModeAsync(PaymentReminderMode.Aggressive);

        Assert.Equal(PaymentReminderMode.Aggressive, await service.GetModeAsync());
    }

    [Fact]
    public async Task GetRemindersAsync_ModKapaliyken_BosListeDoner()
    {
        var service = CreateService(out _, out _);
        await SeedCurrentPlanAsync();

        var reminders = await service.GetRemindersAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        Assert.Empty(reminders);
    }

    [Fact]
    public async Task GetRemindersAsync_ModAcikken_HatirlaticilariSiraliDoner()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);
        await service.SaveModeAsync(PaymentReminderMode.Relaxed);

        var now = new DateTime(2026, 9, 1, 10, 0, 0);
        var reminders = await service.GetRemindersAsync(now);

        Assert.NotEmpty(reminders);
        Assert.True(reminders[0].NotifyAt >= now);
    }

    [Fact]
    public async Task GetBoardAsync_ModKapaliyken_BosPanoDoner()
    {
        var service = CreateService(out _, out _);

        var board = await service.GetBoardAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        Assert.Equal(PaymentReminderMode.Off, board.Mode);
        Assert.Empty(board.Reminders);
        Assert.Empty(board.Upcoming);
        Assert.Null(board.Sample);
    }

    [Fact]
    public async Task GetBoardAsync_ModAcikken_PanoDetaylariniDoldurur()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);
        await service.SaveModeAsync(PaymentReminderMode.Relaxed);

        var now = new DateTime(2026, 9, 1, 10, 0, 0);
        var board = await service.GetBoardAsync(now);

        Assert.Equal(PaymentReminderMode.Relaxed, board.Mode);
        Assert.NotEmpty(board.Reminders);
        Assert.NotEmpty(board.Upcoming);
        Assert.NotNull(board.Sample);
    }

    [Fact]
    public async Task RecordAnswerAsync_BosGeldiginde_HicbirSeyYapmaz()
    {
        var service = CreateService(out _, out _);

        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 5, 12, 0, 0),
            null,
            []));

        var responses = await service.GetResponsesAsync();
        Assert.Empty(responses);
    }

    [Fact]
    public async Task RecordAnswerAsync_YanitlariKaydeder_VePaidOlaninUstuneSnoozedYazmaz()
    {
        var service = CreateService(out _, out _);
        var dueKey = "loan:123:2026-09-10";

        // Önce Paid kaydet
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 5, 12, 0, 0),
            null,
            [new PaymentDue(dueKey, "Kredi", new DateOnly(2026, 9, 10), 10_000m)]));

        var stored = await service.GetResponsesAsync();
        Assert.Single(stored);
        Assert.Equal(PaymentReminderAnswerKind.Paid, stored[0].Kind);

        // Şimdi aynı anahtara Snoozed gönder
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Snoozed,
            new DateTime(2026, 9, 6, 12, 0, 0),
            new DateTime(2026, 9, 7, 9, 0, 0),
            [new PaymentDue(dueKey, "Kredi", new DateOnly(2026, 9, 10), 10_000m)]));

        var afterSnooze = await service.GetResponsesAsync();
        Assert.Single(afterSnooze);
        // Hâlâ Paid kalmalı!
        Assert.Equal(PaymentReminderAnswerKind.Paid, afterSnooze[0].Kind);
    }

    [Fact]
    public async Task UndoAnswerAsync_YanitiSiler()
    {
        var service = CreateService(out _, out _);
        var dueKey = "loan:123:2026-09-10";

        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 5, 12, 0, 0),
            null,
            [new PaymentDue(dueKey, "Kredi", new DateOnly(2026, 9, 10), 10_000m)]));

        await service.UndoAnswerAsync(dueKey);

        var responses = await service.GetResponsesAsync();
        Assert.Empty(responses);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_AcikPlanYoksa_BosListeDoner()
    {
        var service = CreateService(out _, out _);

        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        Assert.Empty(dues);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_OdenmisGozlemleri_VePaidYanitlariFiltreler()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        var snapshotPlan = await SeedCurrentPlanAsync(plan);

        var line = snapshotPlan.PaymentLines[0];
        var dueKey = PaymentReminderPlanner.DueKey(line.SourceEntityId, line.Name, line.PlannedDate);

        // 1. Hatırlatıcı yanıtı olarak Paid kaydet
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 2, 10, 0, 0),
            null,
            [new PaymentDue(dueKey, line.Name, line.PlannedDate, line.PlannedAmount)]));

        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        // Paid yanıtı verildiği için listede olmamalı
        Assert.DoesNotContain(dues, x => x.Key == dueKey);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_OdendiIsaretliSatir_ListedeOlmaz()
    {
        // Hazırla
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        var snapshotPlan = await SeedCurrentPlanAsync(plan);
        var line = snapshotPlan.PaymentLines[0];
        var dueKey = PaymentReminderPlanner.DueKey(line.SourceEntityId, line.Name, line.PlannedDate);
        await _observationRepo.UpsertPaymentMarkAsync(new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = snapshotPlan.Id,
            PeriodPlanPaymentLineId = line.Id,
            Status = ActualPaymentStatus.Paid,
            ActualAmount = 10_000m
        });

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        // Doğrula
        Assert.DoesNotContain(dues, x => x.Key == dueKey);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_OdenmediIsaretliSatir_ListedeKalir()
    {
        // Hazırla
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        var snapshotPlan = await SeedCurrentPlanAsync(plan);
        var line = snapshotPlan.PaymentLines[0];
        var dueKey = PaymentReminderPlanner.DueKey(line.SourceEntityId, line.Name, line.PlannedDate);
        await _observationRepo.UpsertPaymentMarkAsync(new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = snapshotPlan.Id,
            PeriodPlanPaymentLineId = line.Id,
            Status = ActualPaymentStatus.Unpaid
        });

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        // Doğrula
        Assert.Contains(dues, x => x.Key == dueKey);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_UfukDonemiAstiginda_ProjeksiyondanVeBuyukHarcamalardanToplar()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        var expenseId = Guid.NewGuid();
        plan = plan with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Id = expenseId,
                    Name = "Yıllık Sigorta",
                    Amount = 15_000m,
                    Status = PlannedExpenseStatus.Planned,
                    ExactDate = new DateOnly(2026, 10, 3)
                }
            ]
        };
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);

        // 15 Eylül'de çağır: Ufuk 20 Ekim'e kadar uzanır (> 1 Ekim dönem sonu)
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 15, 10, 0, 0));

        Assert.NotEmpty(dues);
        Assert.Contains(dues, x => x.Name == "Yıllık Sigorta" && x.DueDate == new DateOnly(2026, 10, 3));
    }

    private async Task<PeriodPlanSnapshot> SeedCurrentPlanAsync(FinancialPlan? plan = null)
    {
        var snapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = InitialDate,
            ProjectionAnchorDate = InitialDate,
            ProjectionOpeningBalance = 50_000m,
            Anchor = new PeriodAnchor(1),
            Source = FinancialSnapshotSource.Initial,
            IsCurrent = true,
            CreatedAtUtc = InitialNowUtc
        };

        var frozenPlan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            SettlementAvailableFrom = SettlementDate,
            OpeningBalance = 50_000m,
            PlannedIncome = 60_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            CreatedAtUtc = InitialNowUtc,
            PaymentLines =
            [
                new PeriodPlanPaymentLine
                {
                    Id = Guid.NewGuid(),
                    PeriodPlanSnapshotId = Guid.Empty,
                    SourceType = PlanPaymentSourceType.Loan,
                    SourceEntityId = Guid.NewGuid(),
                    Name = "Kira / Kredi",
                    PlannedDate = new DateOnly(2026, 9, 10),
                    PlannedAmount = 10_000m
                }
            ]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);
        return frozenPlan;
    }

    private static FinancialPlan CreateSampleFinancialPlan()
    {
        var incomeId = Guid.NewGuid();
        var loanId = Guid.NewGuid();

        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                PeriodVariableExpenseAllowance = 20_000m,
                ProjectionAnchorDate = InitialDate,
                ProjectionOpeningBalance = 50_000m,
                DeficitFinancingInterestRate = 0.05m
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = incomeId,
                    Name = "Maaş",
                    PaymentDay = 1,
                    IsActive = true
                }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = incomeId,
                    Amount = 60_000m,
                    EffectiveDate = InitialDate
                }
            ],
            Loans =
            [
                new Loan
                {
                    Id = loanId,
                    Name = "İhtiyaç Kredisi",
                    Bank = "İş Bankası",
                    MonthlyPayment = 10_000m,
                    PaymentDay = 10,
                    NextPaymentDate = new DateOnly(2026, 9, 10),
                    RemainingInstallmentCount = 12
                }
            ]
        };
    }

    private sealed class FakePlanReader : IPlanReader
    {
        public FinancialPlan PlanToReturn { get; set; } = new();

        public Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(PlanToReturn);

        public Task<ProjectionQueryPlan> GetProjectionPlanAsync(DateOnly asOf, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProjectionQueryPlan(PlanToReturn, null));
    }

    private sealed class MutableClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today { get; private set; } = today;
        public DateTimeOffset UtcNow { get; private set; } = utcNow;

        public void SetDate(DateOnly today)
        {
            Today = today;
            UtcNow = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }
    }
}
