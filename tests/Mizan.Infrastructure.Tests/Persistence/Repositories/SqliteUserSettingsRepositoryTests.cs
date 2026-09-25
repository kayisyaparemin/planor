using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteUserSettingsRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqliteUserSettingsRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_settings_{Guid.NewGuid():N}.db3");
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        new DatabaseSchema().EnsureInitializedAsync(_connection).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _connection.CloseAsync().GetAwaiter().GetResult();
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* cleanup */ }
        }
    }

    [Fact]
    public async Task GetSettingsAsync_KayitYoksa_VarsayilanAyarlariDondurur()
    {
        var repository = new SqliteUserSettingsRepository(_connection);

        var settings = await repository.GetSettingsAsync();

        Assert.NotNull(settings);
        Assert.Equal(10, settings.PeriodAnchor.DayOfMonth);
        Assert.Equal(0.05m, settings.CreditCardCarryInterestRate);
        Assert.Equal(0.05m, settings.DeficitFinancingInterestRate);
    }

    [Fact]
    public async Task SaveSettingsAsync_YeniAyarKaydeder_GetSettingsAsyncIleAynenOkur()
    {
        var repository = new SqliteUserSettingsRepository(_connection);
        var expected = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            PeriodVariableExpenseAllowance = 12500m,
            ProjectionOpeningBalance = 35000m,
            ProjectionAnchorDate = new DateOnly(2026, 9, 15),
            CreditCardCarryInterestRate = 0.0425m,
            DeficitFinancingInterestRate = 0.055m
        };

        await repository.SaveSettingsAsync(expected);
        var actual = await repository.GetSettingsAsync();

        Assert.Equal(expected.PeriodAnchor.DayOfMonth, actual.PeriodAnchor.DayOfMonth);
        Assert.Equal(expected.PeriodVariableExpenseAllowance, actual.PeriodVariableExpenseAllowance);
        Assert.Equal(expected.ProjectionOpeningBalance, actual.ProjectionOpeningBalance);
        Assert.Equal(expected.ProjectionAnchorDate, actual.ProjectionAnchorDate);
        Assert.Equal(expected.CreditCardCarryInterestRate, actual.CreditCardCarryInterestRate);
        Assert.Equal(expected.DeficitFinancingInterestRate, actual.DeficitFinancingInterestRate);
    }

    [Fact]
    public async Task SaveSettingsAsync_MevcutAyariGunceller_AyarDegerleriGuncellenir()
    {
        var repository = new SqliteUserSettingsRepository(_connection);
        var initial = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(1),
            PeriodVariableExpenseAllowance = 5000m
        };
        await repository.SaveSettingsAsync(initial);

        var updated = initial with
        {
            PeriodAnchor = new PeriodAnchor(20),
            PeriodVariableExpenseAllowance = 8000m
        };
        await repository.SaveSettingsAsync(updated);

        var actual = await repository.GetSettingsAsync();
        Assert.Equal(20, actual.PeriodAnchor.DayOfMonth);
        Assert.Equal(8000m, actual.PeriodVariableExpenseAllowance);
    }
}
