using Mizan.Domain.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// Kart kontrol ekranında sıradaki ödemeden sonraki bir vadenin satırı: ne kadar ödeneceği
/// ve bu tutarın hangi kuraldan geldiği (EK-V7 S3).
/// </summary>
public sealed record UpcomingPaymentItem
{
    /// <summary>Ekstrenin son ödeme tarihi.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Uygulanan kurala göre vadede ödenecek tutar; kural yoksa boş.</summary>
    public decimal? PaymentAmount { get; init; }

    /// <summary>Tahmini ekstre tutarı.</summary>
    public decimal? StatementBalance { get; init; }

    /// <summary>Uygulanan ödeme tipi (Asgari, Tamamı, sabit tutar).</summary>
    public CreditCardPaymentType? AppliedPaymentType { get; init; }

    /// <summary>Ödeme tipinin nereden geldiği: vadeye özel karar, kartın varsayılanı ya da varsayım.</summary>
    public CreditCardPaymentResolution Resolution { get; init; }

    /// <summary>Bu vade için kartın varsayılanından ayrı bir karar verilmiş mi.</summary>
    public bool IsDueDateOverride => Resolution == CreditCardPaymentResolution.DueDateOverride;
}
