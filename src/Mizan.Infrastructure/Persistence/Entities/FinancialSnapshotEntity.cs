using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Belirli bir tarihteki genel finansal durum anlık görüntüsünün SQLite tablo varlığı.
/// </summary>
[Table("financial_snapshots")]
internal sealed class FinancialSnapshotEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string SnapshotDate { get; set; } = string.Empty;

    public string ProjectionAnchorDate { get; set; } = string.Empty;

    public string NextSettlementDate { get; set; } = string.Empty;

    public decimal ProjectionOpeningBalance { get; set; }

    public int IncomeDay { get; set; }

    public string? PreviousSnapshotId { get; set; }

    public int Source { get; set; }

    public bool IsCurrent { get; set; }

    public string CreatedAtUtc { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;
}
