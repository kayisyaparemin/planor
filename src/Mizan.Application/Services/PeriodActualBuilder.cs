using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kullanıcının sunduğu kapanış taslağını (<see cref="PeriodSettlementDraft"/>) iş kurallarına göre
/// doğrulayarak kapanan dönemin kesinleşmiş fiilî durum karnesini (<see cref="PeriodActual"/>) inşa eden
/// bağımsız saf yardımcı sınıftır.
/// </summary>
public static class PeriodActualBuilder
{
    /// <summary>Kapanan dönemin kesinleşen <see cref="PeriodActual"/> nesnesini üretir.</summary>
    public static PeriodActual Build(
        PeriodPlanSnapshot plan,
        FinancialSnapshot snapshot,
        PeriodPlanRevision? revision,
        IReadOnlyList<PeriodPlanPaymentLine> paymentLines,
        PeriodSettlementDraft draft,
        DateTimeOffset finalizedAtUtc,
        Guid resultSnapshotId,
        bool validateDates,
        IReadOnlyDictionary<Guid, decimal>? currentCardPayments = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(paymentLines);
        ArgumentNullException.ThrowIfNull(draft);

        ValidateDraft(plan, draft, validateDates);

        var actualId = Guid.NewGuid();
        var payments = BuildPayments(plan, paymentLines, draft, actualId, validateDates, currentCardPayments);
        var flows = BuildFlows(draft, actualId);
        var breakdown = BuildBreakdown(draft, actualId);

        return AssemblePeriodActual(plan, snapshot, revision, draft, finalizedAtUtc, resultSnapshotId, actualId, payments, flows, breakdown);
    }

    private static void ValidateDraft(PeriodPlanSnapshot plan, PeriodSettlementDraft draft, bool validateDates)
    {
        if (draft.ActualLivingSpend < 0m || draft.ActualInterest < 0m)
        {
            throw new InvalidOperationException("Gerçekleşen tutarlar negatif olamaz.");
        }
        if (draft.LivingBreakdown.Any(x => x.Amount < 0m) || draft.LivingBreakdown.Sum(x => x.Amount) > draft.ActualLivingSpend)
        {
            throw new InvalidOperationException("Yaşam ayrıntıları toplam yaşam giderini aşamaz.");
        }
        if (draft.Flows.Any(x => x.Amount <= 0m || string.IsNullOrWhiteSpace(x.Name)))
        {
            throw new InvalidOperationException("Plan dışı gelir ve ödemelerde ad ile pozitif tutar gereklidir.");
        }
        if (validateDates && draft.Flows.Any(x => x.Date < plan.PeriodStart || x.Date >= plan.PeriodEnd))
        {
            throw new InvalidOperationException("Plan dışı hareket tarihi dönem aralığında olmalıdır.");
        }
    }

    private static ActualPayment[] BuildPayments(
        PeriodPlanSnapshot plan,
        IReadOnlyList<PeriodPlanPaymentLine> paymentLines,
        PeriodSettlementDraft draft,
        Guid actualId,
        bool validateDates,
        IReadOnlyDictionary<Guid, decimal>? currentCardPayments)
    {
        var paymentDrafts = draft.Payments.GroupBy(x => x.PeriodPlanPaymentLineId).ToDictionary(x => x.Key, x => x.Single());
        return paymentLines.Select(line =>
        {
            var input = paymentDrafts.GetValueOrDefault(line.Id) ?? DefaultDraft(line, currentCardPayments);
            return BuildActualPayment(plan, line, input, actualId, validateDates);
        }).ToArray();
    }

    private static ActualPaymentDraft DefaultDraft(
        PeriodPlanPaymentLine line,
        IReadOnlyDictionary<Guid, decimal>? currentCardPayments)
    {
        var amount = currentCardPayments is not null
            ? ProjectedPaymentAmount.Of(line, currentCardPayments)
            : line.PlannedAmount.GetValueOrDefault();
        var isPaid = amount > 0m || (currentCardPayments is null && line.PlannedAmount is not null);
        return new ActualPaymentDraft
        {
            PeriodPlanPaymentLineId = line.Id,
            Status = isPaid ? ActualPaymentStatus.Paid : ActualPaymentStatus.Unpaid,
            ActualAmount = amount,
            ActualPaymentDate = line.PlannedAmount is null && amount == 0m ? null : line.PlannedDate
        };
    }

