namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir son ödeme tarihi (ekstre vadesi) için kullanıcının tanımladığı istisnai ödeme planını temsil eder.
/// Kartın genel ödeme stratejisinden bağımsız olarak tekil bir ay için farklı bir ödeme tutarı veya türü belirlenmesini sağlar.
/// </summary>
public sealed record CreditCardPaymentPlan
{
    /// <summary>Ödeme planının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Planın ait olduğu kredi kartının kimliği.</summary>
    public Guid CreditCardId { get; init; }

    /// <summary>Planın geçerli olduğu son ödeme tarihi.</summary>
    public DateOnly DueDate { get; init; }

    /// <summary>Bu vade için uygulanacak ödeme türü (Asgari, Tamamı, Sabit vb.).</summary>
    public CreditCardPaymentType PaymentType { get; init; }

    /// <summary>Sabit veya özel tutar seçildiğinde ödenecek miktar.</summary>
    public decimal? Amount { get; init; }
}
