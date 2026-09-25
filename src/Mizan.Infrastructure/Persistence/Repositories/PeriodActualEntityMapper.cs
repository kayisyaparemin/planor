using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kapanan dönemin karneleşmiş gerçekleşmeleri, fiilî ödemeleri ve yaşam harcaması dökümlerinin
/// SQLite entity modellerinden Domain modellerine dönüştürülmesini sağlayan dahili haritalayıcı.
/// </summary>
internal static class PeriodActualEntityMapper
{
    public static PeriodActual MapActual(
        PeriodActualEntity e,
        IEnumerable<ActualPaymentEntity> payments,
        IEnumerable<ActualFlowEntity> flows,
        IEnumerable<ActualLivingBreakdownEntity> living) =>
        new()
        {
            Id = Guid.Parse(e.Id),
            PeriodPlanSnapshotId = Guid.Parse(e.PeriodPlanSnapshotId),
            SourceFinancialSnapshotId = Guid.Parse(e.SourceFinancialSnapshotId),
            ResultFinancialSnapshotId = Guid.Parse(e.ResultFinancialSnapshotId),
            PeriodStart = DateOnly.ParseExact(e.PeriodStart, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PeriodEnd = DateOnly.ParseExact(e.PeriodEnd, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            FinalizedAtUtc = DateTimeOffset.Parse(e.FinalizedAtUtc, CultureInfo.InvariantCulture),
            ActualIncome = e.ActualIncome,
            ActualLoanPayments = e.ActualLoanPayments,
            ActualCardPayments = e.ActualCardPayments,
            ActualTemporaryPayments = e.ActualTemporaryPayments,
            ActualInstallmentPayments = e.ActualInstallmentPayments,
            ActualOtherScheduledPayments = e.ActualOtherScheduledPayments,
            ActualLargeExpenses = e.ActualLargeExpenses,
            ActualMandatoryPayments = e.ActualMandatoryPayments,
            ActualLivingSpend = e.ActualLivingSpend,
            ActualInterest = e.ActualInterest,
            UnplannedIncome = e.UnplannedIncome,
            UnplannedPayments = e.UnplannedPayments,
            DerivedEndingBalance = e.DerivedEndingBalance,
            ConfirmedEndingBalance = e.ConfirmedEndingBalance,
            ReconciliationAdjustment = e.ReconciliationAdjustment,
            ComparisonSummary = e.ComparisonSummary,
            Note = e.Note,
            Payments = payments.Select(MapActualPayment).ToArray(),
            Flows = flows.Select(MapActualFlow).ToArray(),
            LivingBreakdown = living.Select(MapActualLivingBreakdown).ToArray()
        };

    private static ActualPayment MapActualPayment(ActualPaymentEntity a) =>
        new()
        {
            Id = Guid.Parse(a.Id),
            PeriodActualId = Guid.Parse(a.PeriodActualId),
            PeriodPlanPaymentLineId = Guid.Parse(a.PeriodPlanPaymentLineId),
            SourceEntityId = Guid.Parse(a.SourceEntityId),
            SourceType = (PlanPaymentSourceType)a.SourceType,
            Name = a.Name,
            PlannedDate = DateOnly.ParseExact(a.PlannedDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PlannedAmount = a.PlannedAmount,
            ActualPaymentDate = string.IsNullOrWhiteSpace(a.ActualPaymentDate) ? null : DateOnly.ParseExact(a.ActualPaymentDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ActualAmount = a.ActualAmount,
            Status = (ActualPaymentStatus)a.Status,
            Note = a.Note
        };

    private static ActualFlow MapActualFlow(ActualFlowEntity f) =>
        new()
        {
            Id = Guid.Parse(f.Id),
            PeriodActualId = Guid.Parse(f.PeriodActualId),
            Type = (ActualFlowType)f.Type,
            Name = f.Name,
            Category = f.Category,
            Date = DateOnly.ParseExact(f.Date, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Amount = f.Amount
        };

    private static ActualLivingBreakdown MapActualLivingBreakdown(ActualLivingBreakdownEntity b) =>
        new()
        {
            Id = Guid.Parse(b.Id),
            PeriodActualId = Guid.Parse(b.PeriodActualId),
            Category = b.Category,
            Amount = b.Amount
        };
}
