namespace Mizan.Application.Abstractions;

/// <summary>
/// Uygulama genelinde zaman erişimini soyutlayan saat portu.
/// Doğrudan DateTime.Now kullanımını engelleyerek zaman bağımlı iş kurallarının
/// ve projeksiyonların test edilebilir olmasını sağlar (K1 kuralı).
/// </summary>
public interface IClock
{
    /// <summary>
    /// Sistemin yerel bugünkü takvim tarihi.
    /// </summary>
    DateOnly Today { get; }

    /// <summary>
    /// Eşgüdümlü Evrensel Zaman (UTC) anlık zaman damgası.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Cihazın yerel saatiyle şimdi. Kayıt zamanı UTC'dir; ama ödeme günü, bildirim saati ve gece sessizliği
    /// kullanıcının duvar saatine göre yazılmıştır. Hatırlatıcı kartı bu ikisini karıştırıp UTC verdiği için gece
    /// önceki günün ödemesini gösteriyor, ertelemeyi gece sessizliğine düşürüyordu.
    /// </summary>
    DateTimeOffset Now { get; }
}
