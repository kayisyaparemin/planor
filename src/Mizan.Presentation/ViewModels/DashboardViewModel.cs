using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Charts;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Ana sayfanın görünüm modeli: açık dönemin sonunda ne kalacağını ve plandan nerede saptığını (S87), yaşam
/// giderinin temposunu, son girilen bakiyeyi ve kalan ödemeleri tek ekranda sunar (EK-V3, S72). Bakiye girişi
/// ayrı sayfadır (V3b); ödeme cevapları hatırlatıcı kartından ve kalan ödemeler listesinden yazılır (S88).
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase
{
    private readonly IPeriodProgressService _progressService;
    private readonly INavigationService _navigationService;

    /// <summary>Vadesi gelen veya ertelenmiş acil ödemeleri yöneten çocuk görünüm modeli.</summary>
    public ReminderCardViewModel Reminders { get; }

    /// <summary>Kaydırılan kartın plan / şu an tablosu ve yaşam giderinin tutarları (S87).</summary>
    public PeriodComparisonViewModel Comparison { get; } = new();

    /// <summary>"Kalan ödemeler" kartı: satırlar, adet, toplam, "+N daha" ve satırdan "Ödedim" (S88).</summary>
    public RemainingPaymentsViewModel Remaining { get; }

    [ObservableProperty] private bool hasActivePeriod;
    [ObservableProperty] private DateOnly? periodStart; [ObservableProperty] private DateOnly? periodLastDay;
    [ObservableProperty] private int elapsedDays; [ObservableProperty] private int totalDays;
    [ObservableProperty] private bool isPeriodEnded;
    [ObservableProperty] private int heroPageIndex;

    [ObservableProperty] private bool hasObservation;
    [ObservableProperty] private decimal? endingBalance; [ObservableProperty] private decimal? plannedEndingBalance;
    [ObservableProperty] private decimal? endingDeviation;
    [ObservableProperty] private bool isBehindPlan; [ObservableProperty] private bool isAheadOfPlan;

    [ObservableProperty] private decimal? remainingVariableExpenseAllowance;
    [ObservableProperty] private bool hasPace;
    [ObservableProperty] private decimal? spentRatio; [ObservableProperty] private decimal? paceElapsedRatio;
    [ObservableProperty] private int? paceGapPoints;
    [ObservableProperty] private ChartGauge? gauge;

    [ObservableProperty] private decimal? lastObservedBalance; [ObservableProperty] private DateOnly? lastObservedOn;

    /// <summary>
    /// Ana sayfa görünüm modelini gidişat okuma portu, gezinme portu, hatırlatıcı kartı ve kalan ödemelerin onayını
    /// soracak diyalog servisiyle başlatır.
    /// </summary>
    public DashboardViewModel(
        IPeriodProgressService progressService, INavigationService navigationService, ReminderCardViewModel reminders,
        IDialogService dialogService)
    {
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        Reminders = reminders ?? throw new ArgumentNullException(nameof(reminders));
        Remaining = new RemainingPaymentsViewModel(reminders, dialogService);
        // Kartta ya da listede verilen her ödeme cevabı gidişatı değiştirir; sayfa yeniden açılmayı beklemez (S88-4).
        Reminders.AnswersChanged += (_, _) => RefreshCommand.Execute(null);
    }

    /// <summary>Açık dönemin gidişatını ve hatırlatıcıyı yükler; kaydırılan kart her yüklemede ilk sayfaya döner.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        SetBusy(true);
        State = ScreenState.Loading;
        HeroPageIndex = 0;
        Remaining.Collapse();
        try
        {
            await RefreshAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>
    /// Gidişatı ekranı yükleniyor hâline düşürmeden yeniden okur: ödeme cevabından sonra iskelet çıkmaz, sayfa başa
    /// kaymaz, kaydırılan kart ve açılmış liste yerinde kalır (S88-4).
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            var progress = await _progressService.GetAsync();
            if (progress is null)
            {
                ShowEmptyState();
                return;
            }

            ApplyPeriod(progress);
            ApplyEnding(progress);
            ApplyPace(progress);
            Comparison.Show(progress);
            Remaining.Show(progress);
            await Reminders.LoadAsync();
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel ERROR] {ex}");
            // Okuma hatası ekranda StateBlock (Hata) olarak görünür; "Tekrar dene" yüklemeyi yeniden çalıştırır.
            State = ScreenState.Error;
        }
    }

    private void ApplyPeriod(PeriodProgress progress)
    {
        HasActivePeriod = true;
        PeriodStart = progress.PeriodStart;
        PeriodLastDay = progress.PeriodEnd.AddDays(-1); // PeriodEnd sonraki dönemin ilk günü (S72-7)
        ElapsedDays = progress.ElapsedDays;
        TotalDays = progress.TotalDays;
        IsPeriodEnded = progress.IsClosable;
        LastObservedBalance = progress.Observation?.ObservedBalance;
        LastObservedOn = progress.Observation?.ObservedOn;
    }

    private void ApplyEnding(PeriodProgress progress)
    {
        HasObservation = progress.Observation is not null;
        PlannedEndingBalance = progress.PlannedEndingBalance;
        EndingBalance = progress.ProjectedEndingBalance ?? progress.PlannedEndingBalance; // S72-1
        EndingDeviation = progress.EndingDeviation;
        IsBehindPlan = EndingDeviation < 0m;
        IsAheadOfPlan = EndingDeviation > 0m;
    }

    private void ApplyPace(PeriodProgress progress)
    {
        var pace = progress.Pace;
        RemainingVariableExpenseAllowance = progress.RemainingVariableExpenseAllowance;
        HasPace = pace is not null;
        SpentRatio = pace?.SpentRatio;
        PaceElapsedRatio = pace?.ElapsedRatio;
        PaceGapPoints = pace is null ? null : (int)Math.Round(pace.GapPoints, MidpointRounding.AwayFromZero);
        // Halka 1'de doyar; aşım oranı kırpılmadan SpentRatio'da kalır (S70-5).
        Gauge = pace is null ? new ChartGauge(0m, null) : new ChartGauge(Math.Clamp(pace.SpentRatio, 0m, 1m), pace.ElapsedRatio);
    }

    private void ShowEmptyState()
    {
        Remaining.Clear();
        HasActivePeriod = HasObservation = HasPace = IsPeriodEnded = false;
        EndingBalance = EndingDeviation = LastObservedBalance = null;
        Comparison.Clear();
        Gauge = null;
        State = ScreenState.Empty;
    }

    /// <summary>"Bakiye gir" sayfasını açar (V3b).</summary>
    [RelayCommand] public Task OpenBalanceEntryAsync() => _navigationService.NavigateToAsync(Routes.BalanceEntry);
    /// <summary>Biten dönemin kapanışını açar (V11).</summary>
    [RelayCommand] public Task ClosePeriodAsync() => _navigationService.NavigateToAsync(Routes.PeriodSettlement);
    /// <summary>Açık dönem yokken kurulum sihirbazını açar.</summary>
    [RelayCommand] public Task OpenOnboardingAsync() => _navigationService.NavigateToAsync(Routes.Onboarding);
}
