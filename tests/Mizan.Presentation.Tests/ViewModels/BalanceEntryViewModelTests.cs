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
/// "Bakiye gir" görünüm modelinin (EK-V3 sayfa 2, S73) davranışı: gün dönemle sınırlı ve varsayılan bugün,
/// kaydetmeden canlı önizleme (son istek kazanır), eksi bakiye, kaydetme, kapanışı bekleyen dönem ve üç durum.
/// </summary>
public sealed class BalanceEntryViewModelTests
{
    private static readonly DateOnly Start = new(2026, 9, 10);
    private static readonly DateOnly End = new(2026, 10, 10);
    private static readonly DateOnly Today = new(2026, 9, 30);
    private static readonly DateOnly LastDay = new(2026, 9, 27);
    private static readonly Guid PlanId = Guid.NewGuid();
    private const decimal Planned = 43900m;
    // Taslak gidişat: girilen bakiyeden dönem sonuna 17.920 ₺ iner (62.300 → 44.380, plana göre +480).
    private const decimal RemainingOutflow = 17920m;

    private readonly FakePeriodProgressService _progress = new();
    private readonly FakePeriodWorkflowService _workflow = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialogs = new();
    private readonly BalanceEntryViewModel _viewModel;

    public BalanceEntryViewModelTests()
    {
        _progress.CurrentProgress = Progress();
        _progress.Preview = Draft;
        _viewModel = new BalanceEntryViewModel(_progress, _workflow, _navigation, _dialogs);
    }

    [Fact]
    public async Task Yukle_AcikDonem_GunBugundurVeDonemBasiIleBugunArasindanSecilir()
    {
        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(Today, _viewModel.ObservedOn);
        Assert.Equal(Start, _viewModel.EarliestDay);
        Assert.Equal(Today, _viewModel.LatestDay);
        Assert.True(_viewModel.IsToday);
    }

    [Fact]
    public async Task Yukle_DonemIlkGunu_EnErkenVeEnGecGunAynidir()
    {
        _progress.CurrentProgress = Progress(today: Start);

        await _viewModel.LoadAsync();

        Assert.Equal(Start, _viewModel.EarliestDay);
        Assert.Equal(Start, _viewModel.LatestDay);
        Assert.Equal(Start, _viewModel.ObservedOn);
    }

