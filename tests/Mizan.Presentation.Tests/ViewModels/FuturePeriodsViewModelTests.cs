using Mizan.Domain.Models;
using Mizan.Presentation.Charts;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// 12 Dönem görünüm modelinin (EK-V8, S74) davranışı: karolar, en düşük dönem sonu, 12 dönem sonrası, faiz,
/// grafik ve eksen uçları, üç durum ve boş hâlin aksiyonu.
/// </summary>
public sealed class FuturePeriodsViewModelTests
{
    private static readonly DateOnly ChainStart = new(2026, 10, 10);
    private const decimal Opening = 41_723m;
    private static readonly decimal[] DippingEndings =
        [44_120m, 47_900m, 38_300m, 41_050m, 45_600m, -18_250m, -12_900m, -6_100m, 1_450m, 9_800m, 17_600m, 26_300m];

    private readonly FakeFutureProjectionService _projectionService = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FuturePeriodsViewModel _viewModel;

    public FuturePeriodsViewModelTests()
    {
        _viewModel = new FuturePeriodsViewModel(_projectionService, _navigation);
    }

    [Fact]
    public async Task Yukle_Zincir_OnIkiKaroyuSirasiylaKurar()
    {
        _projectionService.Result = Projection(DippingEndings);

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(12, _viewModel.Periods.Count);
        Assert.Equal(Enumerable.Range(0, 12), _viewModel.Periods.Select(x => x.Index));
        Assert.Equal(ChainStart, _viewModel.Periods[0].PeriodStart);
        Assert.Equal(new DateOnly(2027, 9, 10), _viewModel.Periods[11].PeriodStart);
        Assert.Equal(-18_250m, _viewModel.Periods[5].EndingBalance);
        Assert.True(_viewModel.Periods[5].IsNegative);
        Assert.False(_viewModel.Periods[4].IsNegative);
    }

    [Fact]
    public async Task Yukle_EnDusukDonemSonu_HeroOlurVeKarosuIsaretlenir()
    {
        _projectionService.Result = Projection(DippingEndings);

        await _viewModel.LoadAsync();

        Assert.Equal(-18_250m, _viewModel.LowestEndingBalance);
        Assert.Equal(new DateOnly(2027, 3, 10), _viewModel.LowestPeriodStart);
        Assert.Equal(5, Assert.Single(_viewModel.Periods, x => x.IsLowest).Index);
    }

    [Fact]
    public async Task Yukle_EnDusukBirdenFazlaDonemdeyse_IlkiSecilir()
    {
        _projectionService.Result = Projection([5_000m, 3_000m, 7_000m, 3_000m, 9_000m, 10_000m, 11_000m, 12_000m, 13_000m, 14_000m, 15_000m, 16_000m]);

        await _viewModel.LoadAsync();

        Assert.Equal(new DateOnly(2026, 11, 10), _viewModel.LowestPeriodStart);
        Assert.Equal(1, Assert.Single(_viewModel.Periods, x => x.IsLowest).Index);
    }

    [Fact]
    public async Task Yukle_HicEksiyeDusmuyorsa_EnDusukYineGosterilir()
    {
        _projectionService.Result = Projection([44_120m, 47_900m, 38_300m, 41_050m, 45_600m, 49_800m, 52_400m, 50_100m, 54_300m, 57_900m, 61_200m, 64_500m]);

        await _viewModel.LoadAsync();

        Assert.Equal(38_300m, _viewModel.LowestEndingBalance);
        Assert.DoesNotContain(_viewModel.Periods, x => x.IsNegative);
        Assert.False(_viewModel.IsFinalNegative);
    }

    [Fact]
    public async Task Yukle_OnIkiDonemSonrasiVeFaiz_SonDonemdenVeToplamdanGelir()
    {
        _projectionService.Result = Projection(DippingEndings);

        await _viewModel.LoadAsync();

        Assert.Equal(26_300m, _viewModel.FinalEndingBalance);
        Assert.False(_viewModel.IsFinalNegative);
        Assert.Equal(2_140m + 1_610m, _viewModel.TotalInterest);
    }

    [Fact]
    public async Task Yukle_SonDonemEksiyse_OnIkiDonemSonrasiOlumsuzdur()
    {
        _projectionService.Result = Projection([40_000m, 30_000m, 20_000m, 10_000m, 5_000m, 1_000m, -2_000m, -4_000m, -6_000m, -8_000m, -9_000m, -9_500m]);

        await _viewModel.LoadAsync();

        Assert.Equal(-9_500m, _viewModel.FinalEndingBalance);
        Assert.True(_viewModel.IsFinalNegative);
    }

    [Fact]
    public async Task Yukle_FaizYoksa_SifirGosterilir()
    {
        _projectionService.Result = Projection(DippingEndings, cardInterest: 0m, deficitInterest: 0m);

        await _viewModel.LoadAsync();

        Assert.Equal(0m, _viewModel.TotalInterest);
    }

