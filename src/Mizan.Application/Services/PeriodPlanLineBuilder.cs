using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Bir dönem projeksiyonundan, dondurulacak planın ödeme ve gelir satırlarını üreten
/// bağımlılıksız yardımcı. Ödeme satırları vadesi dönem aralığına düşen yükümlülüklerden,
/// gelir satırları projeksiyonun o döneme saydığı gelir kalemlerinden türer.
/// </summary>
public static class PeriodPlanLineBuilder
{
    /// <summary>
    /// Vadesi dönemin yarı açık aralığına düşen yükümlülükleri, planlı büyük harcamaları ve
    /// tutarı belirlenmemiş kart ödemelerini tarih ve ad sırasıyla ödeme satırlarına dönüştürür.
    /// </summary>
    public static PeriodPlanPaymentLine[] BuildPaymentLines(
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

    /// <summary>
    /// Projeksiyonun döneme saydığı gelir kalemlerini tarihleriyle gelir satırlarına dönüştürür.
    /// </summary>
    /// <remarks>
    /// Dönem filtresi bilerek uygulanmaz: satırlar projeksiyonun gelir toplamını oluşturan
    /// kalemlerin kendisidir, böylece toplamları planlanan gelire kuruşu kuruşuna eşit kalır.
    /// </remarks>
    public static PeriodPlanIncomeLine[] BuildIncomeLines(CashFlowPeriodProjection projection, Guid planId) =>
        projection.IncomeItems
            .Select(item => new PeriodPlanIncomeLine
            {
                PeriodPlanSnapshotId = planId,
                SourceType = item.Type,
                RecurringIncomeId = item.RecurringIncomeId,
                AdHocIncomeId = item.AdHocIncomeId,
                Name = item.Name,
                PlannedDate = item.SourceDate,
                PlannedAmount = item.Amount
            })
            .ToArray();

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
