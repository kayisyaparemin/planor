using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Finansal anlık görüntüler, dönem planları ve revizyonların SQLite entity modellerinden
/// Domain modellerine dönüştürülmesini sağlayan dahili haritalayıcı.
/// </summary>
internal static class PeriodPlanEntityMapper
{
    public static FinancialSnapshot MapSnapshot(FinancialSnapshotEntity e) =>
        new()
        {
            Id = Guid.Parse(e.Id),
            SnapshotDate = DateOnly.ParseExact(e.SnapshotDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ProjectionAnchorDate = DateOnly.ParseExact(e.ProjectionAnchorDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            NextSettlementDate = DateOnly.ParseExact(e.NextSettlementDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ProjectionOpeningBalance = e.ProjectionOpeningBalance,
            Anchor = new PeriodAnchor(e.IncomeDay),
            PreviousSnapshotId = string.IsNullOrWhiteSpace(e.PreviousSnapshotId) ? null : Guid.Parse(e.PreviousSnapshotId),
            Source = (FinancialSnapshotSource)e.Source,
            IsCurrent = e.IsCurrent,
            CreatedAtUtc = DateTimeOffset.Parse(e.CreatedAtUtc, CultureInfo.InvariantCulture),
            Note = e.Note
        };

    public static PeriodPlanSnapshot MapPlan(
        PeriodPlanSnapshotEntity e,
        IEnumerable<PeriodPlanPaymentLineEntity> payments,
        IEnumerable<PeriodPlanIncomeLineEntity> incomes) =>
        new()
        {
            Id = Guid.Parse(e.Id),
            FinancialSnapshotId = Guid.Parse(e.FinancialSnapshotId),
            PeriodStart = DateOnly.ParseExact(e.PeriodStart, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PeriodEnd = DateOnly.ParseExact(e.PeriodEnd, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            SettlementAvailableFrom = DateOnly.ParseExact(e.SettlementAvailableFrom, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            CreatedAtUtc = DateTimeOffset.Parse(e.CreatedAtUtc, CultureInfo.InvariantCulture),
            OpeningBalance = e.OpeningBalance,
            PlannedIncome = e.PlannedIncome,
            PlannedLoanPayments = e.PlannedLoanPayments,
            PlannedCardPayments = e.PlannedCardPayments,
            PlannedTemporaryPayments = e.PlannedTemporaryPayments,
            PlannedInstallmentPayments = e.PlannedInstallmentPayments,
            PlannedOtherScheduledPayments = e.PlannedOtherScheduledPayments,
            PlannedMandatoryPayments = e.PlannedMandatoryPayments,
            PlannedVariableExpenseAllowance = e.PlannedVariableExpenseAllowance,
            PlannedLargeExpenses = e.PlannedLargeExpenses,
            PlannedCardInterest = e.PlannedCardInterest,
            PlannedDeficitInterest = e.PlannedDeficitInterest,
            PlannedEndingBalance = e.PlannedEndingBalance,
            PaymentLines = payments.Select(MapPaymentLine).ToArray(),
            IncomeLines = incomes.Select(MapIncomeLine).ToArray()
        };

    public static PeriodPlanRevision MapRevision(
        PeriodPlanRevisionEntity e,
        IEnumerable<PeriodPlanRevisionPaymentLineEntity> payments,
        IEnumerable<PeriodPlanRevisionIncomeLineEntity> incomes) =>
        new()
        {
            Id = Guid.Parse(e.Id),
            PeriodPlanSnapshotId = Guid.Parse(e.PeriodPlanSnapshotId),
            RevisionNumber = e.RevisionNumber,
            CreatedAtUtc = DateTimeOffset.Parse(e.CreatedAtUtc, CultureInfo.InvariantCulture),
            Trigger = e.Trigger,
            PlannedIncome = e.PlannedIncome,
            PlannedLoanPayments = e.PlannedLoanPayments,
            PlannedCardPayments = e.PlannedCardPayments,
            PlannedTemporaryPayments = e.PlannedTemporaryPayments,
            PlannedInstallmentPayments = e.PlannedInstallmentPayments,
            PlannedOtherScheduledPayments = e.PlannedOtherScheduledPayments,
            PlannedMandatoryPayments = e.PlannedMandatoryPayments,
            PlannedVariableExpenseAllowance = e.PlannedVariableExpenseAllowance,
            PlannedLargeExpenses = e.PlannedLargeExpenses,
            PlannedCardInterest = e.PlannedCardInterest,
            PlannedDeficitInterest = e.PlannedDeficitInterest,
            PlannedEndingBalance = e.PlannedEndingBalance,
            Note = e.Note,
            PaymentLines = payments.Select(MapRevisionPaymentLine).ToArray(),
            IncomeLines = incomes.Select(MapRevisionIncomeLine).ToArray()
        };

    private static PeriodPlanPaymentLine MapPaymentLine(PeriodPlanPaymentLineEntity p) =>
        new()
        {
            Id = Guid.Parse(p.Id),
            PeriodPlanSnapshotId = Guid.Parse(p.PeriodPlanSnapshotId),
            SourceEntityId = Guid.Parse(p.SourceEntityId),
            SourceType = (PlanPaymentSourceType)p.SourceType,
            Name = p.Name,
            PlannedDate = DateOnly.ParseExact(p.PlannedDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PlannedAmount = p.PlannedAmount,
            IsEstimate = p.IsEstimate,
            Detail = p.Detail
        };

    private static PeriodPlanIncomeLine MapIncomeLine(PeriodPlanIncomeLineEntity i) =>
        new()
        {
            Id = Guid.Parse(i.Id),
            PeriodPlanSnapshotId = Guid.Parse(i.PeriodPlanSnapshotId),
            SourceType = (IncomeSourceType)i.SourceType,
            RecurringIncomeId = string.IsNullOrWhiteSpace(i.RecurringIncomeId) ? null : Guid.Parse(i.RecurringIncomeId),
            AdHocIncomeId = string.IsNullOrWhiteSpace(i.AdHocIncomeId) ? null : Guid.Parse(i.AdHocIncomeId),
            Name = i.Name,
            PlannedDate = DateOnly.ParseExact(i.PlannedDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PlannedAmount = i.PlannedAmount
        };

    private static PeriodPlanPaymentLine MapRevisionPaymentLine(PeriodPlanRevisionPaymentLineEntity p) =>
        new()
        {
            Id = Guid.Parse(p.Id),
            PeriodPlanSnapshotId = Guid.Parse(p.PeriodPlanRevisionId),
            SourceEntityId = Guid.Parse(p.SourceEntityId),
            SourceType = (PlanPaymentSourceType)p.SourceType,
            Name = p.Name,
            PlannedDate = DateOnly.ParseExact(p.PlannedDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PlannedAmount = p.PlannedAmount,
            IsEstimate = p.IsEstimate,
            Detail = p.Detail
        };

    private static PeriodPlanIncomeLine MapRevisionIncomeLine(PeriodPlanRevisionIncomeLineEntity i) =>
        new()
        {
            Id = Guid.Parse(i.Id),
            PeriodPlanSnapshotId = Guid.Parse(i.PeriodPlanRevisionId),
            SourceType = (IncomeSourceType)i.SourceType,
            RecurringIncomeId = string.IsNullOrWhiteSpace(i.RecurringIncomeId) ? null : Guid.Parse(i.RecurringIncomeId),
            AdHocIncomeId = string.IsNullOrWhiteSpace(i.AdHocIncomeId) ? null : Guid.Parse(i.AdHocIncomeId),
            Name = i.Name,
            PlannedDate = DateOnly.ParseExact(i.PlannedDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PlannedAmount = i.PlannedAmount
        };
}
