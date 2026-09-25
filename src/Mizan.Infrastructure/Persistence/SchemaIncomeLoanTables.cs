namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Gelirler, krediler, vadeli planlar, büyük harcamalar ve kullanıcı ayarları
/// için SQLite temiz v1 DDL tanımlarını üreten dahili yardımcı sınıf.
/// </summary>
internal static class SchemaIncomeLoanTables
{
    public static IReadOnlyList<string> Commands =>
    [
        """
        CREATE TABLE IF NOT EXISTS recurring_incomes (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            PaymentDay INTEGER NOT NULL,
            IsActive INTEGER NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS income_amount_histories (
            Id TEXT PRIMARY KEY NOT NULL,
            RecurringIncomeId TEXT NOT NULL,
            Amount decimal NOT NULL,
            EffectiveDate TEXT NOT NULL,
            Description TEXT NOT NULL,
            FOREIGN KEY (RecurringIncomeId) REFERENCES recurring_incomes (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_income_amount_histories_stream ON income_amount_histories (RecurringIncomeId, EffectiveDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS ad_hoc_incomes (
            Id TEXT PRIMARY KEY NOT NULL,
            Amount decimal NOT NULL,
            ExactDate TEXT NOT NULL,
            Description TEXT NOT NULL
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_ad_hoc_incomes_date ON ad_hoc_incomes (ExactDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS loans (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            Bank TEXT NOT NULL,
            MonthlyPayment decimal NOT NULL,
            PaymentDay INTEGER NOT NULL,
            NextPaymentDate TEXT NOT NULL,
            RemainingInstallmentCount INTEGER NOT NULL,
            FinalPaymentAmount decimal,
            RemainingDebt decimal,
            EarlyClosureAmount decimal,
            EarlyClosureAmountAsOf TEXT,
            Kind INTEGER NOT NULL,
            IsActive INTEGER NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS loan_prepayments (
            Id TEXT PRIMARY KEY NOT NULL,
            LoanId TEXT NOT NULL,
            Date TEXT NOT NULL,
            Mode INTEGER NOT NULL,
            PrincipalAmount decimal,
            FOREIGN KEY (LoanId) REFERENCES loans (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_loan_prepayments_loan ON loan_prepayments (LoanId, Date);
        """,
        """
        CREATE TABLE IF NOT EXISTS payment_plans (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            Kind INTEGER NOT NULL,
            OriginalAmount decimal,
            TotalRepaymentAmount decimal
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS payment_installments (
            Id TEXT PRIMARY KEY NOT NULL,
            PlanId TEXT NOT NULL,
            DueDate TEXT NOT NULL,
            Amount decimal NOT NULL,
            IsPaid INTEGER NOT NULL,
            FOREIGN KEY (PlanId) REFERENCES payment_plans (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_payment_installments_plan ON payment_installments (PlanId, DueDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS planned_large_expenses (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            Amount decimal NOT NULL,
            ExactDate TEXT NOT NULL,
            Note TEXT NOT NULL,
            Status INTEGER NOT NULL
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_planned_large_expenses_date ON planned_large_expenses (ExactDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS settings (
            Id INTEGER PRIMARY KEY NOT NULL,
            PeriodAnchorDay INTEGER NOT NULL,
            PeriodVariableExpenseAllowance decimal NOT NULL,
            ProjectionOpeningBalance decimal NOT NULL,
            ProjectionAnchorDate TEXT NOT NULL,
            CreditCardCarryInterestRate decimal NOT NULL,
            DeficitFinancingInterestRate decimal NOT NULL,
            PaymentReminderMode INTEGER NOT NULL
        );
        """
    ];
}
