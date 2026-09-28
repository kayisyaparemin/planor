using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteRecurringIncomeRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqliteRecurringIncomeRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_recurring_income_{Guid.NewGuid():N}.db3");
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
    public async Task GetRecurringIncomesAsync_Bosken_BosDiziDondurur()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);

        var incomes = await repository.GetRecurringIncomesAsync();

        Assert.Empty(incomes);
    }

    [Fact]
    public async Task UpsertRecurringIncomeAsync_YeniAkisEklerVeGunceller()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var incomeId = Guid.NewGuid();
        var income = new RecurringIncome
        {
            Id = incomeId,
            Name = "Maaş",
            PaymentDay = 15,
            IsActive = true
        };

        await repository.UpsertRecurringIncomeAsync(income);

        var list = await repository.GetRecurringIncomesAsync();
        Assert.Single(list);
        Assert.Equal("Maaş", list[0].Name);
        Assert.Equal(15, list[0].PaymentDay);

        var updated = income with { Name = "Ana Maaş", PaymentDay = 1 };
        await repository.UpsertRecurringIncomeAsync(updated);

        var updatedList = await repository.GetRecurringIncomesAsync();
        Assert.Single(updatedList);
        Assert.Equal("Ana Maaş", updatedList[0].Name);
        Assert.Equal(1, updatedList[0].PaymentDay);
    }

    [Fact]
    public async Task UpsertRecurringIncomeAsync_KayitliAkisiGuncellerTutarGecmisiniKaybetmez()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var income = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira", PaymentDay = 5, IsActive = true };
        await repository.UpsertRecurringIncomeAsync(income);
        var raise = new IncomeAmountHistory
        {
            Id = Guid.NewGuid(), RecurringIncomeId = income.Id, Amount = 20000m, EffectiveDate = new DateOnly(2026, 7, 1), Description = "Ara Zam"
        };
        await repository.UpsertIncomeAmountHistoryAsync(raise);

        await repository.UpsertRecurringIncomeAsync(income with { Name = "Dükkân Kirası", PaymentDay = 10 });

        Assert.Equal("Dükkân Kirası", Assert.Single(await repository.GetRecurringIncomesAsync()).Name);
        Assert.Equal([raise], await repository.GetIncomeAmountHistoriesAsync());
    }

    [Fact]
    public async Task UpsertIncomeAmountHistoryAsync_TutarGecmisiEklerVeListeler()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var incomeId = Guid.NewGuid();
        var income = new RecurringIncome { Id = incomeId, Name = "Kira", PaymentDay = 5 };
        await repository.UpsertRecurringIncomeAsync(income);

        var history1 = new IncomeAmountHistory
        {
            Id = Guid.NewGuid(),
            RecurringIncomeId = incomeId,
            Amount = 15000m,
            EffectiveDate = new DateOnly(2026, 1, 1),
            Description = "Başlangıç Kirası"
        };
        var history2 = new IncomeAmountHistory
        {
            Id = Guid.NewGuid(),
            RecurringIncomeId = incomeId,
            Amount = 20000m,
            EffectiveDate = new DateOnly(2026, 7, 1),
            Description = "Ara Zam"
        };

        await repository.UpsertIncomeAmountHistoryAsync(history1);
        await repository.UpsertIncomeAmountHistoryAsync(history2);

        var histories = await repository.GetIncomeAmountHistoriesAsync();
        Assert.Equal(2, histories.Count);
        Assert.Contains(histories, h => h.Amount == 15000m);
        Assert.Contains(histories, h => h.Amount == 20000m);
    }

    [Fact]
    public async Task DeleteRecurringIncomeAsync_GelirAkisiniSiler_Ve_CascadeIleTutarGecmisiniDeSiler()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var incomeId = Guid.NewGuid();
        await repository.UpsertRecurringIncomeAsync(new RecurringIncome { Id = incomeId, Name = "Serbest Gelir", PaymentDay = 10 });
        await repository.UpsertIncomeAmountHistoryAsync(new IncomeAmountHistory
        {
            Id = Guid.NewGuid(),
            RecurringIncomeId = incomeId,
            Amount = 30000m,
            EffectiveDate = new DateOnly(2026, 3, 1),
            Description = "İlk sözleşme"
        });

        await repository.DeleteRecurringIncomeAsync(incomeId);

        var incomes = await repository.GetRecurringIncomesAsync();
        var histories = await repository.GetIncomeAmountHistoriesAsync();

        Assert.Empty(incomes);
        Assert.Empty(histories); // ON DELETE CASCADE doğrulaması
    }

    [Fact]
    public async Task DeleteIncomeAmountHistoryAsync_YalnizBelirtilenTutarGecmisiniSiler()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var incomeId = Guid.NewGuid();
        await repository.UpsertRecurringIncomeAsync(new RecurringIncome { Id = incomeId, Name = "Gelir", PaymentDay = 1 });

        var h1 = new IncomeAmountHistory { Id = Guid.NewGuid(), RecurringIncomeId = incomeId, Amount = 1000m, EffectiveDate = new DateOnly(2026, 1, 1), Description = "H1" };
        var h2 = new IncomeAmountHistory { Id = Guid.NewGuid(), RecurringIncomeId = incomeId, Amount = 2000m, EffectiveDate = new DateOnly(2026, 2, 1), Description = "H2" };
        await repository.UpsertIncomeAmountHistoryAsync(h1);
        await repository.UpsertIncomeAmountHistoryAsync(h2);

        await repository.DeleteIncomeAmountHistoryAsync(h1.Id);

        var remaining = await repository.GetIncomeAmountHistoriesAsync();
        Assert.Single(remaining);
        Assert.Equal(h2.Id, remaining[0].Id);
    }
}
