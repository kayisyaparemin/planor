using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Nakit akış döneminin gözlem defteri (anlık bakiye ve ara ödeme işaretleri) ile
/// dönem sonu mutabakat ve kapanış süreçlerini orkestre eden kullanım senaryosu portu.
/// </summary>
public interface IPeriodWorkflowService
{
    /// <summary>Yürürlükteki nakit akış döneminin kapatılmaya hazır olup olmadığını sorgular.</summary>
    Task<PeriodSettlementAvailability> GetSettlementAvailabilityAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Dönem kapanışı ekranı ve sihirbazı için bağlam nesnesini hazırlar.</summary>
    Task<PeriodSettlementContext> GetSettlementContextAsync(
        Guid? planId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Açık dönemin gözlem defterinden ve ödeme işaretlerinden dönem kapanış taslağını üretir; ikisi de yoksa <c>null</c> döner.</summary>
    Task<PeriodSettlementDraft?> GetObservedSettlementDraftAsync(
        Guid periodPlanSnapshotId,
        CancellationToken cancellationToken = default);

    /// <summary>Kapanış taslağının onay öncesi önizleme sonuçlarını hesaplar.</summary>
    Task<PeriodSettlementPreview> PreviewSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default);

    /// <summary>Dönem mutabakatını kesinleştirir, yeni dönemin planını dondurur ve gözlemleri yerinde bırakır: dönemin bakiye yolu tarihçede kalır (S68-7).</summary>
    Task<PeriodSettlementResult> FinalizeSettlementAsync(
        PeriodSettlementDraft draft,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Açık döneme yeni bir bakiye gözlemi ekler; aynı günün gözlemi varsa yerine geçer (S68-2). Gün verilmezse bugündür.
    /// Gün dönemin içinde olmalı ve bugünden sonra olamaz; kapanışı ertelenmiş dönem gözlem almaz, önce kapanış gerekir (S68-4).
    /// </summary>
    /// <exception cref="InvalidOperationException">Açık dönem yoksa ya da gün yazılamıyorsa.</exception>
    Task<PeriodObservation> ObserveCurrentBalanceAsync(
        decimal balance,
        DateOnly? observedOn = null,
        CancellationToken cancellationToken = default);

    /// <summary>Açık dönemin planlanan bir ödeme satırına ödeme işareti koyar; aynı satırın işareti yenisiyle değişir ve hiçbir gözlem değişmez (S68-8).</summary>
    Task<PeriodPaymentMark> ObservePaymentAsync(
        Guid periodPlanPaymentLineId,
        ActualPaymentStatus status,
        decimal actualAmount,
        DateOnly? actualPaymentDate = null,
        string note = "",
        CancellationToken cancellationToken = default);
}
