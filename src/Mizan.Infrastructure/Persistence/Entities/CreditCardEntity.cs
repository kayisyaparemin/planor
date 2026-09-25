using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kredi kartı ana tanımlarını ve hesap kesim parametrelerini saklayan
/// <c>credit_cards</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableCreditCards)]
internal sealed class CreditCardEntity
{
    /// <summary>Kartın metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Kullanıcının karta verdiği ad.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Kartın ait olduğu banka veya finans kuruluşu.</summary>
    public string Bank { get; set; } = string.Empty;

    /// <summary>Kartın tahsis edilen toplam limiti.</summary>
    public decimal Limit { get; set; }

    /// <summary>Geçmiş ekstrelerden devreden, faiz işletilmeye devam eden ödenmemiş anapara borcu.</summary>
    public decimal CarriedBalance { get; set; }

    /// <summary>Henüz bir ekstreye girmemiş, güncel dönem içi faturalanmamış harcama toplamı.</summary>
    public decimal UnbilledSpending { get; set; }

    /// <summary>Bakiye ve harcama bilgilerinin girildiği veya mutabakatın yapıldığı referans tarih (yyyy-MM-dd).</summary>
    public string BalanceAsOfDate { get; set; } = string.Empty;

    /// <summary>Her ay ekstrenin kesildiği hesap kesim günü (1-31).</summary>
    public int StatementClosingDay { get; set; }

    /// <summary>Her ay ekstrenin son ödeme günü (1-31).</summary>
    public int PaymentDueDay { get; set; }

    /// <summary>Asgari ödeme oranı (0.20 veya 0.40).</summary>
    public decimal MinimumPaymentRate { get; set; }

    /// <summary>Kullanıcının varsayılan ekstre ödeme stratejisi.</summary>
    public int PaymentStrategy { get; set; }

    /// <summary>Sabit tutar ödeme stratejisi seçildiğinde her dönem ödenmesi hedeflenen tutar.</summary>
    public decimal? FixedPaymentAmount { get; set; }

    /// <summary>Simülasyonlarda kullanılacak yedek ödeme stratejisi.</summary>
    public int ProjectionFallbackStrategy { get; set; }

    /// <summary>Yedek ödeme stratejisi sabit tutar olduğunda kullanılacak varsayılan miktar.</summary>
    public decimal? ProjectionFallbackFixedAmount { get; set; }

    /// <summary>Bir sonraki bilinen kesin hesap kesim tarihi (yyyy-MM-dd).</summary>
    public string? KnownNextStatementDate { get; set; }

    /// <summary>Bir sonraki bilinen kesin son ödeme tarihi (yyyy-MM-dd).</summary>
    public string? KnownNextDueDate { get; set; }

    /// <summary>Kartın aktif olarak nakit akış planında kullanılıp kullanılmadığı.</summary>
    public bool IsActive { get; set; } = true;
}
