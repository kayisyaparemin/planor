using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kapanan döneme ait kesinleşmiş fiilî durumun ve bağlı satırların SQLite veritabanına
/// yazılmasını koordine eden dahili yardımcı sınıf.
/// </summary>
internal static class PeriodActualEntityWriter
{
    public static void InsertActual(SQLiteConnection conn, PeriodActual actual)
    {
        var actId = actual.Id.ToString();
        conn.Insert(new PeriodActualEntity
        {
            Id = actId,
            PeriodPlanSnapshotId = actual.PeriodPlanSnapshotId.ToString(),
            SourceFinancialSnapshotId = actual.SourceFinancialSnapshotId.ToString(),
            ResultFinancialSnapshotId = actual.ResultFinancialSnapshotId.ToString(),
            PeriodStart = actual.PeriodStart.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PeriodEnd = actual.PeriodEnd.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            FinalizedAtUtc = actual.FinalizedAtUtc.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            ActualIncome = actual.ActualIncome,
            ActualLoanPayments = actual.ActualLoanPayments,
            ActualCardPayments = actual.ActualCardPayments,
            ActualTemporaryPayments = actual.ActualTemporaryPayments,
            ActualInstallmentPayments = actual.ActualInstallmentPayments,
            ActualOtherScheduledPayments = actual.ActualOtherScheduledPayments,
            ActualLargeExpenses = actual.ActualLargeExpenses,
            ActualMandatoryPayments = actual.ActualMandatoryPayments,
            ActualLivingSpend = actual.ActualLivingSpend,
            ActualInterest = actual.ActualInterest,
            UnplannedIncome = actual.UnplannedIncome,
            UnplannedPayments = actual.UnplannedPayments,
            DerivedEndingBalance = actual.DerivedEndingBalance,
            ConfirmedEndingBalance = actual.ConfirmedEndingBalance,
            ReconciliationAdjustment = actual.ReconciliationAdjustment,
            ComparisonSummary = actual.ComparisonSummary,
            Note = actual.Note
        });

        InsertActualPayments(conn, actId, actual.Payments);
        InsertActualFlows(conn, actId, actual.Flows);
        InsertActualLivingBreakdowns(conn, actId, actual.LivingBreakdown);
    }

    private static void InsertActualPayments(SQLiteConnection conn, string actId, IReadOnlyList<ActualPayment> payments)
    {
        foreach (var p in payments)
        {
            conn.Insert(new ActualPaymentEntity
            {
                Id = p.Id.ToString(),
                PeriodActualId = actId,
                PeriodPlanPaymentLineId = p.PeriodPlanPaymentLineId.ToString(),
                SourceEntityId = p.SourceEntityId.ToString(),
                SourceType = (int)p.SourceType,
                Name = p.Name,
                PlannedDate = p.PlannedDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PlannedAmount = p.PlannedAmount,
                ActualPaymentDate = p.ActualPaymentDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                ActualAmount = p.ActualAmount,
                Status = (int)p.Status,
                Note = p.Note
            });
        }
    }

    private static void InsertActualFlows(SQLiteConnection conn, string actId, IReadOnlyList<ActualFlow> flows)
    {
        foreach (var f in flows)
        {
            conn.Insert(new ActualFlowEntity
            {
                Id = f.Id.ToString(),
                PeriodActualId = actId,
                Type = (int)f.Type,
                Name = f.Name,
                Category = f.Category,
                Date = f.Date.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                Amount = f.Amount
            });
        }
    }

    private static void InsertActualLivingBreakdowns(SQLiteConnection conn, string actId, IReadOnlyList<ActualLivingBreakdown> breakdowns)
    {
        foreach (var b in breakdowns)
        {
            conn.Insert(new ActualLivingBreakdownEntity
            {
                Id = b.Id.ToString(),
                PeriodActualId = actId,
                Category = b.Category,
                Amount = b.Amount
            });
        }
    }
}
