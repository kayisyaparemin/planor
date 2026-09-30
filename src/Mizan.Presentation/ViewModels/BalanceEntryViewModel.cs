using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// "Bakiye gir" sayfasının görünüm modeli: bankadaki bakiyeyi dönemin bir gününe (varsayılan bugün) yazar ve
/// kaydetmeden önce bu girişle dönem sonunun ne olacağını gösterir (EK-V3 sayfa 2, S73). Eskiden bakiye her
/// zaman bugüne giriliyordu ve etkisi ancak kaydettikten sonra görülüyordu.
/// </summary>
public sealed partial class BalanceEntryViewModel : ViewModelBase
{
    private const string SaveFailedTitle = "Bakiye kaydedilemedi";
    private const string InvalidAmountMessage = "Bakiye bir tutar olmalı; eksi bakiye için başına - yaz.";
    private const string UnexpectedErrorMessage = "Bakiye kaydedilirken bir sorun oluştu. Tekrar dene.";

    private readonly IPeriodProgressService _progressService;
    private readonly IPeriodWorkflowService _workflowService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private DateOnly? _today;
    private int _previewRequest;

    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private DateOnly? observedOn;
    [ObservableProperty] private DateOnly? earliestDay; [ObservableProperty] private DateOnly? latestDay;
    [ObservableProperty] private bool isToday;

    [ObservableProperty] private bool hasLastObservation;
    [ObservableProperty] private decimal? lastObservedBalance; [ObservableProperty] private DateOnly? lastObservedOn;
    [ObservableProperty] private decimal? previousEndingBalance;

    /// <summary>Kaydetmeden önizleme kartı: bu girişle dönem sonu, plana göre fark ve rota.</summary>
    public BalancePreviewViewModel Preview { get; } = new();

    /// <summary>Görünüm modelini gidişat okuma, bakiye yazma, gezinme ve diyalog portlarıyla başlatır.</summary>
    public BalanceEntryViewModel(
        IPeriodProgressService progressService, IPeriodWorkflowService workflowService,
        INavigationService navigationService, IDialogService dialogService)
    {
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>
    /// Açık dönemi, son girişi ve seçilebilecek günleri yükler; gün varsayılan olarak bugündür. Kapanışı bekleyen
    /// dönem bakiye almaz, sayfa önce kapanışı ister (S68-4); açık dönem yoksa yazılacak yer olmadığı için geri döner.
    /// </summary>
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
                await _navigationService.NavigateBackAsync();
                return;
            }

            State = progress.IsClosable ? ScreenState.Empty : ScreenState.Content;
            ApplyPeriod(progress);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BalanceEntryViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ApplyPeriod(PeriodProgress progress)
    {
        _today = progress.Today;
        EarliestDay = progress.PeriodStart;
        LatestDay = progress.Today;
        HasLastObservation = progress.Observation is not null;
        LastObservedBalance = progress.Observation?.ObservedBalance;
        LastObservedOn = progress.Observation?.ObservedOn;
        PreviousEndingBalance = progress.ProjectedEndingBalance; // bakiye girilmediyse null: önceki rakam plandır (S73-6)
        ObservedOn = progress.Today;
    }

    partial void OnAmountInputChanged(string value) => RefreshPreviewCommand.Execute(null);

    partial void OnObservedOnChanged(DateOnly? value)
    {
        IsToday = value is not null && value == _today;
        RefreshPreviewCommand.Execute(null);
    }

    /// <summary>
    /// Girilen tutar ve gün için kaydetmeden önizleme ister; üst üste binen isteklerde son istek kazanır, eski
    /// cevap atılır. Tutar geçersizse ya da önizleme hata verirse önizleme gizlenir, uyarı çıkmaz (S73-2).
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task RefreshPreviewAsync()
    {
        var request = ++_previewRequest;
        if (State != ScreenState.Content || ObservedOn is not { } day || !TryParseBalance(AmountInput, out var balance))
        {
            Preview.Clear();
            return;
        }

        try
        {
            var preview = await _progressService.PreviewAsync(balance, day);
            if (request == _previewRequest)
            {
                Preview.Show(preview);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BalanceEntryViewModel PREVIEW] {ex.Message}");
            if (request == _previewRequest)
            {
                Preview.Clear();
            }
        }
    }

    /// <summary>
    /// Tutarı seçilen güne yazar ve ana sayfaya döner. Geçersiz tutarda uyarır; servis reddederse (ör. dönem gece
    /// yarısı bitti) servisin mesajını gösterir ve girilen tutar yerinde kalır (S73-4).
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy || ObservedOn is not { } day)
        {
            return;
        }

        if (!TryParseBalance(AmountInput, out var balance))
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, InvalidAmountMessage);
            return;
        }

        SetBusy(true);
        try
        {
            await _workflowService.ObserveCurrentBalanceAsync(balance, day);
            await _navigationService.NavigateBackAsync();
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BalanceEntryViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Biten dönemin kapanışını açar (V11).</summary>
    [RelayCommand]
    private Task ClosePeriodAsync() => _navigationService.NavigateToAsync(Routes.PeriodSettlement);

    // Banka bakiyesi KMH'de eksiye düşebilir (S73-3). Ortak okuyucu noktalı binlik ayraçlı eksi tutarı
    // ("-12.500") okuyamıyor; eksi işareti burada ayrılır, kalan tutar ortak kurallarla okunur.
    private static bool TryParseBalance(string? text, out decimal balance)
    {
        var trimmed = (text ?? string.Empty).Trim();
        var isNegative = trimmed.StartsWith('-');
        if (!StatementEntryViewModel.TryParseAmount(isNegative ? trimmed[1..] : trimmed, out var magnitude) || magnitude < 0m)
        {
            balance = 0m;
            return false;
        }

        balance = isNegative ? -magnitude : magnitude;
        return true;
    }
}
