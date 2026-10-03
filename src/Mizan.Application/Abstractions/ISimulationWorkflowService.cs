using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcının What-If denemelerini tek çalışma listesinde saklamasını ve onaylanan denemeleri canlı finansal
/// plana aktarmasını sağlayan kullanım senaryosu portu. Denemelerin sonucu burada hesaplanmaz:
/// <see cref="ISimulationResultService"/> okur, bu port yazar (M4).
/// </summary>
public interface ISimulationWorkflowService
{
    /// <summary>
    /// Simülatörün çalışma listesini kurulduğu sırayla okur (S76-4); her denemenin sorunu bugüne göre
    /// değerlendirilir (S76-5). Liste hiç yazılmadıysa boş döner.
    /// </summary>
    Task<IReadOnlyList<SimulationWorkingCondition>> GetWorkingListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Çalışma listesini verilen sırayla baştan yazar: ekleme, düzenleme, silme ve aç/kapa hep bu yoldan geçer
    /// (S76-4). Boş liste listeyi temizler.
    /// </summary>
    Task SaveWorkingListAsync(
        IReadOnlyList<SimulationDraftCondition> conditions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tek bir simülasyon isteğini açık kullanıcı onayıyla canlı finansal plana uygular.
    /// </summary>
    Task<SimulationApplyResult> ApplySimulationAsync(
        SimulationRequest request,
        bool confirmed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Çoklu simülasyon isteklerini açık kullanıcı onayıyla atomik olarak canlı finansal plana uygular.
    /// </summary>
    Task<SimulationApplyResult> ApplySimulationAsync(
        IReadOnlyList<SimulationRequest> requests,
        bool confirmed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finansal Yapı ekranından doğrudan girilen senaryo türlerini doğrular ve canlı finansal plana uygular.
    /// </summary>
    Task<SimulationApplyResult> AddRecordFromScenarioAsync(
        SimulationRequest request,
        CancellationToken cancellationToken = default);
}
