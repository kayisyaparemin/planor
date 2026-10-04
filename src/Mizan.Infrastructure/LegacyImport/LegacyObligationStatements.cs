namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski kredi, kart, ödeme planı ve hatırlatıcı kayıtlarının yeni şemaya çevrilmesi. Eski şemada ilişkiler
/// zorlanmadığı için karşı kaydı olmayan çocuk satırlar (yetim) <c>EXISTS</c> ile elenir. İki kolon yalan
/// söylüyordu ve doğru adlarına taşınır: kredinin <c>StartDate</c>'i sonraki ödeme tarihini, kart
/// harcamasının <c>DueDate</c>'i işlem tarihini taşıyordu.
/// </summary>
internal static class LegacyObligationStatements
{
    public static IReadOnlyList<string> Commands { get; } =
    [
        """
        INSERT INTO loans (Id, Name, Bank, MonthlyPayment, PaymentDay, NextPaymentDate, RemainingInstallmentCount,
                           FinalPaymentAmount, RemainingDebt, EarlyClosureAmount, EarlyClosureAmountAsOf, Kind, IsActive)
        SELECT Id, Name, Bank, MonthlyInstallment, PaymentDay, StartDate, COALESCE(InstallmentCount, 0),
               FinalPaymentAmount, RemainingDebt, EarlyClosureAmount, EarlyClosureAmountAsOf, Kind, IsActive
        FROM old.loans;
        """,
        """
        INSERT INTO loan_prepayments (Id, LoanId, Date, Mode, PrincipalAmount)
        SELECT p.Id, p.LoanId, p.Date, p.Mode, p.PrincipalAmount
        FROM old.loan_prepayments p WHERE EXISTS (SELECT 1 FROM old.loans l WHERE l.Id = p.LoanId);
        """,
        """
        INSERT INTO payment_plans (Id, Name, Kind, OriginalAmount, TotalRepaymentAmount)
        SELECT Id, Name, Kind, OriginalAmount, TotalRepaymentAmount FROM old.payment_plans;
        """,
        """
        INSERT INTO payment_installments (Id, PlanId, DueDate, Amount, IsPaid)
        SELECT i.Id, i.PlanId, i.DueDate, i.Amount, i.IsPaid
        FROM old.payment_installments i WHERE EXISTS (SELECT 1 FROM old.payment_plans p WHERE p.Id = i.PlanId);
        """,
        // Eski şemada kartın etkin/pasif bilgisi yoktu; her kart etkindi.
        """
        INSERT INTO credit_cards (Id, Name, Bank, "Limit", CarriedBalance, UnbilledSpending, BalanceAsOfDate,
                                  StatementClosingDay, PaymentDueDay, MinimumPaymentRate, PaymentStrategy,
                                  FixedPaymentAmount, ProjectionFallbackStrategy, ProjectionFallbackFixedAmount,
                                  KnownNextStatementDate, KnownNextDueDate, IsActive)
        SELECT Id, Name, Bank, "Limit", CarriedBalance, UnbilledSpending, BalanceAsOfDate,
               StatementClosingDay, PaymentDueDay, MinimumPaymentRate, PaymentStrategy,
               FixedPaymentAmount, ProjectionFallbackStrategy, ProjectionFallbackFixedAmount,
               KnownNextStatementDate, KnownNextDueDate, 1
        FROM old.credit_cards;
        """,
        """
        INSERT INTO card_installments (Id, CreditCardId, Description, PostingDate, Amount)
        SELECT i.Id, i.CreditCardId, i.Description, i.DueDate, i.Amount
        FROM old.card_installments i WHERE EXISTS (SELECT 1 FROM old.credit_cards c WHERE c.Id = i.CreditCardId);
        """,
        """
        INSERT INTO credit_card_statements (Id, CreditCardId, StatementDate, DueDate, StatementAmount,
                                            MinimumPaymentAmount, NextStatementDate, NextDueDate, CreatedAt,
                                            UpdatedAt, CurrentPaymentMode, CurrentPaymentCustomAmount)
        SELECT s.Id, s.CreditCardId, s.StatementDate, s.DueDate, s.StatementAmount,
               s.MinimumPaymentAmount, s.NextStatementDate, s.NextDueDate, s.CreatedAt,
               s.UpdatedAt, s.CurrentPaymentMode, s.CurrentPaymentCustomAmount
        FROM old.credit_card_statements s WHERE EXISTS (SELECT 1 FROM old.credit_cards c WHERE c.Id = s.CreditCardId);
        """,
        """
        INSERT INTO credit_card_payment_plans (Id, CreditCardId, DueDate, PaymentType, Amount)
        SELECT p.Id, p.CreditCardId, p.DueDate, p.PaymentType, p.Amount
        FROM old.credit_card_payment_plans p WHERE EXISTS (SELECT 1 FROM old.credit_cards c WHERE c.Id = p.CreditCardId);
        """,
        """
        INSERT INTO credit_card_payment_preferences (Id, CreditCardId, Mode, CustomAmount,
                                                     EffectiveFromStatementDate, CreatedAt, Note)
        SELECT p.Id, p.CreditCardId, p.Mode, p.CustomAmount, p.EffectiveFromStatementDate, p.CreatedAt, p.Note
        FROM old.credit_card_payment_preferences p
        WHERE EXISTS (SELECT 1 FROM old.credit_cards c WHERE c.Id = p.CreditCardId);
        """,
        """
        INSERT INTO planned_large_expenses (Id, Name, Amount, ExactDate, Note, Status)
        SELECT Id, Name, Amount, ExactDate, Note, Status FROM old.planned_large_expenses;
        """,
        """
        INSERT INTO payment_reminder_responses (DueKey, Name, DueDate, Amount, Kind, AnsweredAt, SnoozedUntil)
        SELECT DueKey, Name, DueDate, Amount, Kind, AnsweredAt, SnoozedUntil FROM old.payment_reminder_responses;
        """
    ];
}
