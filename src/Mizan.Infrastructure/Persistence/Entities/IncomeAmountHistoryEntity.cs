using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Düzenli gelir akışlarının etkin tarihli tutar ve zam revizyonlarını saklayan
/// <c>income_amount_histories</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableIncomeAmountHistories)]
internal sealed class IncomeAmountHistoryEntity
{
    /// <summary>Tutar geçmiş kaydının metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Bağlı olduğu düzenli gelir akışının metin formatındaki GUID kimliği.</summary>
    [Indexed]
    public string RecurringIncomeId { get; set; } = string.Empty;

    /// <summary>Geçerli olan net gelir tutarı.</summary>
    public decimal Amount { get; set; }

    /// <summary>Tutarın yürürlüğe girdiği etkin başlangıç tarihi (yyyy-MM-dd).</summary>
    [Indexed]
    public string EffectiveDate { get; set; } = string.Empty;

    /// <summary>Revizyona dair açıklama.</summary>
    public string Description { get; set; } = string.Empty;
}
