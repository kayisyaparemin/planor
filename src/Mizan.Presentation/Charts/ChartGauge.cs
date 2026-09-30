namespace Mizan.Presentation.Charts;

/// <summary>
/// Halka göstergesinin ham verisi: doluluk ve geçen sürenin oranı. İkisi birlikte anlam taşır (harcama temposu,
/// S70); tek kayıtta durmaları halkanın iki değeri hep aynı yüklemeden çizmesini sağlar.
/// </summary>
/// <param name="Ratio">Doluluk oranı, 0 ile 1 arasında.</param>
/// <param name="TimeRatio">Geçen sürenin oranı, 0 ile 1 arasında; işaret çizilmeyecekse <c>null</c>.</param>
public sealed record ChartGauge(decimal Ratio, decimal? TimeRatio);
