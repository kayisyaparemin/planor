using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Dondurulan plan revizyonuna ait tekil revize gelir satırının SQLite tablo varlığı.
/// </summary>
[Table("period_plan_revision_income_lines")]
internal sealed class PeriodPlanRevisionIncomeLineEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodPlanRevisionId { get; set; } = string.Empty;

    public int SourceType { get; set; }

    public string? RecurringIncomeId { get; set; }

    public string? AdHocIncomeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PlannedDate { get; set; } = string.Empty;

    public decimal PlannedAmount { get; set; }
}
