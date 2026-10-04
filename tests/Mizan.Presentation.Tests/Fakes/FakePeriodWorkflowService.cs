using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde dönem gözlem defteri ve mutabakat orkestrasyonunu taklit eden test çiftidir.
/// </summary>
public sealed class FakePeriodWorkflowService : IPeriodWorkflowService
{
    /// <summary>Kaydedilen en son bakiye gözlemi tutarı.</summary>
    public decimal? LastObservedBalance { get; private set; }

    /// <summary>GetSettlementAvailabilityAsync çağrıldığında dönecek uygunluk nesnesi.</summary>
    public PeriodSettlementAvailability Availability { get; set; } = new()
    {
        HasCurrentSnapshot = true,
        IsDue = false,
        CurrentSnapshot = null,
        PendingPlan = null
    };

    /// <summary>Dönem mutabakat uygunluğunu döndürür.</summary>
    public Task<PeriodSettlementAvailability> GetSettlementAvailabilityAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Availability);

    /// <summary>Bağlam nesnesi.</summary>
    public PeriodSettlementContext? Context { get; set; }

    /// <summary>Gözlemli taslak nesnesi.</summary>
    public PeriodSettlementDraft? ObservedDraft { get; set; }

    /// <summary>Önizleme sonucu.</summary>
    public PeriodSettlementPreview? Preview { get; set; }

    /// <summary>Kesinleştirme sonucu.</summary>
    public PeriodSettlementResult? FinalizeResult { get; set; }

    /// <summary>Son kesinleştirilen taslak.</summary>
    public PeriodSettlementDraft? FinalizedDraft { get; private set; }

    /// <summary>Bağlam nesnesini döndürür.</summary>
    public Task<PeriodSettlementContext> GetSettlementContextAsync(
        Guid? planId = null,
        CancellationToken cancellationToken = default) =>
        Context is not null ? Task.FromResult(Context) : throw new InvalidOperationException("Bağlam bulunamadı.");

    /// <summary>Gözlemli taslak nesnesini döndürür.</summary>
    public Task<PeriodSettlementDraft?> GetObservedSettlementDraftAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(ObservedDraft);

    /// <summary>Önizlemeyi döndürür.</summary>
    public Task<PeriodSettlementPreview> PreviewSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default) =>
        Preview is not null ? Task.FromResult(Preview) : throw new InvalidOperationException("Önizleme bulunamadı.");

    /// <summary>Mutabakatı kesinleştirir.</summary>
    public Task<PeriodSettlementResult> FinalizeSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default)
    {
        FinalizedDraft = draft;
        if (FinalizeResult is not null)
        {
            return Task.FromResult(FinalizeResult);
        }

        var dummySnapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = new DateOnly(2026, 10, 10),
            ProjectionAnchorDate = new DateOnly(2026, 10, 10),
            ProjectionOpeningBalance = 42180m,
            Anchor = new PeriodAnchor(10),
            Source = FinancialSnapshotSource.MonthlyUpdate,
            IsCurrent = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var dummyPlan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = dummySnapshot.Id,
            PeriodStart = new DateOnly(2026, 10, 10),
            PeriodEnd = new DateOnly(2026, 11, 10),
            SettlementAvailableFrom = new DateOnly(2026, 11, 10),
            OpeningBalance = 42180m,
            PlannedIncome = 70000m,
            PlannedVariableExpenseAllowance = 29650m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var dummyActual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = draft.PeriodPlanSnapshotId,
            SourceFinancialSnapshotId = dummySnapshot.Id,
            ResultFinancialSnapshotId = dummySnapshot.Id,
            PeriodStart = new DateOnly(2026, 9, 10),
            PeriodEnd = new DateOnly(2026, 10, 10),
            FinalizedAtUtc = DateTimeOffset.UtcNow,
            ConfirmedEndingBalance = draft.ConfirmedEndingBalance ?? 42180m
        };
        var dummyComparison = new PlanActualComparison(43900m, 42180m, -1720m, "Özet", []);

        return Task.FromResult(new PeriodSettlementResult
        {
            NewSnapshot = dummySnapshot,
            NewPlan = dummyPlan,
            Actual = dummyActual,
            Comparison = dummyComparison
        });
    }

    /// <summary>Kaydedilen en son bakiye gözleminin istenen günü; gün verilmediyse <c>null</c>.</summary>
    public DateOnly? LastObservedOn { get; private set; }

    /// <summary>Doluysa ObserveCurrentBalanceAsync bu hatayı fırlatır ve hiçbir şey kaydetmez.</summary>
    public Exception? ObserveFailure { get; set; }

    /// <summary>Anlık bakiye gözlemini taklit olarak kaydeder.</summary>
    public Task<PeriodObservation> ObserveCurrentBalanceAsync(
        decimal balance,
        DateOnly? observedOn = null,
        CancellationToken cancellationToken = default)
    {
        if (ObserveFailure is not null) { return Task.FromException<PeriodObservation>(ObserveFailure); }

        LastObservedBalance = balance;
        LastObservedOn = observedOn;
        var observation = new PeriodObservation
        {
            PeriodPlanSnapshotId = Guid.NewGuid(),
            ObservedOn = observedOn ?? new DateOnly(2026, 9, 27),
            ObservedBalance = balance
        };
        return Task.FromResult(observation);
    }

    /// <summary>Ödeme gözlemini taklit olarak kaydeder.</summary>
    public Task<PeriodPaymentMark> ObservePaymentAsync(
        Guid periodPlanPaymentLineId,
        ActualPaymentStatus status,
        decimal actualAmount,
        DateOnly? actualPaymentDate = null,
        string note = "",
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
