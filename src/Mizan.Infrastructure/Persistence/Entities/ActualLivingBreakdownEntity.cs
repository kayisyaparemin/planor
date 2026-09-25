using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kapanan dönemin fiilî serbest harcama kategori dökümünün SQLite tablo varlığı.
/// </summary>
[Table("actual_living_breakdowns")]
internal sealed class ActualLivingBreakdownEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodActualId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
