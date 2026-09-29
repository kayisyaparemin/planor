using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Düzenli gelir deposunun sahtesi: gelirleri ve tutar kayıtlarını bellekte tutar, okuma ve yazma
/// hatası verebilir, gelirin tutarlarıyla birlikte kaç kez yazıldığını sayar.
/// </summary>
internal sealed class FakeRecurringIncomeRepository : IRecurringIncomeRepository
{
    public Dictionary<Guid, RecurringIncome> Incomes { get; } = [];
    public List<IncomeAmountHistory> Amounts { get; } = [];
    public Exception? ReadException { get; set; }
    public Exception? WriteException { get; set; }
    public int CombinedWriteCount { get; private set; }

    public Task<IReadOnlyList<RecurringIncome>> GetRecurringIncomesAsync(CancellationToken cancellationToken = default) =>
        ReadException is null
            ? Task.FromResult<IReadOnlyList<RecurringIncome>>(Incomes.Values.ToList())
            : Task.FromException<IReadOnlyList<RecurringIncome>>(ReadException);

    public Task UpsertRecurringIncomeAsync(RecurringIncome income, CancellationToken cancellationToken = default)
    {
        Incomes[income.Id] = income;
        return Task.CompletedTask;
    }

    public Task UpsertRecurringIncomeWithAmountsAsync(
        RecurringIncome income, IReadOnlyList<IncomeAmountHistory> newAmounts, CancellationToken cancellationToken = default)
    {
        if (WriteException is not null)
        {
            return Task.FromException(WriteException);
        }

        CombinedWriteCount++;
        Incomes[income.Id] = income;
        Amounts.AddRange(newAmounts);
        return Task.CompletedTask;
    }

    public Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Incomes.Remove(id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IncomeAmountHistory>> GetIncomeAmountHistoriesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<IncomeAmountHistory>>(Amounts.ToList());

    public Task UpsertIncomeAmountHistoryAsync(IncomeAmountHistory history, CancellationToken cancellationToken = default)
    {
        Amounts.RemoveAll(x => x.Id == history.Id);
        Amounts.Add(history);
        return Task.CompletedTask;
    }
}
