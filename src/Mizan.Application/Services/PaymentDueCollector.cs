using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık dönem planından, güncel revizyonlardan ve gelecek dönem projeksiyonlarından
/// hatırlatma ufkuna (35 gün) giren tüm ödeme vadelerini derleyen yardımcı servis.
/// </summary>
public sealed class PaymentDueCollector(
    IPeriodObservationRepository periodObservationRepository,
    IPlanReader planReader,
    FinancialProjectionService projectionService,
    CreditCardStatementCalculator cardStatementCalculator)
{
    private readonly IPeriodObservationRepository _periodObservationRepository =
        periodObservationRepository ?? throw new ArgumentNullException(nameof(periodObservationRepository));
    private readonly IPlanReader _planReader =
        planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly FinancialProjectionService _projectionService =
        projectionService ?? throw new ArgumentNullException(nameof(projectionService));
    private readonly CreditCardStatementCalculator _cardStatementCalculator =
        cardStatementCalculator ?? throw new ArgumentNullException(nameof(cardStatementCalculator));

    /// <summary>
    /// Açık dönemin vadesi gelmemiş ödemelerini ve dönem ufkunu aşan sonraki dönem taahhütlerini toplar.
    /// </summary>
    public async Task<List<PaymentDue>> CollectAsync(
        PeriodPlanSnapshot openPlan,
        FinancialHistoryData history,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(openPlan);
        ArgumentNullException.ThrowIfNull(history);

        var today = DateOnly.FromDateTime(now);
        var last = today.AddDays(PaymentReminderPlanner.HorizonDays);
        var currentLines = CurrentLines(openPlan, history);
        var plan = await _planReader.GetPlanAsync(cancellationToken);

        var dues = await CollectOpenPlanDuesAsync(openPlan, currentLines, plan, today, last, cancellationToken);
        // Ufuk kapsayıcıdır (<= last): son günü dönem bitişine denk gelse bile o günün vadesi sonraki
        // dönemden gelir (S85).
        if (last >= openPlan.PeriodEnd)
        {
            var openPlanKeys = currentLines
                .Select(x => PaymentReminderPlanner.DueKey(x.SourceEntityId, x.Name, x.PlannedDate))
                .ToHashSet();
            AppendFutureDues(dues, plan, openPlan.PeriodEnd, openPlanKeys, today, last);
        }

        return dues;
    }

    private static IReadOnlyList<PeriodPlanPaymentLine> CurrentLines(
        PeriodPlanSnapshot openPlan,
        FinancialHistoryData history)
    {
        var revisions = history.FindFinalRevisions(openPlan);
        return revisions.Count > 0 && revisions[^1].PaymentLines.Count > 0
            ? revisions[^1].PaymentLines
            : openPlan.PaymentLines;
    }

    // Kart satırı dondurulan tahminle değil kartın bugünkü hâliyle söylenir: dönem içinde girilen harcama ve ekstre
    // ödenecek tutarı değiştirir, ana sayfanın "şu an"ı da budur (I23, S87-1).
    private async Task<List<PaymentDue>> CollectOpenPlanDuesAsync(
        PeriodPlanSnapshot openPlan,
        IReadOnlyList<PeriodPlanPaymentLine> currentLines,
        FinancialPlan plan,
        DateOnly today,
        DateOnly last,
        CancellationToken cancellationToken)
    {
        var marks = await _periodObservationRepository.GetPaymentMarksAsync(openPlan.Id, cancellationToken);
        var settled = marks
            .Where(x => x.IsSettled)
            .Select(x => x.PeriodPlanPaymentLineId)
            .ToHashSet();
        var cardPayments = CurrentCardPayments.Of(
            plan.CreditCards,
            new CashFlowPeriod(openPlan.PeriodStart, openPlan.PeriodEnd),
            plan.Settings.CreditCardCarryInterestRate,
            _cardStatementCalculator);

        return currentLines
            .Where(x => !settled.Contains(x.Id) && x.PlannedDate >= today && x.PlannedDate <= last)
            .Select(x => new PaymentDue(
                PaymentReminderPlanner.DueKey(x.SourceEntityId, x.Name, x.PlannedDate),
                x.Name,
                x.PlannedDate,
                ProjectedPaymentAmount.Known(x, cardPayments)))
            .ToList();
    }

    private void AppendFutureDues(
        List<PaymentDue> dues,
        FinancialPlan plan,
        DateOnly periodEnd,
        HashSet<string> openPlanKeys,
        DateOnly today,
        DateOnly last)
    {
        var futurePeriods = _projectionService.BuildFuturePeriods(plan, periodEnd, periodCount: 3);

        // Gelecek dönemler açık dönemin kendisinden başlar; açık plan [Start, End) vadelerini zaten
        // taşıdığı için yalnız onu ayıklıyoruz. Bitiş günü sonraki dönemin ilk günüdür, buradan gelir (S85).
        var mandatoryDues = futurePeriods
            .SelectMany(x => x.MandatoryItems)
            .Where(x => x.DueDate >= periodEnd && x.DueDate >= today && x.DueDate <= last)
            .Select(x => new PaymentDue(
                PaymentReminderPlanner.DueKey(x.PaymentId, x.Name, x.DueDate),
                x.Name,
                x.DueDate,
                x.Amount));

        var largeExpenseDues = plan.PlannedLargeExpenses
            .Where(x => x.Status == PlannedExpenseStatus.Planned && x.ExactDate >= periodEnd && x.ExactDate >= today && x.ExactDate <= last)
            .Select(x => new PaymentDue(
                PaymentReminderPlanner.DueKey(x.Id, x.Name, x.ExactDate),
                x.Name,
                x.ExactDate,
                x.Amount));

        // Açık planda zaten bulunan vade buradan ikinci kez gelmez: eskiden içe aktarılan plan bitiş gününe
        // satır taşıyabilir ve o satır ödendi işaretliyse projeksiyon onu geri getirirdi (S85).
        dues.AddRange(mandatoryDues.Concat(largeExpenseDues).Where(x => !openPlanKeys.Contains(x.Key)));
    }
}
