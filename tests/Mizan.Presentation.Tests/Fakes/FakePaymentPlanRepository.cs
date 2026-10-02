using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

internal sealed class FakePaymentPlanRepository : ITemporaryPaymentPlanRepository
{
    public List<TemporaryPaymentPlan> Plans { get; set; } = [];

    public Task<IReadOnlyList<TemporaryPaymentPlan>> GetPaymentPlansAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TemporaryPaymentPlan>>(Plans);

    public Task UpsertPaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default)
    {
        Plans.RemoveAll(x => x.Id == plan.Id);
        Plans.Add(plan);
        return Task.CompletedTask;
    }

    public Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Plans.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }
}
