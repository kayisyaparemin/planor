namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir dönem veya zaman aralığındaki zorunlu nakit çıkışlarını kategori bazında toplayan özet sözleşmesi.
/// Kredi, kredi kartı, vadeli ve taksitli harcamaların alt toplamlarını ve genel zorunlu ödeme toplamını sunar.
/// </summary>
public sealed record MandatoryPaymentSummary
{
    /// <summary>Özete dahil edilen filtrelenmiş ve sıralanmış yükümlülük kalemleri.</summary>
    public IReadOnlyList<ObligationItem> Items { get; init; } = [];

    /// <summary>Dönemdeki toplam kredi taksiti ve erken ödeme tutarı.</summary>
    public decimal LoanPayments { get; init; }

    /// <summary>Dönemdeki toplam kredi kartı ekstre ödemeleri tutarı.</summary>
    public decimal CreditCardPayments { get; init; }

    /// <summary>Dönemdeki geçici borç ödemeleri toplamı.</summary>
    public decimal TemporaryPayments { get; init; }

    /// <summary>Dönemdeki taksitli plan ödemeleri toplamı.</summary>
    public decimal InstallmentPayments { get; init; }

    /// <summary>Dönemdeki diğer periyodik planlı ödemeler toplamı.</summary>
    public decimal OtherScheduledPayments { get; init; }

    /// <summary>Tüm zorunlu ödeme kategorilerinin genel toplamı.</summary>
    public decimal Total { get; init; }
}
