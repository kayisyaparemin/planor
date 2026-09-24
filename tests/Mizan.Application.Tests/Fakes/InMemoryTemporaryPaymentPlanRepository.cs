using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi geçici ödeme planı deposu implementasyonu.
/// </summary>
public sealed class InMemoryTemporaryPaymentPlanRepository : ITemporaryPaymentPlanRepository
{
    private readonly Dictionary<Guid, TemporaryPaymentPlan> _plans = [];

    public Task<IReadOnlyList<TemporaryPaymentPlan>> GetPaymentPlansAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<TemporaryPaymentPlan> result = _plans.Values.OrderBy(x => x.Name).ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertPaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _plans[plan.Id] = plan;
        return Task.CompletedTask;
    }

    public Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _plans.Remove(id);
        return Task.CompletedTask;
    }
}
