using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Simülatörün sonucu (S76-1, 2, 5, 6): şu anki gidişat 12 Dönem'in zinciriyle kuruşu kuruşuna aynı, açık döneme
/// düşen deneme senaryo zincirinin açılışına eklenir, hesaba yalnız açık ve tarihi geçmemiş denemeler girer.
/// </summary>
public sealed class SimulationResultServiceTests
{
    private static readonly DateOnly OpenStart = new(2026, 9, 10);
    private static readonly DateOnly OpenEnd = new(2026, 10, 10);
    private static readonly DateOnly Today = new(2026, 9, 30);
    private const decimal Income = 50_000m;
    private const decimal LivingAllowance = 30_000m;

    private static readonly LoanScheduleCalculator ScheduleCalculator = new();
    private static readonly LoanAmortizationCalculator AmortizationCalculator = new(ScheduleCalculator);
    private static readonly LoanPaymentScheduleBuilder ScheduleBuilder = new(ScheduleCalculator, AmortizationCalculator);

    private readonly FakePeriodProgressService _progressService = new();
    private readonly FakePlanReader _planReader = new();
    private readonly FinancialProjectionCalculator _projectionCalculator = CreateProjectionCalculator();
    private readonly ScenarioPlanBuilder _planBuilder = new(
        new InstallmentScheduleCalculator(), new LoanPrepaymentValidator(AmortizationCalculator, ScheduleBuilder));

    private SimulationResultService CreateSut() => new(
        _progressService,
        _planReader,
        _projectionCalculator,
        new SimulationCalculator(_projectionCalculator, _planBuilder, ScheduleBuilder),
        new TestClock(Today));

    [Fact]
    public void Kurucu_BagimlilikEksikse_ArgumentNullExceptionFirlatir()
    {
        var simulation = new SimulationCalculator(_projectionCalculator, _planBuilder, ScheduleBuilder);
        var clock = new TestClock(Today);

        Assert.Throws<ArgumentNullException>(() => new SimulationResultService(null!, _planReader, _projectionCalculator, simulation, clock));
        Assert.Throws<ArgumentNullException>(() => new SimulationResultService(_progressService, null!, _projectionCalculator, simulation, clock));
        Assert.Throws<ArgumentNullException>(() => new SimulationResultService(_progressService, _planReader, null!, simulation, clock));
        Assert.Throws<ArgumentNullException>(() => new SimulationResultService(_progressService, _planReader, _projectionCalculator, null!, clock));
        Assert.Throws<ArgumentNullException>(() => new SimulationResultService(_progressService, _planReader, _projectionCalculator, simulation, null!));
    }

    [Fact]
    public async Task Hesapla_AcikDonemYoksa_BosDoner()
    {
        _progressService.Current = null;
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync([Condition(Cash(5_000m, Today), true)]);

        Assert.Null(outcome);
    }

    [Fact]
    public async Task Hesapla_PlanKurulamiyorsa_BosDoner()
    {
        _progressService.Current = Progress(projected: null, planned: 0m);
        _planReader.Plan = PlanWithoutIncome();

        var outcome = await CreateSut().CalculateAsync([]);

        Assert.Null(outcome);
    }

    [Fact]
    public async Task Hesapla_DenemeYoksa_YalnizSuAnkiGidisatDonerVe12DonemleKurusuKurusunaAynidir()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync([]);
        var future = await CreateFuture().GetAsync();

