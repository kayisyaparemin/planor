using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>Tek seferlik gelir deposunun sahtesi: gelirleri bellekte tutar.</summary>
internal sealed class FakeAdHocIncomeRepository : IAdHocIncomeRepository
{
    public Dictionary<Guid, AdHocIncome> Incomes { get; } = [];
    public Exception? ThrowOnGet { get; set; }

    public Task<IReadOnlyList<AdHocIncome>> GetAdHocIncomesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnGet is not null)
        {
            throw ThrowOnGet;
        }

        return Task.FromResult<IReadOnlyList<AdHocIncome>>(Incomes.Values.ToList());
    }

    public Task UpsertAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default)
    {
        Incomes[income.Id] = income;
        return Task.CompletedTask;
    }

    public Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Incomes.Remove(id);
        return Task.CompletedTask;
    }
}
