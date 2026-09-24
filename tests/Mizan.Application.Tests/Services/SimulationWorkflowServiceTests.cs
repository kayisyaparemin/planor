using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class SimulationWorkflowServiceTests
{
    private readonly TestClock _clock = new(
        new DateOnly(2026, 9, 25),
        new DateTimeOffset(2026, 9, 25, 10, 0, 0, TimeSpan.Zero));

    private readonly InMemorySimulationDraftRepository _draftRepository = new();
    private readonly FakePlanReader _planReader = new();
    private readonly FakeSimulationPlanApplier _planApplier = new();

    private readonly SimulationCalculator _calculator;

    public SimulationWorkflowServiceTests()
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

        var projectionCalculator = new FinancialProjectionCalculator(
            periodCalc,
            incomeCalc,
            cardCalc,
            mandatoryCalc,
            grouper);

        var installments = new InstallmentScheduleCalculator();
        var planBuilder = new ScenarioPlanBuilder(installments, loanValidator);
        _calculator = new SimulationCalculator(projectionCalculator, planBuilder, loanScheduleBuilder);
    }

    private SimulationWorkflowService CreateSut() =>
        new(_clock, _planReader, _draftRepository, _calculator, _planApplier);

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(null!, _planReader, _draftRepository, _calculator, _planApplier));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(_clock, null!, _draftRepository, _calculator, _planApplier));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(_clock, _planReader, null!, _calculator, _planApplier));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(_clock, _planReader, _draftRepository, null!, _planApplier));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(_clock, _planReader, _draftRepository, _calculator, null!));
    }

    [Fact]
    public async Task SimulateAsync_CannotBuildProjection_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        _planReader.PlanToReturn = new FinancialPlan(); // CanBuildProjection = false

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Harcama",
            Amount = 5_000m,
            StartDate = new DateOnly(2026, 10, 1)
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SimulateAsync(request));
        Assert.Equal("Simülasyon yapabilmek için önce gelirini veya açılış bakiyeni tanımlamalısın.", ex.Message);
    }

    [Fact]
    public async Task SimulateAsync_ValidPlan_ReturnsSimulationResult()
    {
        var sut = CreateSut();
        _planReader.PlanToReturn = CreateValidPlan();

        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Harcama",
            Amount = 5_000m,
            StartDate = new DateOnly(2026, 10, 1)
        };

        var result = await sut.SimulateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(12, result.Baseline.Count);
        Assert.Equal(12, result.Scenario.Count);
    }

    [Fact]
    public async Task SaveSimulationDraftAsync_EmptyName_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var condition = new SimulationDraftCondition(new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "x",
            Amount = 100m,
            StartDate = new DateOnly(2026, 10, 1)
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SaveSimulationDraftAsync("   ", [condition]));
        Assert.Equal("Geçici plana bir ad vermelisin.", ex.Message);
    }

    [Fact]
    public async Task SaveSimulationDraftAsync_EmptyConditions_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SaveSimulationDraftAsync("Taslak", []));
        Assert.Equal("Kaydedilecek en az bir koşul gerekiyor.", ex.Message);
    }

    [Fact]
    public async Task SaveSimulationDraftAsync_NewDraft_SavesAndReturns()
    {
        var sut = CreateSut();
        var condition = new SimulationDraftCondition(new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "x",
            Amount = 100m,
            StartDate = new DateOnly(2026, 10, 1)
        });

        var draft = await sut.SaveSimulationDraftAsync("Tatil Planı", [condition]);

        Assert.NotNull(draft);
        Assert.Equal("Tatil Planı", draft.Name);
        Assert.Equal(_clock.UtcNow, draft.CreatedAt);
        Assert.Equal(_clock.UtcNow, draft.UpdatedAt);

        var drafts = await _draftRepository.GetDraftsAsync();
        Assert.Single(drafts);
    }

    [Fact]
    public async Task SaveSimulationDraftAsync_ExistingDraft_UpdatesAndPreservesCreatedAt()
    {
        var sut = CreateSut();
        var existingId = Guid.NewGuid();
        var originalCreated = _clock.UtcNow.AddDays(-5);
        await _draftRepository.UpsertDraftAsync(new SimulationDraft(
            existingId,
            "Eski Ad",
            originalCreated,
            originalCreated,
            []));

        var condition = new SimulationDraftCondition(new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "x",
            Amount = 100m,
            StartDate = new DateOnly(2026, 10, 1)
        });

        var updated = await sut.SaveSimulationDraftAsync("Yeni Ad", [condition], existingId);

        Assert.Equal(existingId, updated.Id);
        Assert.Equal("Yeni Ad", updated.Name);
        Assert.Equal(originalCreated, updated.CreatedAt);
        Assert.Equal(_clock.UtcNow, updated.UpdatedAt);
    }

    [Fact]
    public async Task ApplySimulationAsync_NotConfirmed_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "x",
            Amount = 100m,
            StartDate = new DateOnly(2026, 10, 1),
            ScenarioId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ApplySimulationAsync(request, confirmed: false));
        Assert.Equal("Plan, açık kullanıcı onayı olmadan uygulanamaz.", ex.Message);
        Assert.Null(_planApplier.LastTrigger);
    }

    [Fact]
    public async Task ApplySimulationAsync_Confirmed_CallsPlanApplier()
    {
        var sut = CreateSut();
        _planReader.PlanToReturn = CreateValidPlan();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "x",
            Amount = 100m,
            StartDate = new DateOnly(2026, 10, 1),
            ScenarioId = Guid.NewGuid()
        };

        var result = await sut.ApplySimulationAsync(request, confirmed: true);

        Assert.NotNull(result);
        Assert.Equal("Simülasyon planı uygulandı", _planApplier.LastTrigger);
    }

    [Fact]
    public async Task AddRecordFromScenarioAsync_InvalidDirectEntryType_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.IncomeChange,
            Name = "Zam",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 1),
            ScenarioId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddRecordFromScenarioAsync(request));
        Assert.Contains("Finansal Yapı'dan bu formla girilemez", ex.Message);
    }

    [Fact]
    public async Task AddRecordFromScenarioAsync_ValidDirectEntry_CallsPlanApplier()
    {
        var sut = CreateSut();
        _planReader.PlanToReturn = CreateValidPlan();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Koltuk",
            Amount = 15_000m,
            StartDate = new DateOnly(2026, 10, 1),
            ScenarioId = Guid.NewGuid()
        };

        var result = await sut.AddRecordFromScenarioAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Finansal Yapı'dan eklendi", _planApplier.LastTrigger);
    }

    [Fact]
    public async Task SimulateAsync_WithAllowanceOverride_CalculatesWithModifiedBudget()
    {
        var sut = CreateSut();
        _planReader.PlanToReturn = CreateValidPlan();
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Harcama",
            Amount = 5_000m,
            StartDate = new DateOnly(2026, 10, 1)
        };

        var withoutOverride = await sut.SimulateAsync([request]);
        var withOverride = await sut.SimulateAsync([request], variableExpenseAllowanceOverride: 50_000m);

        Assert.NotEqual(
            withoutOverride.Scenario[0].EndingBalance,
            withOverride.Scenario[0].EndingBalance);
    }

    [Fact]
    public async Task GetSimulationDraftsAsync_AndDeleteDraftAsync_DelegatesToRepository()
    {
        var sut = CreateSut();
        var draftId = Guid.NewGuid();
        await _draftRepository.UpsertDraftAsync(new SimulationDraft(
            draftId, "Taslak", _clock.UtcNow, _clock.UtcNow, []));

        var list = await sut.GetSimulationDraftsAsync();
        Assert.Single(list);

        await sut.DeleteSimulationDraftAsync(draftId);
        var afterDelete = await sut.GetSimulationDraftsAsync();
        Assert.Empty(afterDelete);
    }

    [Fact]
    public async Task AddRecordFromScenarioAsync_IncomeBeforeAnchor_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        _planReader.PlanToReturn = CreateValidPlan(); // Anchor = 2026-09-15
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.FutureIncome,
            Name = "Geçmiş Prim",
            Amount = 10_000m,
            StartDate = new DateOnly(2026, 8, 1),
            ScenarioId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddRecordFromScenarioAsync(request));
    }

    private static FinancialPlan CreateValidPlan()
    {
        var incomeId = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(15),
                ProjectionAnchorDate = new DateOnly(2026, 9, 15),
                ProjectionOpeningBalance = 20_000m
            },
            RecurringIncomes =
            [
                new RecurringIncome { Id = incomeId, Name = "Maaş", PaymentDay = 15 }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = incomeId,
                    Amount = 50_000m,
                    EffectiveDate = new DateOnly(2026, 1, 1)
                }
            ]
        };
    }

    private sealed class FakePlanReader : IPlanReader
    {
        public FinancialPlan PlanToReturn { get; set; } = new();

        public Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(PlanToReturn);

        public Task<ProjectionQueryPlan> GetProjectionPlanAsync(
            DateOnly asOf, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProjectionQueryPlan(PlanToReturn, null));
    }

    private sealed class FakeSimulationPlanApplier : ISimulationPlanApplier
    {
        public string? LastTrigger { get; private set; }

        public Task<SimulationApplyResult> ApplyAsync(
            FinancialPlan currentPlan,
            IReadOnlyList<SimulationRequest> requests,
            string trigger,
            CancellationToken cancellationToken = default)
        {
            LastTrigger = trigger;
            return Task.FromResult(new SimulationApplyResult(
                requests[0].ScenarioId,
                Guid.NewGuid(),
                SimulationApplyDestination.Payments,
                AlreadyApplied: false,
                "Uygulandı"));
        }
    }

    private sealed class TestClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => utcNow;
    }
}
