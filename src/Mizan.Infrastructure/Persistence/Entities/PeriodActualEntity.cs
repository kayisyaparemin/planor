using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kapanan bir döneme ait kesinleşmiş fiilî durumun SQLite tablo varlığı.
/// </summary>
[Table("period_actuals")]
internal sealed class PeriodActualEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Unique]
    public string PeriodPlanSnapshotId { get; set; } = string.Empty;

    public string SourceFinancialSnapshotId { get; set; } = string.Empty;

    public string ResultFinancialSnapshotId { get; set; } = string.Empty;

    public string PeriodStart { get; set; } = string.Empty;

    public string PeriodEnd { get; set; } = string.Empty;

    public string FinalizedAtUtc { get; set; } = string.Empty;

    public decimal ActualIncome { get; set; }

    public decimal ActualLoanPayments { get; set; }

    public decimal ActualCardPayments { get; set; }

    public decimal ActualTemporaryPayments { get; set; }

    public decimal ActualInstallmentPayments { get; set; }

    public decimal ActualOtherScheduledPayments { get; set; }

    public decimal ActualLargeExpenses { get; set; }

    public decimal ActualMandatoryPayments { get; set; }

    public decimal ActualLivingSpend { get; set; }

    public decimal ActualInterest { get; set; }

    public decimal UnplannedIncome { get; set; }

    public decimal UnplannedPayments { get; set; }

    public decimal DerivedEndingBalance { get; set; }

    public decimal ConfirmedEndingBalance { get; set; }

    public decimal ReconciliationAdjustment { get; set; }

    public string ComparisonSummary { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;
}
