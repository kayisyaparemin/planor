namespace Mizan.Domain.Models;

/// <summary>
/// Açık olan nakit akış döneminin canlı gözlem defteri sözleşmesi.
/// Dönem içinde kullanıcının girdiği anlık kasa/hesap bakiyesini kesinleşene kadar geçici olarak saklamak,
/// dönem içi canlı kalan yaşam havuzunu hesaplamak ve dönem kapanış taslağına hazır veri sağlamak için vardır.
/// </summary>
/// <remarks>
/// Bu kayıt snapshot zincirinin dışındadır; yazılması veya güncellenmesi finansal planı
/// veya dondurulmuş dönem planını mutasyona uğratmaz. Gün başına tek gözlem tutulur; hangi güne
/// yazılabileceği ve hangisinin son gözlem olduğu <see cref="Calculations.PeriodObservationRules"/>'ta (S68).
/// Ödeme işaretleri gözlemin parçası değildir; <see cref="PeriodPaymentMark"/> olarak plana bağlanır.
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

    /// <summary>Kullanıcının anlık bir serbest bakiye girip girmediğini belirtir.</summary>
    public bool HasObservedBalance => ObservedBalance.HasValue;
}
