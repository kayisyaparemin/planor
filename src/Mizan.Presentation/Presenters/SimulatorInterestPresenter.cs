using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.Presenters;

/// <summary>
/// Baz durum ile What-If simülasyon senaryosu arasındaki faiz maliyetlerini karşılaştırma tablosuna dönüştüren sunum yardımcısı.
/// </summary>
public static class SimulatorInterestPresenter
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Baz durum ve senaryo arasındaki kart, KMH ve kredi faiz maliyetlerini kıyaslar.
    /// </summary>
    public static List<SimulatorInterestRow> BuildInterestComparison(
        ProjectionInterestSummary baseline,
        ProjectionInterestSummary scenario,
        decimal? scenarioFinancingCost = null)
    {
        var financing = scenarioFinancingCost ?? 0m;
        var rows = new List<SimulatorInterestRow>
        {
            InterestRow("Kredi kartı faizi", baseline.CreditCardInterest, scenario.CreditCardInterest),
            InterestRow("Finansman açığı (KMH) faizi", baseline.DeficitFinancingInterest, scenario.DeficitFinancingInterest)
        };

        if (financing > 0m)
        {
            rows.Add(InterestRow("Kredi finansman maliyeti", 0m, financing));
        }

        rows.Add(InterestRow(
            "Toplam faiz yükü",
            baseline.TotalInterestCost,
            scenario.TotalInterestCost + financing,
            isTotal: true));

        return rows;
    }

    private static SimulatorInterestRow InterestRow(
        string label,
        decimal baseline,
        decimal scenario,
        bool isTotal = false)
    {
        var difference = scenario - baseline;
        var differenceText = difference switch
        {
            0m => "Değişmiyor",
            > 0m => $"+{Money(difference)}",
            _ => Money(difference)
        };

        return new SimulatorInterestRow(
            label,
            Money(baseline),
            Money(scenario),
            differenceText,
            difference)
        {
            IsTotal = isTotal
        };
    }

    private static string Money(decimal value) => $"{value.ToString("N2", TurkishCulture)} TL";
}
