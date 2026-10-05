using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;
using Mizan.Presentation.Charts;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Ana sayfa görünüm modelinin (EK-V3, S72) davranışı: dönem sonu tahmin ya da plan, bakiye rotası, harcama
/// temposu, son bakiye, biten dönem, kalan ödemeler, üç durum ve gezinme.
/// </summary>
public sealed class DashboardViewModelTests : IDisposable
{
    private static readonly DateOnly Start = new(2026, 9, 10);
    private static readonly DateOnly End = new(2026, 10, 10);
    private static readonly DateOnly Today = new(2026, 9, 30);

    private readonly FakePeriodProgressService _progressService = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakePaymentReminderService _reminderService = new();
    private readonly FakeDialogService _dialog = new();
    private readonly ProfileService _profileService;
    private readonly DashboardViewModel _viewModel;

    public DashboardViewModelTests()
    {
        var clock = new SabitSaat(Today);
        _profileService = new ProfileService(new FakeProfileRepository(), new FakeProfileStoreSwitch(), clock);
        var reminders = new ReminderCardViewModel(
            _reminderService, new FakePaymentReminderScheduler(), _dialog, clock, _profileService);
        _viewModel = new DashboardViewModel(_progressService, _navigation, reminders, _dialog);
    }

    [Fact]
    public async Task Yukle_BakiyeGirildiyse_DonemSonuTahmindirVePlanaGoreGeridedir()
    {
        _progressService.CurrentProgress = Progress(projected: 41723m, planned: 43900m);

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.True(_viewModel.HasActivePeriod);
        Assert.True(_viewModel.HasObservation);
        Assert.Equal(41723m, _viewModel.EndingBalance);
        Assert.Equal(43900m, _viewModel.PlannedEndingBalance);
        Assert.Equal(-2177m, _viewModel.EndingDeviation);
        Assert.True(_viewModel.IsBehindPlan);
        Assert.False(_viewModel.IsAheadOfPlan);
    }

    [Fact]
    public async Task Yukle_TahminPlaninUstundeyse_PlaninOnundedir()
    {
        _progressService.CurrentProgress = Progress(projected: 44380m, planned: 43900m);

        await _viewModel.LoadAsync();

        Assert.Equal(480m, _viewModel.EndingDeviation);
        Assert.True(_viewModel.IsAheadOfPlan);
        Assert.False(_viewModel.IsBehindPlan);
    }

    [Fact]
    public async Task Yukle_BakiyeGirilmediyse_DonemSonuPlandirFarkYoktur()
    {
        _progressService.CurrentProgress = Progress(projected: null, planned: 43900m);

        await _viewModel.LoadAsync();

        Assert.False(_viewModel.HasObservation);
        Assert.Equal(43900m, _viewModel.EndingBalance);
        Assert.Null(_viewModel.EndingDeviation);
        Assert.False(_viewModel.IsBehindPlan);
        Assert.False(_viewModel.IsAheadOfPlan);
        Assert.Null(_viewModel.LastObservedBalance);
        Assert.Null(_viewModel.LastObservedOn);
    }

    // GS34: rota grafiği çıktı; kaydırılan kartın 1. sayfası gidişatın plan / şu an tablosudur (S87).
    [Fact]
    public async Task Yukle_PlanSuAnTablosu_GidisattanKurulur()
    {
        _progressService.CurrentProgress = Progress(cards:
        [
            new PeriodCardComparison(Guid.NewGuid(), "Akbank Axess", new DateOnly(2026, 10, 5), 24233m, 26747m)
        ]);

        await _viewModel.LoadAsync();

        Assert.Equal([new PeriodCardComparisonRow("Akbank Axess", 24233m, 26747m, IsAbovePlan: true)], _viewModel.Comparison.Cards);
        Assert.True(_viewModel.Comparison.HasRows);
        Assert.Equal(29650m, _viewModel.Comparison.PlannedLivingExpense);
        Assert.Equal(21050m, _viewModel.Comparison.SpentLivingExpense);
    }

