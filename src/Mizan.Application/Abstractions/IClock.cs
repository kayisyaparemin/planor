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
}
