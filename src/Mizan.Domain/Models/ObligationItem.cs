namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir vadede ödenmesi gereken tekil bir borç veya harcama yükümlülüğünü temsil eden sözleşme.
/// Kredi taksiti, kart ödemesi veya vadeli plan taksiti gibi farklı kaynaklardan gelen ödemeleri ortak yapıda birleştirir.
/// </summary>
public sealed record ObligationItem(
    string Name,
    ObligationType Type,
    DateOnly DueDate,
    decimal Amount)
{
    /// <summary>Bu ödemenin ilgili sözleşme veya planın son taksiti olup olmadığı.</summary>
    public bool IsFinalPayment { get; init; }

    /// <summary>Tutarın henüz kesinleşmemiş bir tahmin (örn. kesilmemiş ekstre projeksiyonu) olup olmadığı.</summary>
    public bool IsEstimate { get; init; }

    /// <summary>Ödemeye dair ek açıklama veya detay bilgisi (örn. erken ödeme kırılımı).</summary>
    public string Detail { get; init; } = string.Empty;

    /// <summary>Ödemenin kaynaklandığı sözleşme, taksit veya erken ödeme kaydının tekil kimliği.</summary>
    public Guid PaymentId { get; init; }
}
