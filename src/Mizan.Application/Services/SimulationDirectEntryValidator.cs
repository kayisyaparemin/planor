using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Finansal Yapı ekranındaki ortak formdan simülasyon senaryo türlerinin
/// doğrudan eklenip eklenemeyeceğini doğrulayan yardımcı sınıftır.
/// Kendi özel ekranı olan türlerin (düzenli gelir, kart ödeme tercihi vb.) ortak formdan girilmesini engeller.
/// </summary>
public static class SimulationDirectEntryValidator
{
    /// <summary>
    /// Belirtilen simülasyon senaryo türünün Finansal Yapı ekranındaki ortak formdan doğrudan girilip girilemeyeceğini belirtir.
    /// </summary>
    public static bool IsDirectEntry(SimulationScenarioType type) =>
        SimulationScenarioCatalog.IsDirectEntry(type);

    /// <summary>
    /// Senaryo türünün doğrudan giriş için uygun olup olmadığını doğrular; uygun değilse kural ihlali fırlatır.
    /// </summary>
    public static void Validate(SimulationScenarioType type)
    {
        if (!IsDirectEntry(type))
        {
            throw new InvalidOperationException($"{SimulationScenarioCatalog.TypeText(type)} Finansal Yapı'dan bu formla girilemez.");
        }
    }
}
