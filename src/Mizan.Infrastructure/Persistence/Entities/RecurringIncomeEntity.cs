using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kullanıcının düzenli gelir akışlarını (aylık gelir, kira vb.) saklayan
/// <c>recurring_incomes</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableRecurringIncomes)]
internal sealed class RecurringIncomeEntity
{
    /// <summary>Gelir akışının metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gelir akışının adı.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gelirin her ay hesaba geçtiği gün (1-31).</summary>
    public int PaymentDay { get; set; }

    /// <summary>Gelir akışının aktif olarak devam edip etmediği (0: Hayır, 1: Evet).</summary>
    public bool IsActive { get; set; } = true;
}
