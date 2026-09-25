using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık dönem planından, güncel revizyonlardan ve gelecek dönem projeksiyonlarından
/// hatırlatma ufkuna (35 gün) giren tüm ödeme vadelerini derleyen yardımcı servis.
/// </summary>
public sealed class PaymentDueCollector(
    IPeriodObservationRepository periodObservationRepository,
    IPlanReader planReader,
    FinancialProjectionService projectionService)
{
    private readonly IPeriodObservationRepository _periodObservationRepository =
        periodObservationRepository ?? throw new ArgumentNullException(nameof(periodObservationRepository));
    private readonly IPlanReader _planReader =
        planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly FinancialProjectionService _projectionService =
        projectionService ?? throw new ArgumentNullException(nameof(projectionService));

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

        var dues = await CollectOpenPlanDuesAsync(openPlan, history, today, last, cancellationToken);
        if (last > openPlan.PeriodEnd)
        {
            await AppendFutureDuesAsync(dues, openPlan.PeriodEnd, today, last, cancellationToken);
        }

        return dues;
    }

    private async Task<List<PaymentDue>> CollectOpenPlanDuesAsync(
        PeriodPlanSnapshot openPlan,
        FinancialHistoryData history,
        DateOnly today,
        DateOnly last,
        CancellationToken cancellationToken)
    {
        var revisions = history.FindFinalRevisions(openPlan);
        var currentLines = revisions.Count > 0 && revisions[^1].PaymentLines.Count > 0
            ? revisions[^1].PaymentLines
            : openPlan.PaymentLines;

        var observation = await _periodObservationRepository.GetPeriodObservationAsync(openPlan.Id, cancellationToken);
        var settled = observation?.Payments
            .Where(x => x.Status != ActualPaymentStatus.Unpaid)
            .Select(x => x.PeriodPlanPaymentLineId)
            .ToHashSet() ?? [];

        return currentLines
            .Where(x => !settled.Contains(x.Id) && x.PlannedDate >= today && x.PlannedDate <= last)
            .Select(x => new PaymentDue(
                PaymentReminderPlanner.DueKey(x.SourceEntityId, x.Name, x.PlannedDate),
                x.Name,
                x.PlannedDate,
                x.PlannedAmount))
            .ToList();
    }

    private async Task AppendFutureDuesAsync(
        List<PaymentDue> dues,
        DateOnly periodEnd,
        DateOnly today,
        DateOnly last,
        CancellationToken cancellationToken)
    {
        var plan = await _planReader.GetPlanAsync(cancellationToken);
        var futurePeriods = _projectionService.BuildFuturePeriods(plan, periodEnd, periodCount: 3);

        dues.AddRange(futurePeriods
            .SelectMany(x => x.MandatoryItems)
            .Where(x => x.DueDate > periodEnd && x.DueDate >= today && x.DueDate <= last)
            .Select(x => new PaymentDue(
                PaymentReminderPlanner.DueKey(x.PaymentId, x.Name, x.DueDate),
                x.Name,
                x.DueDate,
                x.Amount)));

        dues.AddRange(plan.PlannedLargeExpenses
            .Where(x => x.Status == PlannedExpenseStatus.Planned && x.ExactDate > periodEnd && x.ExactDate >= today && x.ExactDate <= last)
            .Select(x => new PaymentDue(
                PaymentReminderPlanner.DueKey(x.Id, x.Name, x.ExactDate),
                x.Name,
                x.ExactDate,
                x.Amount)));
    }
}
