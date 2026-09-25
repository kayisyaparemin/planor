using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Geçici veya vadeli ödeme planlarına ait taksit kayıtlarını saklayan
/// <c>payment_installments</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TablePaymentInstallments)]
internal sealed class PaymentInstallmentEntity
{
    /// <summary>Taksitin metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Bağlı olduğu ödeme planının metin formatındaki GUID kimliği.</summary>
    [Indexed]
    public string PlanId { get; set; } = string.Empty;

    /// <summary>Taksitin son ödeme tarihi (yyyy-MM-dd).</summary>
    [Indexed]
    public string DueDate { get; set; } = string.Empty;

    /// <summary>Taksit tutarı.</summary>
    public decimal Amount { get; set; }

    /// <summary>Taksitin ödenip ödenmediği (0: Hayır, 1: Evet).</summary>
    public bool IsPaid { get; set; }
}
