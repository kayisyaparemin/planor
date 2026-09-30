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
    ChartSeries? Markers);
