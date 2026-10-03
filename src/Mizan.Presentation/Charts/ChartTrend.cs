namespace Mizan.Presentation.Charts;

/// <summary>
/// Tarih eksenli trend grafiğinin ekrandan aldığı ham veri paketi. Ana sayfa ile "Bakiye gir" önizlemesi aynı
/// bakiye rotasını çizer; değerleri tek kayıtta taşımak iki ekranın grafiği aynı biçimde kurmasını sağlar ve
/// sayfada tek bir bağlamaya yeter.
/// </summary>
/// <param name="Series">Düz çizilen seri: katedilen yol.</param>
/// <param name="Projection">Kesikli devam: önümüzdeki yol, ilk noktası serinin son noktası; yoksa <c>null</c>.</param>
/// <param name="Today">"Bugün" çizgisinin günü; yoksa <c>null</c>.</param>
/// <param name="PlanLevel">Planın dönem sonu seviyesi; yoksa <c>null</c>.</param>
/// <param name="Markers">Nokta işareti konan günler: kullanıcının bakiye girdiği günler (S68-1); yoksa <c>null</c>.</param>
public sealed record ChartTrend(
    ChartSeries Series,
    ChartSeries? Projection,
    DateOnly? Today,
    ChartThreshold? PlanLevel,
    ChartSeries? Markers)
{
    /// <summary>
    /// Eşik çizgisi; verilirse ölçeğe girer ve alan dolgusu tabana değil ona iner. 12 dönem sıfırı verir: sıfıra
    /// uzaklık güvenlik payıdır, eksi dönemler sıfırın altında ayrı bir cep olur (GS26-3, 4). Ana sayfa vermez.
    /// </summary>
    public ChartThreshold? Threshold { get; init; }

    /// <summary>
    /// Karşılaştırma serisi: simülatörde denemeli çizginin yanında "şu anki gidişat" (<c>planned</c> rolü: kesikli,
    /// dolgusuz). Verilmezse tek çizgi vardır ve grafik 12 Dönem'inkiyle aynıdır (GS28-2).
    /// </summary>
    public ChartSeries? Comparison { get; init; }
}
