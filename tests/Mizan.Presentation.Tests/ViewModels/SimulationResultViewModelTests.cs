using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Charts;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Simülatörün sonuç kartı (EK-V10 S1, S76-1, 6, 9): denemeyle en düşük dönem sonu, aynı dönemde şu anki gidişata
/// göre fark, iki çizgili grafik, uç tarihler ve sonucun kendi üç durumu. Canlı hesapta son istek kazanır.
/// </summary>
public sealed class SimulationResultViewModelTests
{
    private static readonly DateOnly ChainStart = new(2026, 10, 10);
    private const decimal BaselineOpening = 41_723m;
    private const decimal ScenarioOpening = 11_723m;

    private static readonly decimal[] BaselineEndings =
        [44_120m, 47_900m, 30_000m, 41_050m, 45_600m, 38_000m, 40_000m, 42_000m, 44_000m, 46_000m, 48_000m, 50_000m];

    // En düşük dönem baza göre farklı: 6. dönem (indeks 5); baz o dönemde 38.000'de.
    private static readonly decimal[] ScenarioEndings =
        [44_120m, 47_900m, 30_000m, 41_050m, 45_600m, -10_000m, -8_000m, -6_000m, -4_000m, -2_000m, 0m, 2_000m];

    private readonly FakeSimulationResultService _service = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly SimulationResultViewModel _viewModel;

    public SimulationResultViewModelTests()
    {
        _viewModel = new SimulationResultViewModel(_service, _navigation);
    }

