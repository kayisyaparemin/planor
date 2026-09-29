using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// "Bakiye gir" sayfasının kaydetmeden önizlemesini doğrular: girilecek bakiye deftere taslak olarak eklenir,
/// gidişat onunla hesaplanır, hiçbir şey yazılmaz (S68, A28).
/// </summary>
public sealed class PeriodProgressServicePreviewTests
{
    private static readonly DateOnly DonemBasi = new(2026, 9, 1);
    private static readonly DateOnly DonemSonu = new(2026, 10, 1);
    private static readonly DateOnly Bugun = new(2026, 9, 20);

    private readonly InMemoryPeriodHistoryRepository _tarihce = new();
    private readonly InMemoryPeriodObservationRepository _gozlemler = new();
    private readonly InMemoryPaymentReminderRepository _hatirlaticilar = new();

    [Fact]
    public async Task PreviewAsync_GirilenBakiyeyleGidisatiHesaplar_HicbirSeyYazmaz()
    {
        // Hazırla — açılış 50.000, havuz 10.000; gözlem yok
        var plan = await AcikDonemKurAsync();

        // Uygula
        var onizleme = await Servis().PreviewAsync(40_000m, new DateOnly(2026, 9, 12));

        // Doğrula
        Assert.Equal(10_000m, onizleme.ObservedLivingSpend);
        Assert.Equal(40_000m, onizleme.ProjectedEndingBalance);
        Assert.Equal(40_000m, onizleme.Observation?.ObservedBalance);
        Assert.Empty(await _gozlemler.GetPeriodObservationsAsync(plan.Id));
    }

    [Fact]
    public async Task PreviewAsync_AyniGununGozlemiVarsa_OnunYerineGecer()
    {
        // Hazırla — 12 Eylül'de 45.000 girilmiş
        var plan = await AcikDonemKurAsync();
        await GozlemKaydetAsync(plan, new DateOnly(2026, 9, 12), 45_000m);

        // Uygula
        var onizleme = await Servis().PreviewAsync(40_000m, new DateOnly(2026, 9, 12));

        // Doğrula
        Assert.Equal(40_000m, onizleme.ProjectedEndingBalance);
        Assert.Equal(45_000m, Assert.Single(await _gozlemler.GetPeriodObservationsAsync(plan.Id)).ObservedBalance);
    }

    [Fact]
    public async Task PreviewAsync_GeriyeTarihliGiris_SonGozlemiDegistirmez()
    {
        // Hazırla — 20 Eylül'de 30.000 girilmiş; önizleme 10 Eylül'e
        var plan = await AcikDonemKurAsync();
        await GozlemKaydetAsync(plan, new DateOnly(2026, 9, 20), 30_000m);

        // Uygula
        var onizleme = await Servis().PreviewAsync(40_000m, new DateOnly(2026, 9, 10));

        // Doğrula — gidişat hâlâ 20'sindeki gözlemden
        Assert.Equal(new DateOnly(2026, 9, 20), onizleme.Observation?.ObservedOn);
        Assert.Equal(30_000m, onizleme.ProjectedEndingBalance);
    }

    [Fact]
    public async Task PreviewAsync_GelecekGun_KaydetmeyleAyniKuraldanReddeder()
    {
        await AcikDonemKurAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Servis().PreviewAsync(40_000m, Bugun.AddDays(1)));
    }

    [Fact]
    public async Task PreviewAsync_AcikDonemYoksa_Reddeder()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Servis().PreviewAsync(40_000m, Bugun));
    }

    private PeriodProgressService Servis() => new(
        new OpenPeriodLedgerReader(_tarihce, _gozlemler, _hatirlaticilar),
        new InMemoryUserSettingsRepository(),
        new InMemoryCreditCardRepository(),
        new CreditCardStatementCalculator(),
        new SabitSaat(Bugun));

    private async Task<PeriodPlanSnapshot> AcikDonemKurAsync()
    {
        var anlik = new DateTimeOffset(DonemBasi.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var durum = new FinancialSnapshot { Id = Guid.NewGuid(), SnapshotDate = DonemBasi, IsCurrent = true, CreatedAtUtc = anlik };
        var plan = new PeriodPlanSnapshot
        {
            FinancialSnapshotId = durum.Id,
            PeriodStart = DonemBasi,
            PeriodEnd = DonemSonu,
            SettlementAvailableFrom = DonemSonu,
            OpeningBalance = 50_000m,
            PlannedVariableExpenseAllowance = 10_000m,
            CreatedAtUtc = anlik
        };
        await _tarihce.SaveCurrentFinancialSnapshotAsync(durum, plan);
        return plan;
    }

    private Task GozlemKaydetAsync(PeriodPlanSnapshot plan, DateOnly gun, decimal bakiye) =>
        _gozlemler.UpsertPeriodObservationAsync(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedOn = gun,
            ObservedBalance = bakiye,
            RecordedAtUtc = new DateTimeOffset(gun.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero)
        });

    private sealed class SabitSaat(DateOnly bugun) : IClock
    {
        public DateOnly Today { get; } = bugun;

        public DateTimeOffset UtcNow { get; } = new(bugun.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }
}
