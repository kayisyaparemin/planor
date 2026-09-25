using System.Globalization;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.Presenters;

/// <summary>
/// Simülasyon zaman çizelgesinin 2–5 cümlelik anlatı içgörülerini ve anahtar metriklerini derleyen sunum yardımcısı.
/// </summary>
internal static class SimulatorTimelineNarrative
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    public static IReadOnlyList<string> BuildNarrative(
        SimulatorPeriodView highestNeed,
        SimulatorPeriodView lowestEnding,
        SimulatorPeriodView? firstIncomeInsufficient,
        SimulatorPeriodView? firstDeficit,
        SimulatorPeriodView? deficitRecovery,
        SimulatorPeriodView? burdenRelief,
        SimulatorPeriodView finalPeriod)
    {
        var sentences = new List<string>();
        AddDeficitNarrative(sentences, firstDeficit, deficitRecovery);

        if (firstIncomeInsufficient is not null && !SamePeriod(firstIncomeInsufficient, firstDeficit))
        {
            sentences.Add(
                $"{Month(firstIncomeInsufficient)} döneminde dönem gelirlerin toplam ihtiyacını tek başına karşılamıyor; yaklaşık {Money(Math.Abs(firstIncomeInsufficient.IncomeCoverage))} dönem başı durumundan kullanılıyor.");
        }

        AddLowestAndHighestNarrative(sentences, highestNeed, lowestEnding, firstDeficit);

        if (burdenRelief is not null)
        {
            sentences.Add(
                $"{Month(burdenRelief)} döneminde ödeme yükü belirgin azalıyor ve finansal durum yeniden toparlanmaya başlıyor.");
        }

        if (sentences.Count < 2)
        {
            sentences.Add($"12 ay sonu tahmini durumun {Money(finalPeriod.EndingBalance)}.");
        }

        return sentences.Distinct().Take(5).ToArray();
    }

    public static IReadOnlyList<SimulatorSummaryMetric> BuildKeyMetrics(
        SimulatorPeriodView highestNeed,
        SimulatorPeriodView lowestEnding,
        SimulatorPeriodView? firstIncomeInsufficient,
        SimulatorPeriodView? firstDeficit,
        SimulatorPeriodView? deficitRecovery,
        SimulatorPeriodView finalPeriod)
    {
        var metrics = new List<SimulatorSummaryMetric>
        {
            BuildTightestPeriodMetric(lowestEnding),
            new("En yüksek ihtiyaç", Month(highestNeed), Money(highestNeed.NeedTotal), DetailSemanticType.Mandatory)
        };

        AddOptionalMetrics(metrics, firstIncomeInsufficient, firstDeficit, deficitRecovery);
        metrics.Add(new SimulatorSummaryMetric(
            "12 ay sonu",
            Money(finalPeriod.EndingBalance),
            string.Empty,
            finalPeriod.EndingBalance < 0m ? DetailSemanticType.Deficit : DetailSemanticType.Projection));

        return metrics;
    }

    public static string Month(SimulatorPeriodView? period) =>
        period?.Projection.PeriodStart.ToString("MMMM yyyy", TurkishCulture) ?? string.Empty;

    public static bool SamePeriod(SimulatorPeriodView? left, SimulatorPeriodView? right) =>
        left is not null && right is not null && left.Projection.PeriodStart == right.Projection.PeriodStart;

    public static string Money(decimal value) => $"{value.ToString("N2", TurkishCulture)} TL";

    private static void AddDeficitNarrative(
        List<string> sentences,
        SimulatorPeriodView? firstDeficit,
        SimulatorPeriodView? deficitRecovery)
    {
        if (firstDeficit is not null)
        {
            sentences.Add(
                $"{Month(firstDeficit)} döneminde finansman açığı oluşuyor; dönem sonu tahmini açık {Money(Math.Abs(firstDeficit.EndingBalance))}.");
            sentences.Add(deficitRecovery is not null
                ? $"Açığın {Month(deficitRecovery)} döneminde kapanması bekleniyor."
                : "Açık 12 dönemlik görünüm içinde kapanmıyor.");
        }
        else
        {
            sentences.Add("12 dönemlik görünümde finansman açığı oluşmuyor.");
        }
    }

    private static void AddLowestAndHighestNarrative(
        List<string> sentences,
        SimulatorPeriodView highestNeed,
        SimulatorPeriodView lowestEnding,
        SimulatorPeriodView? firstDeficit)
    {
        if (lowestEnding.EndingBalance < 0m)
        {
            if (!SamePeriod(lowestEnding, firstDeficit))
            {
                sentences.Add(
                    $"En sıkışık dönem {Month(lowestEnding)}; yaklaşık {Money(Math.Abs(lowestEnding.EndingBalance))} finansman açığı oluşuyor.");
            }
        }
        else
        {
            sentences.Add(
                $"En sıkışık dönem {Month(lowestEnding)}; dönem sonu tahmini durumun {Money(lowestEnding.EndingBalance)}.");
        }

        if (!SamePeriod(highestNeed, lowestEnding) && highestNeed.NeedTotal > 0m)
        {
            sentences.Add(
                $"Önündeki 12 ayda en yüksek toplam ihtiyaç {Month(highestNeed)} döneminde: {Money(highestNeed.NeedTotal)}.");
        }
    }

    private static SimulatorSummaryMetric BuildTightestPeriodMetric(SimulatorPeriodView lowestEnding)
    {
        var isNegative = lowestEnding.EndingBalance < 0m;
        return new SimulatorSummaryMetric(
            "En sıkışık dönem",
            Month(lowestEnding),
            isNegative ? $"{Money(Math.Abs(lowestEnding.EndingBalance))} açık" : Money(lowestEnding.EndingBalance),
            isNegative ? DetailSemanticType.Deficit : DetailSemanticType.Projection);
    }

    private static void AddOptionalMetrics(
        List<SimulatorSummaryMetric> metrics,
        SimulatorPeriodView? firstIncomeInsufficient,
        SimulatorPeriodView? firstDeficit,
        SimulatorPeriodView? deficitRecovery)
    {
        if (firstIncomeInsufficient is not null)
        {
            metrics.Add(new SimulatorSummaryMetric(
                "Gelirin ilk yetmediği dönem",
                Month(firstIncomeInsufficient),
                Money(Math.Abs(firstIncomeInsufficient.IncomeCoverage)),
                DetailSemanticType.Projection));
        }

        metrics.Add(firstDeficit is null
            ? new SimulatorSummaryMetric("Finansman açığı", "Yok", "12 ay içinde oluşmuyor", DetailSemanticType.Surplus)
            : new SimulatorSummaryMetric("Finansman açığı", Month(firstDeficit), Money(Math.Abs(firstDeficit.EndingBalance)), DetailSemanticType.Deficit));

        if (deficitRecovery is not null)
        {
            metrics.Add(new SimulatorSummaryMetric("Açığın kapanışı", Month(deficitRecovery), Money(deficitRecovery.EndingBalance), DetailSemanticType.Surplus));
        }
    }
}
