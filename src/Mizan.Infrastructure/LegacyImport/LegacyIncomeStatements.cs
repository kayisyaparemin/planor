namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski ayarların ve gelirlerin yeni şemaya çevrilmesi. Eski <c>salary_schedule</c> tek bir gelirin etkin
/// tarihli geçmişidir (S2); tek düzenli gelir akışı kurulur, ödeme günü eski global gelir günüdür (S3).
/// Akış geçici <c>income_stream</c> tablosunda tutulur ki plan gelir satırları (S31) aynı kimliği göstersin.
/// </summary>
internal static class LegacyIncomeStatements
{
    public static IReadOnlyList<string> Commands { get; } =
    [
        // Gelir günü dönem çapası (S1), aylık yaşam bütçesi dönem yaşam gideri payı olur.
        """
        INSERT INTO settings (Id, PeriodAnchorDay, PeriodVariableExpenseAllowance, ProjectionOpeningBalance,
                              ProjectionAnchorDate, CreditCardCarryInterestRate, DeficitFinancingInterestRate,
                              PaymentReminderMode)
        SELECT s.Id, s.SalaryDay, s.MonthlyLivingBudget, s.ProjectionStartingSavings,
               COALESCE(s.ProjectionAnchorDate,
                        (SELECT f.ProjectionAnchorDate FROM old.financial_snapshots f WHERE f.IsCurrent = 1 LIMIT 1)),
               s.CreditCardCarryInterestRate, s.DeficitFinancingInterestRate, s.PaymentReminderMode
        FROM old.settings s
        ORDER BY s.Id
        LIMIT 1;
        """,
        $"""
        CREATE TEMP TABLE income_stream AS
        SELECT {LegacySchemaV17.NewGuidExpression} AS Id,
               '{LegacySchemaV17.IncomeStreamName}' AS Name,
               (SELECT SalaryDay FROM old.settings ORDER BY Id LIMIT 1) AS PaymentDay
        WHERE EXISTS (SELECT 1 FROM old.salary_schedule);
        """,
        """
        INSERT INTO recurring_incomes (Id, Name, PaymentDay, IsActive)
        SELECT Id, Name, PaymentDay, 1 FROM temp.income_stream;
        """,
        """
        INSERT INTO income_amount_histories (Id, RecurringIncomeId, Amount, EffectiveDate, Description)
        SELECT s.Id, (SELECT Id FROM temp.income_stream), s.NetAmount, s.EffectiveFrom,
               CASE WHEN s.EffectiveFrom = (SELECT min(EffectiveFrom) FROM old.salary_schedule)
                    THEN 'Başlangıç Tutarı'
                    ELSE COALESCE(NULLIF(s.Note, ''), 'Tutar değişikliği') END
        FROM old.salary_schedule s;
        """,
        """
        INSERT INTO ad_hoc_incomes (Id, Amount, ExactDate, Description)
        SELECT Id, Amount, ExactDate, Description FROM old.other_incomes;
        """
    ];
}
