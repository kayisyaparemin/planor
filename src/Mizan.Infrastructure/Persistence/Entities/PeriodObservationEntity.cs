using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Açık nakit akış döneminin anlık gözlem defterinin SQLite tablo varlığı.
/// </summary>
[Table("period_observations")]
internal sealed class PeriodObservationEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Unique]
    public string PeriodPlanSnapshotId { get; set; } = string.Empty;

    public string ObservedOn { get; set; } = string.Empty;

    public decimal? ObservedBalance { get; set; }

    public decimal ObservedLivingSpend { get; set; }

    public string Note { get; set; } = string.Empty;

    public string CreatedAtUtc { get; set; } = string.Empty;

    public string UpdatedAtUtc { get; set; } = string.Empty;
}
