using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.Presenters;

/// <summary>
/// 12 dönemlik What-If simülasyon sonuçlarını analiz ederek zaman çizelgesi hikayesini,
/// içgörü çiplerini ve anahtar metrikleri üreten sunum yardımcısı.
/// </summary>
public sealed class SimulatorInsightService
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Baz durum ve senaryo arasındaki faiz maliyetlerini karşılaştırma satırlarına dönüştürür.
    /// </summary>
    public static List<SimulatorInterestRow> BuildInterestComparison(
        ProjectionInterestSummary baseline,
        ProjectionInterestSummary scenario,
        decimal? scenarioFinancingCost = null) =>
        SimulatorInterestPresenter.BuildInterestComparison(baseline, scenario, scenarioFinancingCost);

    /// <summary>
    /// Verilen simülasyon projeksiyon dönemlerini analiz ederek özet zaman çizelgesi sunum modelini inşa eder.
    /// </summary>
    public SimulatorProjectionSummary Build(IReadOnlyList<CashFlowPeriodProjection> scenario)
    {
        if (scenario.Count == 0)
        {
            throw new InvalidOperationException("Simülasyon sonucu bulunamadı.");
        }

        var periodViews = scenario.Select(CreatePeriodView).ToArray();
        var highestNeed = periodViews.OrderByDescending(x => x.NeedTotal).ThenBy(x => x.Projection.PeriodStart).First();
        var lowestEnding = periodViews.OrderBy(x => x.EndingBalance).ThenBy(x => x.Projection.PeriodStart).First();
        var firstIncomeInsufficient = periodViews.FirstOrDefault(x => x.IncomeCoverage < 0m);
        var firstDeficit = periodViews.FirstOrDefault(x => x.EndingBalance < 0m);
        var deficitRecovery = FindDeficitRecovery(periodViews, firstDeficit);
        var burdenRelief = FindBurdenRelief(periodViews, lowestEnding);

        var withChips = periodViews
            .Select(p => p with
            {
                InsightChips = BuildChips(p, highestNeed, lowestEnding, firstIncomeInsufficient, firstDeficit, deficitRecovery, burdenRelief)
            })
            .ToArray();

        return new SimulatorProjectionSummary
        {
            NarrativeInsights = SimulatorTimelineNarrative.BuildNarrative(
                highestNeed, lowestEnding, firstIncomeInsufficient, firstDeficit, deficitRecovery, burdenRelief, periodViews[^1]),
            KeyMetrics = SimulatorTimelineNarrative.BuildKeyMetrics(
                highestNeed, lowestEnding, firstIncomeInsufficient, firstDeficit, deficitRecovery, periodViews[^1]),
            Periods = withChips,
            HighestNeedPeriod = highestNeed,
            LowestEndingPeriod = lowestEnding,
            FirstIncomeInsufficientPeriod = firstIncomeInsufficient,
            FirstDeficitPeriod = firstDeficit,
            DeficitRecoveryPeriod = deficitRecovery,
            BurdenReliefPeriod = burdenRelief,
            EndingBalance = periodViews[^1].EndingBalance
        };
    }

    private static SimulatorPeriodView CreatePeriodView(CashFlowPeriodProjection row)
    {
        var need = SimulatorProjectionMath.PeriodNeed(row);
        return new SimulatorPeriodView
        {
            Projection = row,
            Period = FormatPeriodTitle(row.PeriodStart),
            OpeningBalance = row.OpeningBalance,
            Income = row.TotalIncome,
            NeedTotal = need,
            IncomeCoverage = row.TotalIncome - need,
            EndingBalance = row.EndingBalance,
            NeedBreakdown = SimulatorProjectionMath.BuildNeedBreakdown(row),
            InsightChips = []
        };
    }

    private static SimulatorPeriodView? FindDeficitRecovery(
        IReadOnlyList<SimulatorPeriodView> periods,
        SimulatorPeriodView? firstDeficit)
    {
        if (firstDeficit is null)
        {
            return null;
        }

        return periods
            .SkipWhile(x => x.Projection.PeriodStart <= firstDeficit.Projection.PeriodStart)
            .FirstOrDefault(x => x.EndingBalance >= 0m);
    }

    private static SimulatorPeriodView? FindBurdenRelief(
        IReadOnlyList<SimulatorPeriodView> periods,
        SimulatorPeriodView lowestEnding)
    {
        var lowestIndex = periods.ToList().FindIndex(x => SimulatorTimelineNarrative.SamePeriod(x, lowestEnding));
        if (lowestIndex < 0)
        {
            return null;
        }

        for (var i = lowestIndex + 1; i < periods.Count; i++)
        {
            var prev = periods[i - 1];
            var curr = periods[i];
            var needDrop = prev.NeedTotal - curr.NeedTotal;
            var materialDrop = needDrop >= 10_000m && prev.NeedTotal > 0m && (needDrop / prev.NeedTotal) >= 0.15m;
            var trendContinues = i == periods.Count - 1 || periods[i + 1].EndingBalance >= curr.EndingBalance;
            if (materialDrop && curr.EndingBalance > prev.EndingBalance && trendContinues)
            {
                return curr;
            }
        }

        return null;
    }

    private static string[] BuildChips(
        SimulatorPeriodView period,
        SimulatorPeriodView highestNeed,
        SimulatorPeriodView lowestEnding,
        SimulatorPeriodView? firstIncomeInsufficient,
        SimulatorPeriodView? firstDeficit,
        SimulatorPeriodView? deficitRecovery,
        SimulatorPeriodView? burdenRelief)
    {
        var candidates = new List<(int Priority, string Text)>();
        if (SimulatorTimelineNarrative.SamePeriod(period, firstDeficit))
        {
            candidates.Add((0, "Finansman açığı oluşuyor"));
        }

        if (SimulatorTimelineNarrative.SamePeriod(period, deficitRecovery))
        {
            candidates.Add((1, "Açık bu ay kapanıyor"));
        }

        if (SimulatorTimelineNarrative.SamePeriod(period, firstIncomeInsufficient))
        {
            candidates.Add((2, "Gelirler bu ay tek başına yetmiyor"));
        }

        if (SimulatorTimelineNarrative.SamePeriod(period, lowestEnding))
        {
            candidates.Add((3, "En düşük dönem sonu"));
        }

        if (SimulatorTimelineNarrative.SamePeriod(period, highestNeed))
        {
            candidates.Add((4, "En yüksek ihtiyaç"));
        }

        if (SimulatorTimelineNarrative.SamePeriod(period, burdenRelief))
        {
            candidates.Add((5, "Ödeme yükü belirgin azalıyor"));
        }

        return candidates.OrderBy(x => x.Priority).Select(x => x.Text).Distinct().Take(2).ToArray();
    }

    private static string FormatPeriodTitle(DateOnly date) =>
        $"{date.Day} {date.ToString("MMMM yyyy", TurkishCulture)} Dönemi";
}
