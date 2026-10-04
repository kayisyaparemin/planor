using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kullanıcının denemelerini tek çalışma listesinde saklamasını (S76-4) ve açık onayla canlı plana aktarmasını
/// sağlayan kullanım senaryosu servisidir. Sonucu hesaplamaz: o iş <see cref="SimulationResultService"/>'in (S76-6).
/// </summary>
public sealed class SimulationWorkflowService(
    IClock clock,
    IPlanReader planReader,
    ISimulationDraftRepository draftRepository,
    ISimulationPlanApplier planApplier) : ISimulationWorkflowService
{
    // Çalışma listesi taslak tablosunda tek kayıttır (S76-4): kimliği sabit, adı hiçbir ekranda görünmez.
    private static readonly Guid WorkingListId = new("6a1f0c3e-2b7d-4e59-9c1a-5d8e7f3b2a10");
    private const string WorkingListName = "Çalışma listesi";

    private readonly IClock _clock =
        clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly IPlanReader _planReader =
        planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly ISimulationDraftRepository _draftRepository =
        draftRepository ?? throw new ArgumentNullException(nameof(draftRepository));
    private readonly ISimulationPlanApplier _planApplier =
        planApplier ?? throw new ArgumentNullException(nameof(planApplier));

    /// <inheritdoc />
    public async Task<IReadOnlyList<SimulationWorkingCondition>> GetWorkingListAsync(
        CancellationToken cancellationToken = default)
    {
        var draft = await _draftRepository.GetDraftByIdAsync(WorkingListId, cancellationToken);
        if (draft is null)
        {
            return [];
        }

        var today = _clock.Today;
        return draft.Conditions
            .Select(x => new SimulationWorkingCondition(x.Request, x.IsEnabled, IssueOf(x.Request, today)))
            .ToArray();
    }

    /// <inheritdoc />
    public async Task SaveWorkingListAsync(
        IReadOnlyList<SimulationDraftCondition> conditions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        var existing = await _draftRepository.GetDraftByIdAsync(WorkingListId, cancellationToken);
        var now = _clock.UtcNow;
        var draft = new SimulationDraft(WorkingListId, WorkingListName, existing?.CreatedAt ?? now, now, conditions);
        await _draftRepository.UpsertDraftAsync(draft, cancellationToken);
    }

    // Sorun saklanmaz, her okumada bugüne göre değerlendirilir: dün geçerli olan deneme bugün geçmiş olabilir (S76-5).
    private static SimulationConditionIssue IssueOf(SimulationRequest request, DateOnly today) =>
        SimulationConditionRules.IsDatePassed(request, today)
            ? SimulationConditionIssue.DatePassed
            : SimulationConditionIssue.None;


    /// <inheritdoc />
    public async Task<SimulationApplyResult> ApplySimulationAsync(
        IReadOnlyList<SimulationRequest> requests,
        bool confirmed,
        CancellationToken cancellationToken = default)
    {
        if (!confirmed)
        {
            throw new InvalidOperationException("Plan, açık kullanıcı onayı olmadan uygulanamaz.");
        }

        var plan = await _planReader.GetPlanAsync(cancellationToken);
        return await _planApplier.ApplyAsync(plan, requests, "Simülasyon planı uygulandı", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SimulationApplyResult> AddRecordFromScenarioAsync(
        SimulationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        SimulationDirectEntryValidator.Validate(request.Type);

        var plan = await _planReader.GetPlanAsync(cancellationToken);
        var anchor = plan.Settings.ProjectionAnchorDate;
        SimulationRequestValidator.Validate(request, anchor == default ? null : anchor);

        return await _planApplier.ApplyAsync(plan, [request], "Finansal Yapı'dan eklendi", cancellationToken);
    }
}
