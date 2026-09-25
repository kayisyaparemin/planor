using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Finansal durum, dönem planı ve plan revizyonu varlıklarının SQLite veritabanına
/// yazılmasını koordine eden dahili yardımcı sınıf.
/// </summary>
internal static class PeriodPlanEntityWriter
{
    public static void InsertSnapshot(SQLiteConnection conn, FinancialSnapshot snapshot) =>
        conn.Insert(new FinancialSnapshotEntity
        {
            Id = snapshot.Id.ToString(),
            SnapshotDate = snapshot.SnapshotDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ProjectionAnchorDate = snapshot.ProjectionAnchorDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            NextSettlementDate = snapshot.NextSettlementDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ProjectionOpeningBalance = snapshot.ProjectionOpeningBalance,
            IncomeDay = snapshot.Anchor.DayOfMonth,
            PreviousSnapshotId = snapshot.PreviousSnapshotId?.ToString(),
            Source = (int)snapshot.Source,
            IsCurrent = snapshot.IsCurrent,
            CreatedAtUtc = snapshot.CreatedAtUtc.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            Note = snapshot.Note
        });

    public static void InsertPlan(SQLiteConnection conn, PeriodPlanSnapshot plan)
    {
        var planId = plan.Id.ToString();
        conn.Insert(new PeriodPlanSnapshotEntity
        {
            Id = planId,
            FinancialSnapshotId = plan.FinancialSnapshotId.ToString(),
            PeriodStart = plan.PeriodStart.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PeriodEnd = plan.PeriodEnd.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            SettlementAvailableFrom = plan.SettlementAvailableFrom.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            CreatedAtUtc = plan.CreatedAtUtc.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            OpeningBalance = plan.OpeningBalance,
            PlannedIncome = plan.PlannedIncome,
            PlannedLoanPayments = plan.PlannedLoanPayments,
            PlannedCardPayments = plan.PlannedCardPayments,
            PlannedTemporaryPayments = plan.PlannedTemporaryPayments,
            PlannedInstallmentPayments = plan.PlannedInstallmentPayments,
            PlannedOtherScheduledPayments = plan.PlannedOtherScheduledPayments,
            PlannedMandatoryPayments = plan.PlannedMandatoryPayments,
            PlannedVariableExpenseAllowance = plan.PlannedVariableExpenseAllowance,
            PlannedLargeExpenses = plan.PlannedLargeExpenses,
            PlannedCardInterest = plan.PlannedCardInterest,
            PlannedDeficitInterest = plan.PlannedDeficitInterest,
            PlannedEndingBalance = plan.PlannedEndingBalance
        });

        InsertPlanPaymentLines(conn, planId, plan.PaymentLines);
        InsertPlanIncomeLines(conn, planId, plan.IncomeLines);
    }

    public static void InsertRevision(SQLiteConnection conn, PeriodPlanRevision revision)
    {
        var revId = revision.Id.ToString();
        conn.Insert(new PeriodPlanRevisionEntity
        {
            Id = revId,
            PeriodPlanSnapshotId = revision.PeriodPlanSnapshotId.ToString(),
            RevisionNumber = revision.RevisionNumber,
            CreatedAtUtc = revision.CreatedAtUtc.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            Trigger = revision.Trigger,
            PlannedIncome = revision.PlannedIncome,
            PlannedLoanPayments = revision.PlannedLoanPayments,
            PlannedCardPayments = revision.PlannedCardPayments,
            PlannedTemporaryPayments = revision.PlannedTemporaryPayments,
            PlannedInstallmentPayments = revision.PlannedInstallmentPayments,
            PlannedOtherScheduledPayments = revision.PlannedOtherScheduledPayments,
            PlannedMandatoryPayments = revision.PlannedMandatoryPayments,
            PlannedVariableExpenseAllowance = revision.PlannedVariableExpenseAllowance,
            PlannedLargeExpenses = revision.PlannedLargeExpenses,
            PlannedCardInterest = revision.PlannedCardInterest,
            PlannedDeficitInterest = revision.PlannedDeficitInterest,
            PlannedEndingBalance = revision.PlannedEndingBalance,
            Note = revision.Note
        });

        InsertRevisionPaymentLines(conn, revId, revision.PaymentLines);
        InsertRevisionIncomeLines(conn, revId, revision.IncomeLines);
    }

    private static void InsertPlanPaymentLines(SQLiteConnection conn, string planId, IReadOnlyList<PeriodPlanPaymentLine> lines)
    {
        foreach (var p in lines)
        {
            conn.Insert(new PeriodPlanPaymentLineEntity
            {
                Id = p.Id.ToString(),
                PeriodPlanSnapshotId = planId,
                SourceEntityId = p.SourceEntityId.ToString(),
                SourceType = (int)p.SourceType,
                Name = p.Name,
                PlannedDate = p.PlannedDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PlannedAmount = p.PlannedAmount,
                IsEstimate = p.IsEstimate,
                Detail = p.Detail
            });
        }
    }

    private static void InsertPlanIncomeLines(SQLiteConnection conn, string planId, IReadOnlyList<PeriodPlanIncomeLine> lines)
    {
        foreach (var i in lines)
        {
            conn.Insert(new PeriodPlanIncomeLineEntity
            {
                Id = i.Id.ToString(),
                PeriodPlanSnapshotId = planId,
                SourceType = (int)i.SourceType,
                RecurringIncomeId = i.RecurringIncomeId?.ToString(),
                AdHocIncomeId = i.AdHocIncomeId?.ToString(),
                Name = i.Name,
                PlannedDate = i.PlannedDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PlannedAmount = i.PlannedAmount
            });
        }
    }

    private static void InsertRevisionPaymentLines(SQLiteConnection conn, string revId, IReadOnlyList<PeriodPlanPaymentLine> lines)
    {
        foreach (var p in lines)
        {
            conn.Insert(new PeriodPlanRevisionPaymentLineEntity
            {
                Id = p.Id.ToString(),
                PeriodPlanRevisionId = revId,
                SourceEntityId = p.SourceEntityId.ToString(),
                SourceType = (int)p.SourceType,
                Name = p.Name,
                PlannedDate = p.PlannedDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PlannedAmount = p.PlannedAmount,
                IsEstimate = p.IsEstimate,
                Detail = p.Detail
            });
        }
    }

    private static void InsertRevisionIncomeLines(SQLiteConnection conn, string revId, IReadOnlyList<PeriodPlanIncomeLine> lines)
    {
        foreach (var i in lines)
        {
            conn.Insert(new PeriodPlanRevisionIncomeLineEntity
            {
                Id = i.Id.ToString(),
                PeriodPlanRevisionId = revId,
                SourceType = (int)i.SourceType,
                RecurringIncomeId = i.RecurringIncomeId?.ToString(),
                AdHocIncomeId = i.AdHocIncomeId?.ToString(),
                Name = i.Name,
                PlannedDate = i.PlannedDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PlannedAmount = i.PlannedAmount
            });
        }
    }
}
