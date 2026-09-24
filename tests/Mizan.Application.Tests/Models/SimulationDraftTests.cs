using Mizan.Application.Models;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Models;

public sealed class SimulationDraftTests
{
    [Fact]
    public void SimulationDraft_OzelliklerVeKondisyonSayilari_DogruHesaplanmalidir()
    {
        var scenarioId1 = Guid.NewGuid();
        var scenarioId2 = Guid.NewGuid();

        var request1 = new SimulationRequest(SimulationScenarioType.CashPurchase, "Tadilat", 50_000m, new DateOnly(2026, 10, 15)) { ScenarioId = scenarioId1 };
        var request2 = new SimulationRequest(SimulationScenarioType.CreditCardInstallmentPurchase, "Telefon", 30_000m, new DateOnly(2026, 11, 1), paymentCount: 6) { ScenarioId = scenarioId2 };

        var condition1 = new SimulationDraftCondition(request1, IsEnabled: true);
        var condition2 = new SimulationDraftCondition(request2, IsEnabled: false);

        var draftId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var draft = new SimulationDraft(draftId, "Deneme Taslağı", now, now, [condition1, condition2]);

        Assert.Equal(draftId, draft.Id);
        Assert.Equal("Deneme Taslağı", draft.Name);
        Assert.Equal(2, draft.Conditions.Count);
        Assert.Equal(1, draft.EnabledConditionCount);
        Assert.True(draft.HasEnabledConditions);
    }

    [Fact]
    public void SimulationDraft_KosulYoksa_AktifKondisyonSayisiSifirOlmali()
    {
        var draft = new SimulationDraft(Guid.NewGuid(), "Boş Taslak", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null!);

        Assert.NotNull(draft.Conditions);
        Assert.Empty(draft.Conditions);
        Assert.Equal(0, draft.EnabledConditionCount);
        Assert.False(draft.HasEnabledConditions);
    }

    [Fact]
    public void SimulationDraftCondition_VarsayilanDeger_AktifOlmali()
    {
        var request = new SimulationRequest(SimulationScenarioType.CashPurchase, "Alışveriş", 10_000m, new DateOnly(2026, 10, 1));
        var condition = new SimulationDraftCondition(request);

        Assert.True(condition.IsEnabled);
        Assert.Equal(request, condition.Request);
    }

    [Fact]
    public async Task SimulationDraftRepository_UpsertVeGetById_KaydiBulmalidir()
    {
        var repo = new InMemorySimulationDraftRepository();
        var draftId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var request = new SimulationRequest(SimulationScenarioType.CashPurchase, "Masa", 5_000m, new DateOnly(2026, 10, 20)) { ScenarioId = Guid.NewGuid() };

        var draft = new SimulationDraft(draftId, "Mobilya Denemesi", now, now, [new SimulationDraftCondition(request, true)]);

        await repo.UpsertDraftAsync(draft);
        var loaded = await repo.GetDraftByIdAsync(draftId);

        Assert.NotNull(loaded);
        Assert.Equal(draftId, loaded.Id);
        Assert.Equal("Mobilya Denemesi", loaded.Name);
        Assert.Single(loaded.Conditions);
        Assert.Equal("Masa", loaded.Conditions[0].Request.Name);
    }

    [Fact]
    public async Task SimulationDraftRepository_KosulSirasiniVeDurumunu_Korumalidir()
    {
        var repo = new InMemorySimulationDraftRepository();
        var draftId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var cond1 = new SimulationDraftCondition(new SimulationRequest(SimulationScenarioType.CashPurchase, "Birinci", 1_000m, new DateOnly(2026, 10, 1)), IsEnabled: true);
        var cond2 = new SimulationDraftCondition(new SimulationRequest(SimulationScenarioType.CashPurchase, "İkinci", 2_000m, new DateOnly(2026, 10, 2)), IsEnabled: false);
        var cond3 = new SimulationDraftCondition(new SimulationRequest(SimulationScenarioType.CashPurchase, "Üçüncü", 3_000m, new DateOnly(2026, 10, 3)), IsEnabled: true);

        var draft = new SimulationDraft(draftId, "Sıralı Plan", now, now, [cond1, cond2, cond3]);

        await repo.UpsertDraftAsync(draft);
        var loaded = await repo.GetDraftByIdAsync(draftId);

        Assert.NotNull(loaded);
        Assert.Equal(3, loaded.Conditions.Count);
        Assert.Equal("Birinci", loaded.Conditions[0].Request.Name);
        Assert.True(loaded.Conditions[0].IsEnabled);
        Assert.Equal("İkinci", loaded.Conditions[1].Request.Name);
        Assert.False(loaded.Conditions[1].IsEnabled);
        Assert.Equal("Üçüncü", loaded.Conditions[2].Request.Name);
        Assert.True(loaded.Conditions[2].IsEnabled);
        Assert.Equal(2, loaded.EnabledConditionCount);
    }

    [Fact]
    public async Task SimulationDraftRepository_GetDrafts_GuncellenmeTarihineGoreAzalanSiralamalidir()
    {
        var repo = new InMemorySimulationDraftRepository();
        var baseTime = DateTimeOffset.UtcNow;

        var draft1 = new SimulationDraft(
            Guid.NewGuid(), "Eski Taslak", baseTime.AddHours(-2), baseTime.AddHours(-2),
            [new SimulationDraftCondition(new SimulationRequest(SimulationScenarioType.CashPurchase, "A", 100m, new DateOnly(2026, 10, 1)))]);

        var draft2 = new SimulationDraft(
            Guid.NewGuid(), "Yeni Taslak", baseTime.AddHours(-1), baseTime.AddMinutes(-5),
            [new SimulationDraftCondition(new SimulationRequest(SimulationScenarioType.CashPurchase, "B", 200m, new DateOnly(2026, 10, 1)))]);

        await repo.UpsertDraftAsync(draft1);
        await repo.UpsertDraftAsync(draft2);

        var drafts = await repo.GetDraftsAsync();

        Assert.Equal(2, drafts.Count);
        Assert.Equal("Yeni Taslak", drafts[0].Name);
        Assert.Equal("Eski Taslak", drafts[1].Name);
    }

    [Fact]
    public async Task SimulationDraftRepository_Delete_TaslagiSilmelidir()
    {
        var repo = new InMemorySimulationDraftRepository();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var draft1 = new SimulationDraft(id1, "Kalacak", now, now, []);
        var draft2 = new SimulationDraft(id2, "Silinecek", now, now, []);

        await repo.UpsertDraftAsync(draft1);
        await repo.UpsertDraftAsync(draft2);

        await repo.DeleteDraftAsync(id2);

        var drafts = await repo.GetDraftsAsync();
        Assert.Single(drafts);
        Assert.Equal(id1, drafts[0].Id);

        var deleted = await repo.GetDraftByIdAsync(id2);
        Assert.Null(deleted);
    }
}
