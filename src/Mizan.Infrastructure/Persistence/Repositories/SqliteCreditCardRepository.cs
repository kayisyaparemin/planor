using System.Globalization;
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

        var cardIdStr = card.Id.ToString();
        await _connection.RunInTransactionAsync(conn =>
        {
            SaveCardMain(conn, cardIdStr, card);
            SaveCharges(conn, cardIdStr, card.Charges);
            SavePaymentPlans(conn, cardIdStr, card.PaymentPlans);
            SaveStatement(conn, cardIdStr, card.CurrentStatement, card.CurrentStatementPaymentPlan);
            SavePreferences(conn, cardIdStr, card.PaymentPreferences);
        });
    }

    /// <inheritdoc />
    public async Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<CreditCardEntity>(id.ToString());
    }

    private static void SaveCardMain(SQLiteConnection conn, string cardId, CreditCard card) =>
        conn.InsertOrReplace(new CreditCardEntity
        {
            Id = cardId,
            Name = card.Name,
            Bank = card.Bank,
            Limit = card.Limit,
            CarriedBalance = card.CarriedBalance,
            UnbilledSpending = card.UnbilledSpending,
            BalanceAsOfDate = card.BalanceAsOfDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            StatementClosingDay = card.StatementClosingDay,
            PaymentDueDay = card.PaymentDueDay,
            MinimumPaymentRate = card.MinimumPaymentRate,
            PaymentStrategy = (int)card.PaymentStrategy,
            FixedPaymentAmount = card.FixedPaymentAmount,
            ProjectionFallbackStrategy = (int)card.ProjectionFallbackStrategy,
            ProjectionFallbackFixedAmount = card.ProjectionFallbackFixedAmount,
            KnownNextStatementDate = card.KnownNextStatementDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            KnownNextDueDate = card.KnownNextDueDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            IsActive = card.IsActive
        });

    private static void SaveCharges(SQLiteConnection conn, string cardId, IReadOnlyList<CardCharge> charges)
    {
        conn.Execute("DELETE FROM card_installments WHERE CreditCardId = ?", cardId);
        foreach (var c in charges)
        {
            conn.Insert(new CardInstallmentEntity
            {
                Id = c.Id.ToString(),
                CreditCardId = cardId,
                Description = c.Description,
                PostingDate = c.PostingDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                Amount = c.Amount
            });
        }
    }

    private static void SavePaymentPlans(SQLiteConnection conn, string cardId, IReadOnlyList<CreditCardPaymentPlan> plans)
    {
        conn.Execute("DELETE FROM credit_card_payment_plans WHERE CreditCardId = ?", cardId);
        foreach (var p in plans)
        {
            conn.Insert(new CreditCardPaymentPlanEntity
            {
                Id = p.Id.ToString(),
                CreditCardId = cardId,
                DueDate = p.DueDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PaymentType = (int)p.PaymentType,
                Amount = p.Amount
            });
        }
    }

    private static void SaveStatement(SQLiteConnection conn, string cardId, CreditCardStatement? stmt, CurrentStatementPaymentPlan? plan)
    {
        conn.Execute("DELETE FROM credit_card_statements WHERE CreditCardId = ?", cardId);
        if (stmt is null)
        {
            return;
        }

        conn.Insert(new CreditCardStatementEntity
        {
            Id = stmt.Id.ToString(),
            CreditCardId = cardId,
            StatementDate = stmt.StatementDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            DueDate = stmt.DueDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            StatementAmount = stmt.StatementAmount,
            MinimumPaymentAmount = stmt.MinimumPaymentAmount,
            NextStatementDate = stmt.NextStatementDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            NextDueDate = stmt.NextDueDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            CreatedAt = stmt.CreatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            UpdatedAt = stmt.UpdatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            CurrentPaymentMode = plan is not null ? (int)plan.Mode : 0,
            CurrentPaymentCustomAmount = plan?.CustomAmount
        });
    }

    private static void SavePreferences(SQLiteConnection conn, string cardId, IReadOnlyList<CreditCardPaymentPreference> prefs)
    {
        conn.Execute("DELETE FROM credit_card_payment_preferences WHERE CreditCardId = ?", cardId);
        foreach (var p in prefs)
        {
            conn.Insert(new CreditCardPaymentPreferenceEntity
            {
                Id = p.Id.ToString(),
                CreditCardId = cardId,
                Mode = (int)p.Mode,
                CustomAmount = p.CustomAmount,
                EffectiveFromStatementDate = p.EffectiveFromStatementDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                CreatedAt = p.CreatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
                Note = p.Note
            });
        }
    }
}
