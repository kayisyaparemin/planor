namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının bir finansal kuruluşa olan vadeli borçlanma sözleşmesini ve taksit parametrelerini temsil eder.
/// Nakit akış projeksiyonunda zorunlu taksit çıkışlarının türetilmesi ve erken kapama/itfa analizleri için temel kaynaktır.
/// </summary>
public sealed record Loan
{
    /// <summary>Kredinin benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Kredinin kullanıcı tarafından tanımlanan adı (örn. Konut Kredisi, İhtiyaç Kredisi).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Kredinin çekildiği banka veya finansman kuruluşu.</summary>
    public string Bank { get; init; } = string.Empty;

    /// <summary>Aylık standart taksit ödeme tutarı.</summary>
    public decimal MonthlyPayment { get; init; }

    /// <summary>Taksitin her ay ödeneceği gün (1-31).</summary>
    public int PaymentDay { get; init; }

    /// <summary>
    /// Bir sonraki taksit ödeme tarihi.
    /// İlk taksit veya ödenmemiş en yakın taksitin vadesidir.
    /// </summary>
    public DateOnly NextPaymentDate { get; init; }

    /// <summary>Ödenmesi gereken kalan toplam taksit adedi.</summary>
    public int RemainingInstallmentCount { get; init; }

    /// <summary>
    /// Son taksit standart taksite eşit değilse tutarı.
    /// Vade kısaltan ara ödeme sonucunda son taksit küçülebilir; null ise son taksit de <see cref="MonthlyPayment"/> tutarıdır.
    /// </summary>
    public decimal? FinalPaymentAmount { get; init; }

    /// <summary>
    /// Kredinin kalan anapara borcu.
    /// <see cref="NextPaymentDate"/> tarihindeki taksit ödenmeden hemen önceki anapara bakiyesidir.
    /// Kalan taksitlerin toplamı değildir (faiz ve vergileri içermez).
    /// </summary>
    public decimal? RemainingDebt { get; init; }

    /// <summary>
    /// Bankanın bildirdiği erken kapama tutarı.
    /// Yalnızca <see cref="EarlyClosureAmountAsOf"/> tarihi ile birlikte geçerlidir.
    /// </summary>
    public decimal? EarlyClosureAmount { get; init; }

    /// <summary>Bankanın bildirdiği erken kapama tutarının geçerli olduğu referans tarih.</summary>
    public DateOnly? EarlyClosureAmountAsOf { get; init; }

    /// <summary>6502 sayılı Kanun kapsamındaki yasal kredi türü.</summary>
    public LoanKind Kind { get; init; } = LoanKind.Consumer;

    /// <summary>Kredinin aktif olarak devam edip etmediği.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Kredinin son taksit tutarı.
    /// Özel bir <see cref="FinalPaymentAmount"/> belirlenmişse onu, aksi halde standart <see cref="MonthlyPayment"/> tutarını döner.
    /// </summary>
    public decimal LastInstallmentAmount => FinalPaymentAmount ?? MonthlyPayment;

    /// <summary>
    /// Kalan tüm taksitlerin nominal toplam tutarı (anapara + faiz + vergiler).
    /// Kalan taksit sayısı &lt; 1 ise 0 döner.
    /// </summary>
    public decimal RemainingInstallmentTotal => RemainingInstallmentCount < 1
        ? 0m
        : MonthlyPayment * (RemainingInstallmentCount - 1) + LastInstallmentAmount;
}
