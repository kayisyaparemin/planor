namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski kapanmış dönemlerin (gerçekleşenler) ve açık dönemin gözleminin yeni şemaya çevrilmesi. Gözlem ve
/// ödeme işareti v1 şekliyle yazılır; güncel şekle (S68) normal şema yükseltmesi getirir. Eski
/// <c>period_observation_flows</c> v2'de karşılığı olmadığı için taşınmaz (S20).
/// </summary>
internal static class LegacyActualStatements
{
    public static IReadOnlyList<string> Commands { get; } =
    [
        """
        INSERT INTO period_actuals (Id, PeriodPlanSnapshotId, SourceFinancialSnapshotId, ResultFinancialSnapshotId,
                                    PeriodStart, PeriodEnd, FinalizedAtUtc, ActualIncome, ActualLoanPayments,
                                    ActualCardPayments, ActualTemporaryPayments, ActualInstallmentPayments,
                                    ActualOtherScheduledPayments, ActualLargeExpenses, ActualMandatoryPayments,
                                    ActualLivingSpend, ActualInterest, UnplannedIncome, UnplannedPayments,
                                    DerivedEndingBalance, ConfirmedEndingBalance, ReconciliationAdjustment,
                                    ComparisonSummary, Note)
        SELECT a.Id, a.PeriodPlanSnapshotId, a.SourceFinancialSnapshotId, a.ResultFinancialSnapshotId,
               a.PeriodStart, a.PeriodEnd, a.FinalizedAtUtc, a.ActualIncome, a.ActualLoanPayments,
               a.ActualCardPayments, a.ActualTemporaryPayments, a.ActualInstallmentPayments,
               a.ActualOtherScheduledPayments, a.ActualLargeExpenses, a.ActualMandatoryPayments,
               a.ActualLivingSpend, a.ActualInterest, a.UnplannedIncome, a.UnplannedPayments,
               a.DerivedEndingSavings, a.ConfirmedEndingSavings, a.ReconciliationAdjustment,
               a.ComparisonSummary, a.Note
        FROM old.period_actuals a
        WHERE EXISTS (SELECT 1 FROM main.period_plan_snapshots p WHERE p.Id = a.PeriodPlanSnapshotId);
        """,
        """
        INSERT INTO actual_payments (Id, PeriodActualId, PeriodPlanPaymentLineId, SourceEntityId, SourceType, Name,
                                     PlannedDate, PlannedAmount, ActualPaymentDate, ActualAmount, Status, Note)
        SELECT x.Id, x.PeriodActualId, x.PeriodPlanPaymentLineId, x.SourceEntityId, x.SourceType, x.Name,
               x.PlannedDate, x.PlannedAmount, x.ActualPaymentDate, x.ActualAmount, x.Status, x.Note
        FROM old.actual_payments x WHERE EXISTS (SELECT 1 FROM main.period_actuals a WHERE a.Id = x.PeriodActualId);
        """,
        """
        INSERT INTO actual_flows (Id, PeriodActualId, Type, Name, Category, Date, Amount)
        SELECT x.Id, x.PeriodActualId, x.Type, x.Name, x.Category, x.Date, x.Amount
        FROM old.actual_flows x WHERE EXISTS (SELECT 1 FROM main.period_actuals a WHERE a.Id = x.PeriodActualId);
        """,
        """
        INSERT INTO actual_living_breakdowns (Id, PeriodActualId, Category, Amount)
        SELECT x.Id, x.PeriodActualId, x.Category, x.Amount
        FROM old.actual_living_breakdowns x
        WHERE EXISTS (SELECT 1 FROM main.period_actuals a WHERE a.Id = x.PeriodActualId);
        """,
        """
        INSERT INTO period_observations (Id, PeriodPlanSnapshotId, ObservedOn, ObservedBalance, ObservedLivingSpend,
                                         Note, CreatedAtUtc, UpdatedAtUtc)
        SELECT o.Id, o.PeriodPlanSnapshotId, o.ObservedOn, o.ObservedBalance, o.ObservedLivingSpend,
               o.Note, o.CreatedAtUtc, o.UpdatedAtUtc
        FROM old.period_observations o
        WHERE EXISTS (SELECT 1 FROM main.period_plan_snapshots p WHERE p.Id = o.PeriodPlanSnapshotId);
        """,
        """
        INSERT INTO period_observation_payments (Id, PeriodObservationId, PeriodPlanPaymentLineId, Status,
                                                 ActualAmount, ActualPaymentDate, Note)
        SELECT x.Id, x.PeriodObservationId, x.PeriodPlanPaymentLineId, x.Status, x.ActualAmount, x.ActualPaymentDate,
               x.Note
        FROM old.period_observation_payments x
        WHERE EXISTS (SELECT 1 FROM main.period_observations o WHERE o.Id = x.PeriodObservationId);
        """
    ];
}
