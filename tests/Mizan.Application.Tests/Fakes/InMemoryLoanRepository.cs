using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi kredi deposu implementasyonu.
/// </summary>
public sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly Dictionary<Guid, Loan> _loans = [];
    private readonly Dictionary<Guid, LoanPrepayment> _prepayments = [];

    public Task<IReadOnlyList<Loan>> GetLoansAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Loan> result = _loans.Values.OrderBy(x => x.NextPaymentDate).ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertLoanAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _loans[loan.Id] = loan;
        return Task.CompletedTask;
    }

    /// <summary>Tek işlemde yazma çağrısının sayısı; kayıt ayrı ayrı yazmaya dönerse testler görür.</summary>
    public int CombinedWriteCount { get; private set; }

    public Task UpsertLoanWithPrepaymentsAsync(
        Loan loan, IReadOnlyList<LoanPrepayment> prepayments, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CombinedWriteCount++;
        _loans[loan.Id] = loan;
        foreach (var stale in _prepayments.Values.Where(p => p.LoanId == loan.Id).Select(p => p.Id).ToArray())
        {
            _prepayments.Remove(stale);
        }

        foreach (var prepayment in prepayments)
        {
            _prepayments[prepayment.Id] = prepayment;
        }

        return Task.CompletedTask;
    }

    public Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _loans.Remove(id);
        var relatedPrepayments = _prepayments.Values.Where(p => p.LoanId == id).Select(p => p.Id).ToArray();
        foreach (var prepayId in relatedPrepayments)
        {
            _prepayments.Remove(prepayId);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LoanPrepayment>> GetLoanPrepaymentsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<LoanPrepayment> result = _prepayments.Values.OrderBy(x => x.Date).ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertLoanPrepaymentAsync(LoanPrepayment prepayment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _prepayments[prepayment.Id] = prepayment;
        return Task.CompletedTask;
    }

    public Task DeleteLoanPrepaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _prepayments.Remove(id);
        return Task.CompletedTask;
    }
}
