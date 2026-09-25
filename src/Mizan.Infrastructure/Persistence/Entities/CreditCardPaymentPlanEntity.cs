using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kredi kartının belirli bir vadesi için tanımlanmış istisnai ödeme planlarını saklayan
/// <c>credit_card_payment_plans</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableCreditCardPaymentPlans)]
internal sealed class CreditCardPaymentPlanEntity
{
    /// <summary>Planın metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Bağlı olduğu kredi kartının metin formatındaki GUID kimliği.</summary>
    [Indexed]
    public string CreditCardId { get; set; } = string.Empty;

    /// <summary>Planın geçerli olduğu son ödeme tarihi (yyyy-MM-dd).</summary>
    [Indexed]
    public string DueDate { get; set; } = string.Empty;

    /// <summary>Ödeme türü (0: Minimum, 1: StatementBalance, 2: FixedAmount vb.).</summary>
    public int PaymentType { get; set; }

    /// <summary>Ödenecek miktar.</summary>
    public decimal? Amount { get; set; }
}
