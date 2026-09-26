namespace Mizan.Presentation.Charts;

/// <summary>
/// Grafiklerde kritik finansal sınırları veya referans seviyelerini (örn: sıfır bakiye, KMH limiti) temsil etmek için vardır.
/// </summary>
/// <param name="Value">Referans eşik seviyesinin sayısal değeri.</param>
public sealed record ChartThreshold(decimal Value);
