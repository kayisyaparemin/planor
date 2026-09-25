namespace Mizan.Presentation.Models;

/// <summary>
/// Dönem ayrıntısı ekranının (V9) tüm gelir, borç, harcama, faiz ve senaryo karşılaştırma verilerini taşıyan sunum paketi.
/// </summary>
public sealed record CashFlowPeriodDetailData
{
    /// <summary>Dönemin başlangıç tarihi.</summary>
    public required DateOnly PeriodStart { get; init; }

    /// <summary>Ekran başlığı (örn: "20 Ağustos 2026 Dönemi").</summary>
    public required string PeriodTitle { get; init; }

    /// <summary>Doğal dönemsellik tarih aralığı metni (örn: "20 Ağustos – 20 Eylül").</summary>
    public required string PeriodWindowText { get; init; }

    /// <summary>Bu ayrıntının geçici bir simülasyon senaryosuna ait olup olmadığını belirtir.</summary>
    public required bool IsSimulationScenario { get; init; }

    /// <summary>Dönem başı bakiye özeti (DÖNEM BAŞI).</summary>
    public required DetailMetric OpeningBalanceSummary { get; init; }

    /// <summary>Bu dönem gereken toplam nakit ihtiyacı özeti (BU DÖNEM GEREKEN).</summary>
    public required DetailMetric PeriodNeedSummary { get; init; }

    /// <summary>Dönem gelirlerinin ihtiyacı karşılama durumu (GELİRLERDEN KALAN / GELİRLERİN KARŞILAMADIĞI).</summary>
    public required DetailMetric IncomeCoverageSummary { get; init; }

    /// <summary>Gelir karşılama durumu açıklama metni.</summary>
    public required string IncomeCoverageMessage { get; init; }

    /// <summary>Bu dönem gereken toplam ihtiyacın kalem bazında kırılımı.</summary>
    public required IReadOnlyList<DetailMetric> NeedBreakdownRows { get; init; }

    /// <summary>Dönem toplam geliri özeti (GELİR / DÖNEM GELİRLERİ).</summary>
    public required DetailMetric IncomeSummary { get; init; }

    /// <summary>Dönem zorunlu borç çıkışları özeti (ZORUNLU).</summary>
    public required DetailMetric MandatorySummary { get; init; }

    /// <summary>Dönem net fazlalık veya açık özeti (DÖNEM NETİ).</summary>
    public required DetailMetric NetSurplusSummary { get; init; }

    /// <summary>Dönem sonu tahmini durum özeti (DÖNEM SONU).</summary>
    public required DetailMetric EndingSummary { get; init; }

    /// <summary>Nakit akışı adım adım hesap dökümü.</summary>
    public required IReadOnlyList<DetailMetric> CashFlow { get; init; }

    /// <summary>Devreden finansman açığı çağrısı (varsa).</summary>
    public DetailDeficitCallout? Deficit { get; init; }

    /// <summary>Zorunlu ödemelerin kategori bazlı özet satırları (Krediler, Kartlar vb.).</summary>
    public required IReadOnlyList<DetailMetric> MandatoryRows { get; init; }

    /// <summary>Dönem içinde üretilen faizlerin kırılım satırları.</summary>
    public required IReadOnlyList<DetailMetric> InterestRows { get; init; }

    /// <summary>Kart bazında devreden borç faizi satırları.</summary>
    public required IReadOnlyList<DetailMetric> CardInterestRows { get; init; }

    /// <summary>Dönem içinde vadesi gelen münferit borç ve harcama ödeme satırları.</summary>
    public required IReadOnlyList<DetailPaymentRow> PaymentRows { get; init; }

    /// <summary>Baz durum ile simülasyon arasındaki karşılaştırma satırları.</summary>
    public required IReadOnlyList<DetailComparisonRow> ComparisonRows { get; init; }

    /// <summary>Karşılaştırma satırlarının bulunup bulunmadığını belirtir.</summary>
    public bool HasComparison => ComparisonRows.Count > 0;

    /// <summary>Devreden açık uyarısının bulunup bulunmadığını belirtir.</summary>
    public bool HasDeficit => Deficit is not null;

    /// <summary>Faiz satırlarının bulunup bulunmadığını belirtir.</summary>
    public bool HasInterestRows => InterestRows.Count > 0;

    /// <summary>Kart faiz satırlarının bulunup bulunmadığını belirtir.</summary>
    public bool HasCardInterestRows => CardInterestRows.Count > 0;
}
