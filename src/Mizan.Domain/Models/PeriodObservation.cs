namespace Mizan.Domain.Models;

/// <summary>
/// Açık dönemin bakiye yolundaki tek nokta: kullanıcının bir günde girdiği anlık bakiye.
/// Ana sayfa grafiği her gözlemi bir nokta olarak çizer, gidişat en geç tarihlisinden hesaplanır.
/// </summary>
/// <remarks>
/// Bu kayıt snapshot zincirinin dışındadır; yazılması veya güncellenmesi finansal planı
/// veya dondurulmuş dönem planını mutasyona uğratmaz. Gün başına tek gözlem tutulur; hangi güne
/// yazılabileceği ve hangisinin son gözlem olduğu <see cref="Calculations.PeriodObservationRules"/>'ta (S68).
/// Ödeme işaretleri gözlemin parçası değildir; <see cref="PeriodPaymentMark"/> olarak plana bağlanır.
/// </remarks>
public sealed record PeriodObservation
{
    /// <summary>Gözlem kaydının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gözlemin ait olduğu dondurulmuş açık dönem planının kimliği.</summary>
    public Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Gözlemin yapıldığı takvim günü.</summary>
    public DateOnly ObservedOn { get; init; }

    /// <summary>Kullanıcının gözlem anında teyit ettiği anlık serbest nakit bakiyesi.</summary>
    public decimal ObservedBalance { get; init; }

    /// <summary>Gözlemin kaydedildiği UTC zaman damgası; aynı gün yeni giriş kendi zamanıyla yerine geçer.</summary>
    public DateTimeOffset RecordedAtUtc { get; init; }
}
