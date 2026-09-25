using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Dönem içinde yapılan dondurulmuş plan revizyonunun SQLite tablo varlığı.
/// </summary>
[Table("period_plan_revisions")]
internal sealed class PeriodPlanRevisionEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string PeriodPlanSnapshotId { get; set; } = string.Empty;

    public int RevisionNumber { get; set; }

    public string CreatedAtUtc { get; set; } = string.Empty;

    [Column("Trigger")]
    public string Trigger { get; set; } = string.Empty;

    public decimal PlannedIncome { get; set; }

    public decimal PlannedLoanPayments { get; set; }

    public decimal PlannedCardPayments { get; set; }

    public decimal PlannedTemporaryPayments { get; set; }

    public decimal PlannedInstallmentPayments { get; set; }

    public decimal PlannedOtherScheduledPayments { get; set; }

    public decimal PlannedMandatoryPayments { get; set; }

    public decimal PlannedVariableExpenseAllowance { get; set; }

    public decimal PlannedLargeExpenses { get; set; }

    public decimal PlannedCardInterest { get; set; }

    public decimal PlannedDeficitInterest { get; set; }

    public decimal PlannedEndingBalance { get; set; }

    public string Note { get; set; } = string.Empty;
}
