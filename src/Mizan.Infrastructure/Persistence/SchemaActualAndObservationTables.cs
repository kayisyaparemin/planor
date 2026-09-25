namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Dönem gerçekleşmeleri, gözlem defteri, simülasyon taslakları ve hatırlatıcı defteri
/// için SQLite temiz v1 DDL tanımlarını üreten dahili yardımcı sınıf.
/// </summary>
internal static class SchemaActualAndObservationTables
{
    public static IReadOnlyList<string> Commands =>
    [
        """
        CREATE TABLE IF NOT EXISTS period_actuals (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodPlanSnapshotId TEXT UNIQUE NOT NULL,
            SourceFinancialSnapshotId TEXT NOT NULL,
            ResultFinancialSnapshotId TEXT NOT NULL,
            PeriodStart TEXT NOT NULL,
            PeriodEnd TEXT NOT NULL,
            FinalizedAtUtc TEXT NOT NULL,
            ActualIncome decimal NOT NULL,
            ActualLoanPayments decimal NOT NULL,
            ActualCardPayments decimal NOT NULL,
            ActualTemporaryPayments decimal NOT NULL,
            ActualInstallmentPayments decimal NOT NULL,
            ActualOtherScheduledPayments decimal NOT NULL,
            ActualLargeExpenses decimal NOT NULL,
            ActualMandatoryPayments decimal NOT NULL,
            ActualLivingSpend decimal NOT NULL,
            ActualInterest decimal NOT NULL,
            UnplannedIncome decimal NOT NULL,
            UnplannedPayments decimal NOT NULL,
            DerivedEndingBalance decimal NOT NULL,
            ConfirmedEndingBalance decimal NOT NULL,
            ReconciliationAdjustment decimal NOT NULL,
            ComparisonSummary TEXT NOT NULL,
            Note TEXT NOT NULL,
            FOREIGN KEY (PeriodPlanSnapshotId) REFERENCES period_plan_snapshots (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_actuals_dates ON period_actuals (PeriodStart, PeriodEnd);
        """,
        """
        CREATE TABLE IF NOT EXISTS actual_payments (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodActualId TEXT NOT NULL,
            PeriodPlanPaymentLineId TEXT NOT NULL,
            SourceEntityId TEXT NOT NULL,
            SourceType INTEGER NOT NULL,
            Name TEXT NOT NULL,
            PlannedDate TEXT NOT NULL,
            PlannedAmount decimal,
            ActualPaymentDate TEXT,
            ActualAmount decimal NOT NULL,
            Status INTEGER NOT NULL,
            Note TEXT NOT NULL,
            FOREIGN KEY (PeriodActualId) REFERENCES period_actuals (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_actual_payments_actual ON actual_payments (PeriodActualId);
        """,
        """
        CREATE TABLE IF NOT EXISTS actual_flows (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodActualId TEXT NOT NULL,
            Type INTEGER NOT NULL,
            Name TEXT NOT NULL,
            Category TEXT NOT NULL,
            Date TEXT NOT NULL,
            Amount decimal NOT NULL,
            FOREIGN KEY (PeriodActualId) REFERENCES period_actuals (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_actual_flows_actual ON actual_flows (PeriodActualId);
        """,
        """
        CREATE TABLE IF NOT EXISTS actual_living_breakdowns (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodActualId TEXT NOT NULL,
            Category TEXT NOT NULL,
            Amount decimal NOT NULL,
            FOREIGN KEY (PeriodActualId) REFERENCES period_actuals (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_actual_living_breakdowns_actual ON actual_living_breakdowns (PeriodActualId);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_observations (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodPlanSnapshotId TEXT UNIQUE NOT NULL,
            ObservedOn TEXT NOT NULL,
            ObservedBalance decimal,
            ObservedLivingSpend decimal NOT NULL,
            Note TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL,
            UpdatedAtUtc TEXT NOT NULL,
            FOREIGN KEY (PeriodPlanSnapshotId) REFERENCES period_plan_snapshots (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_observations_plan ON period_observations (PeriodPlanSnapshotId);
        """,
        """
        CREATE TABLE IF NOT EXISTS period_observation_payments (
            Id TEXT PRIMARY KEY NOT NULL,
            PeriodObservationId TEXT NOT NULL,
            PeriodPlanPaymentLineId TEXT NOT NULL,
            Status INTEGER NOT NULL,
            ActualAmount decimal NOT NULL,
            ActualPaymentDate TEXT,
            Note TEXT NOT NULL,
            FOREIGN KEY (PeriodObservationId) REFERENCES period_observations (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_period_observation_payments_obs ON period_observation_payments (PeriodObservationId);
        """,
        """
        CREATE TABLE IF NOT EXISTS simulation_drafts (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS simulation_draft_conditions (
            Id TEXT PRIMARY KEY NOT NULL,
            DraftId TEXT NOT NULL,
            Position INTEGER NOT NULL,
            IsEnabled INTEGER NOT NULL,
            Type INTEGER NOT NULL,
            Name TEXT NOT NULL,
            Amount decimal NOT NULL,
            StartDate TEXT NOT NULL,
            PaymentCount INTEGER NOT NULL,
            FirstPaymentDate TEXT,
            CreditCardId TEXT,
            TotalRepaymentAmount decimal,
            RecurringIncomeId TEXT,
            ScenarioId TEXT NOT NULL,
            CardPaymentType INTEGER,
            AppliesToAllStatements INTEGER NOT NULL,
            LoanId TEXT,
            PrepaymentMode INTEGER,
            FOREIGN KEY (DraftId) REFERENCES simulation_drafts (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_simulation_draft_conditions_draft ON simulation_draft_conditions (DraftId, Position);
        """,
        """
        CREATE TABLE IF NOT EXISTS payment_reminder_responses (
            DueKey TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            DueDate TEXT NOT NULL,
            Amount decimal,
            Kind INTEGER NOT NULL,
            AnsweredAt TEXT NOT NULL,
            SnoozedUntil TEXT
        );
        """
    ];
}
