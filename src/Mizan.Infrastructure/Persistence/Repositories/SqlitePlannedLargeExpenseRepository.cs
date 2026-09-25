using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Gelecekte tek seferde gerçekleşmesi beklenen planlı büyük harcamaları
/// SQLite veritabanında saklayan ve yöneten somut depo adaptörü.
/// </summary>
public sealed class SqlitePlannedLargeExpenseRepository(SQLiteAsyncConnection connection) : IPlannedLargeExpenseRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlannedLargeExpense>> GetPlannedLargeExpensesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _connection.Table<PlannedLargeExpenseEntity>()
            .OrderBy(x => x.ExactDate)
            .ToListAsync();

        return rows.Select(r => new PlannedLargeExpense
        {
            Id = Guid.Parse(r.Id),
            Name = r.Name,
            Amount = r.Amount,
            ExactDate = DateOnly.ParseExact(r.ExactDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Note = r.Note,
            Status = Enum.IsDefined(typeof(PlannedExpenseStatus), r.Status) ? (PlannedExpenseStatus)r.Status : PlannedExpenseStatus.Planned
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertPlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expense);
        cancellationToken.ThrowIfCancellationRequested();

        var entity = new PlannedLargeExpenseEntity
        {
            Id = expense.Id.ToString(),
            Name = expense.Name,
            Amount = expense.Amount,
            ExactDate = expense.ExactDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Note = expense.Note,
            Status = (int)expense.Status
        };

        await _connection.InsertOrReplaceAsync(entity);
    }

    /// <inheritdoc />
    public async Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<PlannedLargeExpenseEntity>(id.ToString());
    }
}
