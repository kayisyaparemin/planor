namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Kredi kartları, ekstreler, taksitli işlemler ve ödeme tercihleri
/// için SQLite temiz v1 DDL tanımlarını üreten dahili yardımcı sınıf.
/// </summary>
internal static class SchemaCardTables
{
    public static IReadOnlyList<string> Commands =>
    [
        """
        CREATE TABLE IF NOT EXISTS credit_cards (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            Bank TEXT NOT NULL,
            "Limit" decimal NOT NULL,
            CarriedBalance decimal NOT NULL,
            UnbilledSpending decimal NOT NULL,
            BalanceAsOfDate TEXT NOT NULL,
            StatementClosingDay INTEGER NOT NULL,
            PaymentDueDay INTEGER NOT NULL,
            MinimumPaymentRate decimal NOT NULL,
            PaymentStrategy INTEGER NOT NULL,
            FixedPaymentAmount decimal,
            ProjectionFallbackStrategy INTEGER NOT NULL,
            ProjectionFallbackFixedAmount decimal,
            KnownNextStatementDate TEXT,
            KnownNextDueDate TEXT,
            IsActive INTEGER NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS card_installments (
            Id TEXT PRIMARY KEY NOT NULL,
            CreditCardId TEXT NOT NULL,
            Description TEXT NOT NULL,
            PostingDate TEXT NOT NULL,
            Amount decimal NOT NULL,
            FOREIGN KEY (CreditCardId) REFERENCES credit_cards (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_card_installments_card ON card_installments (CreditCardId, PostingDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS credit_card_statements (
            Id TEXT PRIMARY KEY NOT NULL,
            CreditCardId TEXT NOT NULL,
            StatementDate TEXT NOT NULL,
            DueDate TEXT NOT NULL,
            StatementAmount decimal NOT NULL,
            MinimumPaymentAmount decimal NOT NULL,
            NextStatementDate TEXT,
            NextDueDate TEXT,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            CurrentPaymentMode INTEGER NOT NULL,
            CurrentPaymentCustomAmount decimal,
            FOREIGN KEY (CreditCardId) REFERENCES credit_cards (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_credit_card_statements_card ON credit_card_statements (CreditCardId, StatementDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS credit_card_payment_plans (
            Id TEXT PRIMARY KEY NOT NULL,
            CreditCardId TEXT NOT NULL,
            DueDate TEXT NOT NULL,
            PaymentType INTEGER NOT NULL,
            Amount decimal,
            FOREIGN KEY (CreditCardId) REFERENCES credit_cards (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_credit_card_payment_plans_card ON credit_card_payment_plans (CreditCardId, DueDate);
        """,
        """
        CREATE TABLE IF NOT EXISTS credit_card_payment_preferences (
            Id TEXT PRIMARY KEY NOT NULL,
            CreditCardId TEXT NOT NULL,
            Mode INTEGER NOT NULL,
            CustomAmount decimal,
            EffectiveFromStatementDate TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            Note TEXT NOT NULL,
            FOREIGN KEY (CreditCardId) REFERENCES credit_cards (Id) ON DELETE CASCADE
        );
        """,
        """
        CREATE INDEX IF NOT EXISTS ix_credit_card_payment_preferences_card ON credit_card_payment_preferences (CreditCardId, EffectiveFromStatementDate);
        """
    ];
}
