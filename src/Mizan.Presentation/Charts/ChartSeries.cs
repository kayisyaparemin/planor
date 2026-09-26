namespace Mizan.Presentation.Charts;

/// <summary>
/// Grafik bileşenlerine sunum katmanından aktarılan adlandırılmış ve sıralı veri dizisini temsil etmek için vardır.
/// </summary>
/// <param name="Key">Serinin tasarım sistemi rol anahtarı (örn: "actual", "planned").</param>
/// <param name="Points">Zaman içindeki veri noktaları koleksiyonu.</param>
public sealed record ChartSeries(string Key, IReadOnlyList<ChartPoint> Points);
