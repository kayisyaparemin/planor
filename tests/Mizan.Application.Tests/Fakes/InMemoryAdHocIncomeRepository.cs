using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi münferit tek seferlik gelir deposu implementasyonu.
/// </summary>
public sealed class InMemoryAdHocIncomeRepository : IAdHocIncomeRepository
{
    private readonly Dictionary<Guid, AdHocIncome> _incomes = [];

    public Task<IReadOnlyList<AdHocIncome>> GetAdHocIncomesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<AdHocIncome> result = _incomes.Values.OrderBy(x => x.ExactDate).ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _incomes[income.Id] = income;
        return Task.CompletedTask;
    }

    public Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _incomes.Remove(id);
        return Task.CompletedTask;
    }
}
