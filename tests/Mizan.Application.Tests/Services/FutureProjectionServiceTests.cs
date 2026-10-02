using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// 12 dönemlik zincirin ana sayfanın dönem sonuna bağlanması (S74): açılış tahmin ya da plan, ilk dönem açık
/// dönemin bitişi, açık dönem yoksa ya da plan kurulamıyorsa boş.
/// </summary>
public sealed class FutureProjectionServiceTests
{
    private static readonly DateOnly OpenStart = new(2026, 9, 10);
    private static readonly DateOnly OpenEnd = new(2026, 10, 10);
    private const decimal Income = 50_000m;
    private const decimal LivingAllowance = 30_000m;

    private readonly FakePeriodProgressService _progressService = new();
    private readonly FakePlanReader _planReader = new();
    private readonly FinancialProjectionCalculator _calculator = CreateCalculator();
    private TestClock _clock = new(new DateOnly(2026, 9, 30));

    private FutureProjectionService CreateSut() => new(_progressService, _planReader, _calculator, _clock);

    [Fact]
    public void Kurucu_BagimlilikEksikse_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => new FutureProjectionService(null!, _planReader, _calculator, _clock));
        Assert.Throws<ArgumentNullException>(() => new FutureProjectionService(_progressService, null!, _calculator, _clock));
        Assert.Throws<ArgumentNullException>(() => new FutureProjectionService(_progressService, _planReader, null!, _clock));
        Assert.Throws<ArgumentNullException>(() => new FutureProjectionService(_progressService, _planReader, _calculator, null!));
    }

    [Fact]
    public async Task Getir_BakiyeGirildiyse_ZincirAnaSayfaninTahmindenVeAcikDonemdenSonraBaslar()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var result = await CreateSut().GetAsync();

        Assert.NotNull(result);
        Assert.Equal(12, result.Periods.Count);
        Assert.Equal(OpenEnd, result.Periods[0].PeriodStart);
        Assert.Equal(41_723m, result.Periods[0].OpeningBalance);
        Assert.Equal(new DateOnly(2027, 9, 10), result.Periods[^1].PeriodStart);
    }

    [Fact]
    public async Task Getir_BakiyeGirilmediyse_ZincirPlaninDonemSonundanBaslar()
    {
        _progressService.Current = Progress(projected: null, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var result = await CreateSut().GetAsync();

        Assert.NotNull(result);
        Assert.Equal(43_900m, result.Periods[0].OpeningBalance);
    }

    [Fact]
    public async Task Getir_IlkDonem_KendiIlkGunundekiGeliriBirKezSayar()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithIncome();

        var result = await CreateSut().GetAsync();

        Assert.NotNull(result);
        Assert.Equal(Income, result.Periods[0].TotalIncome);
        Assert.Equal(41_723m + Income - LivingAllowance, result.Periods[0].EndingBalance);
    }

    [Fact]
    public async Task Getir_KapanisiErtelenmisDonemde_ZincirBitenDonemSonundanBaslar()
    {
        _clock = new TestClock(new DateOnly(2026, 10, 12));
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m) with { IsClosable = true };
        _planReader.Plan = PlanWithIncome();

        var result = await CreateSut().GetAsync();

        Assert.NotNull(result);
        Assert.Equal(OpenEnd, result.Periods[0].PeriodStart);
        Assert.Equal(41_723m, result.Periods[0].OpeningBalance);
    }

    [Fact]
    public async Task Getir_AcikDonemYoksa_BosDoner()
    {
        _progressService.Current = null;
        _planReader.Plan = PlanWithIncome();

        var result = await CreateSut().GetAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task Getir_GelirYokVeDonemSonuSifirsa_BosDoner()
    {
        _progressService.Current = Progress(projected: null, planned: 0m);
        _planReader.Plan = PlanWithoutIncome();

        var result = await CreateSut().GetAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task Getir_GelirYokAmaDonemSonuVarsa_ZincirBakiyeninErimesiniGosterir()
    {
        _progressService.Current = Progress(projected: 41_723m, planned: 43_900m);
        _planReader.Plan = PlanWithoutIncome();

        var result = await CreateSut().GetAsync();

        Assert.NotNull(result);
        Assert.Equal(41_723m - LivingAllowance, result.Periods[0].EndingBalance);
        Assert.True(result.Periods[^1].EndingBalance < 0m);
    }

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

    // Planın kendi çapası ve açılışı açık dönemin başıdır; zincir ikisini de ana sayfanın dönem sonuyla değiştirmeli.
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

    private static PeriodProgress Progress(decimal? projected, decimal planned) => new()
    {
        PeriodPlanSnapshotId = Guid.NewGuid(),
        PeriodStart = OpenStart,
        PeriodEnd = OpenEnd,
        Today = new DateOnly(2026, 9, 30),
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
        ProjectedDeficitInterest = null,
        ProjectedEndingBalance = projected,
        Pace = null,
        Cards = [],
        Observation = null,
        Observations = [],
        Path = new PeriodBalancePath([], []),
        RemainingLines = [],
        RemainingPlannedTotal = 0m,
        IsClosable = false,
        SnoozedLineIds = new HashSet<Guid>()
    };

    private static FinancialProjectionCalculator CreateCalculator()
    {
        var scheduleCalculator = new LoanScheduleCalculator();
        var amortizationCalculator = new LoanAmortizationCalculator(scheduleCalculator);
        var loanScheduleBuilder = new LoanPaymentScheduleBuilder(scheduleCalculator, amortizationCalculator);
        return new FinancialProjectionCalculator(
            new CashFlowPeriodCalculator(),
            new IncomeProjectionCalculator(new IncomeResolver()),
            new CreditCardStatementCalculator(),
            new MandatoryPaymentCalculator(loanScheduleBuilder, new ScheduledPaymentCalculator()),
            new PeriodObligationGrouper());
    }

    private sealed class FakePeriodProgressService : IPeriodProgressService
    {
        public PeriodProgress? Current { get; set; }

        public Task<PeriodProgress?> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(Current);

        public Task<PeriodProgress> PreviewAsync(decimal balance, DateOnly observedOn, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("12 dönem önizleme istemez.");
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
    }
}
