using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Belirli bir nakit akış döneminin başlangıcında, finansal planı ve projeksiyon sonuçlarını
/// baz alarak dönemin taahhüt edilen planını (PeriodPlanSnapshot) ve ödeme satırlarını
/// (PaymentLines) donduran uygulama servisidir.
/// Dondurulan planın daha sonraki finansal hareketlerden etkilenmemesi (I23 invariant'ı)
/// garantisini sağlar.
/// </summary>
public sealed class PeriodPlanSnapshotService(
    FinancialProjectionCalculator projectionCalculator,
    CashFlowPeriodCalculator periodCalculator)
{
    private readonly FinancialProjectionCalculator _projectionCalculator =
        projectionCalculator ?? throw new ArgumentNullException(nameof(projectionCalculator));
    private readonly CashFlowPeriodCalculator _periodCalculator =
        periodCalculator ?? throw new ArgumentNullException(nameof(periodCalculator));

    /// <summary>
    /// Belirtilen finansal plan ve durum anlık görüntüsüne dayanarak yeni bir dönem planını dondurur.
    /// </summary>
    public PeriodPlanSnapshot Freeze(
        FinancialPlan financialPlan,
        FinancialSnapshot snapshot,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(financialPlan);
        ArgumentNullException.ThrowIfNull(snapshot);

        var settlementDate = _periodCalculator.GetNextSettlementDate(snapshot.SnapshotDate, snapshot.Anchor);
        var period = new CashFlowPeriod(snapshot.SnapshotDate, settlementDate);
        var effectivePlan = financialPlan with
        {
            Settings = financialPlan.Settings with
            {
                ProjectionOpeningBalance = snapshot.ProjectionOpeningBalance,
                ProjectionAnchorDate = snapshot.SnapshotDate,
                PeriodAnchor = snapshot.Anchor
            }
        };
        var projectionResult = _projectionCalculator.CalculatePlan(effectivePlan, snapshot.ProjectionAnchorDate, 2, period.Start);
        var projection = projectionResult.Periods[0];
        var planId = Guid.NewGuid();
        var lines = BuildPaymentLines(projection, period, planId);

        return MapFrozenSnapshot(planId, snapshot, period, projection, createdAtUtc, lines);
    }

    private static PeriodPlanSnapshot MapFrozenSnapshot(
        Guid planId,
        FinancialSnapshot snapshot,
        CashFlowPeriod period,
        CashFlowPeriodProjection projection,
        DateTimeOffset createdAtUtc,
        PeriodPlanPaymentLine[] lines) => new()
    {
        Id = planId,
        FinancialSnapshotId = snapshot.Id,
        PeriodStart = period.Start,
        PeriodEnd = period.End,
        SettlementAvailableFrom = period.End,
        CreatedAtUtc = createdAtUtc,
        OpeningBalance = snapshot.ProjectionOpeningBalance,
        PlannedIncome = projection.TotalIncome,
        PlannedLoanPayments = projection.LoanPayments,
        PlannedCardPayments = projection.CreditCardPayments,
        PlannedTemporaryPayments = projection.TemporaryPayments,
        PlannedInstallmentPayments = projection.InstallmentPayments,
        PlannedOtherScheduledPayments = projection.OtherScheduledPayments,
        PlannedMandatoryPayments = projection.MandatoryOutflow,
        PlannedVariableExpenseAllowance = projection.VariableExpenseAllowance,
        PlannedLargeExpenses = projection.PlannedLargeCashExpenses,
        PlannedCardInterest = projection.CardInterestGenerated,
        PlannedDeficitInterest = projection.DeficitFinancingInterest,
        PlannedEndingBalance = projection.EndingBalance,
        PaymentLines = lines
    };

    private static PeriodPlanPaymentLine[] BuildPaymentLines(
        CashFlowPeriodProjection projection,
        CashFlowPeriod period,
        Guid planId)
    {
        var lines = new List<PeriodPlanPaymentLine>();

        foreach (var item in projection.MandatoryItems)
        {
            if (period.Contains(item.DueDate))
            {
                lines.Add(MapObligationLine(item, planId));
            }
        }

        foreach (var expense in projection.LargeExpenseItems)
        {
            if (period.Contains(expense.ExactDate))
            {
                lines.Add(MapLargeExpenseLine(expense, planId));
            }
        }

        AppendUndeterminedCards(lines, projection.CardPaymentStatuses, period, planId);

        return lines.OrderBy(x => x.PlannedDate).ThenBy(x => x.Name).ToArray();
    }

    private static PeriodPlanPaymentLine MapObligationLine(ObligationItem item, Guid planId) => new()
    {
        PeriodPlanSnapshotId = planId,
        SourceEntityId = item.PaymentId,
        SourceType = MapSourceType(item.Type),
        Name = item.Name,
        PlannedDate = item.DueDate,
        PlannedAmount = item.Amount,
        IsEstimate = item.IsEstimate,
        Detail = item.Detail
    };

    private static PeriodPlanPaymentLine MapLargeExpenseLine(PlannedLargeExpense expense, Guid planId) => new()
    {
        PeriodPlanSnapshotId = planId,
        SourceEntityId = expense.Id,
        SourceType = PlanPaymentSourceType.PlannedLargeExpense,
        Name = expense.Name,
        PlannedDate = expense.ExactDate,
        PlannedAmount = expense.Amount,
        IsEstimate = false,
        Detail = expense.Note
    };

    private static void AppendUndeterminedCards(
        List<PeriodPlanPaymentLine> lines,
        IEnumerable<CreditCardPaymentProjectionStatus> cardStatuses,
        CashFlowPeriod period,
        Guid planId)
    {
        foreach (var card in cardStatuses)
        {
            if (!period.Contains(card.PaymentDueDate))
            {
                continue;
            }

            var exists = lines.Any(x =>
                x.SourceType == PlanPaymentSourceType.CreditCard &&
                x.SourceEntityId == card.CardId &&
                x.PlannedDate == card.PaymentDueDate);

            if (!exists)
            {
                lines.Add(new PeriodPlanPaymentLine
                {
                    PeriodPlanSnapshotId = planId,
                    SourceEntityId = card.CardId,
                    SourceType = PlanPaymentSourceType.CreditCard,
                    Name = card.CardName,
                    PlannedDate = card.PaymentDueDate,
                    PlannedAmount = card.Payment ?? 0m,
                    IsEstimate = card.Resolution == CreditCardPaymentResolution.ProjectionFallback,
                    Detail = card.Resolution == CreditCardPaymentResolution.Undetermined
                        ? "Ödeme tutarı dönem başında belirlenmemişti."
                        : string.Empty
                });
            }
        }
    }

    private static PlanPaymentSourceType MapSourceType(ObligationType type) => type switch
    {
        ObligationType.Loan => PlanPaymentSourceType.Loan,
        ObligationType.CreditCard => PlanPaymentSourceType.CreditCard,
        ObligationType.TemporaryPayment => PlanPaymentSourceType.TemporaryPayment,
        ObligationType.InstallmentPayment => PlanPaymentSourceType.InstallmentPayment,
        ObligationType.OtherScheduledPayment => PlanPaymentSourceType.OtherScheduledPayment,
        ObligationType.PlannedLargeExpense => PlanPaymentSourceType.PlannedLargeExpense,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
