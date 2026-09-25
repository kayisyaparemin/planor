using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Münferit ve tek seferlik arızi gelirleri SQLite veritabanında saklayan
/// ve yöneten somut depo adaptörü.
/// </summary>
public sealed class SqliteAdHocIncomeRepository(SQLiteAsyncConnection connection) : IAdHocIncomeRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdHocIncome>> GetAdHocIncomesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _connection.Table<AdHocIncomeEntity>()
            .OrderBy(x => x.ExactDate)
            .ToListAsync();

        return rows.Select(r => new AdHocIncome
        {
            Id = Guid.Parse(r.Id),
            Amount = r.Amount,
            ExactDate = DateOnly.ParseExact(r.ExactDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Description = r.Description
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(income);
        cancellationToken.ThrowIfCancellationRequested();

        var entity = new AdHocIncomeEntity
        {
            Id = income.Id.ToString(),
            Amount = income.Amount,
            ExactDate = income.ExactDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Description = income.Description
        };

        await _connection.InsertOrReplaceAsync(entity);
    }

    /// <inheritdoc />
    public async Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<AdHocIncomeEntity>(id.ToString());
    }
}
