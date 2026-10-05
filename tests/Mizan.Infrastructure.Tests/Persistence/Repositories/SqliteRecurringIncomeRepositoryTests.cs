using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteRecurringIncomeRepositoryTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_recurring_income_{Guid.NewGuid():N}.db3");
    private SQLiteAsyncConnection _connection = null!;

    public async Task InitializeAsync()
    {
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        await new DatabaseSchema().EnsureInitializedAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
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
    public async Task UpsertRecurringIncomeWithAmountsAsync_YeniGelirVeIlkTutariniBirlikteYazar()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var income = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira geliri", PaymentDay = 20 };
        var first = Amount(income.Id, 12_500m, new DateOnly(2026, 10, 12));

        await repository.UpsertRecurringIncomeWithAmountsAsync(income, [first], []);

        Assert.Equal(income, Assert.Single(await repository.GetRecurringIncomesAsync()));
        Assert.Equal([first], await repository.GetIncomeAmountHistoriesAsync());
    }

    [Fact]
    public async Task UpsertRecurringIncomeWithAmountsAsync_KayitliGelirde_VarOlanTutarlaraDokunmazYenileriEkler()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var income = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira geliri", PaymentDay = 20 };
        var first = Amount(income.Id, 12_500m, new DateOnly(2026, 1, 1));
        await repository.UpsertRecurringIncomeWithAmountsAsync(income, [first], []);
        var raise = Amount(income.Id, 14_000m, new DateOnly(2027, 1, 20));

        await repository.UpsertRecurringIncomeWithAmountsAsync(income with { Name = "Dükkân kirası", PaymentDay = 5 }, [raise], []);

        var saved = Assert.Single(await repository.GetRecurringIncomesAsync());
        Assert.Equal(("Dükkân kirası", 5), (saved.Name, saved.PaymentDay));
        Assert.Equal([first, raise], await repository.GetIncomeAmountHistoriesAsync());
    }

    [Fact]
    public async Task UpsertRecurringIncomeWithAmountsAsync_TutarYazilamazsa_GelirDeYazilmaz()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var income = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira geliri", PaymentDay = 20 };
        var first = Amount(income.Id, 12_500m, new DateOnly(2026, 10, 12));

        await Assert.ThrowsAsync<SQLiteException>(() => repository.UpsertRecurringIncomeWithAmountsAsync(income, [first, first], []));

        Assert.Empty(await repository.GetRecurringIncomesAsync());
        Assert.Empty(await repository.GetIncomeAmountHistoriesAsync());
    }

    [Fact]
    public async Task UpsertRecurringIncomeWithAmountsAsync_SilinecekTutariSilerYeniTutariAyniIslemdeEkler()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var income = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira geliri", PaymentDay = 20 };
        var first = Amount(income.Id, 12_500m, new DateOnly(2026, 1, 1));
        var planned = Amount(income.Id, 14_000m, new DateOnly(2027, 1, 20));
        await repository.UpsertRecurringIncomeWithAmountsAsync(income, [first, planned], []);
        var replacement = Amount(income.Id, 15_000m, new DateOnly(2027, 2, 20));

        await repository.UpsertRecurringIncomeWithAmountsAsync(income, [replacement], [planned.Id]);

        Assert.Equal([first, replacement], await repository.GetIncomeAmountHistoriesAsync());
    }

    [Fact]
    public async Task UpsertRecurringIncomeWithAmountsAsync_BaskaGelirinTutarKimligiGelirse_OnuSilmez()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var income = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira geliri", PaymentDay = 20 };
        var other = new RecurringIncome { Id = Guid.NewGuid(), Name = "Emekli aylığı", PaymentDay = 5 };
        var otherAmount = Amount(other.Id, 9_000m, new DateOnly(2027, 3, 5));
        await repository.UpsertRecurringIncomeWithAmountsAsync(other, [otherAmount], []);

        await repository.UpsertRecurringIncomeWithAmountsAsync(income, [], [otherAmount.Id]);

        Assert.Equal([otherAmount], await repository.GetIncomeAmountHistoriesAsync());
    }

    [Fact]
    public async Task UpsertRecurringIncomeWithAmountsAsync_EklemeYazilamazsa_SilmeDeGeriAlinir()
    {
        var repository = new SqliteRecurringIncomeRepository(_connection);
        var income = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira geliri", PaymentDay = 20 };
        var first = Amount(income.Id, 12_500m, new DateOnly(2026, 1, 1));
        var planned = Amount(income.Id, 14_000m, new DateOnly(2027, 1, 20));
        await repository.UpsertRecurringIncomeWithAmountsAsync(income, [first, planned], []);

        await Assert.ThrowsAsync<SQLiteException>(() => repository.UpsertRecurringIncomeWithAmountsAsync(income, [first], [planned.Id]));

        Assert.Equal([first, planned], await repository.GetIncomeAmountHistoriesAsync());
    }

    private static IncomeAmountHistory Amount(Guid incomeId, decimal amount, DateOnly effectiveDate) =>
        new() { Id = Guid.NewGuid(), RecurringIncomeId = incomeId, Amount = amount, EffectiveDate = effectiveDate };
}
