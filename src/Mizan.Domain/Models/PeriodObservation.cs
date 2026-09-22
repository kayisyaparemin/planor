namespace Mizan.Domain.Models;

/// <summary>
/// Açık olan nakit akış döneminin canlı gözlem defteri sözleşmesi.
/// Dönem içinde kullanıcının girdiği anlık kasa/hesap bakiyesini ve erken işaretlenen
/// plan ödemelerini kesinleşene kadar geçici olarak saklamak, dönem içi canlı kalan
/// yaşam havuzunu hesaplamak ve dönem kapanış taslağına hazır veri sağlamak için vardır.
/// </summary>
/// <remarks>
/// Bu kayıt snapshot zincirinin dışındadır; yazılması veya güncellenmesi finansal planı
/// veya dondurulmuş dönem planını mutasyona uğratmaz. Açık plan başına tek kayıt tutulur.
/// </remarks>
public sealed record PeriodObservation
{
    /// <summary>Gözlem defteri kaydının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gözlemin ait olduğu dondurulmuş açık dönem planının kimliği.</summary>
    public Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Gözlemin yapıldığı takvim günü.</summary>
    public DateOnly ObservedOn { get; init; }

    /// <summary>Kullanıcının gözlem anında teyit ettiği anlık serbest nakit bakiyesi (girilmediyse null).</summary>
    public decimal? ObservedBalance { get; init; }

    /// <summary>Dönem başından gözlem anına kadar serbest yaşam havuzundan fiilen harcanan kümülatif tutar.</summary>
    public decimal ObservedLivingSpend { get; init; }

    /// <summary>Gözleme dair kullanıcının girdiği isteğe bağlı not.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Gözlemin ilk kaydedildiği UTC zaman damgası.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Gözlemin son güncellendiği UTC zaman damgası.</summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>Dönem içinde fiilen gerçekleştiği gözlenen plan ödeme satırları.</summary>
    public IReadOnlyList<PeriodObservationPayment> Payments { get; init; } = [];

    /// <summary>Kullanıcının anlık bir serbest bakiye girip girmediğini belirtir.</summary>
    public bool HasObservedBalance => ObservedBalance.HasValue;

    /// <summary>Ödenmiş veya farklı tutarla ödenmiş olarak işaretlenen gözlem ödemelerinin toplam tutarı.</summary>
    public decimal TotalObservedPayments => Payments.Where(p => p.IsSettled).Sum(p => p.ActualAmount);

    /// <summary>Belirtilen plan ödeme satırına ait gözlem kaydını bulur.</summary>
    public PeriodObservationPayment? FindPayment(Guid periodPlanPaymentLineId) =>
        Payments.FirstOrDefault(p => p.PeriodPlanPaymentLineId == periodPlanPaymentLineId);

    /// <summary>Belirtilen plan ödeme satırının gözlem defterinde ödenmiş işaretlenip işaretlenmediğini denetler.</summary>
    public bool IsPaymentSettled(Guid periodPlanPaymentLineId) =>
        FindPayment(periodPlanPaymentLineId)?.IsSettled ?? false;
}
