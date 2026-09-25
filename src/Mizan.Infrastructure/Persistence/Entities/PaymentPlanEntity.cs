using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kredi ve kart dışındaki vadeli veya taksitli geçici borç planlarını saklayan
/// <c>payment_plans</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TablePaymentPlans)]
internal sealed class PaymentPlanEntity
{
    /// <summary>Ödeme planının metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Ödeme planının adı.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ödeme planının türü (0: Temporary, 1: Installment, 2: Periodic, 3: Other).</summary>
    public int Kind { get; set; }

    /// <summary>Orijinal anapara tutarı.</summary>
    public decimal? OriginalAmount { get; set; }

    /// <summary>Toplam geri ödeme tutarı.</summary>
    public decimal? TotalRepaymentAmount { get; set; }
}