    [Fact]
    public async Task Yukle_Tempo_HalkaHarcananOraniVeGecenSureyiTasir()
    {
        _progressService.CurrentProgress = Progress(pace: new SpendingPace(new DateOnly(2026, 9, 27), 0.71m, 0.67m));

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasPace);
        Assert.Equal(0.71m, _viewModel.SpentRatio);
        Assert.Equal(0.67m, _viewModel.PaceElapsedRatio);
        Assert.Equal(4, _viewModel.PaceGapPoints);
        Assert.Equal(new ChartGauge(0.71m, 0.67m), _viewModel.Gauge);
        Assert.Equal(8600m, _viewModel.RemainingVariableExpenseAllowance);
    }

    [Fact]
    public async Task Yukle_HavuzAsildiysa_HalkaDoyarOranKirpilmaz()
    {
        _progressService.CurrentProgress = Progress(pace: new SpendingPace(new DateOnly(2026, 9, 27), 1.12m, 0.5m));

        await _viewModel.LoadAsync();

        Assert.Equal(1.12m, _viewModel.SpentRatio);
        Assert.Equal(new ChartGauge(1m, 0.5m), _viewModel.Gauge);
        Assert.Equal(62, _viewModel.PaceGapPoints);
    }

    [Theory]
    [InlineData(0.535, 4)]
    [InlineData(0.465, -4)]
    [InlineData(0.504, 0)]
    [InlineData(0.5, 0)]
    public async Task Yukle_TempoPuani_TamSayiyaSifirdanUzagaYuvarlanir(double spent, int expected)
    {
        _progressService.CurrentProgress = Progress(pace: new SpendingPace(new DateOnly(2026, 9, 27), (decimal)spent, 0.5m));

        await _viewModel.LoadAsync();

        Assert.Equal(expected, _viewModel.PaceGapPoints);
    }

    [Fact]
    public async Task Yukle_TempoYoksa_HalkaBosIsaretYok()
    {
        _progressService.CurrentProgress = Progress(projected: null, pace: null);

        await _viewModel.LoadAsync();

        Assert.False(_viewModel.HasPace);
        Assert.Null(_viewModel.SpentRatio);
        Assert.Null(_viewModel.PaceElapsedRatio);
        Assert.Null(_viewModel.PaceGapPoints);
        Assert.Equal(new ChartGauge(0m, null), _viewModel.Gauge);
    }

    [Fact]
    public async Task Yukle_Baslik_SonGunDonemBitisininBirGunOncesidir()
    {
        _progressService.CurrentProgress = Progress();

        await _viewModel.LoadAsync();

        Assert.Equal(Start, _viewModel.PeriodStart);
        Assert.Equal(new DateOnly(2026, 10, 9), _viewModel.PeriodLastDay);
        Assert.Equal(20, _viewModel.ElapsedDays);
        Assert.Equal(30, _viewModel.TotalDays);
        Assert.False(_viewModel.IsPeriodEnded);
    }

    [Fact]
    public async Task Yukle_DonemBitmisKapanmamissa_DonemBittiSayilir()
    {
        _progressService.CurrentProgress = Progress(isClosable: true);

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsPeriodEnded);
    }

    [Fact]
    public async Task Yukle_SonBakiye_SonGozlemdenGelir()
    {
        _progressService.CurrentProgress = Progress();

        await _viewModel.LoadAsync();

        Assert.Equal(58940m, _viewModel.LastObservedBalance);
        Assert.Equal(new DateOnly(2026, 9, 27), _viewModel.LastObservedOn);
    }

    [Fact]
    public async Task Yukle_KalanOdemeler_SatirAdVadeVeTutarTasir()
    {
        _progressService.CurrentProgress = Progress(lineCount: 3);

        await _viewModel.LoadAsync();

        Assert.Equal(3, _viewModel.Remaining.Items.Count);
        Assert.Equal("Ödeme 0", _viewModel.Remaining.Items[0].Name);
        Assert.Equal(new DateOnly(2026, 10, 1), _viewModel.Remaining.Items[0].DueDate);
        Assert.Equal(1000m, _viewModel.Remaining.Items[0].Amount);
        Assert.Equal(3, _viewModel.Remaining.Count);
        Assert.Equal(3000m, _viewModel.Remaining.Total);
        Assert.False(_viewModel.Remaining.HasOverflow);
    }

    [Fact]
    public async Task Yukle_UctenFazlaKalanOdemeVarsa_IlkUcuGorunurTasmaVardir()
    {
        _progressService.CurrentProgress = Progress(lineCount: 5);

        await _viewModel.LoadAsync();

        Assert.Equal(3, _viewModel.Remaining.Items.Count);
        Assert.Equal(5, _viewModel.Remaining.Count);
        Assert.Equal(2, _viewModel.Remaining.HiddenCount);
        Assert.True(_viewModel.Remaining.HasOverflow);
    }

    [Fact]
    public async Task TasmayaDokunmak_KalanOdemelerinHepsiniYerindeAcar()
    {
        _progressService.CurrentProgress = Progress(lineCount: 5);
        await _viewModel.LoadAsync();

        _viewModel.Remaining.Expand();

        Assert.Equal(5, _viewModel.Remaining.Items.Count);
        Assert.Equal("Ödeme 4", _viewModel.Remaining.Items[4].Name);
        Assert.Equal(0, _viewModel.Remaining.HiddenCount);
        Assert.False(_viewModel.Remaining.HasOverflow);
        Assert.Null(_navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task Yukle_AcilmisListeYenidenYuklenince_IlkUceDaralir()
    {
        _progressService.CurrentProgress = Progress(lineCount: 5);
        await _viewModel.LoadAsync();
        _viewModel.Remaining.Expand();

        await _viewModel.LoadAsync();

        Assert.Equal(3, _viewModel.Remaining.Items.Count);
        Assert.True(_viewModel.Remaining.HasOverflow);
    }

    // S88-6: kalan ödeme yoksa kart görünmez; önceden "0 ödeme · 0 ₺" yazan boş kart duruyordu.
    [Fact]
    public async Task Yukle_KalanOdemeYoksa_KartGizlenirTasmaYoktur()
    {
        _progressService.CurrentProgress = Progress(lineCount: 0);

        await _viewModel.LoadAsync();

        Assert.False(_viewModel.Remaining.HasItems);
        Assert.Empty(_viewModel.Remaining.Items);
        Assert.Equal(0, _viewModel.Remaining.Count);
        Assert.False(_viewModel.Remaining.HasOverflow);
    }

    [Fact]
    public async Task KalanOdemeyeOdedimDenince_SayfaYerindeYenilenirAcikListeVeKartSayfasiKorunur()
    {
        // Hazırla — liste açıldı, kaydırılan kart 2. sayfada; "Ödedim"den sonra gidişat dört ödemeyle gelir (S88-4)
        _progressService.CurrentProgress = Progress(lineCount: 5);
        await _viewModel.LoadAsync();
        _viewModel.Remaining.Expand();
        _viewModel.HeroPageIndex = 1;
        _progressService.CurrentProgress = Progress(lineCount: 4);

        // Uygula
        await _viewModel.Remaining.MarkAsPaidCommand.ExecuteAsync(_viewModel.Remaining.Items[0]);

        // Doğrula
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(4, _viewModel.Remaining.Count);
        Assert.Equal(4, _viewModel.Remaining.Items.Count);
        Assert.False(_viewModel.Remaining.HasOverflow);
        Assert.Equal(1, _viewModel.HeroPageIndex);
    }

    [Fact]
    public async Task KalanOdemeyeOdedimDenince_YenilemeSurerkenYukleniyorDurumunaDusmez()
    {
        // Hazırla — yenileme okuması bitmeden ekran iskelete dönmez, sayfa başa kaymaz (S88-4)
        _progressService.CurrentProgress = Progress(lineCount: 2);
        await _viewModel.LoadAsync();
        var pending = new TaskCompletionSource<PeriodProgress?>();
        _progressService.Pending = pending;

        // Uygula
        await _viewModel.Remaining.MarkAsPaidCommand.ExecuteAsync(_viewModel.Remaining.Items[0]);

        // Doğrula — eski rakamlar yenisi gelene kadar yerinde
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(2, _viewModel.Remaining.Count);
        pending.SetResult(Progress(lineCount: 1));
        await _viewModel.RefreshCommand.ExecutionTask!;
        Assert.Equal(1, _viewModel.Remaining.Count);
    }

    [Fact]
    public async Task KalanOdemeyeOdedimDenince_YenilemeOkunamazsaHataDurumudur()
    {
        _progressService.CurrentProgress = Progress(lineCount: 2);
        await _viewModel.LoadAsync();
        _progressService.Failure = new InvalidOperationException("Gidişat okunamadı.");

        await _viewModel.Remaining.MarkAsPaidCommand.ExecuteAsync(_viewModel.Remaining.Items[0]);

        Assert.Equal(ScreenState.Error, _viewModel.State);
    }

    [Fact]
    public async Task HatirlaticiKartindaOdedimDenince_KalanOdemelerYenilenir()
    {
        // Hazırla — ertelenmiş ödeme hem kartta hem listede; karttan "Ödedim" denince liste sayfa yeniden
        // açılmadan güncellenmeli (S88-4; önceden AnswersChanged'i dinleyen yoktu)
        _reminderService.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            DueToday = [new PaymentDue("loan-1", "İhtiyaç Kredisi", Today, 7000m)]
        };
        _progressService.CurrentProgress = Progress(lineCount: 2);
        await _viewModel.LoadAsync();
        _progressService.CurrentProgress = Progress(lineCount: 1);

        // Uygula
        await _viewModel.Reminders.MarkAsPaidCommand.ExecuteAsync(null);

        // Doğrula
        Assert.Equal(1, _viewModel.Remaining.Count);
    }

    [Fact]
    public async Task Yukle_IkinciSayfadaykenYuklenirse_IlkSayfayaDoner()
    {
        _progressService.CurrentProgress = Progress();
        _viewModel.HeroPageIndex = 1;

        await _viewModel.LoadAsync();

        Assert.Equal(0, _viewModel.HeroPageIndex);
    }

    [Fact]
    public async Task Yukle_AcikDonemYoksa_BosDurumdurOncekiVeriSilinir()
    {
        _progressService.CurrentProgress = Progress(cards:
        [
            new PeriodCardComparison(Guid.NewGuid(), "Akbank Axess", new DateOnly(2026, 10, 5), 24233m, 26747m)
        ]);
        await _viewModel.LoadAsync();
        _progressService.CurrentProgress = null;

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.False(_viewModel.HasActivePeriod);
        Assert.Null(_viewModel.EndingBalance);
        Assert.Empty(_viewModel.Comparison.Cards);
        Assert.False(_viewModel.Comparison.HasRows);
        Assert.Null(_viewModel.Comparison.PlannedLivingExpense);
        Assert.Empty(_viewModel.Remaining.Items);
    }

    [Fact]
    public async Task Yukle_GidisatOkunamazsa_HataDurumudur()
    {
        _progressService.Failure = new InvalidOperationException("Gidişat okunamadı.");

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Yukle_Surerken_YukleniyorDurumudur()
    {
        var pending = new TaskCompletionSource<PeriodProgress?>();
        _progressService.Pending = pending;

        var load = _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Loading, _viewModel.State);
        pending.SetResult(null);
        await load;
    }

    [Fact]
    public async Task GezinmeKomutlari_DogruRotalariAcar()
    {
        await _viewModel.OpenBalanceEntryAsync();
        Assert.Equal(Routes.BalanceEntry, _navigation.LastNavigatedRoute);

        await _viewModel.ClosePeriodAsync();
        Assert.Equal(Routes.PeriodSettlement, _navigation.LastNavigatedRoute);

        await _viewModel.OpenOnboardingAsync();
        Assert.Equal(Routes.Onboarding, _navigation.LastNavigatedRoute);
    }

    private static PeriodProgress Progress(
        decimal? projected = 41723m,
        decimal planned = 43900m,
        SpendingPace? pace = null,
        bool isClosable = false,
        int lineCount = 2,
        IReadOnlyList<PeriodCardComparison>? cards = null)
    {
        var planId = Guid.NewGuid();
        var observedOn = new DateOnly(2026, 9, 27);
        var observation = projected is null
            ? null
            : new PeriodObservation { PeriodPlanSnapshotId = planId, ObservedOn = observedOn, ObservedBalance = 58940m };
        var path = observation is null
            ? new PeriodBalancePath([new(Start, 60000m)], [new(Start, 60000m), new(new DateOnly(2026, 9, 15), 45000m), new(End, planned)])
            : new PeriodBalancePath([new(Start, 60000m), new(observedOn, 58940m)], [new(observedOn, 58940m), new(End, projected!.Value)]);
        var payments = Enumerable.Range(0, lineCount)
            .Select(i => new PeriodRemainingPayment(Guid.NewGuid(), $"key-{i}", $"Ödeme {i}", new DateOnly(2026, 10, 1).AddDays(i), 1000m))
            .ToList();

        return new PeriodProgress
        {
            PeriodPlanSnapshotId = planId,
            PeriodStart = Start,
            PeriodEnd = End,
            Today = Today,
            ElapsedDays = 20,
            TotalDays = 30,
            RevisionCount = 0,
            PlannedIncome = 50000m,
            PlannedMandatoryPayments = 15000m,
            PlannedEndingBalance = planned,
            PlannedVariableExpenseAllowance = 29650m,
            PlannedDeficitInterest = 0m,
            ObservedLivingSpend = observation is null ? null : 21050m,
            RemainingVariableExpenseAllowance = observation is null ? null : 8600m,
            ProjectedDeficitInterest = observation is null ? null : 0m,
            ProjectedEndingBalance = projected,
            Pace = pace,
            Cards = cards ?? [],
            Observation = observation,
            Observations = observation is null ? [] : [observation],
            Path = path,
            RemainingPayments = payments,
            IsClosable = isClosable,
            SnoozedLineIds = new HashSet<Guid>()
        };
    }

    public void Dispose() => _profileService.Dispose();
}
