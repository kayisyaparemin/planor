namespace Mizan.Presentation.Models;

/// <summary>
/// Simülasyon sonuç ekranındaki özet anahtar metrik kartını (örn. en sıkışık dönem, en yüksek ihtiyaç) temsil eder.
/// </summary>
public sealed record SimulatorSummaryMetric(
    string Label,
    string Value,
    string Detail = "",
    DetailSemanticType Semantic = DetailSemanticType.Projection)
{
    /// <summary>Detay açıklamasının bulunup bulunmadığı.</summary>
    public bool HasDetail => !string.IsNullOrWhiteSpace(Detail);
}