    private static ActualPayment BuildActualPayment(
        PeriodPlanSnapshot plan,
        PeriodPlanPaymentLine line,
        ActualPaymentDraft input,
        Guid actualId,
        bool validateDates)
    {
        var amount = input.Status == ActualPaymentStatus.Unpaid ? 0m : input.ActualAmount;
        if (amount < 0m)
        {
            throw new InvalidOperationException("Gerçek ödeme tutarı negatif olamaz.");
        }
        if (input.Status != ActualPaymentStatus.Unpaid && amount <= 0m)
        {
            throw new InvalidOperationException("Ödendi olarak işaretlenen tutar sıfırdan büyük olmalıdır.");
        }
        if (validateDates && input.ActualPaymentDate is DateOnly date && (date < plan.PeriodStart || date >= plan.PeriodEnd))
        {
            throw new InvalidOperationException("Gerçek ödeme tarihi dönem aralığında olmalıdır.");
        }

        var status = input.Status == ActualPaymentStatus.Paid && line.PlannedAmount is decimal planned && planned != amount
            ? ActualPaymentStatus.DifferentAmount
            : input.Status;

        return new ActualPayment
        {
            PeriodActualId = actualId,
            PeriodPlanPaymentLineId = line.Id,
            SourceEntityId = line.SourceEntityId,
            SourceType = line.SourceType,
            Name = line.Name,
            PlannedDate = line.PlannedDate,
            PlannedAmount = line.PlannedAmount,
            ActualPaymentDate = input.Status == ActualPaymentStatus.Unpaid ? null : input.ActualPaymentDate ?? line.PlannedDate,
            ActualAmount = amount,
            Status = status,
            Note = (input.Note ?? string.Empty).Trim()
        };
    }

    private static PeriodActual AssemblePeriodActual(
        PeriodPlanSnapshot plan,
        FinancialSnapshot snapshot,
        PeriodPlanRevision? revision,
        PeriodSettlementDraft draft,
        DateTimeOffset finalizedAtUtc,
        Guid resultSnapshotId,
        Guid actualId,
        ActualPayment[] payments,
        ActualFlow[] flows,
        ActualLivingBreakdown[] breakdown)
    {
        var baselineIncome = revision?.PlannedIncome ?? plan.PlannedIncome;
        var unplannedIncome = flows.Where(x => x.Type == ActualFlowType.UnplannedIncome).Sum(x => x.Amount);
        var unplannedPayments = flows.Where(x => x.Type == ActualFlowType.UnplannedPayment).Sum(x => x.Amount);
        var totalPayments = payments.Sum(x => x.ActualAmount);
        var derived = snapshot.ProjectionOpeningBalance + baselineIncome + unplannedIncome - totalPayments - draft.ActualLivingSpend - draft.ActualInterest - unplannedPayments;
        var confirmed = draft.ConfirmedEndingBalance ?? derived;

        decimal Sum(PlanPaymentSourceType type) => payments.Where(x => x.SourceType == type).Sum(x => x.ActualAmount);
        var mandatory = payments.Where(x => x.SourceType != PlanPaymentSourceType.PlannedLargeExpense).Sum(x => x.ActualAmount);

        return new PeriodActual
        {
            Id = actualId,
            PeriodPlanSnapshotId = plan.Id,
            SourceFinancialSnapshotId = snapshot.Id,
            ResultFinancialSnapshotId = resultSnapshotId,
            PeriodStart = plan.PeriodStart,
            PeriodEnd = plan.PeriodEnd,
            FinalizedAtUtc = finalizedAtUtc,
            ActualIncome = baselineIncome + unplannedIncome,
            ActualLoanPayments = Sum(PlanPaymentSourceType.Loan),
            ActualCardPayments = Sum(PlanPaymentSourceType.CreditCard),
            ActualTemporaryPayments = Sum(PlanPaymentSourceType.TemporaryPayment),
            ActualInstallmentPayments = Sum(PlanPaymentSourceType.InstallmentPayment),
            ActualOtherScheduledPayments = Sum(PlanPaymentSourceType.OtherScheduledPayment),
            ActualLargeExpenses = Sum(PlanPaymentSourceType.PlannedLargeExpense),
            ActualMandatoryPayments = mandatory,
            ActualLivingSpend = draft.ActualLivingSpend, ActualInterest = draft.ActualInterest,
            UnplannedIncome = unplannedIncome, UnplannedPayments = unplannedPayments,
            DerivedEndingBalance = derived, ConfirmedEndingBalance = confirmed,
            ReconciliationAdjustment = confirmed - derived,
            Note = (draft.ActualNote ?? string.Empty).Trim(),
            Payments = payments,
            Flows = flows,
            LivingBreakdown = breakdown
        };
    }

    private static ActualFlow[] BuildFlows(PeriodSettlementDraft draft, Guid actualId) =>
        draft.Flows.Select(x => new ActualFlow
        {
            PeriodActualId = actualId, Type = x.Type, Name = x.Name.Trim(),
            Category = (x.Category ?? string.Empty).Trim(), Date = x.Date, Amount = x.Amount
        }).ToArray();

    private static ActualLivingBreakdown[] BuildBreakdown(PeriodSettlementDraft draft, Guid actualId) =>
        draft.LivingBreakdown.Where(x => x.Amount > 0m).Select(x => new ActualLivingBreakdown
        {
            PeriodActualId = actualId, Category = x.Category.Trim(), Amount = x.Amount
        }).ToArray();
}