        Assert.NotNull(outcome);
        Assert.NotNull(future);
        Assert.Null(outcome.Scenario);
        Assert.Equal(12, outcome.Baseline.Count);
        Assert.Equal(OpenEnd, outcome.Baseline[0].PeriodStart);
        Assert.Equal(41_723m, outcome.Baseline[0].OpeningBalance);
        Assert.Equal(future.Periods.Select(x => x.EndingBalance), outcome.Baseline.Select(x => x.EndingBalance));
    }

    [Fact]
    public async Task Hesapla_TumDenemelerKapaliysa_SenaryoYoktur()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync([Condition(Cash(5_000m, Today), false), Condition(Cash(7_000m, Today), false)]);

        Assert.NotNull(outcome);
        Assert.Null(outcome.Scenario);
    }

    [Fact]
    public async Task Hesapla_TarihiGecenDeneme_HesabaGirmez()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync([Condition(Cash(5_000m, Today.AddDays(-1)), true)]);

        Assert.NotNull(outcome);
        Assert.Null(outcome.Scenario);
    }

    // Bugüne tarihli harcama açık dönemin sonunu düşürür; zincir bu yüzden düşmüş açılıştan başlar, baz değişmez.
    [Fact]
    public async Task Hesapla_BugunTarihliDeneme_SenaryoZincirininAcilisinaEklenir()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync([Condition(Cash(30_000m, Today), true)]);

        Assert.NotNull(outcome);
        Assert.NotNull(outcome.Scenario);
        Assert.Equal(41_723m, outcome.Baseline[0].OpeningBalance);
        Assert.Equal(41_723m - 30_000m, outcome.Scenario[0].OpeningBalance);
        Assert.Equal(outcome.Baseline[0].EndingBalance - 30_000m, outcome.Scenario[0].EndingBalance);
    }

    [Fact]
    public async Task Hesapla_ZincirdekiDeneme_AcilisiDegistirmezZincirdeGorunur()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync([Condition(Cash(5_000m, new DateOnly(2026, 11, 15)), true)]);

        Assert.NotNull(outcome);
        Assert.NotNull(outcome.Scenario);
        Assert.Equal(outcome.Baseline[0].OpeningBalance, outcome.Scenario[0].OpeningBalance);
        Assert.Equal(outcome.Baseline[0].EndingBalance, outcome.Scenario[0].EndingBalance);
        Assert.Equal(outcome.Baseline[1].EndingBalance - 5_000m, outcome.Scenario[1].EndingBalance);
    }

    // Ana sayfa −2.100 diyor (faiz öncesi −2.000, faizi 100). Deneme 10.000 daha çekince faiz −12.000 üzerinden
    // işler (600); etki −10.000 değil −10.500'dür ve plandaki açılış hiçbir rol oynamaz.
    [Fact]
    public async Task Hesapla_AcikDonemZatenEksideyse_KmhFaiziGercekRakamdanIsler()
    {
        _progressService.Current = Progress(projected: -2_100m, planned: 43_900m, projectedInterest: 100m);
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync([Condition(Cash(10_000m, Today), true)]);

        Assert.NotNull(outcome);
        Assert.NotNull(outcome.Scenario);
        Assert.Equal(-2_100m, outcome.Baseline[0].OpeningBalance);
        Assert.Equal(-12_600m, outcome.Scenario[0].OpeningBalance);
    }

    [Fact]
    public async Task Hesapla_KapaliDeneme_HesabaGirmezAcikOlanGirer()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var outcome = await CreateSut().CalculateAsync(
            [Condition(Cash(30_000m, Today), true), Condition(Cash(8_000m, Today), false)]);

        Assert.NotNull(outcome);
        Assert.NotNull(outcome.Scenario);
        Assert.Equal(41_723m - 30_000m, outcome.Scenario[0].OpeningBalance);
    }

    [Fact]
    public async Task Hesapla_KuralBozanDeneme_HataFirlatir()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        await Assert.ThrowsAnyAsync<Exception>(
            () => CreateSut().CalculateAsync([Condition(Cash(0m, Today), true)]));
    }

    private FutureProjectionService CreateFuture() => new(
        _progressService,
        _planReader,
        _projectionCalculator,
        new LoanPayoffAdvisor(_projectionCalculator, _planBuilder, AmortizationCalculator, ScheduleBuilder),
        new TestClock(Today));

    private static SimulationDraftCondition Condition(SimulationRequest request, bool isEnabled) => new(request, isEnabled);

    private static SimulationRequest Cash(decimal amount, DateOnly date) =>
        new(SimulationScenarioType.CashPurchase, "Deneme", amount, date);

    private static FinancialPlan PlanWithIncome()
    {
        var incomeId = Guid.NewGuid();
        return PlanWithoutIncome() with
        {
            RecurringIncomes = [new RecurringIncome { Id = incomeId, Name = "Gelir", PaymentDay = 10, IsActive = true }],
            IncomeHistories =
            [
                new IncomeAmountHistory { RecurringIncomeId = incomeId, Amount = Income, EffectiveDate = new DateOnly(2026, 1, 1) }
            ]
        };
    }

    // Planın kendi çapası ve açılışı açık dönemin başıdır; sonuç servisi ikisini de zincire göre değiştirmeli.
    private static FinancialPlan PlanWithoutIncome() => new()
    {
        Settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(10),
            ProjectionAnchorDate = OpenStart,
            ProjectionOpeningBalance = 0m,
            PeriodVariableExpenseAllowance = LivingAllowance
        }
    };

    private static PeriodProgress Progress(decimal? projected, decimal planned, decimal? projectedInterest = null) => new()
    {
        PeriodPlanSnapshotId = Guid.NewGuid(),
        PeriodStart = OpenStart,
        PeriodEnd = OpenEnd,
        Today = Today,
        ElapsedDays = 20,
        TotalDays = 30,
        RevisionCount = 0,
        PlannedIncome = Income,
        PlannedMandatoryPayments = 0m,
        PlannedEndingBalance = planned,
        PlannedVariableExpenseAllowance = LivingAllowance,
        PlannedDeficitInterest = 0m,
        ObservedLivingSpend = null,
        RemainingVariableExpenseAllowance = null,
        ProjectedDeficitInterest = projectedInterest ?? (projected is null ? null : 0m),
        ProjectedEndingBalance = projected,
        Pace = null,
        Cards = [],
        Observation = null,
        Observations = [],
        Path = new PeriodBalancePath([], []),
        RemainingPayments = [],
        IsClosable = false,
        SnoozedLineIds = new HashSet<Guid>()
    };

    private static FinancialProjectionCalculator CreateProjectionCalculator() => new(
        new CashFlowPeriodCalculator(),
        new IncomeProjectionCalculator(new IncomeResolver()),
        new CreditCardStatementCalculator(),
        new MandatoryPaymentCalculator(ScheduleBuilder, new ScheduledPaymentCalculator()),
        new PeriodObligationGrouper());

    private sealed class FakePeriodProgressService : IPeriodProgressService
    {
        public PeriodProgress? Current { get; set; }

        public Task<PeriodProgress?> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(Current);

        public Task<PeriodProgress> PreviewAsync(decimal balance, DateOnly observedOn, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Simülatör önizleme istemez.");
    }

    private sealed class FakePlanReader : IPlanReader
    {
        public FinancialPlan Plan { get; set; } = new();

        public Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default) => Task.FromResult(Plan);

        public Task<ProjectionQueryPlan> GetProjectionPlanAsync(DateOnly asOf, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProjectionQueryPlan(Plan, null));
    }

    private sealed class TestClock(DateOnly today) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => new(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        public DateTimeOffset Now => UtcNow;
    }
}
