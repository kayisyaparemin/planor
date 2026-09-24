using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kapanmış dönemlerin tarihçesini okuyan salt okuma servisi: her gerçekleşmeyi ait olduğu
/// orijinal planla, dönemin nihai revizyonuyla ve kapanıştan çıkan finansal durumla eşleştirip
/// karnesini çıkarır. Geçmiş ekranının planı ve gerçekleşmeyi hiçbir şey yazmadan yan yana
/// gösterebilmesi için vardır (M4).
/// </summary>
public sealed class HistoryQueryService(
    IPeriodHistoryRepository periodHistoryRepository,
    PlanActualComparisonCalculator comparisonCalculator)
{
    /// <summary>Özette varsayılan olarak dikkate alınan son dönem adedi.</summary>
    public const int DefaultSummaryPeriodCount = 3;

    private readonly IPeriodHistoryRepository _periodHistoryRepository =
        periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
    private readonly PlanActualComparisonCalculator _comparisonCalculator =
        comparisonCalculator ?? throw new ArgumentNullException(nameof(comparisonCalculator));

    /// <summary>Kapanmış tüm dönemleri en yeni dönem başta olacak şekilde getirir.</summary>
    public async Task<IReadOnlyList<HistoryPeriod>> GetPeriodsAsync(
        CancellationToken cancellationToken = default)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);

        return history.Actuals
            .Select(actual => BuildPeriod(history, actual))
            .OrderByDescending(x => x.OriginalPlan.PeriodStart)
            .ToArray();
    }

    /// <summary>Belirtilen gerçekleşmeye ait kapanmış dönemi getirir; kayıt yoksa null döner.</summary>
    public async Task<HistoryPeriod?> GetPeriodAsync(
        Guid actualId,
        CancellationToken cancellationToken = default)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var actual = history.Actuals.SingleOrDefault(x => x.Id == actualId);

        return actual is null ? null : BuildPeriod(history, actual);
    }

    /// <summary>
    /// Son kapanan dönemlerin planlanan ve fiilî net değişim özetini getirir;
    /// kapanmış dönem yoksa null döner.
    /// </summary>
    public async Task<HistorySummary?> GetRecentSummaryAsync(
        int periodCount = DefaultSummaryPeriodCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(periodCount);

        var periods = (await GetPeriodsAsync(cancellationToken))
            .Take(periodCount)
            .ToArray();
        if (periods.Length == 0)
        {
            return null;
        }

        // Kapanış bakiyesi bir stoktur, dönemler arasında toplanamaz; her dönemin kendi
        // açılışına göre net değişimi toplanır (S29). Açılış iki tarafta da aynıdır (I21).
        return new HistorySummary(
            periods.Sum(x => x.Comparison.PlannedEndingBalance - x.OriginalPlan.OpeningBalance),
            periods.Sum(x => x.Comparison.ActualEndingBalance - x.OriginalPlan.OpeningBalance),
            periods.Sum(x => x.Comparison.Difference),
            periods.Length);
    }

    private HistoryPeriod BuildPeriod(FinancialHistoryData history, PeriodActual actual)
    {
        var plan = history.Plans.Single(x => x.Id == actual.PeriodPlanSnapshotId);
        var revisions = SelectFinalPlanRevisions(history, plan);
        var revision = revisions.LastOrDefault();

        return new HistoryPeriod
        {
            OriginalPlan = plan,
            Revision = revision,
            RevisionCount = revisions.Length,
            Actual = actual,
            ResultSnapshot = history.Snapshots.Single(x => x.Id == actual.ResultFinancialSnapshotId),
            Comparison = _comparisonCalculator.Calculate(plan, revision, actual)
        };
    }

    /// <summary>
    /// Dönemin nihai planına sayılan revizyonlar, eskiden yeniye: kapanışa açılış günü (UTC takvim
    /// günü) dahil o güne kadar oluşturulanlar; aynı anda oluşanlarda büyük numara sonra gelir (I27).
    /// Sıralama tarihçenin kendisinden gelir; kapanış günü kesmesi A16'da dönem kapanışı da bu kurala
    /// ihtiyaç duyduğunda ortak yardımcıya çıkarılacak (T6, M8).
    /// </summary>
    private static PeriodPlanRevision[] SelectFinalPlanRevisions(
        FinancialHistoryData history,
        PeriodPlanSnapshot plan) =>
        history.FindRevisions(plan.Id)
            .Where(x => DateOnly.FromDateTime(x.CreatedAtUtc.UtcDateTime) <= plan.SettlementAvailableFrom)
            .ToArray();
}
