using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Simülatör görünüm modellerinin testlerinde çalışma listesini bellekte tutan sahte servis. Yazılan liste
/// aynen geri okunur; sorun işareti (S76-5) servisin kuralı olduğu için testte <see cref="Issues"/> ile verilir.
/// </summary>
public sealed class FakeSimulationWorkflowService : ISimulationWorkflowService
{
    private List<SimulationDraftCondition> _conditions = [];

    /// <summary>Kimliğe göre sorun; listede olmayan deneme sorunsuz okunur.</summary>
    public Dictionary<Guid, SimulationConditionIssue> Issues { get; } = new();

    /// <summary>Son yazılan liste; hiç yazılmadıysa <c>null</c>.</summary>
    public IReadOnlyList<SimulationDraftCondition>? LastSaved { get; private set; }

    public int SaveCount { get; private set; }

    public Exception? ThrowOnGet { get; set; }

    public Exception? ThrowOnSave { get; set; }

    /// <summary>Testin başında listeyi kurar; yazma sayılmaz.</summary>
    public void Seed(params SimulationDraftCondition[] conditions) => _conditions = [.. conditions];

    public Task<IReadOnlyList<SimulationWorkingCondition>> GetWorkingListAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnGet is not null)
        {
            throw ThrowOnGet;
        }

        IReadOnlyList<SimulationWorkingCondition> list = _conditions
            .Select(x => new SimulationWorkingCondition(x.Request, x.IsEnabled, Issues.GetValueOrDefault(x.Request.ScenarioId)))
            .ToList();
        return Task.FromResult(list);
    }

    public Task SaveWorkingListAsync(IReadOnlyList<SimulationDraftCondition> conditions, CancellationToken cancellationToken = default)
    {
        if (ThrowOnSave is not null)
        {
            throw ThrowOnSave;
        }

        SaveCount++;
        LastSaved = conditions;
        _conditions = [.. conditions];
        return Task.CompletedTask;
    }

    public Task<SimulationResult> SimulateAsync(SimulationRequest request, DateOnly? asOf = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<SimulationResult> SimulateAsync(
        IReadOnlyList<SimulationRequest> requests, DateOnly? asOf = null, decimal? variableExpenseAllowanceOverride = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<SimulationApplyResult> ApplySimulationAsync(SimulationRequest request, bool confirmed, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<SimulationApplyResult> ApplySimulationAsync(
        IReadOnlyList<SimulationRequest> requests, bool confirmed, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<SimulationApplyResult> AddRecordFromScenarioAsync(SimulationRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}
