using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PeriodActualBuilderTests
{
    private static readonly DateOnly PeriodStart = new(2026, 9, 1);
    private static readonly DateOnly PeriodEnd = new(2026, 10, 1);
    private static readonly DateTimeOffset FinalizedAtUtc = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid SnapshotId = Guid.NewGuid();
    private static readonly Guid PlanId = Guid.NewGuid();
    private static readonly Guid ResultSnapshotId = Guid.NewGuid();

    private static FinancialSnapshot CreateSnapshot(decimal openingBalance = 50_000m) => new()
    {
        Id = SnapshotId,
        SnapshotDate = PeriodStart,
        ProjectionAnchorDate = PeriodStart,
        ProjectionOpeningBalance = openingBalance,
        Anchor = new PeriodAnchor(1),
        Source = FinancialSnapshotSource.Initial,
        IsCurrent = true,
        CreatedAtUtc = FinalizedAtUtc
    };

    private static PeriodPlanSnapshot CreatePlan(
        decimal plannedIncome = 80_000m,
        decimal plannedLiving = 25_000m,
        decimal plannedInterest = 1_000m,
        IReadOnlyList<PeriodPlanPaymentLine>? paymentLines = null) => new()
    {
        Id = PlanId,
        FinancialSnapshotId = SnapshotId,
        PeriodStart = PeriodStart,
        PeriodEnd = PeriodEnd,
        SettlementAvailableFrom = PeriodEnd,
        OpeningBalance = 50_000m,
        PlannedIncome = plannedIncome,
        PlannedVariableExpenseAllowance = plannedLiving,
        PlannedDeficitInterest = plannedInterest,
        CreatedAtUtc = FinalizedAtUtc,
        PaymentLines = paymentLines ?? []
    };

    [Fact]
    public void Build_NegativeLivingSpendOrInterest_ThrowsInvalidOperationException()
    {
        var plan = CreatePlan();
        var snapshot = CreateSnapshot();

        var negativeLivingDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualLivingSpend = -100m
        };

        var negativeInterestDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualInterest = -50m
        };

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, negativeLivingDraft, FinalizedAtUtc, ResultSnapshotId, false));

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, negativeInterestDraft, FinalizedAtUtc, ResultSnapshotId, false));
    }

    [Fact]
    public void Build_LivingBreakdownExceedsLivingSpend_ThrowsInvalidOperationException()
    {
        var plan = CreatePlan();
        var snapshot = CreateSnapshot();

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualLivingSpend = 20_000m,
            LivingBreakdown =
            [
                new LivingBreakdownDraft { Category = "Market", Amount = 15_000m },
                new LivingBreakdownDraft { Category = "Yakıt", Amount = 8_000m } // Toplam 23.000 > 20.000
            ]
        };

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, draft, FinalizedAtUtc, ResultSnapshotId, false));
    }

    [Fact]
    public void Build_NegativeBreakdownAmount_ThrowsInvalidOperationException()
    {
        var plan = CreatePlan();
        var snapshot = CreateSnapshot();

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualLivingSpend = 20_000m,
            LivingBreakdown =
            [
                new LivingBreakdownDraft { Category = "Market", Amount = -1_000m }
            ]
        };

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, draft, FinalizedAtUtc, ResultSnapshotId, false));
    }

    [Fact]
    public void Build_InvalidFlowNameOrAmount_ThrowsInvalidOperationException()
    {
        var plan = CreatePlan();
        var snapshot = CreateSnapshot();

        var emptyNameDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Flows =
            [
                new ActualFlowDraft
                {
                    Type = ActualFlowType.UnplannedIncome,
                    Name = "   ",
                    Category = "Prim",
                    Date = new DateOnly(2026, 9, 15),
                    Amount = 5_000m
                }
            ]
        };

        var nonPositiveAmountDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Flows =
            [
                new ActualFlowDraft
                {
                    Type = ActualFlowType.UnplannedPayment,
                    Name = "Trafik Cezası",
                    Category = "Ceza",
                    Date = new DateOnly(2026, 9, 15),
                    Amount = 0m
                }
            ]
        };

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, emptyNameDraft, FinalizedAtUtc, ResultSnapshotId, false));

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, nonPositiveAmountDraft, FinalizedAtUtc, ResultSnapshotId, false));
    }

    [Fact]
    public void Build_WhenValidateDatesTrue_FlowDateOutsidePeriod_ThrowsInvalidOperationException()
    {
        var plan = CreatePlan();
        var snapshot = CreateSnapshot();

        // Dönem [2026-09-01, 2026-10-01) yarı açık aralığıdır (S18/S37).
        var beforePeriodDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Flows =
            [
                new ActualFlowDraft
                {
                    Type = ActualFlowType.UnplannedIncome,
                    Name = "Eski Gelir",
                    Category = "Gelir",
                    Date = new DateOnly(2026, 8, 31),
                    Amount = 1_000m
                }
            ]
        };

        var onOrAfterEndDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Flows =
            [
                new ActualFlowDraft
                {
                    Type = ActualFlowType.UnplannedPayment,
                    Name = "Gelecek Harcama",
                    Category = "Gider",
                    Date = new DateOnly(2026, 10, 1), // PeriodEnd hariçtir
                    Amount = 1_000m
                }
            ]
        };

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, beforePeriodDraft, FinalizedAtUtc, ResultSnapshotId, true));

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, onOrAfterEndDraft, FinalizedAtUtc, ResultSnapshotId, true));
    }

    [Fact]
    public void Build_WhenValidateDatesTrue_PaymentDateOutsidePeriod_ThrowsInvalidOperationException()
    {
        var lineId = Guid.NewGuid();
        var line = new PeriodPlanPaymentLine
        {
            Id = lineId,
            PeriodPlanSnapshotId = PlanId,
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "İhtiyaç Kredisi",
            PlannedDate = new DateOnly(2026, 9, 15),
            PlannedAmount = 10_000m
        };
        var plan = CreatePlan(paymentLines: [line]);
        var snapshot = CreateSnapshot();

        var invalidDateDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Payments =
            [
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = lineId,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 10_000m,
                    ActualPaymentDate = new DateOnly(2026, 10, 5) // PeriodEnd dışı
                }
            ]
        };

        Assert.Throws<InvalidOperationException>(() =>
            PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, invalidDateDraft, FinalizedAtUtc, ResultSnapshotId, true));
    }

    [Fact]
    public void Build_WhenValidateDatesFalse_DatesOutsidePeriod_DoesNotThrow()
    {
        var lineId = Guid.NewGuid();
        var line = new PeriodPlanPaymentLine
        {
            Id = lineId,
            PeriodPlanSnapshotId = PlanId,
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi",
            PlannedDate = new DateOnly(2026, 9, 15),
            PlannedAmount = 5_000m
        };
        var plan = CreatePlan(paymentLines: [line]);
        var snapshot = CreateSnapshot();

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Payments =
            [
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = lineId,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 5_000m,
                    ActualPaymentDate = new DateOnly(2026, 10, 5)
                }
            ],
            Flows =
            [
                new ActualFlowDraft
                {
                    Type = ActualFlowType.UnplannedIncome,
                    Name = "Gelir",
                    Category = "Ek",
                    Date = new DateOnly(2026, 8, 20),
                    Amount = 1_000m
                }
            ]
        };

        var actual = PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, draft, FinalizedAtUtc, ResultSnapshotId, false);
        Assert.NotNull(actual);
    }

    [Fact]
    public void Build_CalculatesDerivedEndingBalanceAndReconciliationAdjustmentCorrectly()
    {
        // Başlangıç: 50.000 TL
        // Planlanan Gelir: 80.000 TL
        // Plansız Gelir: 5.000 TL
        // Toplam Gelir: 85.000 TL
        // Ödemeler: Kredi 10.000 TL (ödendi), Kart 15.000 TL (farklı tutarla 12.000 ödendi), Harcama 5.000 TL (ödenmedi) -> Toplam ödenen: 22.000 TL
        // Fiilî Yaşam Harcaması: 20.000 TL
        // Fiilî Faiz: 2.000 TL
        // Plansız Gider: 1.000 TL
        // Toplam Çıkış: 22.000 + 20.000 + 2.000 + 1.000 = 45.000 TL
        // Türetilen Dönem Sonu = 50.000 + 85.000 - 45.000 = 90.000 TL
        // Teyit Edilen Bakiye = 92.500 TL
        // Mutabakat Düzeltmesi = 92.500 - 90.000 = +2.500 TL

        var line1 = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = PlanId,
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Taşıt Kredisi",
            PlannedDate = new DateOnly(2026, 9, 10),
            PlannedAmount = 10_000m
        };
        var line2 = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = PlanId,
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.CreditCard,
            Name = "Kredi Kartı",
            PlannedDate = new DateOnly(2026, 9, 15),
            PlannedAmount = 15_000m
        };
        var line3 = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = PlanId,
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.PlannedLargeExpense,
            Name = "Sigorta",
            PlannedDate = new DateOnly(2026, 9, 20),
            PlannedAmount = 5_000m
        };

        var plan = CreatePlan(paymentLines: [line1, line2, line3]);
        var snapshot = CreateSnapshot(50_000m);

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Payments =
            [
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = line1.Id,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 10_000m,
                    ActualPaymentDate = line1.PlannedDate
                },
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = line2.Id,
                    Status = ActualPaymentStatus.Paid, // Planlanan 15.000'den farklı olduğu için DifferentAmount normalize edilmeli
                    ActualAmount = 12_000m,
                    ActualPaymentDate = line2.PlannedDate
                },
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = line3.Id,
                    Status = ActualPaymentStatus.Unpaid,
                    ActualAmount = 0m
                }
            ],
            ActualLivingSpend = 20_000m,
            ActualInterest = 2_000m,
            Flows =
            [
                new ActualFlowDraft
                {
                    Type = ActualFlowType.UnplannedIncome,
                    Name = "Prim",
                    Category = "Gelir",
                    Date = new DateOnly(2026, 9, 12),
                    Amount = 5_000m
                },
                new ActualFlowDraft
                {
                    Type = ActualFlowType.UnplannedPayment,
                    Name = "Tamir",
                    Category = "Gider",
                    Date = new DateOnly(2026, 9, 22),
                    Amount = 1_000m
                }
            ],
            LivingBreakdown =
            [
                new LivingBreakdownDraft { Category = "Market", Amount = 12_000m },
                new LivingBreakdownDraft { Category = "Yakıt", Amount = 8_000m }
            ],
            ConfirmedEndingBalance = 92_500m,
            ActualNote = "Dönem sonu mutabakatı"
        };

        var actual = PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, draft, FinalizedAtUtc, ResultSnapshotId, true);

        Assert.Equal(ResultSnapshotId, actual.ResultFinancialSnapshotId);
        Assert.Equal(snapshot.Id, actual.SourceFinancialSnapshotId);
        Assert.Equal(plan.Id, actual.PeriodPlanSnapshotId);
        Assert.Equal(85_000m, actual.ActualIncome);
        Assert.Equal(10_000m, actual.ActualLoanPayments);
        Assert.Equal(12_000m, actual.ActualCardPayments);
        Assert.Equal(0m, actual.ActualLargeExpenses);
        Assert.Equal(22_000m, actual.ActualMandatoryPayments);
        Assert.Equal(20_000m, actual.ActualLivingSpend);
        Assert.Equal(2_000m, actual.ActualInterest);
        Assert.Equal(5_000m, actual.UnplannedIncome);
        Assert.Equal(1_000m, actual.UnplannedPayments);
        Assert.Equal(90_000m, actual.DerivedEndingBalance);
        Assert.Equal(92_500m, actual.ConfirmedEndingBalance);
        Assert.Equal(2_500m, actual.ReconciliationAdjustment);
        Assert.Equal("Dönem sonu mutabakatı", actual.Note);
        Assert.Equal(FinalizedAtUtc, actual.FinalizedAtUtc);

        var cardPayment = Assert.Single(actual.Payments, x => x.PeriodPlanPaymentLineId == line2.Id);
        Assert.Equal(ActualPaymentStatus.DifferentAmount, cardPayment.Status);
        Assert.Equal(12_000m, cardPayment.ActualAmount);

        var expensePayment = Assert.Single(actual.Payments, x => x.PeriodPlanPaymentLineId == line3.Id);
        Assert.Equal(ActualPaymentStatus.Unpaid, expensePayment.Status);
        Assert.Equal(0m, expensePayment.ActualAmount);
        Assert.Null(expensePayment.ActualPaymentDate);
    }

    [Fact]
    public void Build_WhenConfirmedEndingBalanceNull_DefaultsToDerivedEndingBalance()
    {
        var plan = CreatePlan();
        var snapshot = CreateSnapshot(10_000m);

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualLivingSpend = 5_000m,
            ConfirmedEndingBalance = null // Kullanıcı boş bıraktı
        };

        var actual = PeriodActualBuilder.Build(plan, snapshot, null, plan.PaymentLines, draft, FinalizedAtUtc, ResultSnapshotId, false);

        // 10.000 + 80.000 - 5.000 = 85.000 TL
        Assert.Equal(85_000m, actual.DerivedEndingBalance);
        Assert.Equal(85_000m, actual.ConfirmedEndingBalance);
        Assert.Equal(0m, actual.ReconciliationAdjustment);
    }

    [Fact]
    public void Build_WithRevision_UsesRevisionPlannedIncomeAsBaseline()
    {
        var plan = CreatePlan(plannedIncome: 60_000m);
        var snapshot = CreateSnapshot(20_000m);

        var revision = new PeriodPlanRevision
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = plan.Id,
            RevisionNumber = 1,
            PlannedIncome = 70_000m, // Revizyonla artmış gelir
            PlannedVariableExpenseAllowance = 25_000m,
            PlannedDeficitInterest = 0m,
            CreatedAtUtc = FinalizedAtUtc.AddDays(-5),
            PaymentLines = []
        };

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualLivingSpend = 20_000m
        };

        var actual = PeriodActualBuilder.Build(plan, snapshot, revision, plan.PaymentLines, draft, FinalizedAtUtc, ResultSnapshotId, false);

        Assert.Equal(70_000m, actual.ActualIncome);
        // 20.000 + 70.000 - 20.000 = 70.000 TL
        Assert.Equal(70_000m, actual.DerivedEndingBalance);
    }
}
