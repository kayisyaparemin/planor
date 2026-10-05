using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PlanReaderTests
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

    private PlanReader CreateSut()
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

        return new PlanReader(
            _userSettingsRepo,
            incomeReader,
            instrumentReader,
            _historyRepo,
            boundaryResolver);
    }

    [Fact]
    public async Task GetPlanAsync_TumVeriDepolariniBirlestirerekPlaniDoner()
    {
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            ProjectionOpeningBalance = 20_000m,
            ProjectionAnchorDate = new DateOnly(2026, 10, 15)
        };
        await _userSettingsRepo.SaveSettingsAsync(settings);

        var recurringIncome = new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", IsActive = true };
        await _recurringIncomeRepo.UpsertRecurringIncomeAsync(recurringIncome);

        var loan = new Loan { Id = Guid.NewGuid(), Name = "Kredi", MonthlyPayment = 3_000m };
        await _loanRepo.UpsertLoanAsync(loan);

        var sut = CreateSut();

        var plan = await sut.GetPlanAsync();

        Assert.NotNull(plan);
        Assert.Equal(15, plan.Settings.PeriodAnchor.DayOfMonth);
        Assert.Single(plan.RecurringIncomes);
        Assert.Equal("Maaş", plan.RecurringIncomes[0].Name);
        Assert.Single(plan.Loans);
        Assert.Equal("Kredi", plan.Loans[0].Name);
    }

    [Fact]
    public async Task GetPlanAsync_AslaTarihceYazmaz_SifirYanEtki()
    {
        var settings = new UserSettings { PeriodAnchor = new PeriodAnchor(1) };
        await _userSettingsRepo.SaveSettingsAsync(settings);

        var sut = CreateSut();

        await sut.GetPlanAsync();

        var history = await _historyRepo.GetFinancialHistoryAsync();
        Assert.Empty(history.Snapshots);
        Assert.Empty(history.Plans);
        Assert.Empty(history.Revisions);
    }

    [Fact]
    public async Task GetProjectionPlanAsync_ProjeksiyonUretilemezIse_BoundaryNullDoner()
    {
        // ProjectionAnchorDate = default ve hiçbir gelir/bakiye yok -> CanBuildProjection = false
        var settings = new UserSettings { ProjectionAnchorDate = default };
        await _userSettingsRepo.SaveSettingsAsync(settings);

        var sut = CreateSut();

        var queryPlan = await sut.GetProjectionPlanAsync(new DateOnly(2026, 10, 1));

        Assert.NotNull(queryPlan.Plan);
        Assert.False(queryPlan.Plan.CanBuildProjection);
        Assert.Null(queryPlan.Boundary);
    }

    [Fact]
    public async Task GetProjectionPlanAsync_GecerliSnapshotYoksa_BoundaryNullDoner()
    {
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            ProjectionOpeningBalance = 10_000m,
            ProjectionAnchorDate = new DateOnly(2026, 10, 15)
        };
        await _userSettingsRepo.SaveSettingsAsync(settings);

        var sut = CreateSut();

        var queryPlan = await sut.GetProjectionPlanAsync(new DateOnly(2026, 10, 1));

        Assert.NotNull(queryPlan.Plan);
        Assert.True(queryPlan.Plan.CanBuildProjection);
        Assert.Null(queryPlan.Boundary);
    }

    [Fact]
    public async Task GetProjectionPlanAsync_AcikDonemKartHarcamalariniFiltreler_I16()
    {
        var anchor = new PeriodAnchor(15);
        var settings = new UserSettings
        {
            PeriodAnchor = anchor,
            ProjectionOpeningBalance = 30_000m,
            ProjectionAnchorDate = new DateOnly(2026, 10, 15)
        };
        await _userSettingsRepo.SaveSettingsAsync(settings);
        await _recurringIncomeRepo.UpsertRecurringIncomeAsync(new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", IsActive = true });

        // Açık dönem: [15 Ekim 2026, 15 Kasım 2026)
        var snapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            IsCurrent = true,
            SnapshotDate = new DateOnly(2026, 10, 15),
            ProjectionOpeningBalance = 30_000m
        };
        var openPlan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = new DateOnly(2026, 10, 15),
            PeriodEnd = new DateOnly(2026, 11, 15)
        };
        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, openPlan);

        // Kredi kartı harcamaları: biri açık dönem içinde (20 Ekim), biri açık dönemden önce (10 Ekim), biri sonra (20 Kasım)
        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            Name = "World",
            Charges =
            [
                new CardCharge { Id = Guid.NewGuid(), PostingDate = new DateOnly(2026, 10, 10), Amount = 1_000m }, // Öncesi -> kalmalı
                new CardCharge { Id = Guid.NewGuid(), PostingDate = new DateOnly(2026, 10, 20), Amount = 2_500m }, // Dönem içi -> filtrelenmeli! (I16)
                new CardCharge { Id = Guid.NewGuid(), PostingDate = new DateOnly(2026, 11, 20), Amount = 3_000m }  // Sonrası -> kalmalı
            ]
        };
        await _creditCardRepo.UpsertCreditCardAsync(card);

        var sut = CreateSut();

        var queryPlan = await sut.GetProjectionPlanAsync(new DateOnly(2026, 10, 15));

        Assert.NotNull(queryPlan.Boundary);
        Assert.Single(queryPlan.Plan.CreditCards);
        var remainingCharges = queryPlan.Plan.CreditCards[0].Charges;
        Assert.Equal(2, remainingCharges.Count);
        Assert.Contains(remainingCharges, c => c.PostingDate == new DateOnly(2026, 10, 10));
        Assert.Contains(remainingCharges, c => c.PostingDate == new DateOnly(2026, 11, 20));
        Assert.DoesNotContain(remainingCharges, c => c.PostingDate == new DateOnly(2026, 10, 20));
    }

    [Fact]
    public async Task GetProjectionPlanAsync_DonemKapandiktanSonra_ProjeksiyonAcikDonemdenBaslar()
    {
        var anchor = new PeriodAnchor(15);
        await _userSettingsRepo.SaveSettingsAsync(new UserSettings
        {
            PeriodAnchor = anchor,
            ProjectionOpeningBalance = 42_500m,
            ProjectionAnchorDate = new DateOnly(2026, 2, 15)
        });
        await _recurringIncomeRepo.UpsertRecurringIncomeAsync(new RecurringIncome { Id = Guid.NewGuid(), Name = "Gelir", IsActive = true });

        // Kapanış [15 Ocak, 15 Şubat) dönemini kapatır, açık dönemi [15 Şubat, 15 Mart) olarak dondurur
        var newSnapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = new DateOnly(2026, 2, 15),
            ProjectionAnchorDate = new DateOnly(2026, 2, 15),
            ProjectionOpeningBalance = 42_500m,
            Anchor = anchor
        };
        var openPlan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = newSnapshot.Id,
            PeriodStart = new DateOnly(2026, 2, 15),
            PeriodEnd = new DateOnly(2026, 3, 15)
        };
        var actualId = Guid.NewGuid();
        await _historyRepo.CommitPeriodSettlementAsync(new PeriodSettlementCommit
        {
            Actual = new PeriodActual
            {
                Id = actualId,
                ResultFinancialSnapshotId = newSnapshot.Id,
                PeriodStart = new DateOnly(2026, 1, 15),
                PeriodEnd = new DateOnly(2026, 2, 15)
            },
            NewSnapshot = newSnapshot,
            NewPlan = openPlan
        });
        var sut = CreateSut();

        var queryPlan = await sut.GetProjectionPlanAsync(new DateOnly(2026, 2, 20));

        // Sınır kurulum yolundan değil kapanıştan çözülür; zincir kapanış bakiyesiyle, o bakiyenin ait olduğu
        // günden başlar ve açık dönem atlanmaz (S84)
        Assert.NotNull(queryPlan.Boundary);
        Assert.Equal(actualId, queryPlan.Boundary.SourcePeriodActualId);
        Assert.Equal(new DateOnly(2026, 2, 15), queryPlan.Boundary.ClosedCheckpointDate);
        Assert.Equal(openPlan.PeriodStart, queryPlan.Boundary.FirstUnrealizedPeriodStartDate);
        Assert.Equal(openPlan.PeriodStart, queryPlan.Boundary.ProjectionAnchorDate);
        Assert.Equal(42_500m, queryPlan.Boundary.StartingBalance);
    }
}