    [Fact]
    public async Task Yukle_YilYalnizIlkKarodaVeYilinIlkDonemindeYazilir()
    {
        _projectionService.Result = Projection(DippingEndings);

        await _viewModel.LoadAsync();

        Assert.Equal([0, 3], _viewModel.Periods.Where(x => x.ShowsYear).Select(x => x.Index));
    }

    [Fact]
    public async Task Yukle_Grafik_ZincirBasiVeOnIkiDonemSonunuSifirEsigiyleCizer()
    {
        _projectionService.Result = Projection(DippingEndings);

        await _viewModel.LoadAsync();

        var trend = Assert.IsType<ChartTrend>(_viewModel.Trend);
        Assert.Equal(13, trend.Series.Points.Count);
        Assert.Equal(new ChartPoint(ChainStart, Opening), trend.Series.Points[0]);
        Assert.Equal(new ChartPoint(new DateOnly(2027, 3, 10), 45_600m), trend.Series.Points[5]);
        Assert.Equal(new ChartPoint(new DateOnly(2027, 10, 10), 26_300m), trend.Series.Points[12]);
        Assert.Equal(0m, trend.Threshold?.Value);
        Assert.Null(trend.Projection);
        Assert.Null(trend.Today);
        Assert.Null(trend.PlanLevel);
    }

    [Fact]
    public async Task Yukle_EksenUclari_ZincirBasiVeSonDonemSonGunudur()
    {
        _projectionService.Result = Projection(DippingEndings);

        await _viewModel.LoadAsync();

        Assert.Equal(ChainStart, _viewModel.ChainStart);
        Assert.Equal(new DateOnly(2027, 10, 9), _viewModel.HorizonLastDay);
    }

    [Fact]
    public async Task Yukle_IkinciKez_KarolarCiftlenmez()
    {
        _projectionService.Result = Projection(DippingEndings);

        await _viewModel.LoadAsync();
        await _viewModel.LoadAsync();

        Assert.Equal(12, _viewModel.Periods.Count);
        Assert.Single(_viewModel.Periods, x => x.IsLowest);
    }

    [Fact]
    public async Task Yukle_ProjeksiyonYoksa_BosDurumdur()
    {
        _projectionService.Result = Projection(DippingEndings);
        await _viewModel.LoadAsync();
        _projectionService.Result = null;

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.Empty(_viewModel.Periods);
        Assert.Null(_viewModel.Trend);
        Assert.Null(_viewModel.LowestEndingBalance);
    }

    [Fact]
    public async Task Yukle_Okunamazsa_HataDurumudur()
    {
        _projectionService.Failure = new InvalidOperationException("Projeksiyon hesaplanamadı.");

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Yukle_Surerken_YukleniyorDurumudur()
    {
        var pending = new TaskCompletionSource<FinancialProjectionResult?>();
        _projectionService.Pending = pending;

        var load = _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Loading, _viewModel.State);
        pending.SetResult(Projection(DippingEndings));
        await load;
        Assert.Equal(ScreenState.Content, _viewModel.State);
    }

    [Fact]
    public async Task FinansalYapiyaGit_BosHalinAksiyonu_FinansalYapiyiAcar()
    {
        await _viewModel.OpenFinancialStructureAsync();

        Assert.Equal(Routes.FinancialStructure, _navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task KaroyaDokunmak_DonemAyrintisiniIlkGunuyleAcar()
    {
        _projectionService.Result = Projection(DippingEndings);
        await _viewModel.LoadAsync();

        await _viewModel.OpenPeriodAsync(_viewModel.Periods[5]);

        Assert.Equal(Routes.PeriodDetail, _navigation.LastNavigatedRoute);
        Assert.Equal(new DateOnly(2027, 3, 10), _navigation.LastParameters![Routes.PeriodStartParameter]);
    }

    // Zincir: her dönemin açılışı bir öncekinin sonu (kural 05); kart faizi ilk dönemde, KMH faizi ilk eksi dönemde.
    private static FinancialProjectionResult Projection(
        IReadOnlyList<decimal> endings, decimal cardInterest = 2_140m, decimal deficitInterest = 1_610m)
    {
        var periods = new List<CashFlowPeriodProjection>();
        var opening = Opening;
        var firstDeficit = endings.ToList().FindIndex(x => x < 0m);
        for (var i = 0; i < endings.Count; i++)
        {
            var start = ChainStart.AddMonths(i);
            periods.Add(new CashFlowPeriodProjection
            {
                Period = new CashFlowPeriod(start, start.AddMonths(1)),
                OpeningBalance = opening,
                EndingBalance = endings[i],
                CardInterestGenerated = i == 0 ? cardInterest : 0m,
                DeficitFinancingInterest = i == firstDeficit ? deficitInterest : 0m
            });
            opening = endings[i];
        }

        return new FinancialProjectionResult(periods, new PeriodObligationPlan([], [], []), []);
    }
}
