using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Gelecekte tek seferde gerçekleşmesi beklenen planlı büyük harcamaları saklayan
/// <c>planned_large_expenses</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TablePlannedLargeExpenses)]
internal sealed class PlannedLargeExpenseEntity
{
    /// <summary>Harcamanın metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Harcamanın adı.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Planlanan harcama tutarı.</summary>
    public decimal Amount { get; set; }

    /// <summary>Harcamanın planlandığı kesin tarih (yyyy-MM-dd).</summary>
    [Indexed]
    public string ExactDate { get; set; } = string.Empty;

    /// <summary>Kullanıcı açıklaması veya notu.</summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>Harcamanın durumu (0: Planned, 1: Realized, 2: Cancelled).</summary>
    public int Status { get; set; }
}
