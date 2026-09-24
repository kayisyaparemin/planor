using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kullanıcının What-If senaryo simülasyonlarını koşturmasını, taslak olarak yönetmesini
/// ve açık onayla canlı plana aktarmasını sağlayan kullanım senaryosu servisidir.
/// </summary>
public sealed class SimulationWorkflowService(
    IClock clock,
    IPlanReader planReader,
    ISimulationDraftRepository draftRepository,
    SimulationCalculator simulationCalculator,
    ISimulationPlanApplier planApplier) : ISimulationWorkflowService
{
    private readonly IClock _clock =
        clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly IPlanReader _planReader =
        planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly ISimulationDraftRepository _draftRepository =
        draftRepository ?? throw new ArgumentNullException(nameof(draftRepository));
    private readonly SimulationCalculator _simulationCalculator =
        simulationCalculator ?? throw new ArgumentNullException(nameof(simulationCalculator));
    private readonly ISimulationPlanApplier _planApplier =
        planApplier ?? throw new ArgumentNullException(nameof(planApplier));

    /// <inheritdoc />
    public Task<SimulationResult> SimulateAsync(
        SimulationRequest request,
        DateOnly? asOf = null,
        CancellationToken cancellationToken = default) =>
        SimulateAsync([request ?? throw new ArgumentNullException(nameof(request))], asOf, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public async Task<SimulationResult> SimulateAsync(
        IReadOnlyList<SimulationRequest> requests,
        DateOnly? asOf = null,
        decimal? variableExpenseAllowanceOverride = null,
        CancellationToken cancellationToken = default)
    {
        var date = asOf ?? _clock.Today;
        var query = await _planReader.GetProjectionPlanAsync(date, cancellationToken);

        if (!query.Plan.CanBuildProjection)
        {
            throw new InvalidOperationException(
                "Simülasyon yapabilmek için önce gelirini veya açılış bakiyeni tanımlamalısın.");
        }

        var plan = ApplyAllowanceOverride(query.Plan, variableExpenseAllowanceOverride);
        return _simulationCalculator.Calculate(
            plan,
            date,
            requests,
            firstPeriodStartDate: query.Boundary?.FirstUnrealizedPeriodStartDate);
    }

    /// <inheritdoc />
    public async Task<SimulationDraft> SaveSimulationDraftAsync(
        string name,
        IReadOnlyList<SimulationDraftCondition> conditions,
        Guid? draftId = null,
        CancellationToken cancellationToken = default)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new InvalidOperationException("Geçici plana bir ad vermelisin.");
        }

        if (conditions.Count == 0)
        {
            throw new InvalidOperationException("Kaydedilecek en az bir koşul gerekiyor.");
        }

        var existing = draftId is { } id
            ? await _draftRepository.GetDraftByIdAsync(id, cancellationToken)
            : null;

        var draft = new SimulationDraft(
            existing?.Id ?? (draftId ?? Guid.NewGuid()),
            trimmed,
            existing?.CreatedAt ?? _clock.UtcNow,
            _clock.UtcNow,
            conditions);

        await _draftRepository.UpsertDraftAsync(draft, cancellationToken);
        return draft;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SimulationDraft>> GetSimulationDraftsAsync(
        CancellationToken cancellationToken = default) =>
        _draftRepository.GetDraftsAsync(cancellationToken);

    /// <inheritdoc />
    public Task DeleteSimulationDraftAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _draftRepository.DeleteDraftAsync(id, cancellationToken);

    /// <inheritdoc />
    public Task<SimulationApplyResult> ApplySimulationAsync(
        SimulationRequest request,
        bool confirmed,
        CancellationToken cancellationToken = default) =>
        ApplySimulationAsync([request ?? throw new ArgumentNullException(nameof(request))], confirmed, cancellationToken);

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

    private static FinancialPlan ApplyAllowanceOverride(FinancialPlan plan, decimal? allowanceOverride) =>
        allowanceOverride is not { } budget || budget == plan.Settings.PeriodVariableExpenseAllowance
            ? plan
            : plan with
            {
                Settings = plan.Settings with
                {
                    PeriodVariableExpenseAllowance = budget
                }
            };
}
