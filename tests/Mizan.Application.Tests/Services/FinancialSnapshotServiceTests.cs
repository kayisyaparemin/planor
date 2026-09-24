using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Tests.Fakes;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class FinancialSnapshotServiceTests
{
    private readonly InMemoryPeriodHistoryRepository _repository = new();
    private readonly FixedClock _clock = new(new DateOnly(2026, 10, 1), new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero));
    private readonly CashFlowPeriodCalculator _periodCalculator = new();
    private readonly FinancialSnapshotService _service;

    public FinancialSnapshotServiceTests()
    {
        var planSnapshotService = new PeriodPlanSnapshotService(CreateCalculator(), _periodCalculator);
        _service = new FinancialSnapshotService(_repository, _clock, planSnapshotService, _periodCalculator);
    }

    [Fact]
    public async Task EnsureInitialSnapshotAsync_PlanHenuzHazirDegilse_NullDondurur()
    {
        var unreadyPlan = new FinancialPlan();

        var result = await _service.EnsureInitialSnapshotAsync(unreadyPlan);

        Assert.Null(result);
    }

    [Fact]
    public async Task EnsureInitialSnapshotAsync_HicSnapshotYoksa_IlkSnapshotVePlaniDondurupKaydeder()
    {
        var plan = CreateBasicPlan(50_000m);

        var result = await _service.EnsureInitialSnapshotAsync(plan);

        Assert.NotNull(result);
        Assert.Equal(FinancialSnapshotSource.Initial, result.Source);
        Assert.True(result.IsCurrent);
        Assert.Equal(new DateOnly(2026, 10, 1), result.SnapshotDate);
        Assert.Equal(new DateOnly(2026, 11, 1), result.NextSettlementDate);

        var history = await _repository.GetFinancialHistoryAsync();
        Assert.Single(history.Snapshots);
        Assert.Single(history.Plans);
    }

    [Fact]
    public async Task EnsureInitialSnapshotAsync_MevcutGecerliSnapshotVarsa_MevcuduDondurur()
    {
        var plan = CreateBasicPlan(50_000m);
        var existing = await _service.EnsureInitialSnapshotAsync(plan);

        var secondCall = await _service.EnsureInitialSnapshotAsync(plan);

        Assert.NotNull(secondCall);
        Assert.Equal(existing!.Id, secondCall.Id);
    }

    [Fact]
    public async Task EnsureInitialSnapshotAsync_TakvimDongusuBozuksa_BekleyenPlaniOnarirVeKaydeder()
    {
        var plan = CreateBasicPlan(50_000m);
        await _service.EnsureInitialSnapshotAsync(plan);

        // Tarihçedeki snapshot'ın mutabakat tarihini kasıtlı olarak bozuyoruz
        var history = await _repository.GetFinancialHistoryAsync();
        var snapshot = history.Snapshots[0];
        var brokenSnapshot = snapshot with { NextSettlementDate = new DateOnly(2026, 12, 1) };
        await _repository.SaveCurrentFinancialSnapshotAsync(brokenSnapshot, history.Plans[0]);

        var result = await _service.EnsureInitialSnapshotAsync(plan);

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2026, 11, 1), result.NextSettlementDate);
    }

    [Fact]
    public async Task CreateCurrentSnapshotAsync_YeniDurumVePlaniOlusturupKaydeder()
    {
        var plan = CreateBasicPlan(50_000m);
        await _service.EnsureInitialSnapshotAsync(plan);

        var bundle = await _service.CreateCurrentSnapshotAsync(
            plan,
            60_000m,
            new DateOnly(2026, 11, 1),
            FinancialSnapshotSource.MonthlyUpdate,
            "Dönem Kapanışı");

        Assert.NotNull(bundle.Snapshot);
        Assert.Equal(60_000m, bundle.Snapshot.ProjectionOpeningBalance);
        Assert.Equal(new DateOnly(2026, 11, 1), bundle.Snapshot.SnapshotDate);
        Assert.Equal(new DateOnly(2026, 12, 1), bundle.Snapshot.NextSettlementDate);
        Assert.Equal("Dönem Kapanışı", bundle.Snapshot.Note);

        var history = await _repository.GetFinancialHistoryAsync();
        Assert.Equal(2, history.Snapshots.Count);
    }

    [Fact]
    public void Build_PlanHazirDegilse_InvalidOperationExceptionFirlatir()
    {
        var unreadyPlan = new FinancialPlan();

        Assert.Throws<InvalidOperationException>(() => _service.Build(
            unreadyPlan,
            10_000m,
            new DateOnly(2026, 10, 1),
            FinancialSnapshotSource.MonthlyUpdate,
            "Not",
            null));
    }

    private static FinancialPlan CreateBasicPlan(decimal income)
    {
        var id = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = new DateOnly(2026, 10, 1),
                ProjectionOpeningBalance = 25_000m,
                PeriodVariableExpenseAllowance = 15_000m
            },
            RecurringIncomes = [new RecurringIncome { Id = id, Name = "Maaş", PaymentDay = 1, IsActive = true }],
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

    private sealed class FixedClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today { get; } = today;
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
