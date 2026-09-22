using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodSettlementCommitTests
{
    [Fact]
    public void VarsayilanDegerler_BosKoleksiyonlarVeNullRevizyonIle_Baslar()
    {
        var commit = new PeriodSettlementCommit();

        Assert.Null(commit.Revision);
        Assert.Empty(commit.UpdatedLoans);
        Assert.Empty(commit.UpdatedPaymentPlans);
        Assert.Empty(commit.UpdatedCreditCards);
        Assert.Empty(commit.UpdatedLargeExpenses);
        Assert.Empty(commit.RemovedLoanPrepaymentIds);
    }

    [Fact]
    public void IsConsistent_GecerliVeTutarlıPaket_TrueDondurur()
    {
        var commit = CreateValidCommit();

        Assert.True(commit.IsConsistent);
    }

    [Fact]
    public void IsConsistent_TarihSürekliligiKopuksa_FalseDondurur()
    {
        var commit = CreateValidCommit();
        var inconsistentCommit = commit with
        {
            NewPlan = commit.NewPlan with
            {
                PeriodStart = new DateOnly(2026, 10, 16) // Actual.PeriodEnd 2026-10-15 idi
            }
        };

        Assert.False(inconsistentCommit.IsConsistent);
    }

    [Fact]
    public void IsConsistent_BakiyeSürekliligiKopuksa_FalseDondurur()
    {
        var commit = CreateValidCommit();
        var inconsistentCommit = commit with
        {
            NewSnapshot = commit.NewSnapshot with
            {
                ProjectionOpeningBalance = 9999m // Actual.ConfirmedEndingBalance 12500m idi
            }
        };

        Assert.False(inconsistentCommit.IsConsistent);
    }

    [Fact]
    public void IsConsistent_YeniPlanAcilisBakiyesiEslesmiyorsa_FalseDondurur()
    {
        var commit = CreateValidCommit();
        var inconsistentCommit = commit with
        {
            NewPlan = commit.NewPlan with
            {
                OpeningBalance = 9999m
            }
        };

        Assert.False(inconsistentCommit.IsConsistent);
    }

    [Fact]
    public void IsConsistent_SnapshotKimlikleriEslesmiyorsa_FalseDondurur()
    {
        var commit = CreateValidCommit();
        var inconsistentCommit = commit with
        {
            NewPlan = commit.NewPlan with
            {
                FinancialSnapshotId = Guid.NewGuid() // NewSnapshot.Id ile eşleşmiyor
            }
        };

        Assert.False(inconsistentCommit.IsConsistent);
    }

    [Fact]
    public void IsConsistent_SonucSnapshotKimligiEslesmiyorsa_FalseDondurur()
    {
        var commit = CreateValidCommit();
        var inconsistentCommit = commit with
        {
            Actual = commit.Actual with
            {
                ResultFinancialSnapshotId = Guid.NewGuid() // NewSnapshot.Id ile eşleşmiyor
            }
        };

        Assert.False(inconsistentCommit.IsConsistent);
    }

    private static PeriodSettlementCommit CreateValidCommit()
    {
        var oldPlanId = Guid.NewGuid();
        var oldSnapshotId = Guid.NewGuid();
        var newSnapshotId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();

        var periodStart = new DateOnly(2026, 9, 15);
        var periodEnd = new DateOnly(2026, 10, 15);
        var nextPeriodEnd = new DateOnly(2026, 11, 15);
        const decimal confirmedEndingBalance = 12500m;

        var actual = new PeriodActual
        {
            PeriodPlanSnapshotId = oldPlanId,
            SourceFinancialSnapshotId = oldSnapshotId,
            ResultFinancialSnapshotId = newSnapshotId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            ConfirmedEndingBalance = confirmedEndingBalance
        };

        var newSnapshot = new FinancialSnapshot
        {
            Id = newSnapshotId,
            SnapshotDate = periodEnd,
            ProjectionOpeningBalance = confirmedEndingBalance
        };

        var newPlan = new PeriodPlanSnapshot
        {
            Id = newPlanId,
            FinancialSnapshotId = newSnapshotId,
            PeriodStart = periodEnd,
            PeriodEnd = nextPeriodEnd,
            OpeningBalance = confirmedEndingBalance
        };

        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            ProjectionOpeningBalance = confirmedEndingBalance
        };

        return new PeriodSettlementCommit
        {
            Actual = actual,
            NewSnapshot = newSnapshot,
            NewPlan = newPlan,
            UpdatedSettings = settings
        };
    }
}
