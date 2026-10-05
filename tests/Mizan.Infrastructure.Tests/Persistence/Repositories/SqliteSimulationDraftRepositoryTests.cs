using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Entities;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteSimulationDraftRepositoryTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_drafts_{Guid.NewGuid():N}.db3");
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
    public async Task GetDraftsAsync_BosVeritabaninda_BosDiziDoner()
    {
        var repository = new SqliteSimulationDraftRepository(_connection);

        var drafts = await repository.GetDraftsAsync();

        Assert.Empty(drafts);
    }

    [Fact]
    public async Task UpsertDraftAsync_YeniTaslakVeKosullari_KaydederVeOkur()
    {
        var repository = new SqliteSimulationDraftRepository(_connection);
        var draftId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var condition1 = new SimulationDraftCondition(
            new SimulationRequest(
                SimulationScenarioType.CreditCardInstallmentPurchase,
                "Yeni Telefon",
                30000m,
                new DateOnly(2026, 9, 25),
                6)
            {
                CreditCardId = Guid.NewGuid()
            },
            IsEnabled: true);

        var condition2 = new SimulationDraftCondition(
            new SimulationRequest(
                SimulationScenarioType.FinancingLoan,
                "İhtiyaç Kredisi",
                50000m,
                new DateOnly(2026, 10, 1),
                12)
            {
                TotalRepaymentAmount = 65000m
            },
            IsEnabled: false);

        var draft = new SimulationDraft(draftId, "Senaryo A", now, now, [condition1, condition2]);

        await repository.UpsertDraftAsync(draft);
        var loaded = await repository.GetDraftByIdAsync(draftId);

        Assert.NotNull(loaded);
        Assert.Equal("Senaryo A", loaded.Name);
        Assert.Equal(2, loaded.Conditions.Count);
        Assert.Equal("Yeni Telefon", loaded.Conditions[0].Request.Name);
        Assert.True(loaded.Conditions[0].IsEnabled);
        Assert.Equal("İhtiyaç Kredisi", loaded.Conditions[1].Request.Name);
        Assert.False(loaded.Conditions[1].IsEnabled);
    }

    [Fact]
    public async Task UpsertDraftAsync_MevcutTaslakGuncellendiginde_KosullariBastanYazar()
    {
        var repository = new SqliteSimulationDraftRepository(_connection);
        var draftId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var initialCondition = new SimulationDraftCondition(
            new SimulationRequest(SimulationScenarioType.CashPurchase, "Buzdolabı", 20000m, new DateOnly(2026, 9, 25)),
            IsEnabled: true);
        var initialDraft = new SimulationDraft(draftId, "Taslak 1", now, now, [initialCondition]);
        await repository.UpsertDraftAsync(initialDraft);

        var newCondition = new SimulationDraftCondition(
            new SimulationRequest(SimulationScenarioType.FutureIncome, "Prim", 15000m, new DateOnly(2026, 10, 5)),
            IsEnabled: true);
        var updatedDraft = new SimulationDraft(draftId, "Taslak 1 - Revize", now, now.AddHours(1), [newCondition]);
        await repository.UpsertDraftAsync(updatedDraft);

        var loaded = await repository.GetDraftByIdAsync(draftId);
        Assert.NotNull(loaded);
        Assert.Equal("Taslak 1 - Revize", loaded.Name);
        var cond = Assert.Single(loaded.Conditions);
        Assert.Equal("Prim", cond.Request.Name);
    }

    [Fact]
    public async Task DeleteDraftAsync_TaslagiSiler_CascadeIleKosullariDaTemizler()
    {
        var repository = new SqliteSimulationDraftRepository(_connection);
        var draftId = Guid.NewGuid();
        var condition = new SimulationDraftCondition(
            new SimulationRequest(SimulationScenarioType.CashPurchase, "Tatil", 40000m, new DateOnly(2026, 10, 1)),
            IsEnabled: true);
        var draft = new SimulationDraft(draftId, "Tatil Planı", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, [condition]);
        await repository.UpsertDraftAsync(draft);

        await repository.DeleteDraftAsync(draftId);

        var loaded = await repository.GetDraftByIdAsync(draftId);
        Assert.Null(loaded);

        var draftIdStr = draftId.ToString();
        var orphanConditions = await _connection.Table<SimulationDraftConditionEntity>()
            .Where(c => c.DraftId == draftIdStr)
            .ToListAsync();
        Assert.Empty(orphanConditions);
    }
}
