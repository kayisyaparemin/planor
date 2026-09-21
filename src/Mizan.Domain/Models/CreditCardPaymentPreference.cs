namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının kredi kartı ödeme tercihinin etkin tarihli (effective-dated) tarihsel değişim kaydını temsil eder.
/// Ekstre ödeme kararlarının zaman içindeki değişimini append-only geçmiş olarak saklar; eski kayıtlar ezilmez.
/// </summary>
public sealed record CreditCardPaymentPreference
{
    /// <summary>Tercih kaydının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Tercihin ait olduğu kredi kartının kimliği.</summary>
    public Guid CreditCardId { get; init; }

    /// <summary>Belirlenen ödeme modu (Asgari, Tamamı, Başka Tutar).</summary>
    public CurrentStatementPaymentMode Mode { get; init; } = CurrentStatementPaymentMode.Minimum;

    /// <summary>Özel tutar modu seçildiğinde ödenecek miktar.</summary>
    public decimal? CustomAmount { get; init; }

    /// <summary>Tercihin hangi ekstre tarihinden itibaren geçerli olduğu.</summary>
    public DateOnly EffectiveFromStatementDate { get; init; }

    /// <summary>Tercihin kaydedildiği an.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Tercihe ilişkin isteğe bağlı kullanıcı notu.</summary>
    public string Note { get; init; } = string.Empty;
}
