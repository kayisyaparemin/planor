using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Onaylanan simülasyon senaryo koşullarını mevcut plana uygulayıp dar veri depolarına kaydeden
/// ve plan değişikliği taahhüdünü tetikleyen uygulama portu.
/// </summary>
public interface ISimulationPlanApplier
{
    /// <summary>
    /// Senaryo isteklerini mevcut plana işler, ilgili dar depolara kaydeder ve plan revizyonunu tetikler.
    /// </summary>
    Task<SimulationApplyResult> ApplyAsync(
        FinancialPlan currentPlan,
        IReadOnlyList<SimulationRequest> requests,
        string trigger,
        CancellationToken cancellationToken = default);
}
