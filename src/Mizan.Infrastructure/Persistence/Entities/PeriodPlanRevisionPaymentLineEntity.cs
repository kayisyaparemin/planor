using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Dondurulan plan revizyonuna ait tekil revize ödeme satırının SQLite tablo varlığı.
/// </summary>
[Table("period_plan_revision_payment_lines")]
internal sealed class PeriodPlanRevisionPaymentLineEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodPlanRevisionId { get; set; } = string.Empty;

    public string SourceEntityId { get; set; } = string.Empty;

    public int SourceType { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PlannedDate { get; set; } = string.Empty;

    public decimal? PlannedAmount { get; set; }

    public bool IsEstimate { get; set; }

    public string Detail { get; set; } = string.Empty;
}
