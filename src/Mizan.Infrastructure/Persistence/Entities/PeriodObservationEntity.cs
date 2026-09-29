using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Açık nakit akış döneminin bakiye gözlemlerinin SQLite tablo varlığı; (plan, gün) çifti tektir (S68-2).
/// </summary>
[Table("period_observations")]
internal sealed class PeriodObservationEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string PeriodPlanSnapshotId { get; set; } = string.Empty;

    public string ObservedOn { get; set; } = string.Empty;

    public decimal ObservedBalance { get; set; }

    public string RecordedAtUtc { get; set; } = string.Empty;
}
