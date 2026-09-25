using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Dönem gözlem defterine kaydedilen tekil plan ödemesi gözleminin SQLite tablo varlığı.
/// </summary>
[Table("period_observation_payments")]
internal sealed class PeriodObservationPaymentEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodObservationId { get; set; } = string.Empty;

    public string PeriodPlanPaymentLineId { get; set; } = string.Empty;

    public int Status { get; set; }

    public decimal ActualAmount { get; set; }

    public string? ActualPaymentDate { get; set; }

    public string Note { get; set; } = string.Empty;
}
