using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Münferit ve tek seferlik arızi gelirleri saklayan
/// <c>ad_hoc_incomes</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableAdHocIncomes)]
internal sealed class AdHocIncomeEntity
{
    /// <summary>Münferit gelirin metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Tahsil edilecek net tutar.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gelirin hesaba geçeceği kesin tarih (yyyy-MM-dd).</summary>
    [Indexed]
    public string ExactDate { get; set; } = string.Empty;

    /// <summary>Gelirin açıklaması.</summary>
    public string Description { get; set; } = string.Empty;
}
