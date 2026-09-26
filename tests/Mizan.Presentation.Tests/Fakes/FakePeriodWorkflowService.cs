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

    /// <summary>Bağlam nesnesini döndürür.</summary>
    public Task<PeriodSettlementContext> GetSettlementContextAsync(
        Guid? planId = null,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Gözlemli taslak nesnesini döndürür.</summary>
    public Task<PeriodSettlementDraft?> GetObservedSettlementDraftAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<PeriodSettlementDraft?>(null);

    /// <summary>Önizlemeyi döndürür.</summary>
    public Task<PeriodSettlementPreview> PreviewSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Mutabakatı kesinleştirir.</summary>
    public Task<PeriodSettlementResult> FinalizeSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <summary>Anlık bakiye gözlemini taklit olarak kaydeder.</summary>
    public Task<PeriodObservation> ObserveCurrentBalanceAsync(
        decimal balance,
        CancellationToken cancellationToken = default)
    {
        LastObservedBalance = balance;
        var observation = new PeriodObservation
        {
            PeriodPlanSnapshotId = Guid.NewGuid(),
            ObservedOn = new DateOnly(2026, 9, 27),
            ObservedBalance = balance,
            ObservedLivingSpend = 0m
        };
        return Task.FromResult(observation);
    }

    /// <summary>Ödeme gözlemini taklit olarak kaydeder.</summary>
    public Task<PeriodObservation> ObservePaymentAsync(
        Guid periodPlanPaymentLineId,
        ActualPaymentStatus status,
        decimal actualAmount,
        DateOnly? actualPaymentDate = null,
        string note = "",
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
