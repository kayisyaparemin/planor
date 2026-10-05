using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PeriodSettlementServiceTests
{
    private static readonly DateOnly InitialDate = new(2026, 9, 1);
    private static readonly DateOnly SettlementDate = new(2026, 10, 1);
    private static readonly DateTimeOffset InitialNowUtc = new(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly MutableClock _clock = new(InitialDate, InitialNowUtc);
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();

    private PeriodSettlementService CreateService()
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

        return new PeriodSettlementService(
            _historyRepo,
            _clock,
            snapshotService,
            instrumentService,
            comparisonCalc);
    }

    private static FinancialPlan CreateSamplePlan(decimal openingBalance = 50_000m)
    {
        var incomeId = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionOpeningBalance = openingBalance,
                ProjectionAnchorDate = InitialDate,
                PeriodVariableExpenseAllowance = 20_000m
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = incomeId,
                    Name = "Gelir",
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

    [Fact]
    public async Task GetAvailabilityAsync_WhenNoCurrentSnapshot_ReturnsNotDueAndNoSnapshot()
    {
        var service = CreateService();

        var availability = await service.GetAvailabilityAsync();

        Assert.False(availability.HasCurrentSnapshot);
        Assert.False(availability.IsDue);
        Assert.Null(availability.CurrentSnapshot);
        Assert.Null(availability.PendingPlan);
        Assert.Null(availability.LastUpdatedDate);
    }

    [Fact]
    public async Task GetAvailabilityAsync_WhenBeforeSettlementDate_ReturnsNotDue()
    {
        var service = CreateService();
        var plan = CreateSamplePlan();

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
            CreatedAtUtc = InitialNowUtc
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        _clock.SetDate(new DateOnly(2026, 9, 30)); // 1 gün önce
        var availability = await service.GetAvailabilityAsync();

        Assert.True(availability.HasCurrentSnapshot);
        Assert.False(availability.IsDue);
        Assert.NotNull(availability.CurrentSnapshot);
        Assert.Null(availability.PendingPlan); // Vadesi gelmediğinde pending null döner
        Assert.Equal(InitialDate, availability.LastUpdatedDate);
    }

    [Fact]
    public async Task GetAvailabilityAsync_WhenOnOrAfterSettlementDate_ReturnsDueWithPendingPlan()
    {
        var service = CreateService();
        var plan = CreateSamplePlan();

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
            CreatedAtUtc = InitialNowUtc
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        _clock.SetDate(SettlementDate); // Tam günü
        var availability = await service.GetAvailabilityAsync();

        Assert.True(availability.HasCurrentSnapshot);
        Assert.True(availability.IsDue);
        Assert.NotNull(availability.PendingPlan);
        Assert.Equal(frozenPlan.Id, availability.PendingPlan!.Id);
    }

    [Fact]
    public async Task GetAvailabilityAsync_WhenPlanAlreadySettled_ReturnsNotDue()
    {
        var service = CreateService();

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
            CreatedAtUtc = InitialNowUtc
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        // Kapanış gerçekleşmesi kaydedilmiş
        var nextSnapshotId = Guid.NewGuid();
        var actual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = frozenPlan.Id,
            SourceFinancialSnapshotId = snapshot.Id,
            ResultFinancialSnapshotId = nextSnapshotId,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            FinalizedAtUtc = InitialNowUtc.AddMonths(1),
            ConfirmedEndingBalance = 65_000m
        };

        await _historyRepo.CommitPeriodSettlementAsync(new PeriodSettlementCommit
        {
            Actual = actual,
            NewSnapshot = snapshot with { Id = nextSnapshotId, SnapshotDate = SettlementDate },
            NewPlan = frozenPlan with { Id = Guid.NewGuid(), PeriodStart = SettlementDate, PeriodEnd = SettlementDate.AddMonths(1) },
            UpdatedSettings = new UserSettings()
        });

        _clock.SetDate(SettlementDate);
        var availability = await service.GetAvailabilityAsync();

        // Kapatılmış plan için IsDue = false döner
        Assert.False(availability.IsDue);
    }

    [Fact]
    public async Task GetContextAsync_WhenNoCurrentSnapshot_ThrowsInvalidOperationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetContextAsync());
    }

    [Fact]
    public async Task GetContextAsync_WhenOpenPlanExists_ReturnsContextWithRevisionsAndSuggestedStartingBalance()
    {
        var service = CreateService();

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

        var line = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = Guid.NewGuid(),
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi",
            PlannedDate = new DateOnly(2026, 9, 15),
            PlannedAmount = 10_000m
        };

        var frozenPlan = new PeriodPlanSnapshot
        {
            Id = line.PeriodPlanSnapshotId,
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            SettlementAvailableFrom = SettlementDate,
            OpeningBalance = 50_000m,
            PlannedIncome = 60_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedDeficitInterest = 0m,
            CreatedAtUtc = InitialNowUtc,
            PaymentLines = [line]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        // Bir plan revizyonu ekleyelim
        var revision = new PeriodPlanRevision
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = frozenPlan.Id,
            RevisionNumber = 1,
            PlannedIncome = 65_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedDeficitInterest = 0m,
            CreatedAtUtc = InitialNowUtc.AddDays(5),
            PaymentLines = [line]
        };
        await _historyRepo.SavePeriodPlanRevisionAsync(revision);

        var context = await service.GetContextAsync(frozenPlan.Id);

        Assert.Equal(snapshot.Id, context.Snapshot.Id);
        Assert.Equal(frozenPlan.Id, context.OriginalPlan.Id);
        Assert.NotNull(context.Revision);
        Assert.Equal(revision.Id, context.Revision!.Id);
        Assert.Equal(1, context.RevisionCount);
        Assert.Null(context.Actual);
        Assert.Null(context.Comparison);
        // Önerilen açılış bakiyesi: 50.000 + 65.000 (revize gelir) - 10.000 (kredi) - 20.000 (yaşam) = 85.000 TL
        Assert.Equal(85_000m, context.SuggestedStartingBalance);
    }

    [Fact]
    public async Task PreviewAsync_CalculatesDerivedAndConfirmedBalancesWithComparison()
    {
        var service = CreateService();

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

        var line = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = Guid.NewGuid(),
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi",
            PlannedDate = new DateOnly(2026, 9, 15),
            PlannedAmount = 10_000m
        };

        var frozenPlan = new PeriodPlanSnapshot
        {
            Id = line.PeriodPlanSnapshotId,
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            SettlementAvailableFrom = SettlementDate,
            OpeningBalance = 50_000m,
            PlannedIncome = 60_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedDeficitInterest = 0m,
            PlannedEndingBalance = 80_000m,
            CreatedAtUtc = InitialNowUtc,
            PaymentLines = [line]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = frozenPlan.Id,
            Payments =
            [
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = line.Id,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 10_000m,
                    ActualPaymentDate = line.PlannedDate
                }
            ],
            ActualLivingSpend = 18_000m,
            ActualInterest = 0m,
            ConfirmedEndingBalance = 83_000m // 50k + 60k - 10k - 18k = 82k derived; teyit 83k (+1.000 fark)
        };

        var preview = await service.PreviewAsync(draft);

        Assert.Equal(82_000m, preview.DerivedEndingBalance);
        Assert.Equal(83_000m, preview.ConfirmedEndingBalance);
        Assert.Equal(1_000m, preview.ReconciliationAdjustment);
        Assert.NotNull(preview.Comparison);
        Assert.Equal(80_000m, preview.Comparison.PlannedEndingBalance);
        Assert.Equal(83_000m, preview.Comparison.ActualEndingBalance);
    }

    [Fact]
    public async Task FinalizeAsync_WhenBeforeSettlementAvailableFrom_ThrowsInvalidOperationException()
    {
        var service = CreateService();
        var plan = CreateSamplePlan();

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
            CreatedAtUtc = InitialNowUtc
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        _clock.SetDate(new DateOnly(2026, 9, 25)); // Henüz hazır değil

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = frozenPlan.Id,
            ActualLivingSpend = 20_000m
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.FinalizeAsync(plan, draft));
    }

    [Fact]
    public async Task FinalizeAsync_WhenPlanAlreadySettled_ThrowsInvalidOperationException()
    {
        var service = CreateService();
        var plan = CreateSamplePlan();

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
            CreatedAtUtc = InitialNowUtc
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        var actual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = frozenPlan.Id,
            SourceFinancialSnapshotId = snapshot.Id,
            ResultFinancialSnapshotId = Guid.NewGuid(),
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            FinalizedAtUtc = InitialNowUtc.AddMonths(1)
        };

        await _historyRepo.CommitPeriodSettlementAsync(new PeriodSettlementCommit
        {
            Actual = actual,
            NewSnapshot = snapshot with { Id = actual.ResultFinancialSnapshotId },
            NewPlan = frozenPlan with { Id = Guid.NewGuid() },
            UpdatedSettings = plan.Settings
        });

        _clock.SetDate(SettlementDate);

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = frozenPlan.Id,
            ActualLivingSpend = 20_000m
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.FinalizeAsync(plan, draft));
    }

    [Fact]
    public async Task FinalizeAsync_SuccessfulSettlement_CommitsConsistentPackageAndReturnsResult()
    {
        var service = CreateService();
        var plan = CreateSamplePlan();

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

        var line = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = Guid.NewGuid(),
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi",
            PlannedDate = new DateOnly(2026, 9, 15),
            PlannedAmount = 10_000m
        };

        var frozenPlan = new PeriodPlanSnapshot
        {
            Id = line.PeriodPlanSnapshotId,
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            SettlementAvailableFrom = SettlementDate,
            OpeningBalance = 50_000m,
            PlannedIncome = 60_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedDeficitInterest = 0m,
            CreatedAtUtc = InitialNowUtc,
            PaymentLines = [line]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        _clock.SetDate(SettlementDate);

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = frozenPlan.Id,
            Payments =
            [
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = line.Id,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 10_000m,
                    ActualPaymentDate = line.PlannedDate
                }
            ],
            ActualLivingSpend = 18_000m,
            ActualInterest = 0m,
            ConfirmedEndingBalance = 82_000m
        };

        var result = await service.FinalizeAsync(plan, draft);

        Assert.NotNull(result);
        Assert.NotNull(result.NewSnapshot);
        Assert.NotNull(result.Actual);
        Assert.NotNull(result.NewPlan);
        Assert.NotNull(result.Comparison);

        // Tarihçe sürekliliği: Eski kapanış = Yeni açılış (I21)
        Assert.Equal(result.Actual.PeriodEnd, result.NewPlan.PeriodStart);
        Assert.Equal(SettlementDate, result.NewPlan.PeriodStart);
        Assert.Equal(82_000m, result.Actual.ConfirmedEndingBalance);
        Assert.Equal(82_000m, result.NewSnapshot.ProjectionOpeningBalance);
        Assert.Equal(82_000m, result.NewPlan.OpeningBalance);
        Assert.Equal(result.NewSnapshot.Id, result.Actual.ResultFinancialSnapshotId);
        Assert.Equal(result.NewSnapshot.Id, result.NewPlan.FinancialSnapshotId);
        Assert.NotEmpty(result.Actual.ComparisonSummary);

        // Veritabanı durumu kontrolü
        var history = await _historyRepo.GetFinancialHistoryAsync();
        var committedActual = Assert.Single(history.Actuals);
        Assert.Equal(result.Actual.Id, committedActual.Id);
        Assert.Equal(2, history.Snapshots.Count);
        Assert.Equal(2, history.Plans.Count);
    }

    [Fact]
    public async Task FinalizeAsync_KartaDonemIcindeHarcamaGirildiyse_KapanisKartiGuncelOdemeIleKapatirVeHaksizDevredenBakiyeUretmez()
    {
        var service = CreateService();
        var cardId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 9, 15);

        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            StatementClosingDay = 27,
            PaymentDueDay = 15,
            Limit = 100_000m,
            CarriedBalance = 0m,
            BalanceAsOfDate = new DateOnly(2026, 8, 28),
            PaymentStrategy = CreditCardPaymentStrategy.FullStatement,
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 27),
                DueDate = dueDate,
                StatementAmount = 30_000m,
                MinimumPaymentAmount = 6_000m
            }
        };

        var plan = CreateSamplePlan() with { CreditCards = [card] };

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

        var line = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = Guid.NewGuid(),
            SourceEntityId = cardId,
            SourceType = PlanPaymentSourceType.CreditCard,
            Name = "Bonus",
            PlannedDate = dueDate,
            PlannedAmount = 20_000m
        };

        var frozenPlan = new PeriodPlanSnapshot
        {
            Id = line.PeriodPlanSnapshotId,
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            SettlementAvailableFrom = SettlementDate,
            OpeningBalance = 50_000m,
            PlannedIncome = 60_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedDeficitInterest = 0m,
            CreatedAtUtc = InitialNowUtc,
            PaymentLines = [line]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);
        _clock.SetDate(SettlementDate);

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = frozenPlan.Id,
            Payments = [],
            ActualLivingSpend = 20_000m,
            ActualInterest = 0m,
            ConfirmedEndingBalance = 60_000m
        };

        var result = await service.FinalizeAsync(plan, draft);

        Assert.Equal(30_000m, result.Actual.ActualCardPayments);

        var commit = Assert.Single(_historyRepo.Commits);
        var updatedCard = Assert.Single(commit.UpdatedCreditCards.Where(c => c.Id == cardId));
        Assert.Equal(0m, updatedCard.CarriedBalance);
    }

    [Fact]
    public async Task GetContextAsync_KartaDonemIcindeHarcamaGirildiyse_OnerilenBakiyeGuncelOdemeIleHesaplanir()
    {
        var service = CreateService();
        var cardId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 9, 15);

        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            StatementClosingDay = 27,
            PaymentDueDay = 15,
            Limit = 100_000m,
            CarriedBalance = 0m,
            BalanceAsOfDate = new DateOnly(2026, 8, 28),
            PaymentStrategy = CreditCardPaymentStrategy.FullStatement,
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 27),
                DueDate = dueDate,
                StatementAmount = 30_000m,
                MinimumPaymentAmount = 6_000m
            }
        };

        var plan = CreateSamplePlan() with { CreditCards = [card] };

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

        var line = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = Guid.NewGuid(),
            SourceEntityId = cardId,
            SourceType = PlanPaymentSourceType.CreditCard,
            Name = "Bonus",
            PlannedDate = dueDate,
            PlannedAmount = 20_000m
        };

        var frozenPlan = new PeriodPlanSnapshot
        {
            Id = line.PeriodPlanSnapshotId,
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            SettlementAvailableFrom = SettlementDate,
            OpeningBalance = 50_000m,
            PlannedIncome = 60_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            PlannedDeficitInterest = 0m,
            CreatedAtUtc = InitialNowUtc,
            PaymentLines = [line]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);

        // 50.000 + 60.000 - 30.000 (güncel ekstre borcu) - 20.000 = 60.000 TL
        var context = await service.GetContextAsync(plan, frozenPlan.Id);
        Assert.Equal(60_000m, context.SuggestedStartingBalance);
    }

    private sealed class MutableClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today { get; private set; } = today;
        public DateTimeOffset UtcNow { get; private set; } = utcNow;
        public DateTimeOffset Now => UtcNow;

        public void SetDate(DateOnly today)
        {
            Today = today;
            UtcNow = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }
    }
}
