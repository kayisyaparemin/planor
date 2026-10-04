namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski anlık görüntülerin, dondurulmuş dönem planlarının ve revizyonlarının yeni şemaya çevrilmesi.
/// Adlar S35'e göre döner (Savings → Balance, Review → Settlement, LivingBudget → VariableExpenseAllowance).
/// Eski plan geliri yalnız toplam olarak saklıyordu (S31); gelir akışı varsa her plana ve revizyona toplamı
/// taşıyan tek satır türetilir: tarih strateji 0'da dönem başı, 1'de dönem sonu, böylece satır toplamı plan
/// gelirine kuruşu kuruşuna eşit kalır (I28). Gelir akışı yoksa ya da gelir sıfırsa satır eklenmez.
/// </summary>
internal static class LegacyPlanStatements
{
    private const string PlanIncomeLineColumns =
        "SourceType, RecurringIncomeId, AdHocIncomeId, Name, PlannedDate, PlannedAmount";

    public static IReadOnlyList<string> Commands { get; } =
    [
        """
        INSERT INTO financial_snapshots (Id, SnapshotDate, ProjectionAnchorDate, NextSettlementDate,
                                         ProjectionOpeningBalance, IncomeDay, PreviousSnapshotId, Source,
                                         IsCurrent, CreatedAtUtc, Note)
        SELECT Id, SnapshotDate, ProjectionAnchorDate, NextReviewDate, ProjectionStartingSavings, SalaryDay,
               PreviousSnapshotId, Source, IsCurrent, CreatedAtUtc, Note
        FROM old.financial_snapshots;
        """,
        """
        INSERT INTO period_plan_snapshots (Id, FinancialSnapshotId, PeriodStart, PeriodEnd, SettlementAvailableFrom,
                                           CreatedAtUtc, OpeningBalance, PlannedIncome, PlannedLoanPayments,
                                           PlannedCardPayments, PlannedTemporaryPayments, PlannedInstallmentPayments,
                                           PlannedOtherScheduledPayments, PlannedMandatoryPayments,
                                           PlannedVariableExpenseAllowance, PlannedLargeExpenses, PlannedCardInterest,
                                           PlannedDeficitInterest, PlannedEndingBalance)
        SELECT p.Id, p.FinancialSnapshotId, p.PeriodStart, p.PeriodEnd, p.ReviewAvailableFrom,
               p.CreatedAtUtc, p.OpeningSavings, p.PlannedIncome, p.PlannedLoanPayments,
               p.PlannedCardPayments, p.PlannedTemporaryPayments, p.PlannedInstallmentPayments,
               p.PlannedOtherScheduledPayments, p.PlannedMandatoryPayments,
               p.PlannedLivingBudget, p.PlannedLargeExpenses, p.PlannedCardInterest,
               p.PlannedDeficitInterest, p.PlannedEndingSavings
        FROM old.period_plan_snapshots p
        WHERE EXISTS (SELECT 1 FROM old.financial_snapshots f WHERE f.Id = p.FinancialSnapshotId);
        """,
        """
        INSERT INTO period_plan_payment_lines (Id, PeriodPlanSnapshotId, SourceEntityId, SourceType, Name,
                                               PlannedDate, PlannedAmount, IsEstimate, Detail)
        SELECT l.Id, l.PeriodPlanSnapshotId, l.SourceEntityId, l.SourceType, l.Name,
               l.PlannedDate, l.PlannedAmount, l.IsEstimate, l.Detail
        FROM old.period_plan_payment_lines l
        WHERE EXISTS (SELECT 1 FROM main.period_plan_snapshots p WHERE p.Id = l.PeriodPlanSnapshotId);
        """,
        """
        INSERT INTO period_plan_revisions (Id, PeriodPlanSnapshotId, RevisionNumber, CreatedAtUtc, "Trigger",
                                           PlannedIncome, PlannedLoanPayments, PlannedCardPayments,
                                           PlannedTemporaryPayments, PlannedInstallmentPayments,
                                           PlannedOtherScheduledPayments, PlannedMandatoryPayments,
                                           PlannedVariableExpenseAllowance, PlannedLargeExpenses, PlannedCardInterest,
                                           PlannedDeficitInterest, PlannedEndingBalance, Note)
        SELECT r.Id, r.PeriodPlanSnapshotId, r.RevisionNumber, r.CreatedAtUtc, r."Trigger",
               r.PlannedIncome, r.PlannedLoanPayments, r.PlannedCardPayments,
               r.PlannedTemporaryPayments, r.PlannedInstallmentPayments,
               r.PlannedOtherScheduledPayments, r.PlannedMandatoryPayments,
               r.PlannedLivingBudget, r.PlannedLargeExpenses, r.PlannedCardInterest,
               r.PlannedDeficitInterest, r.PlannedEndingSavings, r.Note
        FROM old.period_plan_revisions r
        WHERE EXISTS (SELECT 1 FROM main.period_plan_snapshots p WHERE p.Id = r.PeriodPlanSnapshotId);
        """,
        """
        INSERT INTO period_plan_revision_payment_lines (Id, PeriodPlanRevisionId, SourceEntityId, SourceType, Name,
                                                        PlannedDate, PlannedAmount, IsEstimate, Detail)
        SELECT l.Id, l.PeriodPlanRevisionId, l.SourceEntityId, l.SourceType, l.Name,
               l.PlannedDate, l.PlannedAmount, l.IsEstimate, l.Detail
        FROM old.period_plan_revision_payment_lines l
        WHERE EXISTS (SELECT 1 FROM main.period_plan_revisions r WHERE r.Id = l.PeriodPlanRevisionId);
        """,
        $"""
        INSERT INTO period_plan_income_lines (Id, PeriodPlanSnapshotId, {PlanIncomeLineColumns})
        SELECT {LegacySchemaV17.NewGuidExpression}, p.Id, 1, (SELECT Id FROM temp.income_stream), NULL,
               (SELECT Name FROM temp.income_stream),
               CASE WHEN p.StrategyUsed = 1 THEN p.PeriodEnd ELSE p.PeriodStart END,
               p.PlannedIncome
        FROM old.period_plan_snapshots p
        WHERE p.PlannedIncome > 0
          AND EXISTS (SELECT 1 FROM temp.income_stream)
          AND EXISTS (SELECT 1 FROM main.period_plan_snapshots m WHERE m.Id = p.Id);
        """,
        $"""
        INSERT INTO period_plan_revision_income_lines (Id, PeriodPlanRevisionId, {PlanIncomeLineColumns})
        SELECT {LegacySchemaV17.NewGuidExpression}, r.Id, 1, (SELECT Id FROM temp.income_stream), NULL,
               (SELECT Name FROM temp.income_stream),
               CASE WHEN r.StrategyUsed = 1 THEN p.PeriodEnd ELSE p.PeriodStart END,
               r.PlannedIncome
        FROM old.period_plan_revisions r
        JOIN old.period_plan_snapshots p ON p.Id = r.PeriodPlanSnapshotId
        WHERE r.PlannedIncome > 0
          AND EXISTS (SELECT 1 FROM temp.income_stream)
          AND EXISTS (SELECT 1 FROM main.period_plan_revisions m WHERE m.Id = r.Id);
        """
    ];
}
