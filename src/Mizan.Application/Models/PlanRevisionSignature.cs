using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Bir dönem planı veya plan revizyonunun finansal tutarlarını ve ödeme satırlarını
/// özetleyerek iki taahhüt arasında anlamlı bir fark olup olmadığını belirleyen imza nesnesi.
/// Gereksiz mükerrer revizyonların kaydedilmesini önlemek için vardır.
/// </summary>
public sealed record PlanRevisionSignature
{
    /// <summary>Toplam beklenen gelir.</summary>
    public decimal PlannedIncome { get; init; }

    /// <summary>Toplam kredi ödemeleri.</summary>
    public decimal PlannedLoanPayments { get; init; }

    /// <summary>Toplam kredi kartı ödemeleri.</summary>
    public decimal PlannedCardPayments { get; init; }

    /// <summary>Toplam geçici borç ödemeleri.</summary>
    public decimal PlannedTemporaryPayments { get; init; }

    /// <summary>Toplam taksitli borç ödemeleri.</summary>
    public decimal PlannedInstallmentPayments { get; init; }

    /// <summary>Diğer planlı takvimli ödemeler.</summary>
    public decimal PlannedOtherScheduledPayments { get; init; }

    /// <summary>Toplam zorunlu borç ödemeleri.</summary>
    public decimal PlannedMandatoryPayments { get; init; }

    /// <summary>Dönemlik yaşam gideri serbest havuzu.</summary>
    public decimal PlannedVariableExpenseAllowance { get; init; }

    /// <summary>Planlı büyük harcamalar toplamı.</summary>
    public decimal PlannedLargeExpenses { get; init; }

    /// <summary>Kredi kartı devreden borç faizi beklentisi.</summary>
    public decimal PlannedCardInterest { get; init; }

    /// <summary>Negatif bakiye finansman faizi beklentisi.</summary>
    public decimal PlannedDeficitInterest { get; init; }

    /// <summary>Hedeflenen dönem kapanış bakiyesi.</summary>
    public decimal PlannedEndingBalance { get; init; }

    /// <summary>Kronolojik ve tür bazlı sıralanmış ödeme satırları imzası.</summary>
    public IReadOnlyList<PlanRevisionLineSignature> Lines { get; init; } = [];

    /// <summary>Kronolojik ve tür bazlı sıralanmış gelir satırları imzası; toplam aynı kalsa da yatış günü değişikliğini yakalar.</summary>
    public IReadOnlyList<PlanRevisionIncomeLineSignature> IncomeLines { get; init; } = [];

    /// <summary>
    /// Dönem planı anlık görüntüsünden imza nesnesi türetir.
    /// </summary>
    public static PlanRevisionSignature From(PeriodPlanSnapshot plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new PlanRevisionSignature
        {
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
            PlannedEndingBalance = plan.PlannedEndingBalance,
            Lines = OrderLines(plan.PaymentLines),
            IncomeLines = OrderIncomeLines(plan.IncomeLines)
        };
    }

    /// <summary>
    /// Dönem planı revizyonundan imza nesnesi türetir.
    /// </summary>
    public static PlanRevisionSignature From(PeriodPlanRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);

        return new PlanRevisionSignature
        {
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
            Lines = OrderLines(revision.PaymentLines),
            IncomeLines = OrderIncomeLines(revision.IncomeLines)
        };
    }

    /// <summary>
    /// İki plan imzasının finansal tutarlar ve ödeme satırları açısından tamamen özdeş olup olmadığını inceler.
    /// </summary>
    public bool Equals(PlanRevisionSignature? other)
    {
        if (other is null)
        {
            return false;
        }

        return PlannedIncome == other.PlannedIncome &&
               PlannedLoanPayments == other.PlannedLoanPayments &&
               PlannedCardPayments == other.PlannedCardPayments &&
               PlannedTemporaryPayments == other.PlannedTemporaryPayments &&
               PlannedInstallmentPayments == other.PlannedInstallmentPayments &&
               PlannedOtherScheduledPayments == other.PlannedOtherScheduledPayments &&
               PlannedMandatoryPayments == other.PlannedMandatoryPayments &&
               PlannedVariableExpenseAllowance == other.PlannedVariableExpenseAllowance &&
               PlannedLargeExpenses == other.PlannedLargeExpenses &&
               PlannedCardInterest == other.PlannedCardInterest &&
               PlannedDeficitInterest == other.PlannedDeficitInterest &&
               PlannedEndingBalance == other.PlannedEndingBalance &&
               Lines.SequenceEqual(other.Lines) &&
               IncomeLines.SequenceEqual(other.IncomeLines);
    }

    /// <summary>
    /// İmza nesnesi için tutarlı karma kodu hesaplar.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlannedIncome);
        hash.Add(PlannedLoanPayments);
        hash.Add(PlannedCardPayments);
        hash.Add(PlannedTemporaryPayments);
        hash.Add(PlannedInstallmentPayments);
        hash.Add(PlannedOtherScheduledPayments);
        hash.Add(PlannedMandatoryPayments);
        hash.Add(PlannedVariableExpenseAllowance);
        hash.Add(PlannedLargeExpenses);
        hash.Add(PlannedCardInterest);
        hash.Add(PlannedDeficitInterest);
        hash.Add(PlannedEndingBalance);

        foreach (var line in Lines)
        {
            hash.Add(line);
        }

        foreach (var incomeLine in IncomeLines)
        {
            hash.Add(incomeLine);
        }

        return hash.ToHashCode();
    }

    private static PlanRevisionLineSignature[] OrderLines(IEnumerable<PeriodPlanPaymentLine> lines)
    {
        return lines
            .Select(PlanRevisionLineSignature.From)
            .OrderBy(x => x.PlannedDate)
            .ThenBy(x => x.SourceType)
            .ThenBy(x => x.SourceEntityId)
            .ToArray();
    }

    private static PlanRevisionIncomeLineSignature[] OrderIncomeLines(IEnumerable<PeriodPlanIncomeLine> lines)
    {
        return lines
            .Select(PlanRevisionIncomeLineSignature.From)
            .OrderBy(x => x.PlannedDate)
            .ThenBy(x => x.SourceType)
            .ThenBy(x => x.RecurringIncomeId)
            .ThenBy(x => x.AdHocIncomeId)
            .ToArray();
    }
}
