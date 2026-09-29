using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Düzenli gelir formunun görünüm modelidir: geliri ekler ya da düzenler, geliri ve yeni gelirin ilk
/// tutarını tek işlemde kaydedip listeye döner, kaydedilmemiş değişiklikte çıkmadan önce onay sorar.
/// Tanım <see cref="IncomeDefinitionViewModel"/> çocuğundadır (EK-V6d, S67).
/// </summary>
public sealed partial class IncomeFormViewModel : ViewModelBase
{
    private const string SaveFailedTitle = "Gelir kaydedilemedi";
    private const string NotFoundTitle = "Gelir bulunamadı";
    private const string NotFoundMessage = "Gelir silinmiş olabilir.";
    private const string UnexpectedErrorMessage = "Gelir kaydedilirken bir sorun oluştu. Tekrar dene.";
    private const string DiscardTitle = "Kaydetmeden çık";
    private const string DiscardMessage = "Yaptığın değişiklikler kaydedilmeyecek.";
    private const string DiscardAccept = "Çık";
    private const string DiscardCancel = "Kal";

    private readonly IRecurringIncomeRepository _incomeRepository;
    private readonly IIncomePlanService _incomeService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IClock _clock;
    private Guid? _requestedIncomeId;
    private RecurringIncome? _income;

    [ObservableProperty] private bool isEditing;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public IncomeFormViewModel(
        IRecurringIncomeRepository incomeRepository, IIncomePlanService incomeService,
        INavigationService navigationService, IDialogService dialogService, IClock clock)
    {
        _incomeRepository = incomeRepository ?? throw new ArgumentNullException(nameof(incomeRepository));
        _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Gelirin tanımı: ad, ödeme günü; yeni gelirde aylık net tutar.</summary>
    public IncomeDefinitionViewModel Fields { get; } = new();

    /// <summary>Form açıldığından beri bir alan değişti mi.</summary>
    public bool HasChanges => Fields.HasChanges;

    /// <summary>Kimlik verilmezse boş yeni gelir formu, verilirse o gelirin düzenlemesi açılır.</summary>
    [RelayCommand]
    public async Task LoadAsync(Guid? incomeId)
    {
        _requestedIncomeId = incomeId;
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            _income = incomeId is { } id
                ? (await _incomeRepository.GetRecurringIncomesAsync()).FirstOrDefault(x => x.Id == id)
                : null;
            if (incomeId is not null && _income is null)
            {
                await _dialogService.ShowAlertAsync(NotFoundTitle, NotFoundMessage);
                await _navigationService.NavigateBackAsync();
                return;
            }

            IsEditing = _income is not null;
            Fields.Fill(_income);
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IncomeFormViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Okuma hatasından sonra aynı geliri (ya da yeni gelir formunu) yeniden yükler.</summary>
    [RelayCommand]
    private Task RetryAsync() => LoadAsync(_requestedIncomeId);

    /// <summary>
    /// Formu doğrular, geliri kaydeder ve bir önceki sayfaya döner. Yeni gelirin ilk tutarı bugünden
    /// yürürlüğe girer ve gelirle aynı işlemde yazılır; düzenleme tutar geçmişine dokunmaz (S67-2, S67-3).
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (Fields.TryBuild(_income, out var income) is { } error)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, error);
            return;
        }

        IReadOnlyList<IncomeAmountHistory> newAmounts = _income is null
            ? [Fields.BuildFirstAmount(income!.Id, _clock.Today)]
            : [];
        SetBusy(true);
        try
        {
            await _incomeService.SaveRecurringIncomeAsync(income!, newAmounts);
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IncomeFormViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Kaydedilmemiş değişiklik varsa onay alıp bir önceki sayfaya döner.</summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        if (HasChanges && !await _dialogService.ConfirmAsync(DiscardTitle, DiscardMessage, DiscardAccept, DiscardCancel))
        {
            return;
        }

        await _navigationService.NavigateBackAsync();
    }
}
