namespace Mizan.Presentation.Models;

/// <summary>
/// 12 dönemlik What-If simülasyon sonuçlarının zaman çizelgesi hikayesini, anahtar metriklerini ve dönem kartlarını içeren özet sunum modeli.
/// </summary>
public sealed record SimulatorProjectionSummary
{
    /// <summary>Kullanıcıya sunulacak 2–5 cümlelik anlatı içgörüleri listesi.</summary>
    public required IReadOnlyList<string> NarrativeInsights { get; init; }

    /// <summary>Temel gösterge metrikleri (en sıkışık dönem, en yüksek ihtiyaç vb.).</summary>
    public required IReadOnlyList<SimulatorSummaryMetric> KeyMetrics { get; init; }

    /// <summary>12 dönemin her biri için hazırlanmış sunum görünümleri listesi.</summary>
    public required IReadOnlyList<SimulatorPeriodView> Periods { get; init; }

    /// <summary>12 ay içinde en yüksek toplam nakit ihtiyacının olduğu dönem.</summary>
    public required SimulatorPeriodView HighestNeedPeriod { get; init; }

    /// <summary>12 ay içinde dönem sonu bakiyesinin en düşük seviyeye indiği en sıkışık dönem.</summary>
    public required SimulatorPeriodView LowestEndingPeriod { get; init; }

    /// <summary>Dönem gelirlerinin toplam ihtiyacı karşılayamadığı ilk dönem (varsa).</summary>
    public SimulatorPeriodView? FirstIncomeInsufficientPeriod { get; init; }

    /// <summary>İlk kez finansman açığı (KMH) oluşan dönem (varsa).</summary>
    public SimulatorPeriodView? FirstDeficitPeriod { get; init; }

    /// <summary>Oluşan finansman açığının yeniden pozitife dönerek kapandığı ilk dönem (varsa).</summary>
    public SimulatorPeriodView? DeficitRecoveryPeriod { get; init; }

    /// <summary>Ödeme yükünün belirgin azalarak toparlanmanın başladığı dönem (varsa).</summary>
    public SimulatorPeriodView? BurdenReliefPeriod { get; init; }

    /// <summary>12. dönemin sonundaki nihai bakiye durumu.</summary>
    public required decimal EndingBalance { get; init; }
}
