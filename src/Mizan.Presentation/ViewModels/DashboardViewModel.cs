using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Ana sayfa ekranının açık dönem gidişatını, bütçe oranını ve kalan ödemelerini sunan görünüm modelidir.
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    // EK-V3 kesme kararı: ana sayfada en fazla üç kalan ödeme satırı; kalanı "+N ödeme daha" satırına düşer.
    private const int VisibleRemainingLimit = 3;

    private readonly IPeriodProgressService _progressService;
    private readonly IPeriodWorkflowService _workflowService;
    private readonly INavigationService _navigationService;
    private readonly ReminderCardViewModel _reminders;

    /// <summary>Vadesi gelen veya ertelenmiş acil ödemeleri yöneten çocuk görünüm modeli.</summary>
    public ReminderCardViewModel Reminders => _reminders;

    /// <summary>Bu dönemden kalan planlı ödeme satırları listesi.</summary>
    public ObservableCollection<DashboardRemainingItem> RemainingLines { get; } = [];

    [ObservableProperty] private DateOnly? periodStart; [ObservableProperty] private DateOnly? periodEnd;
    [ObservableProperty] private int elapsedDays; [ObservableProperty] private int totalDays;
    [ObservableProperty] private double periodElapsedRatio;
    [ObservableProperty] private decimal? projectedEndingBalance; [ObservableProperty] private decimal? plannedEndingBalance;
    [ObservableProperty] private double budgetRatio;
    [ObservableProperty] private bool isPeriodClosable;
    [ObservableProperty] private string currentBalanceInput = string.Empty;
    [ObservableProperty] private decimal? lastObservedBalance; [ObservableProperty] private DateOnly? lastObservedOn;
    [ObservableProperty] private bool hasObservation; [ObservableProperty] private bool isSavingObservation;
    [ObservableProperty] private bool hasActiveAlert;
    [ObservableProperty] private decimal remainingPlannedTotal;
    [ObservableProperty] private bool hasRemainingLines; [ObservableProperty] private int remainingCount;
    [ObservableProperty] private int overflowCount; [ObservableProperty] private bool hasOverflow;
    [ObservableProperty] private bool hasActivePeriod;
    [ObservableProperty] private decimal? remainingVariableExpenseAllowance; [ObservableProperty] private decimal? observedLivingSpend;

    /// <summary>Ana sayfa görünüm modelini gerekli dört dar portla başlatır (Kural M3).</summary>
    public DashboardViewModel(
        IPeriodProgressService progressService, IPeriodWorkflowService workflowService,
        INavigationService navigationService, ReminderCardViewModel reminders)
    {
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _reminders = reminders ?? throw new ArgumentNullException(nameof(reminders));
    }

    /// <summary>Açık dönemin gidişat verilerini, bildirimlerini ve hatırlatıcı durumlarını yükler.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            var progress = await _progressService.GetAsync();
            if (progress is null)
            {
                ShowEmptyState();
                return;
            }

            ApplyProgress(progress);
            await CheckSettlementAlertAsync();
            await _reminders.LoadAsync();
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel ERROR] {ex}");
            // Okuma hatası ekranda StateBlock (Hata) olarak görünür; "Tekrar dene" bu komutu yeniden çalıştırır.
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Kullanıcının girdiği anlık nakit bakiye gözlemini kaydeder.</summary>
    [RelayCommand]
    public async Task SaveObservationAsync()
    {
        var text = (CurrentBalanceInput ?? string.Empty).Trim();
        if (!decimal.TryParse(text, NumberStyles.Number, Tr, out var amount))
        {
            return;
        }

        IsSavingObservation = true;
        try
        {
            await _workflowService.ObserveCurrentBalanceAsync(amount);
            CurrentBalanceInput = string.Empty;
            await LoadAsync();
        }
        finally
        {
            IsSavingObservation = false;
        }
    }

    private void ApplyProgress(PeriodProgress progress)
    {
        PeriodStart = progress.PeriodStart;
        PeriodEnd = progress.PeriodEnd;
        ElapsedDays = progress.ElapsedDays;
        TotalDays = progress.TotalDays;
        PeriodElapsedRatio = progress.TotalDays > 0 ? (double)progress.ElapsedDays / progress.TotalDays : 0.0;
        ProjectedEndingBalance = progress.ProjectedEndingBalance;
        PlannedEndingBalance = progress.PlannedEndingBalance;
        BudgetRatio = CalculateBudgetRatio(progress);
        IsPeriodClosable = progress.IsClosable;
        RemainingVariableExpenseAllowance = progress.RemainingVariableExpenseAllowance;
        ObservedLivingSpend = progress.ObservedLivingSpend;
        HasObservation = progress.Observation is not null;
        LastObservedBalance = progress.Observation?.ObservedBalance;
        LastObservedOn = progress.Observation?.ObservedOn;
        HasActivePeriod = true;
        ApplyRemainingLines(progress);
    }

    private static double CalculateBudgetRatio(PeriodProgress progress)
    {
        if (progress.PlannedVariableExpenseAllowance <= 0m || !progress.RemainingVariableExpenseAllowance.HasValue)
        {
            return 1.0;
        }
        return Math.Clamp((double)(progress.RemainingVariableExpenseAllowance.Value / progress.PlannedVariableExpenseAllowance), 0.0, 1.0);
    }

    private void ApplyRemainingLines(PeriodProgress progress)
    {
        RemainingLines.Clear();
        foreach (var line in progress.RemainingLines.Take(VisibleRemainingLimit))
        {
            RemainingLines.Add(new DashboardRemainingItem
            {
                DueDate = line.PlannedDate, Name = line.Name, Amount = line.PlannedAmount ?? 0m,
                Detail = line.Detail, IsSnoozed = progress.IsSnoozed(line.Id), IsOverdue = line.PlannedDate < progress.Today
            });
        }
        RemainingCount = progress.RemainingLines.Count;
        HasRemainingLines = RemainingCount > 0;
        OverflowCount = Math.Max(0, RemainingCount - VisibleRemainingLimit);
        HasOverflow = OverflowCount > 0;
        RemainingPlannedTotal = progress.RemainingPlannedTotal;
    }

    private async Task CheckSettlementAlertAsync()
    {
        var availability = await _workflowService.GetSettlementAvailabilityAsync();
        HasActiveAlert = availability.IsDue;
    }

    private void ShowEmptyState()
    {
        RemainingLines.Clear();
        HasRemainingLines = HasOverflow = HasObservation = HasActiveAlert = HasActivePeriod = false;
        ProjectedEndingBalance = PlannedEndingBalance = null;
        State = ScreenState.Empty;
    }

    /// <summary>Dönem kapanış ve mutabakat sihirbazına yönlendirir.</summary>
    [RelayCommand] public Task ClosePeriodAsync() => _navigationService.NavigateToAsync(Routes.PeriodSettlement);
    /// <summary>12 Dönem projeksiyon sayfasına yönlendirir.</summary>
    [RelayCommand] public Task OpenProjectionAsync() => _navigationService.NavigateToAsync(Routes.Projection);
    /// <summary>Geçmiş dönemler sayfasına yönlendirir.</summary>
    [RelayCommand] public Task OpenHistoryAsync() => _navigationService.NavigateToAsync(Routes.History);
    /// <summary>Finansal yapı sayfasına yönlendirir.</summary>
    [RelayCommand] public Task OpenFinancialStructureAsync() => _navigationService.NavigateToAsync(Routes.FinancialStructure);
    /// <summary>What-If simülatörü sayfasına yönlendirir.</summary>
    [RelayCommand] public Task OpenSimulationAsync() => _navigationService.NavigateToAsync(Routes.Simulation);
    /// <summary>Ayarlar sayfasına yönlendirir.</summary>
    [RelayCommand] public Task OpenSettingsAsync() => _navigationService.NavigateToAsync(Routes.Settings);
    /// <summary>Dönem ayrıntısı sayfasına yönlendirir.</summary>
    [RelayCommand] public Task OpenRemainingDetailAsync() => _navigationService.NavigateToAsync(Routes.PeriodDetail);
    /// <summary>İlk kurulum sihirbazı sayfasına yönlendirir.</summary>
    [RelayCommand] public Task OpenOnboardingAsync() => _navigationService.NavigateToAsync(Routes.Onboarding);
}
