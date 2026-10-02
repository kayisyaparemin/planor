using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

internal sealed class FakePlannedLargeExpenseRepository : IPlannedLargeExpenseRepository
{
    public List<PlannedLargeExpense> Expenses { get; set; } = [];

    public Task<IReadOnlyList<PlannedLargeExpense>> GetPlannedLargeExpensesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PlannedLargeExpense>>(Expenses);

    public Task UpsertPlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default)
    {
        Expenses.RemoveAll(x => x.Id == expense.Id);
        Expenses.Add(expense);
        return Task.CompletedTask;
    }

    public Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Expenses.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }
}
