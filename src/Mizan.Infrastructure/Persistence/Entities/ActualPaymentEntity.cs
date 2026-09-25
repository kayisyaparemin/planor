using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kapanan dönemin tekil plan ödemesi fiilî gerçekleşme satırının SQLite tablo varlığı.
/// </summary>
[Table("actual_payments")]
internal sealed class ActualPaymentEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodActualId { get; set; } = string.Empty;

    public string PeriodPlanPaymentLineId { get; set; } = string.Empty;

    public string SourceEntityId { get; set; } = string.Empty;

    public int SourceType { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PlannedDate { get; set; } = string.Empty;

    public decimal? PlannedAmount { get; set; }

    public string? ActualPaymentDate { get; set; }

    public decimal ActualAmount { get; set; }

    public int Status { get; set; }

    public string Note { get; set; } = string.Empty;
}
