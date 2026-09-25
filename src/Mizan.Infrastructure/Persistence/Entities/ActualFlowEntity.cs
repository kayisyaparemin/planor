using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kapanan dönem içinde gerçekleşen plansız gelir ve ödeme akışının SQLite tablo varlığı.
/// </summary>
[Table("actual_flows")]
internal sealed class ActualFlowEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodActualId { get; set; } = string.Empty;

    public int Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Date { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
