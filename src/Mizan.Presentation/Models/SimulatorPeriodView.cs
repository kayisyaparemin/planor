using System.Globalization;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// Simülasyon zaman çizelgesindeki tek bir nakit akış döneminin sunum görünümünü ve içgörü çiplerini taşır.
/// </summary>
public sealed record SimulatorPeriodView
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>İlgili dönemin saf Domain projeksiyon verisi.</summary>
    public required CashFlowPeriodProjection Projection { get; init; }

    /// <summary>Dönem başlığı (örn. "Mart 2027 Dönemi").</summary>
    public required string Period { get; init; }

    /// <summary>Dönem başı açılış bakiyesi.</summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>Dönem toplam geliri.</summary>
    public required decimal Income { get; init; }

    /// <summary>Dönem için gereken toplam nakit ihtiyacı.</summary>
    public required decimal NeedTotal { get; init; }

    /// <summary>Dönem gelirlerinin ihtiyacı karşılama farkı (Gelir - İhtiyaç).</summary>
    public required decimal IncomeCoverage { get; init; }

    /// <summary>Dönem sonu tahmini bakiyesi.</summary>
    public required decimal EndingBalance { get; init; }

    /// <summary>Gereken toplam ihtiyacın kalem bazında kırılımı.</summary>
    public required IReadOnlyList<DetailMetric> NeedBreakdown { get; init; }

    /// <summary>Bu döneme atanan simülasyon içgörü etiketleri/çipleri.</summary>
    public required IReadOnlyList<string> InsightChips { get; init; }

    /// <summary>Biçimlendirilmiş gelir metni.</summary>
    public string IncomeText => Money(Income);

    /// <summary>Biçimlendirilmiş toplam ihtiyaç metni.</summary>
    public string NeedText => Money(NeedTotal);

    /// <summary>Biçimlendirilmiş gelir karşılama tutarı metni.</summary>
    public string CoverageAmountText => Money(Math.Abs(IncomeCoverage));

    /// <summary>Biçimlendirilmiş dönem sonu metni.</summary>
    public string EndingText => Money(EndingBalance);

    /// <summary>Gelir karşılama durumu etiketi.</summary>
    public string CoverageLabel => IncomeCoverage >= 0m
        ? "Gelirlerden kalan"
        : "Gelirlerin karşılamadığı";

    /// <summary>Dönem sonunun negatif açık olup olmadığı.</summary>
    public bool EndingIsNegative => EndingBalance < 0m;

    /// <summary>Birincil içgörü çipi metni.</summary>
    public string PrimaryInsight => InsightChips.Count > 0 ? InsightChips[0] : string.Empty;

    /// <summary>Birincil içgörünün olup olmadığı.</summary>
    public bool HasPrimaryInsight => InsightChips.Count > 0;

    /// <summary>İkincil içgörü çipi metni.</summary>
    public string SecondaryInsight => InsightChips.Count > 1 ? InsightChips[1] : string.Empty;

    /// <summary>İkincil içgörünün olup olmadığı.</summary>
    public bool HasSecondaryInsight => InsightChips.Count > 1;

    private static string Money(decimal value) => $"{value.ToString("N2", TurkishCulture)} TL";
}
