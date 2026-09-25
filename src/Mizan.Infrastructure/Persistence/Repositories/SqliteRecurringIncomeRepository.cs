using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Düzenli gelir akışlarını ve bunların etkin tarihli tutar/zam geçmişini
/// SQLite veritabanında saklayan ve yöneten somut depo adaptörü.
/// </summary>
public sealed class SqliteRecurringIncomeRepository(SQLiteAsyncConnection connection) : IRecurringIncomeRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecurringIncome>> GetRecurringIncomesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _connection.Table<RecurringIncomeEntity>()
            .OrderBy(x => x.PaymentDay)
            .ToListAsync();

        return rows.Select(r => new RecurringIncome
        {
            Id = Guid.Parse(r.Id),
            Name = r.Name,
            PaymentDay = r.PaymentDay,
            IsActive = r.IsActive
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertRecurringIncomeAsync(RecurringIncome income, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(income);
        cancellationToken.ThrowIfCancellationRequested();

        var entity = new RecurringIncomeEntity
        {
            Id = income.Id.ToString(),
            Name = income.Name,
            PaymentDay = income.PaymentDay,
            IsActive = income.IsActive
        };

        await _connection.InsertOrReplaceAsync(entity);
    }

    /// <inheritdoc />
    public async Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<RecurringIncomeEntity>(id.ToString());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IncomeAmountHistory>> GetIncomeAmountHistoriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _connection.Table<IncomeAmountHistoryEntity>()
            .OrderBy(x => x.EffectiveDate)
            .ToListAsync();

        return rows.Select(r => new IncomeAmountHistory
        {
            Id = Guid.Parse(r.Id),
            RecurringIncomeId = Guid.Parse(r.RecurringIncomeId),
            Amount = r.Amount,
            EffectiveDate = DateOnly.ParseExact(r.EffectiveDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Description = r.Description
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertIncomeAmountHistoryAsync(IncomeAmountHistory history, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(history);
        cancellationToken.ThrowIfCancellationRequested();

        var entity = new IncomeAmountHistoryEntity
        {
            Id = history.Id.ToString(),
            RecurringIncomeId = history.RecurringIncomeId.ToString(),
            Amount = history.Amount,
            EffectiveDate = history.EffectiveDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Description = history.Description
        };

        await _connection.InsertOrReplaceAsync(entity);
    }

    /// <inheritdoc />
    public async Task DeleteIncomeAmountHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<IncomeAmountHistoryEntity>(id.ToString());
    }
}
