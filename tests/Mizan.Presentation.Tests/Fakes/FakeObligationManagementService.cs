using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Kredi, ödeme planı ve büyük harcama yazma portunun sahtesi: kaydedilen kredileri ve silinen
/// kimlikleri tutar, istenirse kaydetmede ya da silmede hata fırlatır.
/// </summary>
internal sealed class FakeObligationManagementService : IObligationManagementService
{
    public List<Loan> SavedLoans { get; } = [];
    public List<Guid> DeletedLoanIds { get; } = [];
    public List<Guid> DeletedPaymentPlanIds { get; } = [];
    public List<Guid> DeletedLargeExpenseIds { get; } = [];
    public Exception? SaveException { get; set; }
    public Exception? DeleteException { get; set; }

    public Task SaveLoanAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        if (SaveException is not null)
        {
            return Task.FromException(SaveException);
        }

        SavedLoans.Add(loan);
        return Task.CompletedTask;
    }

    public Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default) => Delete(DeletedLoanIds, id);

    public Task DeleteLoanPrepaymentAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SavePaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default) => Delete(DeletedPaymentPlanIds, id);

    public Task SavePlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default) =>
        Delete(DeletedLargeExpenseIds, id);

    private Task Delete(List<Guid> deleted, Guid id)
    {
        if (DeleteException is not null)
        {
            return Task.FromException(DeleteException);
        }

        deleted.Add(id);
        return Task.CompletedTask;
    }
}
