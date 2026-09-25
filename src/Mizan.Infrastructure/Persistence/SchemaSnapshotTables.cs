namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Finansal anlık görüntüler, dondurulan dönem planları ve revizyon satırları
/// için SQLite temiz v1 DDL tanımlarını üreten dahili yardımcı sınıf.
/// </summary>
internal static class SchemaSnapshotTables
{
    public static IReadOnlyList<string> Commands =>
    [
        """
        CREATE TABLE IF NOT EXISTS financial_snapshots (
            Id TEXT PRIMARY KEY NOT NULL,
            SnapshotDate TEXT NOT NULL,
            ProjectionAnchorDate TEXT NOT NULL,
            NextSettlementDate TEXT NOT NULL,
            ProjectionOpeningBalance decimal NOT NULL,
            IncomeDay INTEGER NOT NULL,
            PreviousSnapshotId TEXT,
            Source INTEGER NOT NULL,
            IsCurrent INTEGER NOT NULL,
            CreatedAtUtc TEXT NOT NULL,
            Note TEXT NOT NULL
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_financial_snapshots_current ON financial_snapshots (IsCurrent, SnapshotDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_plan_snapshots (
            Id TEXT PRIMARY KEY NOT NULL,
            FinancialSnapshotId TEXT NOT NULL,
            PeriodStart TEXT NOT NULL,
            PeriodEnd TEXT NOT NULL,
            SettlementAvailableFrom TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL,
            OpeningBalance decimal NOT NULL,
            PlannedIncome decimal NOT NULL,
            PlannedLoanPayments decimal NOT NULL,
            PlannedCardPayments decimal NOT NULL,
            PlannedTemporaryPayments decimal NOT NULL,
            PlannedInstallmentPayments decimal NOT NULL,
            PlannedOtherScheduledPayments decimal NOT NULL,
            PlannedMandatoryPayments decimal NOT NULL,
            PlannedVariableExpenseAllowance decimal NOT NULL,
            PlannedLargeExpenses decimal NOT NULL,
            PlannedCardInterest decimal NOT NULL,
            PlannedDeficitInterest decimal NOT NULL,
            PlannedEndingBalance decimal NOT NULL,
            FOREIGN KEY (FinancialSnapshotId) REFERENCES financial_snapshots (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_plan_snapshots_snapshot ON period_plan_snapshots (FinancialSnapshotId, PeriodStart);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_plan_payment_lines (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodPlanSnapshotId TEXT NOT NULL,
            SourceEntityId TEXT NOT NULL,
            SourceType INTEGER NOT NULL,
            Name TEXT NOT NULL,
            PlannedDate TEXT NOT NULL,
            PlannedAmount decimal,
            IsEstimate INTEGER NOT NULL,
            Detail TEXT NOT NULL,
            FOREIGN KEY (PeriodPlanSnapshotId) REFERENCES period_plan_snapshots (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_plan_payment_lines_plan ON period_plan_payment_lines (PeriodPlanSnapshotId, PlannedDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_plan_income_lines (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodPlanSnapshotId TEXT NOT NULL,
            SourceType INTEGER NOT NULL,
            RecurringIncomeId TEXT,
            AdHocIncomeId TEXT,
            Name TEXT NOT NULL,
            PlannedDate TEXT NOT NULL,
            PlannedAmount decimal NOT NULL,
            FOREIGN KEY (PeriodPlanSnapshotId) REFERENCES period_plan_snapshots (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_plan_income_lines_plan ON period_plan_income_lines (PeriodPlanSnapshotId, PlannedDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_plan_revisions (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodPlanSnapshotId TEXT NOT NULL,
            RevisionNumber INTEGER NOT NULL,
            CreatedAtUtc TEXT NOT NULL,
            "Trigger" TEXT NOT NULL,
            PlannedIncome decimal NOT NULL,
            PlannedLoanPayments decimal NOT NULL,
            PlannedCardPayments decimal NOT NULL,
            PlannedTemporaryPayments decimal NOT NULL,
            PlannedInstallmentPayments decimal NOT NULL,
            PlannedOtherScheduledPayments decimal NOT NULL,
            PlannedMandatoryPayments decimal NOT NULL,
            PlannedVariableExpenseAllowance decimal NOT NULL,
            PlannedLargeExpenses decimal NOT NULL,
            PlannedCardInterest decimal NOT NULL,
            PlannedDeficitInterest decimal NOT NULL,
            PlannedEndingBalance decimal NOT NULL,
            Note TEXT NOT NULL,
            FOREIGN KEY (PeriodPlanSnapshotId) REFERENCES period_plan_snapshots (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_plan_revisions_plan ON period_plan_revisions (PeriodPlanSnapshotId, RevisionNumber);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_plan_revision_payment_lines (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodPlanRevisionId TEXT NOT NULL,
            SourceEntityId TEXT NOT NULL,
            SourceType INTEGER NOT NULL,
            Name TEXT NOT NULL,
            PlannedDate TEXT NOT NULL,
            PlannedAmount decimal,
            IsEstimate INTEGER NOT NULL,
            Detail TEXT NOT NULL,
            FOREIGN KEY (PeriodPlanRevisionId) REFERENCES period_plan_revisions (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_plan_revision_payment_lines_rev ON period_plan_revision_payment_lines (PeriodPlanRevisionId, PlannedDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_plan_revision_income_lines (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodPlanRevisionId TEXT NOT NULL,
            SourceType INTEGER NOT NULL,
            RecurringIncomeId TEXT,
            AdHocIncomeId TEXT,
            Name TEXT NOT NULL,
            PlannedDate TEXT NOT NULL,
            PlannedAmount decimal NOT NULL,
            FOREIGN KEY (PeriodPlanRevisionId) REFERENCES period_plan_revisions (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_plan_revision_income_lines_rev ON period_plan_revision_income_lines (PeriodPlanRevisionId, PlannedDate);
        """
    ];
}