    [Fact]
    public async Task Yukle_OncekiGirisVarsa_SonGirisVeOncekiTahminGorunur()
    {
        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasLastObservation);
        Assert.Equal(58940m, _viewModel.LastObservedBalance);
        Assert.Equal(LastDay, _viewModel.LastObservedOn);
        Assert.Equal(41723m, _viewModel.PreviousEndingBalance);
    }

    [Fact]
    public async Task Yukle_HicGirisYoksa_SonGirisVeOncekiTahminYoktur()
    {
        _progress.CurrentProgress = Progress(hasObservation: false);

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.False(_viewModel.HasLastObservation);
        Assert.Null(_viewModel.LastObservedBalance);
        Assert.Null(_viewModel.PreviousEndingBalance);
    }

    [Fact]
    public async Task Yukle_TutarGirilmeden_OnizlemeYokturVeIstenmez()
    {
        await _viewModel.LoadAsync();

        Assert.False(_viewModel.Preview.IsShown);
        Assert.Empty(_progress.PreviewRequests);
    }

    [Fact]
    public async Task Yukle_DonemBittiKapanisBekliyor_OnceKapanisIstenir()
    {
        _progress.CurrentProgress = Progress(isClosable: true);

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
    }

    [Fact]
    public async Task Yukle_AcikDonemYoksa_AnaSayfayaDoner()
    {
        _progress.CurrentProgress = null;

        await _viewModel.LoadAsync();

        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Yukle_OkumaHatasi_HataDurumudur()
    {
        _progress.Failure = new InvalidOperationException("okunamadı");

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Error, _viewModel.State);
    }

    [Fact]
    public async Task Tutar_GecerliYazilinca_BugunIcinOnizlenirVePlaninOnundedir()
    {
        await _viewModel.LoadAsync();

        await TypeAsync("62.300");

        Assert.Equal([(62300m, Today)], _progress.PreviewRequests);
        Assert.True(_viewModel.Preview.IsShown);
        Assert.Equal(44380m, _viewModel.Preview.EndingBalance);
        Assert.Equal(480m, _viewModel.Preview.Deviation);
        Assert.True(_viewModel.Preview.IsAheadOfPlan);
        Assert.False(_viewModel.Preview.IsBehindPlan);
    }

    [Fact]
    public async Task Tutar_PlaninAltindaKalirsa_GeridedirVeFarkEksidir()
    {
        await _viewModel.LoadAsync();

        await TypeAsync("50.000");

        Assert.Equal(32080m, _viewModel.Preview.EndingBalance);
        Assert.Equal(-11820m, _viewModel.Preview.Deviation);
        Assert.True(_viewModel.Preview.IsBehindPlan);
        Assert.False(_viewModel.Preview.IsAheadOfPlan);
    }

    [Theory]
    [InlineData("-12.500", -12500)]
    [InlineData("-12500", -12500)]
    [InlineData("-1.250,50", -1250.50)]
    [InlineData("0", 0)]
    public async Task Tutar_EksiYaDaSifir_GecerliBakiyeOlarakOnizlenir(string input, double expected)
    {
        await _viewModel.LoadAsync();

        await TypeAsync(input);

        Assert.Equal([((decimal)expected, Today)], _progress.PreviewRequests);
        Assert.True(_viewModel.Preview.IsShown);
    }

    [Fact]
    public async Task Tutar_Gecersizlesince_OnizlemeGizlenirVeIstenmez()
    {
        await _viewModel.LoadAsync();
        await TypeAsync("62.300");

        await TypeAsync("abc");

        Assert.False(_viewModel.Preview.IsShown);
        Assert.Null(_viewModel.Preview.EndingBalance);
        Assert.Null(_viewModel.Preview.Trend);
        Assert.Single(_progress.PreviewRequests);
    }

    [Fact]
    public async Task Tutar_DonemBittiyse_Onizlenmez()
    {
        _progress.CurrentProgress = Progress(isClosable: true);
        await _viewModel.LoadAsync();

        await TypeAsync("62.300");

        Assert.Empty(_progress.PreviewRequests);
        Assert.False(_viewModel.Preview.IsShown);
    }

    [Fact]
    public async Task Gun_Degisince_YeniGunleOnizlenirVeBugunDegildir()
    {
        await _viewModel.LoadAsync();
        await TypeAsync("62.300");
        var earlier = new DateOnly(2026, 9, 20);

        _viewModel.ObservedOn = earlier;
        await Settled();

        Assert.Equal((62300m, earlier), _progress.PreviewRequests[^1]);
        Assert.False(_viewModel.IsToday);
    }

    [Fact]
    public async Task Onizleme_IstekleriBirbiriniGecerse_SonIstekKazanir()
    {
        await _viewModel.LoadAsync();
        var first = new TaskCompletionSource<PeriodProgress>();
        var second = new TaskCompletionSource<PeriodProgress>();
        _progress.PendingPreviews.Enqueue(first);
        _progress.PendingPreviews.Enqueue(second);
        _viewModel.AmountInput = "60.000";
        var firstRun = Settled();
        _viewModel.AmountInput = "62.300";
        var secondRun = Settled();

        second.SetResult(Draft(62300m, Today));
        await secondRun;
        first.SetResult(Draft(60000m, Today));
        await firstRun;

        Assert.Equal(44380m, _viewModel.Preview.EndingBalance);
    }

    [Fact]
    public async Task Onizleme_HataVerirse_KartGizlenir()
    {
        await _viewModel.LoadAsync();
        await TypeAsync("62.300");
        _progress.PreviewFailure = new InvalidOperationException("Gözlem günü dönemin dışında olamaz.");

        _viewModel.ObservedOn = new DateOnly(2026, 9, 20);
        await Settled();

        Assert.False(_viewModel.Preview.IsShown);
        Assert.Null(_viewModel.Preview.Trend);
    }

    [Fact]
    public async Task Onizleme_Grafik_TaslakGirisRotadaNoktadir()
    {
        await _viewModel.LoadAsync();

        await TypeAsync("62.300");

        var trend = Assert.IsType<ChartTrend>(_viewModel.Preview.Trend);
        Assert.Equal(new ChartPoint(Today, 62300m), trend.Series.Points[^1]);
        Assert.Equal(new ChartPoint(End, 44380m), trend.Projection!.Points[^1]);
        Assert.Equal([new ChartPoint(LastDay, 58940m), new ChartPoint(Today, 62300m)], trend.Markers!.Points);
        Assert.Equal(Today, trend.Today);
        Assert.Equal(Planned, trend.PlanLevel!.Value);
    }

    [Fact]
    public async Task Kaydet_GecerliTutar_SecilenGuneYazarVeGeriDoner()
    {
        await _viewModel.LoadAsync();
        await TypeAsync("62.300");
        _viewModel.ObservedOn = new DateOnly(2026, 9, 20);
        await Settled();

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(62300m, _workflow.LastObservedBalance);
        Assert.Equal(new DateOnly(2026, 9, 20), _workflow.LastObservedOn);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Kaydet_GecersizTutar_UyarirVeYazmaz()
    {
        await _viewModel.LoadAsync();
        await TypeAsync("abc");

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.NotNull(_dialogs.LastAlertMessage);
        Assert.Null(_workflow.LastObservedBalance);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Kaydet_ServisReddederse_MesajiGosterirVeTutarKalir()
    {
        const string reason = "Dönem sona erdi ama kapanış yapılmadı; bakiye girmeden önce kapanışı tamamlayın.";
        await _viewModel.LoadAsync();
        await TypeAsync("62.300");
        _workflow.ObserveFailure = new InvalidOperationException(reason);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(reason, _dialogs.LastAlertMessage);
        Assert.False(_navigation.NavigateBackCalled);
        Assert.Equal("62.300", _viewModel.AmountInput);
    }

    [Fact]
    public async Task DonemiKapat_KapanisSayfasiniAcar()
    {
        _progress.CurrentProgress = Progress(isClosable: true);
        await _viewModel.LoadAsync();

        await _viewModel.ClosePeriodCommand.ExecuteAsync(null);

        Assert.Equal(Routes.PeriodSettlement, _navigation.LastNavigatedRoute);
    }

    private async Task TypeAsync(string input)
    {
        _viewModel.AmountInput = input;
        await Settled();
    }

    // Tutar ya da gün değişince çalışan önizleme komutunun son çalışması.
    private Task Settled() => _viewModel.RefreshPreviewCommand.ExecutionTask ?? Task.CompletedTask;

    private static readonly PeriodObservation Previous =
        new() { PeriodPlanSnapshotId = PlanId, ObservedOn = LastDay, ObservedBalance = 58940m };

    private static PeriodProgress Draft(decimal balance, DateOnly day)
    {
        var projected = balance - RemainingOutflow;
        var draft = new PeriodObservation { PeriodPlanSnapshotId = PlanId, ObservedOn = day, ObservedBalance = balance };
        return Progress() with
        {
            ProjectedEndingBalance = projected,
            Observation = day >= LastDay ? draft : Previous,
            Observations = new[] { Previous, draft }.OrderBy(x => x.ObservedOn).ToList(),
            Path = new PeriodBalancePath([new(Start, 60000m), new(day, balance)], [new(day, balance), new(End, projected)])
        };
    }

    private static PeriodProgress Progress(bool hasObservation = true, bool isClosable = false, DateOnly? today = null) => new()
    {
        PeriodPlanSnapshotId = PlanId,
        PeriodStart = Start,
        PeriodEnd = End,
        Today = today ?? Today,
        ElapsedDays = 20,
        TotalDays = 30,
        RevisionCount = 0,
        PlannedIncome = 50000m,
        PlannedMandatoryPayments = 15000m,
        PlannedEndingBalance = Planned,
        PlannedVariableExpenseAllowance = 29650m,
        PlannedDeficitInterest = 0m,
        ObservedLivingSpend = hasObservation ? 21050m : null,
        RemainingVariableExpenseAllowance = hasObservation ? 8600m : null,
        ProjectedDeficitInterest = hasObservation ? 0m : null,
        ProjectedEndingBalance = hasObservation ? 41723m : null,
        Pace = null,
        Cards = [],
        Observation = hasObservation ? Previous : null,
        Observations = hasObservation ? [Previous] : [],
        Path = new PeriodBalancePath([new(Start, 60000m)], [new(Start, 60000m), new(End, Planned)]),
        RemainingLines = [],
        RemainingPlannedTotal = 0m,
        IsClosable = isClosable,
        SnoozedLineIds = new HashSet<Guid>()
    };
}
