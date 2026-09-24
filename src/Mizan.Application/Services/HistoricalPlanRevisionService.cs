using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık nakit akış dönemi devam ederken kullanıcının finansal planlama kararlarında
/// yaptığı değişiklikleri (yeni borç, ödeme tercihi, harcama iptali vb.) orijinal dondurulmuş planı
/// bozmadan artan sıra numaralı plan revizyonları (PeriodPlanRevision) olarak kaydeden uygulama servisidir.
/// Dönem içi gerçekleşen kart harcamalarını ayıklar ve planda fark yaratmayan durumlarda
/// mükerrer kayıt üretilmesini engeller (I24 invariant'ı).
/// </summary>
public sealed class HistoricalPlanRevisionService(
    IPeriodHistoryRepository periodHistoryRepository,
    IClock clock,
    PeriodPlanSnapshotService planSnapshotService)
{
    private readonly IPeriodHistoryRepository _periodHistoryRepository =
        periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
    private readonly IClock _clock =
        clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly PeriodPlanSnapshotService _planSnapshotService =
        planSnapshotService ?? throw new ArgumentNullException(nameof(planSnapshotService));

    /// <summary>
    /// Açık bir nakit akış dönemi için, güncel finansal planlama değişikliklerini inceleyerek
    /// taahhütte bir fark oluşmuşsa yeni bir plan revizyonu yakalar ve kaydeder.
    /// Fark yoksa veya açık dönem bulunmuyorsa null döner.
    /// </summary>
    public async Task<PeriodPlanRevision?> CaptureOpenPlanRevisionAsync(
        FinancialPlan currentPlan,
        string trigger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentPlan);

        if (!currentPlan.CanBuildProjection)
        {
            return null;
        }

        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var currentSnapshot = history.FindLatestCurrentSnapshot();
        var openPlan = history.FindOpenPlan();
        if (currentSnapshot is null || openPlan is null || _clock.Today > openPlan.SettlementAvailableFrom)
        {
            return null;
        }

        var scopedPlan = PrepareScopedPlan(currentPlan, currentSnapshot, openPlan);
        var latestFrozenPlan = _planSnapshotService.Freeze(scopedPlan, currentSnapshot, _clock.UtcNow);
        if (latestFrozenPlan.PeriodStart != openPlan.PeriodStart || latestFrozenPlan.PeriodEnd != openPlan.PeriodEnd)
        {
            return null;
        }

        var revisions = history.FindRevisions(openPlan.Id);
        if (!HasPlanChanged(openPlan, latestFrozenPlan, revisions))
        {
            return null;
        }

        var revision = MapToRevision(openPlan.Id, latestFrozenPlan, revisions.Count + 1, _clock.UtcNow, trigger);
        await _periodHistoryRepository.SavePeriodPlanRevisionAsync(revision, cancellationToken);
        return revision;
    }

    private static bool HasPlanChanged(
        PeriodPlanSnapshot openPlan,
        PeriodPlanSnapshot latestFrozenPlan,
        IReadOnlyList<PeriodPlanRevision> revisions)
    {
        var currentSignature = revisions.Count == 0
            ? PlanRevisionSignature.From(openPlan)
            : PlanRevisionSignature.From(revisions[^1]);
        var candidateSignature = PlanRevisionSignature.From(latestFrozenPlan);

        return !currentSignature.Equals(candidateSignature);
    }

    private static FinancialPlan PrepareScopedPlan(
        FinancialPlan currentPlan,
        FinancialSnapshot currentSnapshot,
        PeriodPlanSnapshot openPlan)
    {
        var period = new CashFlowPeriod(openPlan.PeriodStart, openPlan.PeriodEnd);

        return currentPlan with
        {
            Settings = currentPlan.Settings with
            {
                ProjectionOpeningBalance = currentSnapshot.ProjectionOpeningBalance,
                ProjectionAnchorDate = currentSnapshot.ProjectionAnchorDate,
                PeriodAnchor = currentSnapshot.Anchor
            },
            CreditCards = currentPlan.CreditCards
                .Select(card => card with
                {
                    Charges = card.Charges
                        .Where(c => !period.Contains(c.PostingDate))
                        .ToArray()
                })
                .ToArray()
        };
    }

    private static PeriodPlanRevision MapToRevision(
        Guid periodPlanSnapshotId,
        PeriodPlanSnapshot frozen,
        int revisionNumber,
        DateTimeOffset createdAtUtc,
        string trigger)
    {
        var cleanTrigger = (trigger ?? string.Empty).Trim();

        return new PeriodPlanRevision
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = periodPlanSnapshotId,
            RevisionNumber = revisionNumber,
            CreatedAtUtc = createdAtUtc,
            Trigger = cleanTrigger,
            PlannedIncome = frozen.PlannedIncome,
            PlannedLoanPayments = frozen.PlannedLoanPayments,
            PlannedCardPayments = frozen.PlannedCardPayments,
            PlannedTemporaryPayments = frozen.PlannedTemporaryPayments,
            PlannedInstallmentPayments = frozen.PlannedInstallmentPayments,
            PlannedOtherScheduledPayments = frozen.PlannedOtherScheduledPayments,
            PlannedMandatoryPayments = frozen.PlannedMandatoryPayments,
            PlannedVariableExpenseAllowance = frozen.PlannedVariableExpenseAllowance,
            PlannedLargeExpenses = frozen.PlannedLargeExpenses,
            PlannedCardInterest = frozen.PlannedCardInterest,
            PlannedDeficitInterest = frozen.PlannedDeficitInterest,
            PlannedEndingBalance = frozen.PlannedEndingBalance,
            Note = cleanTrigger,
            PaymentLines = frozen.PaymentLines
                .Select(line => line with
                {
                    Id = Guid.NewGuid(),
                    PeriodPlanSnapshotId = periodPlanSnapshotId
                })
                .ToArray(),
            IncomeLines = frozen.IncomeLines
                .Select(line => line with
                {
                    Id = Guid.NewGuid(),
                    PeriodPlanSnapshotId = periodPlanSnapshotId
                })
                .ToArray()
        };
    }
}
