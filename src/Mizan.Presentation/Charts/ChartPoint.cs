namespace Mizan.Presentation.Charts;

/// <summary>
/// Grafik serilerinde zaman içindeki tek bir finansal veri noktasını temsil etmek için vardır.
/// </summary>
/// <param name="Date">Veri noktasının ait olduğu tarih.</param>
/// <param name="Value">Veri noktasının sayısal değeri.</param>
public sealed record ChartPoint(DateOnly Date, decimal Value);
