using Mizan.Domain.Calculations;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Nakit akış döneminin ara gözlem defteri (bakiye ve borç ödemesi) ile
/// dönem sonu mutabakat süreçlerini orkestre eden kullanım senaryosu servisidir.
/// </summary>
public sealed class PeriodWorkflowService(
    IPeriodHistoryRepository periodHistoryRepository,
    IPeriodObservationRepository periodObservationRepository,
    IPlanReader planReader,
    PeriodSettlementService settlementService,
    IClock clock) : IPeriodWorkflowService
{
    private readonly IPeriodHistoryRepository _periodHistoryRepository =
        periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
    private readonly IPeriodObservationRepository _periodObservationRepository =
        periodObservationRepository ?? throw new ArgumentNullException(nameof(periodObservationRepository));
    private readonly IPlanReader _planReader =
        planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly PeriodSettlementService _settlementService =
        settlementService ?? throw new ArgumentNullException(nameof(settlementService));
    private readonly IClock _clock =
        clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public Task<PeriodSettlementAvailability> GetSettlementAvailabilityAsync(
        CancellationToken cancellationToken = default) =>
        _settlementService.GetAvailabilityAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<PeriodSettlementContext> GetSettlementContextAsync(
        Guid? planId = null,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planReader.GetPlanAsync(cancellationToken);
        return await _settlementService.GetContextAsync(plan, planId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PeriodSettlementDraft?> GetObservedSettlementDraftAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var observation = PeriodObservationRules.Latest(
            await _periodObservationRepository.GetPeriodObservationsAsync(periodPlanSnapshotId, cancellationToken));
        var marks = await _periodObservationRepository.GetPaymentMarksAsync(periodPlanSnapshotId, cancellationToken);
        if (observation is null && marks.Count == 0)
        {
            return null;
        }

        return new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = periodPlanSnapshotId,
            Payments = marks.Select(MapPaymentDraft).ToArray(),
            ActualLivingSpend = 0m,
            ActualInterest = 0m,
            Flows = [],
            LivingBreakdown = [],
            ConfirmedEndingBalance = observation?.ObservedBalance,
            ActualNote = string.Empty
        };
    }

    /// <inheritdoc />
    public async Task<PeriodSettlementPreview> PreviewSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planReader.GetPlanAsync(cancellationToken);
        return await _settlementService.PreviewAsync(draft, plan, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PeriodSettlementResult> FinalizeSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var plan = await _planReader.GetPlanAsync(cancellationToken);
        return await _settlementService.FinalizeAsync(plan, draft, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PeriodObservation> ObserveCurrentBalanceAsync(
        decimal balance,
        DateOnly? observedOn = null,
        CancellationToken cancellationToken = default)
    {
        var (openPlan, _) = await ResolveOpenPlanAsync(cancellationToken);
        var day = observedOn ?? _clock.Today;
        ObservationDayGuard.EnsureCanObserve(openPlan, day, _clock.Today);

        var observation = new PeriodObservation
        {
            PeriodPlanSnapshotId = openPlan.Id,
            ObservedOn = day,
            ObservedBalance = balance,
            RecordedAtUtc = _clock.UtcNow
        };

        await _periodObservationRepository.UpsertPeriodObservationAsync(observation, cancellationToken);
        return observation;
    }

    /// <inheritdoc />
    public async Task<PeriodPaymentMark> ObservePaymentAsync(
        Guid periodPlanPaymentLineId,
        ActualPaymentStatus status,
        decimal actualAmount,
        DateOnly? actualPaymentDate = null,
        string note = "",
        CancellationToken cancellationToken = default)
    {
        var (openPlan, history) = await ResolveOpenPlanAsync(cancellationToken);
        VerifyPaymentLineExists(openPlan, history, periodPlanPaymentLineId);

        // Aynı satırın işareti yeniden konursa kimliği korunur; gözleme dokunulmaz (S68-8).
        var marks = await _periodObservationRepository.GetPaymentMarksAsync(openPlan.Id, cancellationToken);
        var mark = (marks.FirstOrDefault(x => x.PeriodPlanPaymentLineId == periodPlanPaymentLineId) ?? new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = openPlan.Id,
            PeriodPlanPaymentLineId = periodPlanPaymentLineId
        }) with
        {
            Status = status,
            ActualAmount = actualAmount,
            ActualPaymentDate = actualPaymentDate ?? _clock.Today,
            Note = note.Trim()
        };

        await _periodObservationRepository.UpsertPaymentMarkAsync(mark, cancellationToken);
        return mark;
    }

    private static void VerifyPaymentLineExists(
        PeriodPlanSnapshot openPlan,
        FinancialHistoryData history,
        Guid lineId)
    {
        var revisions = history.FindFinalRevisions(openPlan);
        var currentLines = revisions.Count > 0 && revisions[^1].PaymentLines.Count > 0
            ? revisions[^1].PaymentLines
            : openPlan.PaymentLines;

        if (currentLines.All(x => x.Id != lineId) && openPlan.PaymentLines.All(x => x.Id != lineId))
        {
            throw new InvalidOperationException("Gözlenen ödeme satırı açık dönem planında bulunamadı.");
        }
    }

    private static ActualPaymentDraft MapPaymentDraft(PeriodPaymentMark x) => new()
    {
        PeriodPlanPaymentLineId = x.PeriodPlanPaymentLineId,
        Status = x.Status,
        ActualAmount = x.ActualAmount,
        ActualPaymentDate = x.ActualPaymentDate,
        Note = x.Note
    };

    private async Task<(PeriodPlanSnapshot Plan, FinancialHistoryData History)> ResolveOpenPlanAsync(
        CancellationToken cancellationToken)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var plan = history.FindOpenPlan() ??
            throw new InvalidOperationException("Gözlem kaydedebilmek için önce güncel bir dönem planı gerekir.");
        return (plan, history);
    }
}
