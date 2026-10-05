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

    private SimulationWorkflowService CreateSut() =>
        new(_clock, _planReader, _draftRepository, _planApplier);

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(null!, _planReader, _draftRepository, _planApplier));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(_clock, null!, _draftRepository, _planApplier));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(_clock, _planReader, null!, _planApplier));
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationWorkflowService(_clock, _planReader, _draftRepository, null!));
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
            sut.ApplySimulationAsync([request], confirmed: false));
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

        var result = await sut.ApplySimulationAsync([request], confirmed: true);

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

    [Fact]
    public async Task GetWorkingListAsync_ListeHicYazilmadiysa_BosDoner()
    {
        var sut = CreateSut();

        var list = await sut.GetWorkingListAsync();

        Assert.Empty(list);
    }

    [Fact]
    public async Task SaveWorkingListAsync_SonraOkununca_SiraVeAcikKapaliKorunur()
    {
        var sut = CreateSut();
        var phone = Condition("Telefon", new DateOnly(2026, 10, 1), isEnabled: true);
        var holiday = Condition("Tatil", new DateOnly(2026, 12, 20), isEnabled: false);

        await sut.SaveWorkingListAsync([phone, holiday]);
        var list = await sut.GetWorkingListAsync();

        Assert.Equal(["Telefon", "Tatil"], list.Select(x => x.Request.Name));
        Assert.Equal([true, false], list.Select(x => x.IsEnabled));
        Assert.Equal(phone.Request, list[0].Request);
    }

    [Fact]
    public async Task GetWorkingListAsync_TarihiDundeKalanDeneme_TarihiGectiIsaretlenir()
    {
        var sut = CreateSut();
        var yesterday = Condition("Dün", _clock.Today.AddDays(-1), isEnabled: true);
        var today = Condition("Bugün", _clock.Today, isEnabled: true);
        await sut.SaveWorkingListAsync([yesterday, today]);

        var list = await sut.GetWorkingListAsync();

        Assert.Equal(
            [SimulationConditionIssue.DatePassed, SimulationConditionIssue.None],
            list.Select(x => x.Issue));
    }

    [Fact]
    public async Task SaveWorkingListAsync_BosListe_ListeyiTemizler()
    {
        var sut = CreateSut();
        await sut.SaveWorkingListAsync([Condition("Telefon", new DateOnly(2026, 10, 1), isEnabled: true)]);

        await sut.SaveWorkingListAsync([]);

        Assert.Empty(await sut.GetWorkingListAsync());
    }

    [Fact]
    public async Task SaveWorkingListAsync_TekrarTekrarYazilinca_TaslakTablosundaTekKayitKalir()
    {
        var sut = CreateSut();
        await sut.SaveWorkingListAsync([Condition("Telefon", new DateOnly(2026, 10, 1), isEnabled: true)]);

        await sut.SaveWorkingListAsync([Condition("Tatil", new DateOnly(2026, 12, 20), isEnabled: true)]);

        var drafts = await _draftRepository.GetDraftsAsync();
        Assert.Single(drafts);
        Assert.Equal("Tatil", Assert.Single(drafts[0].Conditions).Request.Name);
    }

    private static SimulationDraftCondition Condition(string name, DateOnly date, bool isEnabled) =>
        new(new SimulationRequest(SimulationScenarioType.CashPurchase, name, 30_000m, date), isEnabled);

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
                new RecurringIncome { Id = incomeId, Name = "Gelir", PaymentDay = 15 }
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
        public DateTimeOffset Now => UtcNow;
    }
}
