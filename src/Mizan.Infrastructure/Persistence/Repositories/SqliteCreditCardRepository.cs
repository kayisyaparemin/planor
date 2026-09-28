using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kullanıcının kredi kartlarını, ekstrelerini, taksitli işlemlerini ve ödeme tercihlerini
/// bütünsel bir kök varlık (aggregate root) olarak SQLite veritabanında saklayan somut depo adaptörü.
/// </summary>
public sealed class SqliteCreditCardRepository(SQLiteAsyncConnection connection) : ICreditCardRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<IReadOnlyList<CreditCard>> GetCreditCardsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cards = await _connection.Table<CreditCardEntity>().ToListAsync();
        var charges = await _connection.Table<CardInstallmentEntity>().ToListAsync();
        var statements = await _connection.Table<CreditCardStatementEntity>().ToListAsync();
        var paymentPlans = await _connection.Table<CreditCardPaymentPlanEntity>().ToListAsync();
        var preferences = await _connection.Table<CreditCardPaymentPreferenceEntity>().ToListAsync();

        return cards.Select(c => CreditCardEntityMapper.MapCreditCard(c, charges, statements, paymentPlans, preferences)).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertCreditCardAsync(CreditCard card, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(card);
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.RunInTransactionAsync(conn => CreditCardEntityWriter.Save(conn, card));
    }

    /// <inheritdoc />
    public async Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<CreditCardEntity>(id.ToString());
    }
}
