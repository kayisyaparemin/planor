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
    // Yalnız bu gelirin tutarı silinir: başka bir gelirin kimliği gelse de ona dokunulmaz.
    private const string DeleteAmountOfIncomeSql =
        $"DELETE FROM {DatabaseConstants.TableIncomeAmountHistories} WHERE Id = ? AND RecurringIncomeId = ?";

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
        await _connection.UpsertAsync(ToEntity(income));
    }

    /// <inheritdoc />
    public async Task UpsertRecurringIncomeWithAmountsAsync(
        RecurringIncome income, IReadOnlyList<IncomeAmountHistory> newAmounts, IReadOnlyList<Guid> removedAmountIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(income);
        ArgumentNullException.ThrowIfNull(newAmounts);
        ArgumentNullException.ThrowIfNull(removedAmountIds);
        cancellationToken.ThrowIfCancellationRequested();

        // Tutar kayıtları etkin tarihlidir: yalnız eklenir ya da silinir, var olan kayıt güncellenmez (kural 05).
        // Silme önce gelir: aynı günün tutarı silinip yeniden girilebilir. Aynı kimlik ikinci kez eklenirse
        // ekleme düşer ve işlem silmeyi de geliri de geri alır (S67-3, S67-5).
        await _connection.RunInTransactionAsync(conn =>
        {
            conn.Upsert(ToEntity(income));
            foreach (var id in removedAmountIds)
            {
                conn.Execute(DeleteAmountOfIncomeSql, id.ToString(), income.Id.ToString());
            }

            foreach (var amount in newAmounts)
            {
                conn.Insert(ToEntity(amount));
            }
        });
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
        await _connection.UpsertAsync(ToEntity(history));
    }

    private static RecurringIncomeEntity ToEntity(RecurringIncome income) => new()
    {
        Id = income.Id.ToString(),
        Name = income.Name,
        PaymentDay = income.PaymentDay,
        IsActive = income.IsActive
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
