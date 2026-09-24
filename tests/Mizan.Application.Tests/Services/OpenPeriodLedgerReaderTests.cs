using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class OpenPeriodLedgerReaderTests
{
    private static readonly DateOnly DonemBasi = new(2026, 9, 10);
    private static readonly DateOnly DonemSonu = new(2026, 10, 10);
    private static readonly DateTimeOffset Sabah = new(2026, 9, 10, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryPeriodHistoryRepository _tarihce = new();
    private readonly InMemoryPeriodObservationRepository _gozlemler = new();
    private readonly InMemoryPaymentReminderRepository _hatirlaticilar = new();

    private OpenPeriodLedgerReader CreateReader() => new(_tarihce, _gozlemler, _hatirlaticilar);

    [Fact]
    public async Task ReadAsync_AcikDonemYoksa_NullDondurur()
    {
        // Hazırla
        var okuyucu = CreateReader();

        // Uygula
        var defter = await okuyucu.ReadAsync();

        // Doğrula
        Assert.Null(defter);
    }

    [Fact]
    public async Task ReadAsync_AcikDonemVarsa_PlanRevizyonGozlemVeCevaplariBirlikteGetirir()
    {
        // Hazırla
        var plan = await AcikDonemKurAsync();
        var ikinciRevizyon = Revizyon(plan.Id, 2, Sabah.AddDays(3));
        var birinciRevizyon = Revizyon(plan.Id, 1, Sabah.AddDays(1));
        await _tarihce.SavePeriodPlanRevisionAsync(ikinciRevizyon);
        await _tarihce.SavePeriodPlanRevisionAsync(birinciRevizyon);
        var gozlem = new PeriodObservation { PeriodPlanSnapshotId = plan.Id, ObservedOn = DonemBasi.AddDays(2), ObservedBalance = 30_000m };
        await _gozlemler.UpsertPeriodObservationAsync(gozlem);
        var cevap = Cevap(DonemBasi.AddDays(5));
        await _hatirlaticilar.UpsertResponsesAsync([cevap]);

        // Uygula
        var defter = await CreateReader().ReadAsync();

        // Doğrula
        Assert.NotNull(defter);
        Assert.Equal(plan.Id, defter.Plan.Id);
        Assert.Equal([birinciRevizyon.Id, ikinciRevizyon.Id], defter.Revisions.Select(x => x.Id));
        Assert.Equal(gozlem.Id, defter.Observation?.Id);
        Assert.Equal([cevap.DueKey], defter.ReminderAnswers.Select(x => x.DueKey));
    }

    [Fact]
    public async Task ReadAsync_VadesiDonemDisindakiCevaplar_DeftereGirmez()
    {
        // Hazırla — dönem [10 Eylül, 10 Ekim): ilk gün dahil, 10 Ekim sonraki dönemin ilk günü
        await AcikDonemKurAsync();
        var oncekiDonem = Cevap(DonemBasi.AddDays(-1));
        var ilkGun = Cevap(DonemBasi);
        var sonGun = Cevap(DonemSonu.AddDays(-1));
        var sonrakiDonem = Cevap(DonemSonu);
        await _hatirlaticilar.UpsertResponsesAsync([oncekiDonem, ilkGun, sonGun, sonrakiDonem]);

        // Uygula
        var defter = await CreateReader().ReadAsync();

        // Doğrula
        Assert.NotNull(defter);
        Assert.Equal(
            new[] { ilkGun.DueKey, sonGun.DueKey }.Order(),
            defter.ReminderAnswers.Select(x => x.DueKey).Order());
    }

    [Fact]
    public async Task ReadAsync_GozlemBaskaDonemPlaninaAitse_DefterdeGozlemYoktur()
    {
        // Hazırla
        await AcikDonemKurAsync();
        await _gozlemler.UpsertPeriodObservationAsync(new PeriodObservation { PeriodPlanSnapshotId = Guid.NewGuid(), ObservedBalance = 1m });

        // Uygula
        var defter = await CreateReader().ReadAsync();

        // Doğrula
        Assert.NotNull(defter);
        Assert.Null(defter.Observation);
        Assert.Empty(defter.Revisions);
    }

    [Fact]
    public void Yapici_BagimlilikEksikse_ArgumentNullExceptionFirlatir()
    {
        // Hazırla — üç portun her biri tek tek eksik bırakılır

        // Uygula
        var tarihceYok = Record.Exception(() => new OpenPeriodLedgerReader(null!, _gozlemler, _hatirlaticilar));
        var gozlemYok = Record.Exception(() => new OpenPeriodLedgerReader(_tarihce, null!, _hatirlaticilar));
        var hatirlaticiYok = Record.Exception(() => new OpenPeriodLedgerReader(_tarihce, _gozlemler, null!));

        // Doğrula
        Assert.IsType<ArgumentNullException>(tarihceYok);
        Assert.IsType<ArgumentNullException>(gozlemYok);
        Assert.IsType<ArgumentNullException>(hatirlaticiYok);
    }

    private async Task<PeriodPlanSnapshot> AcikDonemKurAsync()
    {
        var durum = new FinancialSnapshot { Id = Guid.NewGuid(), SnapshotDate = DonemBasi, IsCurrent = true, CreatedAtUtc = Sabah };
        var plan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = durum.Id,
            PeriodStart = DonemBasi,
            PeriodEnd = DonemSonu,
            SettlementAvailableFrom = DonemSonu,
            CreatedAtUtc = Sabah
        };
        await _tarihce.SaveCurrentFinancialSnapshotAsync(durum, plan);
        return plan;
    }

    private static PeriodPlanRevision Revizyon(Guid planId, int siraNumarasi, DateTimeOffset olusturulma) => new()
    {
        PeriodPlanSnapshotId = planId,
        RevisionNumber = siraNumarasi,
        CreatedAtUtc = olusturulma
    };

    private static PaymentReminderResponse Cevap(DateOnly vade) => new()
    {
        DueKey = $"{Guid.NewGuid():N}-{vade:yyyyMMdd}",
        Name = "Kredi",
        DueDate = vade,
        Kind = PaymentReminderAnswerKind.Paid,
        AnsweredAt = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc)
    };
}
