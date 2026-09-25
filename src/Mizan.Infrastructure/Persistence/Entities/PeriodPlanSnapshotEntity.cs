using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Belirli bir döneme ait dondurulmuş plan taahhüdünün SQLite tablo varlığı.
/// </summary>
[Table("period_plan_snapshots")]
internal sealed class PeriodPlanSnapshotEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string FinancialSnapshotId { get; set; } = string.Empty;

    public string PeriodStart { get; set; } = string.Empty;

    public string PeriodEnd { get; set; } = string.Empty;

    public string SettlementAvailableFrom { get; set; } = string.Empty;

    public string CreatedAtUtc { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }

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
}
