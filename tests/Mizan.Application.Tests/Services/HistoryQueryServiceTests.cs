using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class HistoryQueryServiceTests
{
    private static readonly DateOnly AugustStart = new(2026, 8, 10);
    private static readonly DateOnly SeptemberStart = new(2026, 9, 10);

    private readonly InMemoryPeriodHistoryRepository _repository = new();

    private HistoryQueryService CreateService() =>
        new(_repository, new PlanActualComparisonCalculator());

    [Fact]
    public void Constructor_BagimlilikNullsa_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new HistoryQueryService(null!, new PlanActualComparisonCalculator()));
        Assert.Throws<ArgumentNullException>(() =>
            new HistoryQueryService(_repository, null!));
    }

    [Fact]
    public async Task GetPeriodsAsync_TarihceBossa_BosListeDondurur()
    {
        Assert.Empty(await CreateService().GetPeriodsAsync());
    }

    [Fact]
    public async Task GetPeriodsAsync_GerceklesmesiOlmayanAcikPlan_Listelenmez()
    {
        var closed = await ClosePeriodAsync(AugustStart, 10_000m, 12_000m, 11_500m);

        var periods = await CreateService().GetPeriodsAsync();

        // Kapanış taahhüdü yeni dönemin açık planını da kaydetti; o listede yer almaz.
        Assert.Equal(2, _repository.Plans.Count);
        var period = Assert.Single(periods);
        Assert.Equal(closed.Plan.Id, period.OriginalPlan.Id);
    }

    [Fact]
    public async Task GetPeriodsAsync_DonemleriEnYenidenEskiyeSiralar()
    {
        await ClosePeriodAsync(new DateOnly(2026, 6, 10), 0m, 1_000m, 1_000m);
        await ClosePeriodAsync(new DateOnly(2026, 9, 10), 0m, 1_000m, 1_000m);
        await ClosePeriodAsync(new DateOnly(2026, 7, 10), 0m, 1_000m, 1_000m);
        await ClosePeriodAsync(new DateOnly(2026, 8, 10), 0m, 1_000m, 1_000m);

        var periods = await CreateService().GetPeriodsAsync();

        Assert.Equal(
            new[]
            {
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 8, 10),
                new DateOnly(2026, 7, 10),
                new DateOnly(2026, 6, 10)
            },
            periods.Select(x => x.OriginalPlan.PeriodStart));
    }

    [Fact]
    public async Task GetPeriodsAsync_RevizyonYoksa_KarneOrijinalPlanaGoreCikar()
    {
        var closed = await ClosePeriodAsync(AugustStart, 10_000m, 12_000m, 11_500m);

        var period = Assert.Single(await CreateService().GetPeriodsAsync());

        Assert.Null(period.Revision);
        Assert.Equal(0, period.RevisionCount);
        Assert.Equal(closed.Actual.Id, period.Actual.Id);
        Assert.Equal(closed.Result.Id, period.ResultSnapshot.Id);
        Assert.Equal(12_000m, period.Comparison.PlannedEndingBalance);
        Assert.Equal(11_500m, period.Comparison.ActualEndingBalance);
        Assert.Equal(-500m, period.Comparison.Difference);
    }

    [Fact]
    public async Task GetPeriodsAsync_KapanisaAcilisGunuSonSaniyesindekiRevizyon_NihaiPlandir()
    {
        await ClosePeriodAsync(
            AugustStart,
            10_000m,
            11_000m,
            13_500m,
            (1, Utc(2026, 8, 25, 9, 0, 0), 12_000m),
            (2, Utc(2026, 9, 10, 23, 59, 59), 13_000m));

        var period = Assert.Single(await CreateService().GetPeriodsAsync());

        Assert.Equal(2, period.Revision!.RevisionNumber);
        Assert.Equal(2, period.RevisionCount);
        Assert.Equal(13_000m, period.Comparison.PlannedEndingBalance);
        Assert.Equal(500m, period.Comparison.Difference);
        // Nihai plan karneye girer ama orijinal taahhüt değişmeden korunur.
        Assert.Equal(11_000m, period.OriginalPlan.PlannedEndingBalance);
    }

    [Fact]
    public async Task GetPeriodsAsync_KapanisaAcilisGunundenSonrakiRevizyon_KarneyeGirmez()
    {
        await ClosePeriodAsync(
            AugustStart,
            10_000m,
            11_000m,
            13_500m,
            (1, Utc(2026, 8, 25, 9, 0, 0), 12_000m),
            (2, Utc(2026, 9, 10, 23, 59, 59), 13_000m),
            (3, Utc(2026, 9, 11, 0, 0, 0), 15_000m));

        var period = Assert.Single(await CreateService().GetPeriodsAsync());

        Assert.Equal(2, period.Revision!.RevisionNumber);
        Assert.Equal(2, period.RevisionCount);
        Assert.Equal(13_000m, period.Comparison.PlannedEndingBalance);
        Assert.Equal(500m, period.Comparison.Difference);
    }

    [Fact]
    public async Task GetPeriodsAsync_SaatDilimliRevizyon_UtcTakvimGunuyleDegerlendirilir()
    {
        var turkey = TimeSpan.FromHours(3);
        await ClosePeriodAsync(
            AugustStart,
            10_000m,
            11_000m,
            12_000m,
            // Yerel 11.09 01:00 = UTC 10.09 22:00: kapanışa açılış günü, dahil.
            (1, new DateTimeOffset(2026, 9, 11, 1, 0, 0, turkey), 12_500m),
            // Yerel 11.09 03:00 = UTC 11.09 00:00: ertesi gün, hariç.
            (2, new DateTimeOffset(2026, 9, 11, 3, 0, 0, turkey), 14_000m));

        var period = Assert.Single(await CreateService().GetPeriodsAsync());

        Assert.Equal(1, period.Revision!.RevisionNumber);
        Assert.Equal(1, period.RevisionCount);
        Assert.Equal(12_500m, period.Comparison.PlannedEndingBalance);
    }

    [Fact]
    public async Task GetPeriodsAsync_AyniAndaOlusanRevizyonlarda_BuyukNumaraKazanir()
    {
        var createdAt = Utc(2026, 9, 1, 12, 0, 0);
        await ClosePeriodAsync(
            AugustStart,
            10_000m,
            11_000m,
            11_000m,
            (7, createdAt, 17_000m),
            (2, createdAt, 12_000m));

        var period = Assert.Single(await CreateService().GetPeriodsAsync());

        Assert.Equal(7, period.Revision!.RevisionNumber);
        Assert.Equal(17_000m, period.Comparison.PlannedEndingBalance);
    }

    [Fact]
    public async Task GetPeriodsAsync_RevizyonlarYalnizKendiDonemineSayilir()
    {
        await ClosePeriodAsync(
            AugustStart,
            10_000m,
            11_000m,
            11_000m,
            (1, Utc(2026, 8, 20, 9, 0, 0), 11_500m));
        await ClosePeriodAsync(SeptemberStart, 11_000m, 12_000m, 12_000m);

        var periods = await CreateService().GetPeriodsAsync();

        var september = periods.Single(x => x.OriginalPlan.PeriodStart == SeptemberStart);
        var august = periods.Single(x => x.OriginalPlan.PeriodStart == AugustStart);
        Assert.Null(september.Revision);
        Assert.Equal(0, september.RevisionCount);
        Assert.Equal(1, august.RevisionCount);
        Assert.Equal(11_500m, august.Comparison.PlannedEndingBalance);
    }

    [Fact]
    public async Task GetPeriodAsync_BilinenGerceklesme_OnaAitDonemiDondurur()
    {
        await ClosePeriodAsync(AugustStart, 10_000m, 11_000m, 11_000m);
        var september = await ClosePeriodAsync(SeptemberStart, 11_000m, 12_000m, 12_250m);

        var period = await CreateService().GetPeriodAsync(september.Actual.Id);

        Assert.NotNull(period);
        Assert.Equal(september.Plan.Id, period.OriginalPlan.Id);
        Assert.Equal(september.Result.Id, period.ResultSnapshot.Id);
        Assert.Equal(250m, period.Comparison.Difference);
    }

    [Fact]
    public async Task GetPeriodAsync_BilinmeyenGerceklesme_NullDondurur()
    {
        await ClosePeriodAsync(AugustStart, 10_000m, 11_000m, 11_000m);

        Assert.Null(await CreateService().GetPeriodAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetRecentSummaryAsync_KapanmisDonemYoksa_NullDondurur()
    {
        Assert.Null(await CreateService().GetRecentSummaryAsync());
    }

    [Fact]
    public async Task GetRecentSummaryAsync_SonDonemlerinNetDegisimleriniToplar()
    {
        await SeedFourPeriodsAsync();

        var summary = await CreateService().GetRecentSummaryAsync(2);

        // Eylül (+4.000 / +4.400) ve Ağustos (+3.000 / +2.999,99). Bakiyeler toplansaydı
        // planlanan 35.900,99 çıkardı; bu sayı kullanıcının hesabında hiç olmadı (S29).
        Assert.NotNull(summary);
        Assert.Equal(2, summary.PeriodCount);
        Assert.Equal(7_000m, summary.PlannedNetChange);
        Assert.Equal(7_399.99m, summary.ActualNetChange);
        Assert.Equal(399.99m, summary.Difference);
    }

    [Fact]
    public async Task GetRecentSummaryAsync_VarsayilanOlarakSonUcDonemiOzetler()
    {
        await SeedFourPeriodsAsync();

        var summary = await CreateService().GetRecentSummaryAsync();

        Assert.NotNull(summary);
        Assert.Equal(3, summary.PeriodCount);
        Assert.Equal(9_000m, summary.PlannedNetChange);
        Assert.Equal(9_450.49m, summary.ActualNetChange);
        Assert.Equal(450.49m, summary.Difference);
    }

    [Fact]
    public async Task GetRecentSummaryAsync_IstenenAdetMevcuttanFazlaysa_TumDonemleriToplar()
    {
        await SeedFourPeriodsAsync();

        var summary = await CreateService().GetRecentSummaryAsync(10);

        Assert.NotNull(summary);
        Assert.Equal(4, summary.PeriodCount);
        Assert.Equal(10_000m, summary.PlannedNetChange);
        Assert.Equal(10_350.49m, summary.ActualNetChange);
        Assert.Equal(350.49m, summary.Difference);
    }

    [Fact]
    public async Task GetRecentSummaryAsync_RevizyonluTekDonemde_NihaiPlaninNetDegisiminiKullanir()
    {
        await ClosePeriodAsync(
            AugustStart,
            10_000m,
            11_000m,
            11_500m,
            (1, Utc(2026, 8, 20, 9, 0, 0), 12_000m));

        var summary = await CreateService().GetRecentSummaryAsync();

        Assert.NotNull(summary);
        Assert.Equal(1, summary.PeriodCount);
        Assert.Equal(2_000m, summary.PlannedNetChange);
        Assert.Equal(1_500m, summary.ActualNetChange);
        Assert.Equal(-500m, summary.Difference);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetRecentSummaryAsync_AdetSifirVeyaNegatifse_ArgumentOutOfRangeExceptionFirlatir(int periodCount)
    {
        await SeedFourPeriodsAsync();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CreateService().GetRecentSummaryAsync(periodCount));
    }

    /// <summary>
    /// Açılışları bir önceki dönemin teyitli kapanışına eşit dört dönem (I21), karışık sırada.
    /// Net değişimler: planlanan +1.000, +2.000, +3.000, +4.000; fiilî +900, +2.050,50, +2.999,99, +4.400.
    /// </summary>
    private async Task SeedFourPeriodsAsync()
    {
        await ClosePeriodAsync(new DateOnly(2026, 6, 10), 10_000m, 11_000m, 10_900m);
        await ClosePeriodAsync(new DateOnly(2026, 9, 10), 15_950.49m, 19_950.49m, 20_350.49m);
        await ClosePeriodAsync(new DateOnly(2026, 7, 10), 10_900m, 12_900m, 12_950.50m);
        await ClosePeriodAsync(new DateOnly(2026, 8, 10), 12_950.50m, 15_950.50m, 15_950.49m);
    }

    /// <summary>
    /// Bir dönemi uçtan uca kaydeder: planı dondurur, revizyonları ekler ve dönemi kapatır.
    /// Kapanış taahhüdü, gerçek akıştaki gibi yeni dönemin açık planını da kaydeder.
    /// </summary>
    private async Task<(PeriodPlanSnapshot Plan, PeriodActual Actual, FinancialSnapshot Result)> ClosePeriodAsync(
        DateOnly start,
        decimal opening,
        decimal plannedEnding,
        decimal confirmedEnding,
        params (int Number, DateTimeOffset CreatedAtUtc, decimal PlannedEnding)[] revisions)
    {
        var end = start.AddMonths(1);
        var source = new FinancialSnapshot { SnapshotDate = start, ProjectionOpeningBalance = opening };
        var plan = new PeriodPlanSnapshot
        {
            FinancialSnapshotId = source.Id,
            PeriodStart = start,
            PeriodEnd = end,
            SettlementAvailableFrom = end,
            OpeningBalance = opening,
            PlannedEndingBalance = plannedEnding
        };
        await _repository.SaveCurrentFinancialSnapshotAsync(source, plan);

        foreach (var revision in revisions)
        {
            await _repository.SavePeriodPlanRevisionAsync(new PeriodPlanRevision
            {
                PeriodPlanSnapshotId = plan.Id,
                RevisionNumber = revision.Number,
                CreatedAtUtc = revision.CreatedAtUtc,
                PlannedEndingBalance = revision.PlannedEnding
            });
        }

        var result = new FinancialSnapshot { SnapshotDate = end, ProjectionOpeningBalance = confirmedEnding };
        var actual = new PeriodActual
        {
            PeriodPlanSnapshotId = plan.Id,
            SourceFinancialSnapshotId = source.Id,
            ResultFinancialSnapshotId = result.Id,
            PeriodStart = start,
            PeriodEnd = end,
            ConfirmedEndingBalance = confirmedEnding
        };
        await _repository.CommitPeriodSettlementAsync(new PeriodSettlementCommit
        {
            Actual = actual,
            NewSnapshot = result,
            NewPlan = new PeriodPlanSnapshot
            {
                FinancialSnapshotId = result.Id,
                PeriodStart = end,
                PeriodEnd = end.AddMonths(1),
                SettlementAvailableFrom = end.AddMonths(1),
                OpeningBalance = confirmedEnding
            }
        });

        return (plan, actual, result);
    }

    private static DateTimeOffset Utc(int year, int month, int day, int hour, int minute, int second) =>
        new(year, month, day, hour, minute, second, TimeSpan.Zero);
}
