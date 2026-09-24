using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Belirli bir nakit akış döneminin başlangıcında, finansal planı ve projeksiyon sonuçlarını
/// baz alarak dönemin taahhüt edilen planını (PeriodPlanSnapshot), ödeme satırlarını
/// (PaymentLines) ve gelir satırlarını (IncomeLines) donduran uygulama servisidir.
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

        return MapFrozenSnapshot(Guid.NewGuid(), snapshot, period, projection, createdAtUtc);
    }

    private static PeriodPlanSnapshot MapFrozenSnapshot(
        Guid planId,
        FinancialSnapshot snapshot,
        CashFlowPeriod period,
        CashFlowPeriodProjection projection,
        DateTimeOffset createdAtUtc) => new()
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
        PaymentLines = PeriodPlanLineBuilder.BuildPaymentLines(projection, period, planId),
        IncomeLines = PeriodPlanLineBuilder.BuildIncomeLines(projection, planId)
    };
}
