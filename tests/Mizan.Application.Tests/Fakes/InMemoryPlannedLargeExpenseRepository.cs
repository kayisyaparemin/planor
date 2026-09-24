using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi planlı büyük harcama deposu implementasyonu.
/// </summary>
public sealed class InMemoryPlannedLargeExpenseRepository : IPlannedLargeExpenseRepository
{
    private readonly Dictionary<Guid, PlannedLargeExpense> _expenses = [];

    public Task<IReadOnlyList<PlannedLargeExpense>> GetPlannedLargeExpensesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<PlannedLargeExpense> result = _expenses.Values.OrderBy(x => x.ExactDate).ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertPlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _expenses[expense.Id] = expense;
        return Task.CompletedTask;
    }

    public Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _expenses.Remove(id);
        return Task.CompletedTask;
    }
}
