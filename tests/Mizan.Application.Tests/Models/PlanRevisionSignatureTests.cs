using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Models;

public sealed class PlanRevisionSignatureTests
{
    [Fact]
    public void From_PeriodPlanSnapshot_AlanlariVeSatirlariImzayaAktarir()
    {
        var planId = Guid.NewGuid();
        var snapshot = new PeriodPlanSnapshot
        {
            Id = planId,
            PlannedIncome = 50_000m,
            PlannedLoanPayments = 10_000m,
            PlannedCardPayments = 5_000m,
            PlannedTemporaryPayments = 2_000m,
            PlannedInstallmentPayments = 1_000m,
            PlannedOtherScheduledPayments = 500m,
            PlannedMandatoryPayments = 18_500m,
            PlannedVariableExpenseAllowance = 15_000m,
            PlannedLargeExpenses = 3_000m,
            PlannedCardInterest = 400m,
            PlannedDeficitInterest = 100m,
            PlannedEndingBalance = 13_000m,
            PaymentLines =
            [
                new PeriodPlanPaymentLine
                {
                    PeriodPlanSnapshotId = planId,
                    SourceEntityId = Guid.NewGuid(),
                    SourceType = PlanPaymentSourceType.Loan,
                    Name = "İhtiyaç Kredisi",
                    PlannedDate = new DateOnly(2026, 10, 15),
                    PlannedAmount = 10_000m,
                    IsEstimate = false,
                    Detail = "Aylık taksit"
                }
            ]
        };

        var signature = PlanRevisionSignature.From(snapshot);

        Assert.Equal(50_000m, signature.PlannedIncome);
        Assert.Equal(10_000m, signature.PlannedLoanPayments);
        Assert.Equal(5_000m, signature.PlannedCardPayments);
        Assert.Equal(2_000m, signature.PlannedTemporaryPayments);
        Assert.Equal(1_000m, signature.PlannedInstallmentPayments);
        Assert.Equal(500m, signature.PlannedOtherScheduledPayments);
        Assert.Equal(18_500m, signature.PlannedMandatoryPayments);
        Assert.Equal(15_000m, signature.PlannedVariableExpenseAllowance);
        Assert.Equal(3_000m, signature.PlannedLargeExpenses);
        Assert.Equal(400m, signature.PlannedCardInterest);
        Assert.Equal(100m, signature.PlannedDeficitInterest);
        Assert.Equal(13_000m, signature.PlannedEndingBalance);
        Assert.Single(signature.Lines);
        Assert.Equal("İhtiyaç Kredisi", signature.Lines[0].Name);
    }

    [Fact]
    public void From_PeriodPlanRevision_AlanlariVeSatirlariImzayaAktarir()
    {
        var planId = Guid.NewGuid();
        var revision = new PeriodPlanRevision
        {
            PeriodPlanSnapshotId = planId,
            RevisionNumber = 1,
            PlannedIncome = 60_000m,
            PlannedLoanPayments = 10_000m,
            PlannedCardPayments = 8_000m,
            PlannedEndingBalance = 25_000m,
            PaymentLines =
            [
                new PeriodPlanPaymentLine
                {
                    PeriodPlanSnapshotId = planId,
                    SourceEntityId = Guid.NewGuid(),
                    SourceType = PlanPaymentSourceType.CreditCard,
                    Name = "Bonus",
                    PlannedDate = new DateOnly(2026, 10, 20),
                    PlannedAmount = 8_000m,
                    IsEstimate = false,
                    Detail = "Ekstre ödemesi"
                }
            ]
        };

        var signature = PlanRevisionSignature.From(revision);

        Assert.Equal(60_000m, signature.PlannedIncome);
        Assert.Equal(8_000m, signature.PlannedCardPayments);
        Assert.Equal(25_000m, signature.PlannedEndingBalance);
        Assert.Single(signature.Lines);
        Assert.Equal("Bonus", signature.Lines[0].Name);
    }

    [Fact]
    public void Equals_AyniAlanlaraVeAyniSatirlaraSahipIkiImza_TrueDondurur()
    {
        var entityId = Guid.NewGuid();
        var line1 = new PlanRevisionLineSignature
        {
            SourceEntityId = entityId,
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi",
            PlannedDate = new DateOnly(2026, 10, 15),
            PlannedAmount = 5_000m
        };
        var line2 = new PlanRevisionLineSignature
        {
            SourceEntityId = entityId,
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi",
            PlannedDate = new DateOnly(2026, 10, 15),
            PlannedAmount = 5_000m
        };

        var sig1 = new PlanRevisionSignature
        {
            PlannedIncome = 50_000m,
            PlannedLoanPayments = 5_000m,
            PlannedEndingBalance = 45_000m,
            Lines = [line1]
        };

        var sig2 = new PlanRevisionSignature
        {
            PlannedIncome = 50_000m,
            PlannedLoanPayments = 5_000m,
            PlannedEndingBalance = 45_000m,
            Lines = [line2]
        };

        Assert.True(sig1.Equals(sig2));
        Assert.Equal(sig1.GetHashCode(), sig2.GetHashCode());
    }

    [Fact]
    public void Equals_FinansalTutarFarkliysa_FalseDondurur()
    {
        var sig1 = new PlanRevisionSignature { PlannedIncome = 50_000m };
        var sig2 = new PlanRevisionSignature { PlannedIncome = 55_000m };

        Assert.False(sig1.Equals(sig2));
    }

    [Fact]
    public void Equals_OdemeSatirlariFarkliysa_FalseDondurur()
    {
        var line1 = new PlanRevisionLineSignature
        {
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi A",
            PlannedDate = new DateOnly(2026, 10, 15),
            PlannedAmount = 5_000m
        };
        var line2 = new PlanRevisionLineSignature
        {
            SourceEntityId = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            Name = "Kredi B",
            PlannedDate = new DateOnly(2026, 10, 15),
            PlannedAmount = 5_000m
        };

        var sig1 = new PlanRevisionSignature { Lines = [line1] };
        var sig2 = new PlanRevisionSignature { Lines = [line2] };

        Assert.False(sig1.Equals(sig2));
    }
}
