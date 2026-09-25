using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqlitePlannedLargeExpenseRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqlitePlannedLargeExpenseRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_expense_{Guid.NewGuid():N}.db3");
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
    public async Task GetPlannedLargeExpensesAsync_Bosken_BosDiziDondurur()
    {
        var repository = new SqlitePlannedLargeExpenseRepository(_connection);

        var list = await repository.GetPlannedLargeExpensesAsync();

        Assert.Empty(list);
    }

    [Fact]
    public async Task UpsertPlannedLargeExpenseAsync_BuyukHarcamaEklerVeGeriOkur()
    {
        var repository = new SqlitePlannedLargeExpenseRepository(_connection);
        var id = Guid.NewGuid();
        var expense = new PlannedLargeExpense
        {
            Id = id,
            Name = "Yaz Tatili",
            Amount = 35000m,
            ExactDate = new DateOnly(2027, 7, 15),
            Note = "Otel rezervasyonu",
            Status = PlannedExpenseStatus.Planned
        };

        await repository.UpsertPlannedLargeExpenseAsync(expense);

        var list = await repository.GetPlannedLargeExpensesAsync();
        Assert.Single(list);
        var read = list[0];
        Assert.Equal(id, read.Id);
        Assert.Equal("Yaz Tatili", read.Name);
        Assert.Equal(35000m, read.Amount);
        Assert.Equal(new DateOnly(2027, 7, 15), read.ExactDate);
        Assert.Equal("Otel rezervasyonu", read.Note);
        Assert.Equal(PlannedExpenseStatus.Planned, read.Status);
    }

    [Fact]
    public async Task DeletePlannedLargeExpenseAsync_BelirtilenBuyukHarcamayiSiler()
    {
        var repository = new SqlitePlannedLargeExpenseRepository(_connection);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await repository.UpsertPlannedLargeExpenseAsync(new PlannedLargeExpense { Id = id1, Name = "Harcama 1", Amount = 1000m, ExactDate = new DateOnly(2026, 11, 1) });
        await repository.UpsertPlannedLargeExpenseAsync(new PlannedLargeExpense { Id = id2, Name = "Harcama 2", Amount = 2000m, ExactDate = new DateOnly(2026, 12, 1) });

        await repository.DeletePlannedLargeExpenseAsync(id1);

        var list = await repository.GetPlannedLargeExpensesAsync();
        Assert.Single(list);
        Assert.Equal(id2, list[0].Id);
    }
}
