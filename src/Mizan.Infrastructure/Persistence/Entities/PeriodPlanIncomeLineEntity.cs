using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Dondurulan dönem planına dahil edilen tekil plan gelir satırının SQLite tablo varlığı.
/// </summary>
[Table("period_plan_income_lines")]
internal sealed class PeriodPlanIncomeLineEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodPlanSnapshotId { get; set; } = string.Empty;

    public int SourceType { get; set; }

    public string? RecurringIncomeId { get; set; }

    public string? AdHocIncomeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PlannedDate { get; set; } = string.Empty;

    public decimal PlannedAmount { get; set; }
}
