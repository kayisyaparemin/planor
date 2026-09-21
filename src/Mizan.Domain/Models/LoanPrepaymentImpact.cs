namespace Mizan.Domain.Models;

/// <summary>
/// Simülasyon senaryosundaki erken kapama veya kısmi ara ödemelerin belirli bir krediye olan
/// toplam faiz tasarrufu, vade kısalması ve taksit değişim etkilerini sunan sözleşme.
/// 12 dönemlik projeksiyon ufkunun ötesindeki kümülatif kredi tasarrufunu da kapsar.
/// </summary>
public sealed record LoanPrepaymentImpact
{
    /// <summary>Hedef kredi sözleşmesinin kimliği.</summary>
    public Guid LoanId { get; init; }

    /// <summary>Kredi sözleşmesinin banka ve ürün adı.</summary>
    public string LoanName { get; init; } = string.Empty;

    /// <summary>Senaryonun bu krediye eklediği erken ödemelerin toplam anapara tutarı.</summary>
    public decimal PrepaidAmount { get; init; }

    /// <summary>Kredinin kalan ömrü boyunca ödenmeyecek faizlerden sağlanan net tasarruf tutarı.</summary>
    public decimal InterestSaving { get; init; }

    /// <summary>Erken ödeme yapılmasaydı kredinin biteceği orijinal son taksit tarihi.</summary>
    public DateOnly? BaselineEndDate { get; init; }

    /// <summary>Erken ödeme sonrasında kredinin biteceği yeni son taksit tarihi.</summary>
    public DateOnly? ScenarioEndDate { get; init; }

    /// <summary>Kredinin erken ödeme öncesindeki baz aylık taksit tutarı.</summary>
    public decimal BaselineMonthlyPayment { get; init; }

    /// <summary>Taksit düşürmeli ara ödeme sonrasındaki yeni aylık taksit tutarı (vade kısaltmada değişmez).</summary>
    public decimal? ScenarioMonthlyPayment { get; init; }
}
