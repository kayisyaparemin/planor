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
    public Task<PeriodSettlementContext> GetSettlementContextAsync(
        Guid? planId = null,
        CancellationToken cancellationToken = default) =>
        _settlementService.GetContextAsync(planId, cancellationToken);

    /// <inheritdoc />
    public async Task<PeriodSettlementDraft?> GetObservedSettlementDraftAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var observation = await _periodObservationRepository.GetPeriodObservationAsync(
            periodPlanSnapshotId,
            cancellationToken);
        if (observation is null)
        {
            return null;
        }

        return new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = periodPlanSnapshotId,
            Payments = observation.Payments.Select(MapPaymentDraft).ToArray(),
            ActualLivingSpend = observation.ObservedLivingSpend,
            ActualInterest = 0m,
            Flows = [],
            LivingBreakdown = [],
            ConfirmedEndingBalance = observation.ObservedBalance,
            ActualNote = observation.Note
        };
    }

    /// <inheritdoc />
    public Task<PeriodSettlementPreview> PreviewSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default) =>
        _settlementService.PreviewAsync(draft, cancellationToken);

    /// <inheritdoc />
    public async Task<PeriodSettlementResult> FinalizeSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var plan = await _planReader.GetPlanAsync(cancellationToken);
        var result = await _settlementService.FinalizeAsync(plan, draft, cancellationToken);
        await _periodObservationRepository.DeletePeriodObservationAsync(draft.PeriodPlanSnapshotId, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<PeriodObservation> ObserveCurrentBalanceAsync(
        decimal balance,
        CancellationToken cancellationToken = default)
    {
        var openPlan = await ResolveOpenPlanAsync(cancellationToken);
        var existing = await _periodObservationRepository.GetPeriodObservationAsync(openPlan.Id, cancellationToken);
        var now = _clock.UtcNow;
        var observation = (existing ?? new PeriodObservation
        {
            PeriodPlanSnapshotId = openPlan.Id,
            CreatedAtUtc = now
        }) with
        {
            ObservedOn = _clock.Today,
            ObservedBalance = balance,
            UpdatedAtUtc = now
        };

        await _periodObservationRepository.UpsertPeriodObservationAsync(observation, cancellationToken);
        return observation;
    }

    /// <inheritdoc />
    public async Task<PeriodObservation> ObservePaymentAsync(
        Guid periodPlanPaymentLineId,
        ActualPaymentStatus status,
        decimal actualAmount,
        DateOnly? actualPaymentDate = null,
        string note = "",
        CancellationToken cancellationToken = default)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var openPlan = history.FindOpenPlan() ??
            throw new InvalidOperationException("Gözlem kaydedebilmek için önce güncel bir dönem planı gerekir.");

        VerifyPaymentLineExists(openPlan, history, periodPlanPaymentLineId);

        var now = _clock.UtcNow;
        var existing = await _periodObservationRepository.GetPeriodObservationAsync(openPlan.Id, cancellationToken);
        var observation = existing ?? new PeriodObservation
        {
            PeriodPlanSnapshotId = openPlan.Id,
            ObservedOn = _clock.Today,
            CreatedAtUtc = now
        };

        var date = actualPaymentDate ?? _clock.Today;
        observation = UpsertPayment(observation, periodPlanPaymentLineId, status, actualAmount, date, note, now);
        await _periodObservationRepository.UpsertPeriodObservationAsync(observation, cancellationToken);
        return observation;
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

    private static PeriodObservation UpsertPayment(
        PeriodObservation observation,
        Guid lineId,
        ActualPaymentStatus status,
        decimal amount,
        DateOnly date,
        string note,
        DateTimeOffset now)
    {
        var payments = observation.Payments
            .Where(x => x.PeriodPlanPaymentLineId != lineId)
            .Append(new PeriodObservationPayment
            {
                PeriodObservationId = observation.Id,
                PeriodPlanPaymentLineId = lineId,
                Status = status,
                ActualAmount = amount,
                ActualPaymentDate = date,
                Note = note.Trim()
            })
            .ToArray();

        return observation with
        {
            Payments = payments,
            ObservedOn = date,
            UpdatedAtUtc = now
        };
    }

    private static ActualPaymentDraft MapPaymentDraft(PeriodObservationPayment x) => new()
    {
        PeriodPlanPaymentLineId = x.PeriodPlanPaymentLineId,
        Status = x.Status,
        ActualAmount = x.ActualAmount,
        ActualPaymentDate = x.ActualPaymentDate,
        Note = x.Note
    };

    private async Task<PeriodPlanSnapshot> ResolveOpenPlanAsync(CancellationToken cancellationToken)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        return history.FindOpenPlan() ??
            throw new InvalidOperationException("Gözlem kaydedebilmek için önce güncel bir dönem planı gerekir.");
    }
}
