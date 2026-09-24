using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Tests.Fakes;

public sealed class InMemorySimulationDraftRepository : ISimulationDraftRepository
{
    private readonly Dictionary<Guid, SimulationDraft> _drafts = new();

    public Task<IReadOnlyList<SimulationDraft>> GetDraftsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SimulationDraft> list = _drafts.Values
            .OrderByDescending(x => x.UpdatedAt)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<SimulationDraft?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _drafts.TryGetValue(id, out var draft);
        return Task.FromResult(draft);
    }

    public Task UpsertDraftAsync(SimulationDraft draft, CancellationToken cancellationToken = default)
    {
        _drafts[draft.Id] = draft;
        return Task.CompletedTask;
    }

    public Task DeleteDraftAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _drafts.Remove(id);
        return Task.CompletedTask;
    }
}
