using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteAdHocIncomeRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqliteAdHocIncomeRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_adhoc_income_{Guid.NewGuid():N}.db3");
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
    public async Task GetAdHocIncomesAsync_Bosken_BosDiziDondurur()
    {
        var repository = new SqliteAdHocIncomeRepository(_connection);

        var list = await repository.GetAdHocIncomesAsync();

        Assert.Empty(list);
    }

    [Fact]
    public async Task UpsertAdHocIncomeAsync_TekSeferlikGelirEklerVeListeler()
    {
        var repository = new SqliteAdHocIncomeRepository(_connection);
        var id = Guid.NewGuid();
        var income = new AdHocIncome
        {
            Id = id,
            Amount = 45000m,
            ExactDate = new DateOnly(2026, 12, 25),
            Description = "Yıl Sonu Primi"
        };

        await repository.UpsertAdHocIncomeAsync(income);

        var list = await repository.GetAdHocIncomesAsync();
        Assert.Single(list);
        Assert.Equal(id, list[0].Id);
        Assert.Equal(45000m, list[0].Amount);
        Assert.Equal(new DateOnly(2026, 12, 25), list[0].ExactDate);
        Assert.Equal("Yıl Sonu Primi", list[0].Description);
    }

    [Fact]
    public async Task UpsertAdHocIncomeAsync_MevcutGeliriGunceller()
    {
        var repository = new SqliteAdHocIncomeRepository(_connection);
        var id = Guid.NewGuid();
        var initial = new AdHocIncome
        {
            Id = id,
            Amount = 10000m,
            ExactDate = new DateOnly(2026, 10, 1),
            Description = "Eski Tutar"
        };
        await repository.UpsertAdHocIncomeAsync(initial);

        var updated = initial with { Amount = 12000m, Description = "Yeni Tutar" };
        await repository.UpsertAdHocIncomeAsync(updated);

        var list = await repository.GetAdHocIncomesAsync();
        Assert.Single(list);
        Assert.Equal(12000m, list[0].Amount);
        Assert.Equal("Yeni Tutar", list[0].Description);
    }

    [Fact]
    public async Task DeleteAdHocIncomeAsync_BelirtilenGeliriSiler()
    {
        var repository = new SqliteAdHocIncomeRepository(_connection);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await repository.UpsertAdHocIncomeAsync(new AdHocIncome { Id = id1, Amount = 100m, ExactDate = new DateOnly(2026, 1, 1), Description = "G1" });
        await repository.UpsertAdHocIncomeAsync(new AdHocIncome { Id = id2, Amount = 200m, ExactDate = new DateOnly(2026, 1, 2), Description = "G2" });

        await repository.DeleteAdHocIncomeAsync(id1);

        var list = await repository.GetAdHocIncomesAsync();
        Assert.Single(list);
        Assert.Equal(id2, list[0].Id);
    }
}
