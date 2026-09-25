using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Banka tarafından kesilmiş kredi kartı hesap kesim ekstrelerini saklayan
/// <c>credit_card_statements</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableCreditCardStatements)]
internal sealed class CreditCardStatementEntity
{
    /// <summary>Ekstrenin metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Bağlı olduğu kredi kartının metin formatındaki GUID kimliği.</summary>
    [Indexed]
    public string CreditCardId { get; set; } = string.Empty;

    /// <summary>Hesap kesim tarihi (yyyy-MM-dd).</summary>
    [Indexed]
    public string StatementDate { get; set; } = string.Empty;

    /// <summary>Son ödeme tarihi (yyyy-MM-dd).</summary>
    public string DueDate { get; set; } = string.Empty;

    /// <summary>Toplam ekstre borç tutarı.</summary>
    public decimal StatementAmount { get; set; }

    /// <summary>Yasal asgari ödeme tutarı.</summary>
    public decimal MinimumPaymentAmount { get; set; }

    /// <summary>Bir sonraki hesap kesim tarihi (yyyy-MM-dd).</summary>
    public string? NextStatementDate { get; set; }

    /// <summary>Bir sonraki son ödeme tarihi (yyyy-MM-dd).</summary>
    public string? NextDueDate { get; set; }

    /// <summary>Kaydın oluşturulma zaman damgası (ISO 8601 UTC).</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>Kaydın son güncellenme zaman damgası (ISO 8601 UTC).</summary>
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>Mevcut ekstre için seçilen anlık ödeme modu (0: Minimum, 1: Full, 2: Custom).</summary>
    public int CurrentPaymentMode { get; set; }

    /// <summary>Özel ödeme seçildiyse ödenmesi kararlaştırılan tutar.</summary>
    public decimal? CurrentPaymentCustomAmount { get; set; }
}
