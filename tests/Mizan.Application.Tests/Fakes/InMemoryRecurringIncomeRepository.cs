using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi düzenli gelir deposu implementasyonu.
/// </summary>
public sealed class InMemoryRecurringIncomeRepository : IRecurringIncomeRepository
{
    private readonly Dictionary<Guid, RecurringIncome> _incomes = [];
    private readonly Dictionary<Guid, IncomeAmountHistory> _histories = [];

    /// <summary>Gelirin tutarlarıyla birlikte tek çağrıda yazıldığı kayıt sayısı (S67-3).</summary>
    public int CombinedWriteCount { get; private set; }

    public Task<IReadOnlyList<RecurringIncome>> GetRecurringIncomesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<RecurringIncome> result = _incomes.Values.OrderBy(x => x.PaymentDay).ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertRecurringIncomeAsync(RecurringIncome income, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _incomes[income.Id] = income;
        return Task.CompletedTask;
    }

    public Task UpsertRecurringIncomeWithAmountsAsync(
        RecurringIncome income, IReadOnlyList<IncomeAmountHistory> newAmounts, IReadOnlyList<Guid> removedAmountIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CombinedWriteCount++;
        _incomes[income.Id] = income;
        foreach (var id in removedAmountIds.Where(id => _histories.TryGetValue(id, out var x) && x.RecurringIncomeId == income.Id))
        {
            _histories.Remove(id);
        }

        foreach (var amount in newAmounts)
        {
            _histories[amount.Id] = amount;
        }

        return Task.CompletedTask;
    }

    public Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _incomes.Remove(id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IncomeAmountHistory>> GetIncomeAmountHistoriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<IncomeAmountHistory> result = _histories.Values
            .OrderBy(x => x.EffectiveDate)
            .ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertIncomeAmountHistoryAsync(IncomeAmountHistory history, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _histories[history.Id] = history;
        return Task.CompletedTask;
    }
}
