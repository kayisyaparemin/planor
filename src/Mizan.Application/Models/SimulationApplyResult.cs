namespace Mizan.Application.Models;

/// <summary>
/// Bir simülasyon koşulunun veya planının canlı finansal plana uygulanması sonucunda üretilen sonuç raporu.
/// </summary>
public sealed record SimulationApplyResult(
    Guid ScenarioId,
    Guid EntityId,
    SimulationApplyDestination Destination,
    bool AlreadyApplied,
    string Message);
