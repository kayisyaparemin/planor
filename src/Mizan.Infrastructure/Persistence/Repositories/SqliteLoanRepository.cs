using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Banka kredi sözleşmelerini ve bunlara bağlı kısmi/tam erken kapama taahhütlerini
/// SQLite veritabanında saklayan ve yöneten somut depo adaptörü.
/// </summary>
public sealed class SqliteLoanRepository(SQLiteAsyncConnection connection) : ILoanRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<IReadOnlyList<Loan>> GetLoansAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _connection.Table<LoanEntity>()
            .OrderBy(x => x.NextPaymentDate)
            .ToListAsync();

        return rows.Select(r => new Loan
        {
            Id = Guid.Parse(r.Id),
            Name = r.Name,
            Bank = r.Bank,
            MonthlyPayment = r.MonthlyPayment,
            PaymentDay = r.PaymentDay,
            NextPaymentDate = DateOnly.ParseExact(r.NextPaymentDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            RemainingInstallmentCount = r.RemainingInstallmentCount,
            FinalPaymentAmount = r.FinalPaymentAmount,
            RemainingDebt = r.RemainingDebt,
            EarlyClosureAmount = r.EarlyClosureAmount,
            EarlyClosureAmountAsOf = string.IsNullOrWhiteSpace(r.EarlyClosureAmountAsOf)
                ? null
                : DateOnly.ParseExact(r.EarlyClosureAmountAsOf, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Kind = (LoanKind)r.Kind,
            IsActive = r.IsActive
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertLoanAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loan);
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.UpsertAsync(ToEntity(loan));
    }

    /// <inheritdoc />
    public async Task UpsertLoanWithPrepaymentsAsync(
        Loan loan, IReadOnlyList<LoanPrepayment> prepayments, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(prepayments);
        cancellationToken.ThrowIfCancellationRequested();

        // Formdaki liste kredinin bütün erken ödemeleridir: listede olmayan silinmiş demektir. Bu yüzden
        // liste krediyle aynı işlemde bütünüyle yeniden yazılır (S64-12).
        var loanId = loan.Id.ToString();
        await _connection.RunInTransactionAsync(conn =>
        {
            conn.Upsert(ToEntity(loan));
            conn.Execute("DELETE FROM loan_prepayments WHERE LoanId = ?", loanId);
            foreach (var prepayment in prepayments)
            {
                conn.Insert(ToEntity(prepayment with { LoanId = loan.Id }));
            }
        });
    }

    /// <inheritdoc />
    public async Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<LoanEntity>(id.ToString());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LoanPrepayment>> GetLoanPrepaymentsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _connection.Table<LoanPrepaymentEntity>()
            .OrderBy(x => x.Date)
            .ToListAsync();

        return rows.Select(r => new LoanPrepayment
        {
            Id = Guid.Parse(r.Id),
            LoanId = Guid.Parse(r.LoanId),
            Date = DateOnly.ParseExact(r.Date, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Mode = (LoanPrepaymentMode)r.Mode,
            PrincipalAmount = r.PrincipalAmount
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertLoanPrepaymentAsync(LoanPrepayment prepayment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prepayment);
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.UpsertAsync(ToEntity(prepayment));
    }

    /// <inheritdoc />
    public async Task DeleteLoanPrepaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<LoanPrepaymentEntity>(id.ToString());
    }

    private static LoanEntity ToEntity(Loan loan) => new()
    {
        Id = loan.Id.ToString(),
        Name = loan.Name,
        Bank = loan.Bank,
        MonthlyPayment = loan.MonthlyPayment,
        PaymentDay = loan.PaymentDay,
        NextPaymentDate = loan.NextPaymentDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        RemainingInstallmentCount = loan.RemainingInstallmentCount,
        FinalPaymentAmount = loan.FinalPaymentAmount,
        RemainingDebt = loan.RemainingDebt,
        EarlyClosureAmount = loan.EarlyClosureAmount,
        EarlyClosureAmountAsOf = loan.EarlyClosureAmountAsOf?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        Kind = (int)loan.Kind,
        IsActive = loan.IsActive
    };

    private static LoanPrepaymentEntity ToEntity(LoanPrepayment prepayment) => new()
    {
        Id = prepayment.Id.ToString(),
        LoanId = prepayment.LoanId.ToString(),
        Date = prepayment.Date.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        Mode = (int)prepayment.Mode,
        PrincipalAmount = prepayment.PrincipalAmount
    };
}
