using System.Globalization;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Dondurulan dönem planı (varsa revizyonuyla birlikte) ile dönem sonu kesinleşen fiilî gerçekleşmeyi
/// karşılaştırarak 12 kategoride farkları ve kullanıcıya yönelik Türkçe açıklama özetini üreten saf hesaplayıcı.
/// Kapanan dönemin karne özetini türetmek ve tarihçeye kaydetmek için vardır.
/// </summary>
public sealed class PlanActualComparisonCalculator
{
    private static readonly CultureInfo TurkishCulture =
        CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Dönem planı, varsa revizyonu ve kesinleşen fiilî gerçekleşmeyi karşılaştırarak karne sonucunu hesaplar.
    /// </summary>
    /// <param name="plan">Dönem başında dondurulan orijinal plan.</param>
    /// <param name="revision">Varsa dönem içinde yapılan en güncel plan revizyonu.</param>
    /// <param name="actual">Dönem kapanışında kaydedilen fiilî gerçekleşme.</param>
    /// <returns>Kategori bazlı farkları ve açıklama özetini içeren karşılaştırma sonucu.</returns>
    public PlanActualComparison Calculate(
        PeriodPlanSnapshot plan,
        PeriodPlanRevision? revision,
        PeriodActual actual)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(actual);

        var plannedEnding = revision?.PlannedEndingBalance ?? plan.PlannedEndingBalance;
        var lines = BuildLines(plan, revision, actual);
        var difference = actual.ConfirmedEndingBalance - plannedEnding;

        return new PlanActualComparison(
            plannedEnding,
            actual.ConfirmedEndingBalance,
            difference,
            BuildSummary(difference, lines),
            lines);
    }

    private static IReadOnlyList<PlanActualComparisonLine> BuildLines(
        PeriodPlanSnapshot plan,
        PeriodPlanRevision? revision,
        PeriodActual actual) =>
    [
        Line("Gelir", revision?.PlannedIncome ?? plan.PlannedIncome, actual.ActualIncome),
        Line("Krediler", revision?.PlannedLoanPayments ?? plan.PlannedLoanPayments, actual.ActualLoanPayments),
        Line("Kredi kartları", revision?.PlannedCardPayments ?? plan.PlannedCardPayments, actual.ActualCardPayments),
        Line("Geçici ödemeler", revision?.PlannedTemporaryPayments ?? plan.PlannedTemporaryPayments, actual.ActualTemporaryPayments),
        Line("Taksitli ödemeler", revision?.PlannedInstallmentPayments ?? plan.PlannedInstallmentPayments, actual.ActualInstallmentPayments),
        Line("Diğer planlı ödemeler", revision?.PlannedOtherScheduledPayments ?? plan.PlannedOtherScheduledPayments, actual.ActualOtherScheduledPayments),
        Line("Zorunlu ödemeler", revision?.PlannedMandatoryPayments ?? plan.PlannedMandatoryPayments, actual.ActualMandatoryPayments),
        Line("Büyük ödemeler", revision?.PlannedLargeExpenses ?? plan.PlannedLargeExpenses, actual.ActualLargeExpenses),
        Line("Yaşam giderleri", revision?.PlannedVariableExpenseAllowance ?? plan.PlannedVariableExpenseAllowance, actual.ActualLivingSpend),
        Line("Faiz", revision?.PlannedInterest ?? plan.PlannedInterest, actual.ActualInterest),
        Line("Plan dışı ödemeler", 0m, actual.UnplannedPayments),
        Line("Dönem düzeltmesi", 0m, actual.ReconciliationAdjustment)
    ];

    private static PlanActualComparisonLine Line(
        string category,
        decimal planned,
        decimal actual) =>
        new(category, planned, actual, actual - planned);

    private static string BuildSummary(
        decimal endingDifference,
        IReadOnlyList<PlanActualComparisonLine> lines)
    {
        if (endingDifference == 0m)
        {
            return "Dönem sonu finansal durumun planla aynı seviyede gerçekleşti.";
        }

        var direction = endingDifference > 0m ? "üzerinde" : "altında";
        var lead =
            $"Dönem sonu finansal durumun planın {Math.Abs(endingDifference).ToString("N2", TurkishCulture)} TL {direction} gerçekleşti.";
        var cause = lines
            .Where(x =>
                x.Category is not "Dönem düzeltmesi" and not "Zorunlu ödemeler" &&
                x.Difference != 0m)
            .OrderByDescending(x => Math.Abs(x.Difference))
            .FirstOrDefault();

        return cause is null
            ? lead
            : $"{lead} En belirgin fark {cause.Category} kaleminde {Math.Abs(cause.Difference).ToString("N2", TurkishCulture)} TL oldu.";
    }
}
