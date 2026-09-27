using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Kredi deposunun bellek içi sahtesi: kredileri ve erken ödemeleri tutar, istenirse okumada hata fırlatır.
/// </summary>
internal sealed class FakeLoanRepository : ILoanRepository
{
    public Dictionary<Guid, Loan> Loans { get; } = [];

    public List<LoanPrepayment> Prepayments { get; } = [];

    public Exception? ReadException { get; set; }

    public Task<IReadOnlyList<Loan>> GetLoansAsync(CancellationToken cancellationToken = default) =>
        ReadException is null
            ? Task.FromResult<IReadOnlyList<Loan>>(Loans.Values.ToList())
            : Task.FromException<IReadOnlyList<Loan>>(ReadException);

    public Task UpsertLoanAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        Loans[loan.Id] = loan;
        return Task.CompletedTask;
    }

    public Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Loans.Remove(id);
        Prepayments.RemoveAll(x => x.LoanId == id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LoanPrepayment>> GetLoanPrepaymentsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LoanPrepayment>>(Prepayments.ToList());

    public Task UpsertLoanPrepaymentAsync(LoanPrepayment prepayment, CancellationToken cancellationToken = default)
    {
        Prepayments.RemoveAll(x => x.Id == prepayment.Id);
        Prepayments.Add(prepayment);
        return Task.CompletedTask;
    }

    public Task DeleteLoanPrepaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Prepayments.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }
}
