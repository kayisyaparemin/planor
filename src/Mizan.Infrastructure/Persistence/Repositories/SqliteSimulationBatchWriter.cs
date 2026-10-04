using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Onaylanan simülasyon senaryosundan türetilen tüm varlıkları SQLite veritabanında
/// tek bir atomik transaction altında kaydeden somut altyapı adaptörü (S76-12, V10f).
/// </summary>
public sealed class SqliteSimulationBatchWriter(ISqliteConnectionProvider connectionProvider) : ISimulationBatchWriter
{
    private readonly ISqliteConnectionProvider _connectionProvider =
        connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));

    /// <inheritdoc />
    public async Task WriteBatchAsync(SimulationPersistenceBatch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        cancellationToken.ThrowIfCancellationRequested();

        await _connectionProvider.Connection.RunInTransactionAsync(conn =>
        {
            foreach (var expense in batch.LargeExpenses)
            {
                conn.Upsert(ToEntity(expense));
            }

            foreach (var plan in batch.PaymentPlans)
            {
                PaymentPlanEntityWriter.Save(conn, plan);
            }

            foreach (var card in batch.CreditCards)
            {
                CreditCardEntityWriter.Save(conn, card);
            }

            foreach (var prepayment in batch.LoanPrepayments)
            {
                conn.Upsert(ToEntity(prepayment));
            }

            foreach (var income in batch.AdHocIncomes)
            {
                conn.Upsert(ToEntity(income));
            }

            foreach (var history in batch.IncomeHistories)
            {
                conn.Upsert(ToEntity(history));
            }
        });
    }

    private static PlannedLargeExpenseEntity ToEntity(PlannedLargeExpense expense) => new()
    {
        Id = expense.Id.ToString(),
        Name = expense.Name,
        Amount = expense.Amount,
        ExactDate = expense.ExactDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        Note = expense.Note,
        Status = (int)expense.Status
    };

    private static LoanPrepaymentEntity ToEntity(LoanPrepayment prepayment) => new()
    {
        Id = prepayment.Id.ToString(),
        LoanId = prepayment.LoanId.ToString(),
        Date = prepayment.Date.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        Mode = (int)prepayment.Mode,
        PrincipalAmount = prepayment.PrincipalAmount
    };

    private static AdHocIncomeEntity ToEntity(AdHocIncome income) => new()
    {
        Id = income.Id.ToString(),
        Amount = income.Amount,
        ExactDate = income.ExactDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        Description = income.Description
    };

    private static IncomeAmountHistoryEntity ToEntity(IncomeAmountHistory history) => new()
    {
        Id = history.Id.ToString(),
        RecurringIncomeId = history.RecurringIncomeId.ToString(),
        Amount = history.Amount,
        EffectiveDate = history.EffectiveDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        Description = history.Description
    };
}
