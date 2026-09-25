using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kullanıcının banka kredi sözleşmelerini ve taksit parametrelerini saklayan
/// <c>loans</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableLoans)]
internal sealed class LoanEntity
{
    /// <summary>Kredinin metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Kredinin adı.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Kredinin ait olduğu banka veya finans kurumu.</summary>
    public string Bank { get; set; } = string.Empty;

    /// <summary>Aylık standart taksit ödeme tutarı.</summary>
    public decimal MonthlyPayment { get; set; }

    /// <summary>Taksitin her ay ödeneceği gün (1-31).</summary>
    public int PaymentDay { get; set; }

    /// <summary>Bir sonraki taksit ödeme tarihi (yyyy-MM-dd).</summary>
    public string NextPaymentDate { get; set; } = string.Empty;

    /// <summary>Kalan toplam taksit adedi.</summary>
    public int RemainingInstallmentCount { get; set; }

    /// <summary>Son taksit standart taksite eşit değilse tutarı.</summary>
    public decimal? FinalPaymentAmount { get; set; }

    /// <summary>Kredinin kalan anapara borcu.</summary>
    public decimal? RemainingDebt { get; set; }

    /// <summary>Bankanın bildirdiği erken kapama tutarı.</summary>
    public decimal? EarlyClosureAmount { get; set; }

    /// <summary>Bankanın bildirdiği erken kapama tutarının referans tarihi (yyyy-MM-dd).</summary>
    public string? EarlyClosureAmountAsOf { get; set; }

    /// <summary>Kredi türü (0: Tüketici, 1: KonutSabitFaiz, 2: KonutDegiskenFaiz, 3: Tasit vb.).</summary>
    public int Kind { get; set; }

    /// <summary>Kredinin aktif olarak devam edip etmediği.</summary>
    public bool IsActive { get; set; } = true;
}
