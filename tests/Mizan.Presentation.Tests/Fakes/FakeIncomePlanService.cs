using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Gelir yazma portunun sahtesi: silinen düzenli ve tek seferlik gelir kimliklerini tutar.
/// </summary>
internal sealed class FakeIncomePlanService : IIncomePlanService
{
    public List<Guid> DeletedRecurringIncomeIds { get; } = [];
    public List<Guid> DeletedAdHocIncomeIds { get; } = [];
    public List<AdHocIncome> SavedAdHocIncomes { get; } = [];
    public Exception? ThrowOnSaveAdHocIncome { get; set; }

    public Task SaveRecurringIncomeAsync(
        RecurringIncome income, IReadOnlyList<IncomeAmountHistory> newAmounts, IReadOnlyList<Guid> removedAmountIds,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeletedRecurringIncomeIds.Add(id);
        return Task.CompletedTask;
    }

    public Task SaveAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default)
    {
        if (ThrowOnSaveAdHocIncome is not null)
        {
            throw ThrowOnSaveAdHocIncome;
        }

        SavedAdHocIncomes.Add(income);
        return Task.CompletedTask;
    }

    public Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeletedAdHocIncomeIds.Add(id);
        return Task.CompletedTask;
    }
}
