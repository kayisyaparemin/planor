namespace Mizan.Presentation.Models;

/// <summary>
/// Simülasyon faiz karşılaştırma tablosunun tek bir faiz kalemi satırını ve baz/senaryo farkını temsil eder.
/// </summary>
public sealed record SimulatorInterestRow(
    string Label,
    string Baseline,
    string Scenario,
    string Difference,
    decimal DifferenceAmount)
{
    /// <summary>Toplam satırı olup olmadığı.</summary>
    public bool IsTotal { get; init; }
    /// <summary>Senaryonun bu kalemde faiz tasarrufu sağlayıp sağlamadığı.</summary>
    public bool IsSaving => DifferenceAmount < 0m;

    /// <summary>Senaryonun bu kalemde ek faiz maliyeti doğurup doğurmadığı.</summary>
    public bool IsExtra => DifferenceAmount > 0m;

    /// <summary>Baz durumdan senaryo durumuna geçiş özeti (örn. "7.081,00 TL → 941,00 TL").</summary>
    public string Transition => $"{Baseline} → {Scenario}";
}
