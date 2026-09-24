using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcının What-If senaryo simülasyonlarını koşturmasını, geçici taslak olarak saklamasını
/// ve onaylanan senaryoları canlı finansal plana aktarmasını sağlayan kullanım senaryosu portu.
/// </summary>
public interface ISimulationWorkflowService
{
    /// <summary>
    /// Tek bir simülasyon isteği için mevcut planı temel alarak 12 dönemlik karşılaştırma projeksiyonunu hesaplar.
    /// </summary>
    Task<SimulationResult> SimulateAsync(
        SimulationRequest request,
        DateOnly? asOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Çoklu simülasyon istekleri için isteğe bağlı harcama havuzu ezmesiyle 12 dönemlik karşılaştırma projeksiyonunu hesaplar.
    /// </summary>
    Task<SimulationResult> SimulateAsync(
        IReadOnlyList<SimulationRequest> requests,
        DateOnly? asOf = null,
        decimal? variableExpenseAllowanceOverride = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının simülatörde belirlediği koşulları isimli bir taslak olarak kalıcı veri deposuna kaydeder veya günceller.
    /// </summary>
    Task<SimulationDraft> SaveSimulationDraftAsync(
        string name,
        IReadOnlyList<SimulationDraftCondition> conditions,
        Guid? draftId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kayıtlı tüm simülasyon taslaklarını son güncellenme tarihine göre azalan sırada getirir.
    /// </summary>
    Task<IReadOnlyList<SimulationDraft>> GetSimulationDraftsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kimliğe sahip simülasyon taslağını ve tüm koşullarını kalıcı olarak siler.
    /// </summary>
    Task DeleteSimulationDraftAsync(
        Guid id,
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
