using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kullanıcının finansal planlama parametrelerini ve varsayılan oranlarını SQLite
/// veritabanında saklayan ve yöneten somut depo adaptörü.
/// </summary>
public sealed class SqliteUserSettingsRepository(SQLiteAsyncConnection connection) : IUserSettingsRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<UserSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entity = await _connection.Table<SettingsEntity>().FirstOrDefaultAsync();
        if (entity is null)
        {
            return new UserSettings();
        }

        return new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(entity.PeriodAnchorDay),
            PeriodVariableExpenseAllowance = entity.PeriodVariableExpenseAllowance,
            ProjectionOpeningBalance = entity.ProjectionOpeningBalance,
            ProjectionAnchorDate = string.IsNullOrWhiteSpace(entity.ProjectionAnchorDate)
                ? default
                : DateOnly.ParseExact(entity.ProjectionAnchorDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            CreditCardCarryInterestRate = entity.CreditCardCarryInterestRate,
            DeficitFinancingInterestRate = entity.DeficitFinancingInterestRate
        };
    }

    /// <inheritdoc />
    public async Task SaveSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        var existing = await _connection.Table<SettingsEntity>().FirstOrDefaultAsync();
        var entity = existing ?? new SettingsEntity();

        entity.PeriodAnchorDay = settings.PeriodAnchor.DayOfMonth;
        entity.PeriodVariableExpenseAllowance = settings.PeriodVariableExpenseAllowance;
        entity.ProjectionOpeningBalance = settings.ProjectionOpeningBalance;
        entity.ProjectionAnchorDate = settings.ProjectionAnchorDate == default
            ? string.Empty
            : settings.ProjectionAnchorDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture);
        entity.CreditCardCarryInterestRate = settings.CreditCardCarryInterestRate;
        entity.DeficitFinancingInterestRate = settings.DeficitFinancingInterestRate;

        await _connection.UpsertAsync(entity);
    }
}
