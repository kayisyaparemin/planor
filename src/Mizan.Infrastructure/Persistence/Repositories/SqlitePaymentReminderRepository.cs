using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kullanıcının hatırlatıcı tercih modunu ve ödeme bildirim yanıtlarını
/// SQLite veritabanında saklayan somut depo adaptörü.
/// </summary>
public sealed class SqlitePaymentReminderRepository(SQLiteAsyncConnection connection) : IPaymentReminderRepository
{
    private const string DateTimePattern = "yyyy-MM-ddTHH:mm:ss";
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<PaymentReminderMode> GetModeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var row = await _connection.Table<SettingsEntity>().FirstOrDefaultAsync();
        return row is not null && Enum.IsDefined(typeof(PaymentReminderMode), row.PaymentReminderMode)
            ? (PaymentReminderMode)row.PaymentReminderMode
            : PaymentReminderMode.Off;
    }

    /// <inheritdoc />
    public async Task SaveModeAsync(PaymentReminderMode mode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existing = await _connection.Table<SettingsEntity>().FirstOrDefaultAsync();
        var entity = existing ?? new SettingsEntity();
        entity.PaymentReminderMode = (int)mode;
        await _connection.UpsertAsync(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentReminderResponse>> GetResponsesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _connection.Table<PaymentReminderResponseEntity>().ToListAsync();
        return rows
            .Select(MapResponse)
            .OrderBy(r => r.DueDate)
            .ThenBy(r => r.Name, StringComparer.Ordinal)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertResponsesAsync(IReadOnlyList<PaymentReminderResponse> responses, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responses);
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.RunInTransactionAsync(conn =>
        {
            foreach (var r in responses)
            {
                conn.Upsert(ToEntity(r));
            }
        });
    }

    /// <inheritdoc />
    public async Task DeleteResponseAsync(string dueKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.ExecuteAsync("DELETE FROM payment_reminder_responses WHERE DueKey = ?", dueKey);
    }

    private static PaymentReminderResponse MapResponse(PaymentReminderResponseEntity r) =>
        new()
        {
            DueKey = r.DueKey,
            Name = r.Name,
            DueDate = DateOnly.ParseExact(r.DueDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Amount = r.Amount,
            Kind = (PaymentReminderAnswerKind)r.Kind,
            AnsweredAt = DateTime.ParseExact(r.AnsweredAt, DateTimePattern, CultureInfo.InvariantCulture),
            SnoozedUntil = string.IsNullOrWhiteSpace(r.SnoozedUntil)
                ? null
                : DateTime.ParseExact(r.SnoozedUntil, DateTimePattern, CultureInfo.InvariantCulture)
        };

    private static PaymentReminderResponseEntity ToEntity(PaymentReminderResponse r) =>
        new()
        {
            DueKey = r.DueKey,
            Name = r.Name,
            DueDate = r.DueDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Amount = r.Amount,
            Kind = (int)r.Kind,
            AnsweredAt = r.AnsweredAt.ToString(DateTimePattern, CultureInfo.InvariantCulture),
            SnoozedUntil = r.SnoozedUntil?.ToString(DateTimePattern, CultureInfo.InvariantCulture)
        };
}
