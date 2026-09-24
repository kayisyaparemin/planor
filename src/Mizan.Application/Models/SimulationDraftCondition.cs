using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kaydedilmiş bir simülasyon taslağı içerisindeki tekil senaryo koşulunu
/// ve kullanıcının bu koşulu aktif/pasif tutma tercihini temsil eder.
/// </summary>
public sealed record SimulationDraftCondition(
    SimulationRequest Request,
    bool IsEnabled = true);
