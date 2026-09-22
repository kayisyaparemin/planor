using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class PeriodSettlementCommitValidatorTests
{
    [Fact]
    public void Validate_GecerliTaahhut_HataDondurmez()
    {
        var commit = CreateValidCommit();

        var errors = PeriodSettlementCommitValidator.Validate(commit);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullCommit_HataDondurur()
    {
        var errors = PeriodSettlementCommitValidator.Validate(null!);

        Assert.Contains(errors, e => e.Contains("boş olamaz"));
    }

    [Fact]
    public void Validate_ZorunluAlanlarEksikse_HatalariToplar()
    {
        var commit = new PeriodSettlementCommit();

        var errors = PeriodSettlementCommitValidator.Validate(commit);

        Assert.Contains(errors, e => e.Contains("Actual"));
        Assert.Contains(errors, e => e.Contains("NewSnapshot"));
        Assert.Contains(errors, e => e.Contains("NewPlan"));
        Assert.Contains(errors, e => e.Contains("UpdatedSettings"));
    }

    [Fact]
    public void Validate_TarihKopuklugu_AciklayiciHataUretir()
    {
        var commit = CreateValidCommit();
        var brokenCommit = commit with
        {
            NewPlan = commit.NewPlan with
            {
                PeriodStart = new DateOnly(2026, 10, 20)
            }
        };

        var errors = PeriodSettlementCommitValidator.Validate(brokenCommit);

        Assert.Contains(errors, e => e.Contains("başlangıç tarihi") && e.Contains("eşleşmelidir"));
    }

    [Fact]
    public void Validate_BakiyeUyusmazligi_AciklayiciHataUretir()
    {
        var commit = CreateValidCommit();
        var brokenCommit = commit with
        {
            NewSnapshot = commit.NewSnapshot with
            {
                ProjectionOpeningBalance = 1000m
            },
            NewPlan = commit.NewPlan with
            {
                OpeningBalance = 2000m
            }
        };

        var errors = PeriodSettlementCommitValidator.Validate(brokenCommit);

        Assert.Contains(errors, e => e.Contains("açılış bakiyesi"));
    }

    [Fact]
    public void Validate_SnapshotReferansUyumsuzlugu_AciklayiciHataUretir()
    {
        var commit = CreateValidCommit();
        var brokenCommit = commit with
        {
            NewPlan = commit.NewPlan with
            {
                FinancialSnapshotId = Guid.NewGuid()
            },
            Actual = commit.Actual with
            {
                ResultFinancialSnapshotId = Guid.NewGuid()
            }
        };

        var errors = PeriodSettlementCommitValidator.Validate(brokenCommit);

        Assert.Contains(errors, e => e.Contains("NewPlan") && e.Contains("bağlı olmalıdır"));
        Assert.Contains(errors, e => e.Contains("ResultFinancialSnapshotId"));
    }

    [Fact]
    public void Validate_GecersizPrepaymentKimligi_HataUretir()
    {
        var commit = CreateValidCommit();
        var brokenCommit = commit with
        {
            RemovedLoanPrepaymentIds = [Guid.NewGuid(), Guid.Empty]
        };

        var errors = PeriodSettlementCommitValidator.Validate(brokenCommit);

        Assert.Contains(errors, e => e.Contains("boş") || e.Contains("geçersiz"));
    }

    [Fact]
    public void Validate_BaskaPlanaAitRevizyon_HataUretir()
    {
        var commit = CreateValidCommit();
        var revision = new PeriodPlanRevision
        {
            PeriodPlanSnapshotId = Guid.NewGuid(), // commit.Actual.PeriodPlanSnapshotId'den farklı!
            RevisionNumber = 1
        };
        var brokenCommit = commit with
        {
            Revision = revision
        };

        var errors = PeriodSettlementCommitValidator.Validate(brokenCommit);

        Assert.Contains(errors, e => e.Contains("revizyonu") && e.Contains("kapanan dönemin"));
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
