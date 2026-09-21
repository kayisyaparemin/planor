namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartının belirli bir ekstre döngüsüne ait hesaplanan projeksiyon durumunu ve
/// kart kimlik bilgilerini bir arada sunan sözleşme.
/// Ekstre detaylarının arayüzde ve dönem dökümünde kart bazında gösterilmesini sağlar.
/// </summary>
public sealed record CreditCardPaymentProjectionStatus
{
    /// <summary>İlgili kredi kartının tekil kimliği.</summary>
    public Guid CardId { get; init; }

    /// <summary>Kartın banka ve ürün adı (örn. "Garanti Bonus").</summary>
    public string CardName { get; init; } = string.Empty;

    /// <summary>Ekstrenin hesap kesim tarihi.</summary>
    public DateOnly StatementCloseDate { get; init; }

    /// <summary>Ekstrenin son ödeme tarihi.</summary>
    public DateOnly PaymentDueDate { get; init; }

    /// <summary>Devreden bakiye, harcamalar ve akdi faiz toplamıyla oluşan ekstre borcu.</summary>
    public decimal? StatementBalance { get; init; }

    /// <summary>Yasal veya banka oranına göre ödenmesi gereken asgari tutar.</summary>
    public decimal? MinimumPayment { get; init; }

    /// <summary>Planlanan veya çözümlenen ödeme tutarı.</summary>
    public decimal? Payment { get; init; }

    /// <summary>Önceki ekstrelerden devreden açılış anapara borcu.</summary>
    public decimal? OpeningCarriedBalance { get; init; }

    /// <summary>Dönem içinde karta yansıyan yeni harcamalar toplamı.</summary>
    public decimal NewCharges { get; init; }

    /// <summary>Ödeme yapıldıktan sonra kalan ve devredecek olan anapara borcu.</summary>
    public decimal? CarriedPrincipalAfterPayment { get; init; }

    /// <summary>Devreden borca işletilen akdi faiz (carry faizi).</summary>
    public decimal CarryInterest { get; init; }

    /// <summary>Sonraki ekstreye açılış bakiyesi olarak devreden tutar.</summary>
    public decimal? NextCarriedBalance { get; init; }

    /// <summary>Uygulanan akdi faiz oranı.</summary>
    public decimal AppliedInterestRate { get; init; }

    /// <summary>Ödeme tutarının hangi kaynaktan belirlendiği.</summary>
    public CreditCardPaymentResolution Resolution { get; init; }

    /// <summary>Uygulanan ödeme tipi (Asgari, Tamamı veya Sabit Tutar).</summary>
    public CreditCardPaymentType? PaymentType { get; init; }

    /// <summary>Bu ödemenin doğal dönemsellikle atandığı nakit akış döneminin başlangıç tarihi.</summary>
    public DateOnly AssignedPeriodDate { get; init; }
}
