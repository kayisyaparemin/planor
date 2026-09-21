namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartının belirli bir hesap kesim dönemine ait simüle edilen ekstre durumunu temsil eden sözleşme.
/// Ekstre borcu, devreden anapara, yansıtılan akdi faiz (carry faizi), asgari ödeme ve belirlenen ödeme kararını içerir.
/// </summary>
public sealed record CreditCardStatementProjection
{
    /// <summary>Ekstrenin hesap kesim tarihi.</summary>
    public DateOnly StatementCloseDate { get; init; }

    /// <summary>Ekstrenin son ödeme tarihi.</summary>
    public DateOnly PaymentDueDate { get; init; }

    /// <summary>Önceki ekstrelerden bu ekstreye devreden açılış anapara borcu.</summary>
    public decimal? OpeningCarriedBalance { get; init; }

    /// <summary>Bu ekstre döneminde hesaba yansıyan yeni harcamalar ve taksitler toplamı.</summary>
    public decimal NewCharges { get; init; }

    /// <summary>Devreden bakiye, akdi faiz ve yeni harcamaların toplamıyla oluşan ekstre borcu.</summary>
    public decimal? StatementBalance { get; init; }

    /// <summary>Yasal veya banka oranına göre ödenmesi gereken asgari tutar.</summary>
    public decimal? MinimumPayment { get; init; }

    /// <summary>Kullanıcının tercih ve planlarına göre bu ekstre için planlanan ödeme tutarı.</summary>
    public decimal? Payment { get; init; }

    /// <summary>Ödeme yapıldıktan sonra kalan ve bir sonraki ekstreye devredecek olan anapara borcu.</summary>
    public decimal? CarriedAfterPayment { get; init; }

    /// <summary>Bu ekstreye giren devreden borca işletilen akdi faiz (carry faizi).</summary>
    public decimal CarryInterest { get; init; }

    /// <summary>Bir sonraki ekstreye açılış bakiyesi olarak devreden tutar.</summary>
    public decimal? NextCarriedBalance { get; init; }

    /// <summary>Bu dönemde devreden borca uygulanan akdi faiz oranı.</summary>
    public decimal AppliedInterestRate { get; init; }

    /// <summary>Ödeme tutarının hangi kaynaktan (plan, override, strateji, fallback) belirlendiği.</summary>
    public CreditCardPaymentResolution PaymentResolution { get; init; }

    /// <summary>Uygulanan ödeme tipi (Asgari, Tamamı veya Sabit Tutar).</summary>
    public CreditCardPaymentType? AppliedPaymentType { get; init; }

    /// <summary>Banka tarafından fiilen kesilmiş gerçek bir ekstre olup olmadığı.</summary>
    public bool IsActualStatement { get; init; }

    /// <summary>Kesilmiş gerçek ekstrenin kaynağı (Manuel, PDF içe aktarım vb.).</summary>
    public CreditCardStatementSource? StatementSource { get; init; }
}
