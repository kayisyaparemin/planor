using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PeriodWorkflowServiceTests
{
    private static readonly DateOnly InitialDate = new(2026, 9, 1);
    private static readonly DateOnly SettlementDate = new(2026, 10, 1);
    private static readonly DateTimeOffset InitialNowUtc = new(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly MutableClock _clock = new(InitialDate, InitialNowUtc);
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();
    private readonly InMemoryPeriodObservationRepository _observationRepo = new();

    private PeriodWorkflowService CreateService(out PeriodSettlementService settlementService, out FakePlanReader planReader)
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var cardStatementCalc = new CreditCardStatementCalculator();
        var cardReconciler = new CreditCardActualPaymentReconciler();
        var loanReconciler = new LoanInstrumentReconciler(amortCalc, loanBuilder);

        var projectionCalc = new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            cardStatementCalc,
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());

        var planSnapshotService = new PeriodPlanSnapshotService(projectionCalc, periodCalc);
        var snapshotService = new FinancialSnapshotService(_historyRepo, _clock, planSnapshotService, periodCalc);
        var instrumentService = new FinancialInstrumentReconciliationService(
            cardReconciler,
            cardStatementCalc,
            loanReconciler);
        var comparisonCalc = new PlanActualComparisonCalculator();

        settlementService = new PeriodSettlementService(
            _historyRepo,
            _clock,
            snapshotService,
            instrumentService,
            comparisonCalc);

        planReader = new FakePlanReader();

        return new PeriodWorkflowService(
            _historyRepo,
            _observationRepo,
            planReader,
            settlementService,
            _clock);
    }

    [Fact]
    public void Yapici_NullParametreleri_Reddeder()
    {
        CreateService(out var settlementService, out var planReader);

        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(null!, _observationRepo, planReader, settlementService, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, null!, planReader, settlementService, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, _observationRepo, null!, settlementService, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, _observationRepo, planReader, null!, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, _observationRepo, planReader, settlementService, null!));
    }

    [Fact]
    public async Task GetSettlementAvailabilityAsync_TarihceYokken_HazirDegilDoner()
    {
        var service = CreateService(out _, out _);

        var availability = await service.GetSettlementAvailabilityAsync();

        Assert.False(availability.HasCurrentSnapshot);
        Assert.False(availability.IsDue);
    }

    [Fact]
    public async Task GetSettlementAvailabilityAsync_CapaGunuGeldiginde_HazirDoner()
    {
        var service = CreateService(out _, out _);
        await SeedCurrentPlanAsync();
        _clock.SetDate(SettlementDate);

        var availability = await service.GetSettlementAvailabilityAsync();

        Assert.True(availability.HasCurrentSnapshot);
        Assert.True(availability.IsDue);
        Assert.NotNull(availability.PendingPlan);
    }

    [Fact]
    public async Task GetSettlementContextAsync_AcikPlanin_BaglaminiGetirir()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var context = await service.GetSettlementContextAsync();

        Assert.NotNull(context.OriginalPlan);
        Assert.Equal(plan.Id, context.OriginalPlan.Id);
        Assert.True(context.SuggestedStartingBalance > 0m);
    }

    [Fact]
    public async Task GetObservedSettlementDraftAsync_GozlemYoksa_NullDoner()
    {
        var service = CreateService(out _, out _);

        var draft = await service.GetObservedSettlementDraftAsync(Guid.NewGuid());

        Assert.Null(draft);
    }

    [Fact]
    public async Task GetObservedSettlementDraftAsync_GozlemVarsa_TaslagiUretir_FlowsBosKalir()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        var lineId = plan.PaymentLines[0].Id;

        await _observationRepo.UpsertPeriodObservationAsync(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedBalance = 42_000m,
            ObservedLivingSpend = 8_000m,
            Note = "Dönem ortası gözlemi",
            Payments =
            [
                new PeriodObservationPayment
                {
                    PeriodPlanPaymentLineId = lineId,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 5_000m,
                    ActualPaymentDate = new DateOnly(2026, 9, 15),
                    Note = "Kira ödendi"
                }
            ]
        });

        var draft = await service.GetObservedSettlementDraftAsync(plan.Id);

        Assert.NotNull(draft);
        Assert.Equal(plan.Id, draft.PeriodPlanSnapshotId);
        Assert.Equal(42_000m, draft.ConfirmedEndingBalance);
        Assert.Equal(8_000m, draft.ActualLivingSpend);
        Assert.Equal(0m, draft.ActualInterest);
        Assert.Equal("Dönem ortası gözlemi", draft.ActualNote);
        Assert.Empty(draft.Flows);
        Assert.Single(draft.Payments);
        Assert.Equal(lineId, draft.Payments[0].PeriodPlanPaymentLineId);
        Assert.Equal(5_000m, draft.Payments[0].ActualAmount);
        Assert.Equal(ActualPaymentStatus.Paid, draft.Payments[0].Status);
    }

    [Fact]
    public async Task PreviewSettlementAsync_TaslagiHesaplar()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ConfirmedEndingBalance = 45_000m,
            ActualLivingSpend = 7_000m
        };

        var preview = await service.PreviewSettlementAsync(draft);

        Assert.NotNull(preview);
        Assert.Equal(45_000m, preview.ConfirmedEndingBalance);
        Assert.NotNull(preview.Comparison);
    }

    [Fact]
    public async Task FinalizeSettlementAsync_DonemiKapatir_VeGozlemDefteriniSiler()
    {
        var service = CreateService(out _, out var planReader);
        var samplePlan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = samplePlan;

        var plan = await SeedCurrentPlanAsync();
        _clock.SetDate(SettlementDate);

        await _observationRepo.UpsertPeriodObservationAsync(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedBalance = 48_000m
        });

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ConfirmedEndingBalance = 48_000m,
            ActualLivingSpend = 6_000m,
            Payments =
            [
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = plan.PaymentLines[0].Id,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = plan.PaymentLines[0].PlannedAmount!.Value,
                    ActualPaymentDate = new DateOnly(2026, 9, 10)
                }
            ]
        };

        var result = await service.FinalizeSettlementAsync(draft);

        Assert.NotNull(result);
        Assert.NotNull(result.NewSnapshot);
        Assert.NotNull(result.NewPlan);
        Assert.Equal(SettlementDate, result.NewPlan.PeriodStart);

        var observationAfter = await _observationRepo.GetPeriodObservationAsync(plan.Id);
        Assert.Null(observationAfter);
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_AcikPlanYoksa_HataFirlatir()
    {
        var service = CreateService(out _, out _);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ObserveCurrentBalanceAsync(30_000m));
        Assert.Contains("güncel bir dönem planı", ex.Message);
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_GozlemYoksa_YeniKayitOlusturur()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var observation = await service.ObserveCurrentBalanceAsync(25_000m);

        Assert.Equal(plan.Id, observation.PeriodPlanSnapshotId);
        Assert.Equal(25_000m, observation.ObservedBalance);
        Assert.Equal(InitialDate, observation.ObservedOn);

        var stored = await _observationRepo.GetPeriodObservationAsync(plan.Id);
        Assert.NotNull(stored);
        Assert.Equal(25_000m, stored.ObservedBalance);
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_GozlemVarsa_MevcutKaydiGunceller()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        await service.ObserveCurrentBalanceAsync(20_000m);
        _clock.SetDate(new DateOnly(2026, 9, 10));

        var updated = await service.ObserveCurrentBalanceAsync(18_500m);

        Assert.Equal(18_500m, updated.ObservedBalance);
        Assert.Equal(new DateOnly(2026, 9, 10), updated.ObservedOn);

        var stored = await _observationRepo.GetPeriodObservationAsync(plan.Id);
        Assert.NotNull(stored);
        Assert.Equal(18_500m, stored.ObservedBalance);
    }

    [Fact]
    public async Task ObservePaymentAsync_SatirPlandaYoksa_HataFirlatir()
    {
        var service = CreateService(out _, out _);
        await SeedCurrentPlanAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ObservePaymentAsync(Guid.NewGuid(), ActualPaymentStatus.Paid, 1_000m));
        Assert.Contains("bulunamadı", ex.Message);
    }

    [Fact]
    public async Task ObservePaymentAsync_SatirPlandaysa_OdemeGozleminiKaydeder()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        var lineId = plan.PaymentLines[0].Id;

        var observation = await service.ObservePaymentAsync(
            lineId,
            ActualPaymentStatus.Paid,
            5_000m,
            new DateOnly(2026, 9, 5),
            "  Açıklama notu  ");

        Assert.Single(observation.Payments);
        Assert.Equal(lineId, observation.Payments[0].PeriodPlanPaymentLineId);
        Assert.Equal(5_000m, observation.Payments[0].ActualAmount);
        Assert.Equal(new DateOnly(2026, 9, 5), observation.Payments[0].ActualPaymentDate);
        Assert.Equal("Açıklama notu", observation.Payments[0].Note);
    }

    [Fact]
    public async Task ObservePaymentAsync_SatirSonRevizyondaysa_GozlemiKaydeder()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var newRevisionLineId = Guid.NewGuid();
        var revision = new PeriodPlanRevision
        {
            PeriodPlanSnapshotId = plan.Id,
            RevisionNumber = 1,
            CreatedAtUtc = _clock.UtcNow,
            PaymentLines =
            [
                new PeriodPlanPaymentLine
                {
                    Id = newRevisionLineId,
                    PeriodPlanSnapshotId = plan.Id,
                    SourceType = PlanPaymentSourceType.Loan,
                    SourceEntityId = Guid.NewGuid(),
                    Name = "Yeni Revize Kredi",
                    PlannedDate = new DateOnly(2026, 9, 20),
                    PlannedAmount = 3_000m
                }
            ]
        };
        await _historyRepo.SavePeriodPlanRevisionAsync(revision);

        var observation = await service.ObservePaymentAsync(
            newRevisionLineId,
            ActualPaymentStatus.Paid,
            3_000m);

        Assert.Single(observation.Payments);
        Assert.Equal(newRevisionLineId, observation.Payments[0].PeriodPlanPaymentLineId);
    }

    private async Task<PeriodPlanSnapshot> SeedCurrentPlanAsync()
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
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionOpeningBalance = 50_000m,
                ProjectionAnchorDate = InitialDate,
                PeriodVariableExpenseAllowance = 20_000m
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