    [Fact]
    public void Kurucu_BagimlilikEksikse_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => new SimulationResultViewModel(null!, _navigation));
        Assert.Throws<ArgumentNullException>(() => new SimulationResultViewModel(_service, null!));
    }

    [Fact]
    public async Task Yenile_DenemeYoksa_EnDusukDonemSonuSuAnkiGidisatinKendisidirFarkYoktur()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(30_000m, _viewModel.LowestEndingBalance);
        Assert.Equal(new DateOnly(2026, 12, 10), _viewModel.LowestPeriodStart);
        Assert.Null(_viewModel.DifferenceAtLowest);
        Assert.False(_viewModel.HasDifference);
    }

    [Fact]
    public async Task Yenile_DenemeVarsa_HeroDenemeliSerininEnDusuguOlur()
    {
        _service.Outcome = WithScenario();

        await _viewModel.RefreshAsync([]);

        Assert.Equal(-10_000m, _viewModel.LowestEndingBalance);
        Assert.Equal(new DateOnly(2027, 3, 10), _viewModel.LowestPeriodStart);
    }

    // Denemeyle en düşük dönem 6.; fark o dönemde ölçülür: −10.000 − 38.000. En düşük ↔ en düşük (−10.000 − 30.000) değil.
    [Fact]
    public async Task Yenile_DenemeVarsa_FarkAyniDonemdeOlculur()
    {
        _service.Outcome = WithScenario();

        await _viewModel.RefreshAsync([]);

        Assert.Equal(-48_000m, _viewModel.DifferenceAtLowest);
        Assert.True(_viewModel.HasDifference);
        Assert.True(_viewModel.IsDifferenceNegative);
        Assert.False(_viewModel.IsDifferencePositive);
    }

    [Fact]
    public async Task Yenile_DenemeGelirGetiriyorsa_FarkArtiOlur()
    {
        var better = BaselineEndings.Select(x => x + 5_000m).ToArray();
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), Periods(BaselineOpening + 5_000m, better));

        await _viewModel.RefreshAsync([]);

        Assert.Equal(5_000m, _viewModel.DifferenceAtLowest);
        Assert.True(_viewModel.IsDifferencePositive);
        Assert.False(_viewModel.IsDifferenceNegative);
    }

    [Fact]
    public async Task Yenile_DenemeHicEtkilemiyorsa_FarkSifirdirAmaGorunur()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), Periods(BaselineOpening, BaselineEndings));

        await _viewModel.RefreshAsync([]);

        Assert.Equal(0m, _viewModel.DifferenceAtLowest);
        Assert.True(_viewModel.HasDifference);
        Assert.False(_viewModel.IsDifferenceNegative);
        Assert.False(_viewModel.IsDifferencePositive);
    }

    [Fact]
    public async Task Yenile_EnDusukBirdenFazlaDonemdeyse_IlkiSecilir()
    {
        var endings = new[] { 5_000m, 3_000m, 7_000m, 3_000m, 9_000m, 10_000m, 11_000m, 12_000m, 13_000m, 14_000m, 15_000m, 16_000m };
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, endings), null);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(new DateOnly(2026, 11, 10), _viewModel.LowestPeriodStart);
    }

    [Fact]
    public async Task Yenile_DenemeVarsa_GrafikteDenemeDuzSeriSuAnkiGidisatKarsilastirmaSerisidir()
    {
        _service.Outcome = WithScenario();

        await _viewModel.RefreshAsync([]);

        var trend = Assert.IsType<ChartTrend>(_viewModel.Trend);
        Assert.Equal("actual", trend.Series.Key);
        Assert.Equal(13, trend.Series.Points.Count);
        Assert.Equal(ScenarioOpening, trend.Series.Points[0].Value);
        Assert.Equal(-10_000m, trend.Series.Points[6].Value);
        Assert.NotNull(trend.Comparison);
        Assert.Equal("planned", trend.Comparison.Key);
        Assert.Equal(BaselineOpening, trend.Comparison.Points[0].Value);
        Assert.Equal(50_000m, trend.Comparison.Points[^1].Value);
        Assert.Equal(0m, trend.Threshold?.Value);
    }

    [Fact]
    public async Task Yenile_DenemeYoksa_GrafikTekSeridirVe12DonemininAynisidir()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);

        await _viewModel.RefreshAsync([]);

        var trend = Assert.IsType<ChartTrend>(_viewModel.Trend);
        Assert.Equal("actual", trend.Series.Key);
        Assert.Null(trend.Comparison);
        Assert.Equal(new ChartPoint(ChainStart, BaselineOpening), trend.Series.Points[0]);
        Assert.Equal(new ChartPoint(new DateOnly(2027, 10, 10), 50_000m), trend.Series.Points[^1]);
        Assert.Equal(0m, trend.Threshold?.Value);
    }

    [Fact]
    public async Task Yenile_UcTarihleri_ZincirBasiVeOnIkinciDonemSonGunudur()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(ChainStart, _viewModel.ChainStart);
        Assert.Equal(new DateOnly(2027, 10, 9), _viewModel.HorizonLastDay);
    }

    [Fact]
    public async Task Yenile_ListeyiServiseAynenVerir()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);
        var list = new[] { Draft("Telefon", true), Draft("Tatil", false) };

        await _viewModel.RefreshAsync(list);

        Assert.Same(list, Assert.Single(_service.Calls));
    }

    [Fact]
    public async Task Yenile_AcikDonemYoksa_BosDurumVeSonucTemizlenir()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);
        await _viewModel.RefreshAsync([]);
        _service.Outcome = null;

        await _viewModel.RefreshAsync([]);

        Assert.True(_viewModel.IsEmpty);
        Assert.Null(_viewModel.LowestEndingBalance);
        Assert.Null(_viewModel.DifferenceAtLowest);
        Assert.Null(_viewModel.Trend);
        Assert.Null(_viewModel.ChainStart);
    }

    [Fact]
    public async Task Yenile_HesapDusunce_HataDurumunaGecerTekrarDeneSonListeyleCalisir()
    {
        var list = new[] { Draft("Telefon", true) };
        _service.Failure = new InvalidOperationException("kart bulunamadı");

        await _viewModel.RefreshAsync(list);
        _service.Failure = null;
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);
        await _viewModel.RetryAsync();

        Assert.Equal(2, _service.Calls.Count);
        Assert.All(_service.Calls, call => Assert.Same(list, call));
        Assert.True(_viewModel.IsContent);
    }

    [Fact]
    public async Task Yenile_HesapDusunce_HataDurumuGorunur()
    {
        _service.Failure = new InvalidOperationException("kart bulunamadı");

        await _viewModel.RefreshAsync([]);

        Assert.True(_viewModel.IsError);
    }

    [Fact]
    public async Task Yenile_IlkHesapSurerken_YukleniyorGosterir()
    {
        var gate = _service.HoldNext();

        var refresh = _viewModel.RefreshAsync([]);

        Assert.True(_viewModel.IsLoading);
        gate.SetResult(new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null));
        await refresh;
        Assert.True(_viewModel.IsContent);
    }

    // Canlı yeniden hesapta iskelet yok: eski sonuç yenisi gelene kadar yerinde kalır (EK-V10 § 5).
    [Fact]
    public async Task Yenile_SonucVarkenYenidenHesap_EskiSonucYerindeKalir()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);
        await _viewModel.RefreshAsync([]);
        var gate = _service.HoldNext();

        var refresh = _viewModel.RefreshAsync([Draft("Telefon", true)]);

        Assert.True(_viewModel.IsContent);
        Assert.Equal(30_000m, _viewModel.LowestEndingBalance);
        gate.SetResult(WithScenario());
        await refresh;
        Assert.Equal(-10_000m, _viewModel.LowestEndingBalance);
    }

    [Fact]
    public async Task Yenile_UstUsteBinenIstekler_SonIstekKazanir()
    {
        var first = _service.HoldNext();
        var second = _service.HoldNext();

        var slow = _viewModel.RefreshAsync([Draft("Eski", true)]);
        var fast = _viewModel.RefreshAsync([Draft("Yeni", true)]);
        second.SetResult(WithScenario());
        await fast;
        first.SetResult(new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null));
        await slow;

        Assert.Equal(-10_000m, _viewModel.LowestEndingBalance);
        Assert.True(_viewModel.HasDifference);
    }

    [Fact]
    public async Task FinansalYapiyaGit_BosHalinAksiyonu_FinansalYapiyiAcar()
    {
        await _viewModel.OpenFinancialStructureAsync();

        Assert.Equal(Routes.FinancialStructure, _navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task Yenile_DenemeYoksa_OnIkiDonemSonuSuAnkiGidisatinOnIkinciDonemidirFarkYoktur()
    {
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, BaselineEndings), null);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(50_000m, _viewModel.FinalEndingBalance);
        Assert.Equal(50_000m, _viewModel.BaselineFinalEndingBalance);
        Assert.Null(_viewModel.FinalEndingDifference);
        Assert.False(_viewModel.IsFinalNegative);
    }

    [Fact]
    public async Task Yenile_DenemeVarsa_OnIkiDonemSonuVeFarkiSenaryoVeBazGoreBelirlenir()
    {
        _service.Outcome = WithScenario();

        await _viewModel.RefreshAsync([]);

        Assert.Equal(2_000m, _viewModel.FinalEndingBalance);
        Assert.Equal(50_000m, _viewModel.BaselineFinalEndingBalance);
        Assert.Equal(-48_000m, _viewModel.FinalEndingDifference);
        Assert.Equal("Negative", _viewModel.FinalEndingDifferenceSemantic);
        Assert.False(_viewModel.IsFinalNegative);
    }

    [Fact]
    public async Task Yenile_OnIkinciDonemEksiyse_IsFinalNegativeDogrudur()
    {
        var negativeEndings = BaselineEndings.ToArray();
        negativeEndings[^1] = -5_000m;
        _service.Outcome = new SimulationOutcome(Periods(BaselineOpening, negativeEndings), null);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(-5_000m, _viewModel.FinalEndingBalance);
        Assert.True(_viewModel.IsFinalNegative);
    }

    [Fact]
    public async Task Yenile_Faiz_DenemeYoksaSuAnkiGidisatinFaizidirFarkYoktur()
    {
        _service.Outcome = new SimulationOutcome(
            Periods(BaselineOpening, BaselineEndings, cardInterest: 20m, deficitInterest: 30m),
            null);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(600m, _viewModel.TotalInterest);
        Assert.Null(_viewModel.InterestDifference);
        Assert.False(_viewModel.HasInterestDifference);
    }

    [Fact]
    public async Task Yenile_Faiz_DenemeVarsaToplamVeFarkHesaplanir()
    {
        var baseline = Periods(BaselineOpening, BaselineEndings, cardInterest: 10m, deficitInterest: 30m);
        var scenario = Periods(ScenarioOpening, ScenarioEndings, cardInterest: 30m, deficitInterest: 50m);
        _service.Outcome = new SimulationOutcome(baseline, scenario);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(960m, _viewModel.TotalInterest);
        Assert.Equal(480m, _viewModel.InterestDifference);
        Assert.True(_viewModel.HasInterestDifference);
        Assert.True(_viewModel.IsInterestCostIncreased);
        Assert.False(_viewModel.IsInterestCostDecreased);
    }

    [Fact]
    public async Task Yenile_Faiz_TasarrufSagliyorsa_FarkEksiOlurVeTasarrufIsaretlenir()
    {
        var baseline = Periods(BaselineOpening, BaselineEndings, cardInterest: 20m, deficitInterest: 30m);
        var scenario = Periods(ScenarioOpening, ScenarioEndings, cardInterest: 10m, deficitInterest: 10m);
        _service.Outcome = new SimulationOutcome(baseline, scenario);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(240m, _viewModel.TotalInterest);
        Assert.Equal(-360m, _viewModel.InterestDifference);
        Assert.True(_viewModel.HasInterestDifference);
        Assert.False(_viewModel.IsInterestCostIncreased);
        Assert.True(_viewModel.IsInterestCostDecreased);
    }

    [Fact]
    public async Task Yenile_Izgara_OnIkiDonemKarolariniSiraylaTasar()
    {
        _service.Outcome = WithScenario();

        await _viewModel.RefreshAsync([]);

        Assert.Equal(12, _viewModel.Periods.Count);
        Assert.True(_viewModel.Periods[0].ShowsYear);
        Assert.False(_viewModel.Periods[1].ShowsYear);
        Assert.Equal(0, _viewModel.Periods[0].Index);
        Assert.Equal(ChainStart, _viewModel.Periods[0].PeriodStart);
        Assert.Equal(ScenarioEndings[0], _viewModel.Periods[0].EndingBalance);
        Assert.True(_viewModel.Periods[5].IsLowest);
        Assert.True(_viewModel.Periods[5].IsNegative);
        Assert.False(_viewModel.Periods[0].IsLowest);
    }

    [Fact]
    public async Task Yenile_AcikDonemYoksa_OnIkiDonemVeFaizVeIzgaraTemizlenir()
    {
        _service.Outcome = WithScenario();
        await _viewModel.RefreshAsync([]);
        _service.Outcome = null;

        await _viewModel.RefreshAsync([]);

        Assert.Null(_viewModel.FinalEndingBalance);
        Assert.Null(_viewModel.BaselineFinalEndingBalance);
        Assert.Null(_viewModel.TotalInterest);
        Assert.Null(_viewModel.InterestDifference);
        Assert.Empty(_viewModel.Periods);
        Assert.Null(_viewModel.LoanInterestSaving);
        Assert.Null(_viewModel.FinancingCost);
        Assert.False(_viewModel.HasLoanInterestSaving);
        Assert.False(_viewModel.HasFinancingCost);
    }

    [Fact]
    public async Task Yenile_KrediyeErkenOdemeVarsa_KrediFaizKazanciGorunur()
    {
        _service.Outcome = new SimulationOutcome(
            Periods(BaselineOpening, BaselineEndings),
            Periods(ScenarioOpening, ScenarioEndings),
            LoanInterestSaving: 6_200m,
            FinancingCost: null);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(6_200m, _viewModel.LoanInterestSaving);
        Assert.True(_viewModel.HasLoanInterestSaving);
        Assert.False(_viewModel.HasFinancingCost);
        Assert.Null(_viewModel.FinancingCost);
    }

    [Fact]
    public async Task Yenile_KrediCekmeVarsa_KredininMaliyetiGorunur()
    {
        _service.Outcome = new SimulationOutcome(
            Periods(BaselineOpening, BaselineEndings),
            Periods(ScenarioOpening, ScenarioEndings),
            LoanInterestSaving: null,
            FinancingCost: 4_800m);

        await _viewModel.RefreshAsync([]);

        Assert.Equal(4_800m, _viewModel.FinancingCost);
        Assert.True(_viewModel.HasFinancingCost);
        Assert.False(_viewModel.HasLoanInterestSaving);
        Assert.Null(_viewModel.LoanInterestSaving);
    }

    private static SimulationOutcome WithScenario() =>
        new(Periods(BaselineOpening, BaselineEndings), Periods(ScenarioOpening, ScenarioEndings));

    private static SimulationDraftCondition Draft(string name, bool isEnabled) =>
        new(new SimulationRequest(SimulationScenarioType.CashPurchase, name, 1_000m, ChainStart), isEnabled);

    // Zincir: her dönemin açılışı bir öncekinin sonu (kural 05); ilk dönem açılışı zincirin başıdır.
    private static List<CashFlowPeriodProjection> Periods(
        decimal opening,
        IReadOnlyList<decimal> endings,
        decimal cardInterest = 0m,
        decimal deficitInterest = 0m)
    {
        var periods = new List<CashFlowPeriodProjection>();
        for (var i = 0; i < endings.Count; i++)
        {
            var start = ChainStart.AddMonths(i);
            periods.Add(new CashFlowPeriodProjection
            {
                Period = new CashFlowPeriod(start, start.AddMonths(1)),
                OpeningBalance = opening,
                EndingBalance = endings[i],
                CardInterestGenerated = cardInterest,
                DeficitFinancingInterest = deficitInterest
            });
            opening = endings[i];
        }

        return periods;
    }
}
