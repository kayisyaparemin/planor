using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Tek seferlik gelir formunun görünüm modelidir: arızi geliri ekler ya da düzenler,
/// kaydeder ve listeye döner, kaydedilmemiş değişiklikte çıkmadan önce onay sorar (EK-V6d, S67-6, S67-7).
/// </summary>
public sealed partial class AdHocIncomeFormViewModel : ViewModelBase
{
    private const string SaveFailedTitle = "Gelir kaydedilemedi";
    private const string NotFoundTitle = "Gelir bulunamadı";
    private const string NotFoundMessage = "Gelir silinmiş olabilir.";
    private const string UnexpectedErrorMessage = "Gelir kaydedilirken bir sorun oluştu. Tekrar dene.";
    private const string DiscardTitle = "Kaydetmeden çık";
    private const string DiscardMessage = "Yaptığın değişiklikler kaydedilmeyecek.";
    private const string DiscardAccept = "Çık";
    private const string DiscardCancel = "Kal";
    private const string DescriptionEmptyMessage = "Lütfen bir açıklama gir.";
    private const string AmountInvalidMessage = "Lütfen geçerli bir tutar gir.";
    private const string DateInvalidMessage = "Tarih bugünden önce olamaz.";

    private readonly IAdHocIncomeRepository _repository;
    private readonly IIncomePlanService _incomeService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IClock _clock;

    private Guid? _requestedIncomeId;
    private AdHocIncome? _income;
    private (string Description, string Amount, DateOnly Date) _loaded;

    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private DateOnly exactDate;

    /// <summary>Seçilebilecek en erken tarih: bugünden önce olamaz (S67-6).</summary>
    public DateOnly MinimumDate => _clock.Today;

    /// <summary>Form açıldığından beri bir alan değişti mi.</summary>
    public bool HasChanges => Current() != _loaded;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public AdHocIncomeFormViewModel(
        IAdHocIncomeRepository repository,
        IIncomePlanService incomeService,
        INavigationService navigationService,
        IDialogService dialogService,
        IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        exactDate = _clock.Today;
        _loaded = Current();
    }

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
                ? (await _repository.GetAdHocIncomesAsync()).FirstOrDefault(x => x.Id == id)
                : null;
            if (incomeId is not null && _income is null)
            {
                await _dialogService.ShowAlertAsync(NotFoundTitle, NotFoundMessage);
                await _navigationService.NavigateBackAsync();
                return;
            }

            IsEditing = _income is not null;
            Description = _income?.Description ?? string.Empty;
            AmountInput = _income is null ? string.Empty : StatementEntryViewModel.FormatAmount(_income.Amount);
            ExactDate = _income?.ExactDate ?? _clock.Today;
            _loaded = Current();
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdHocIncomeFormViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Okuma hatasından sonra formu yeniden yükler.</summary>
    [RelayCommand]
    private Task RetryAsync() => LoadAsync(_requestedIncomeId);

    /// <summary>Formu doğrular, geliri kaydeder ve bir önceki sayfaya döner.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (Validate(out var trimmedDescription, out var amount) is { } error)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, error);
            return;
        }

        var target = (_income ?? new AdHocIncome()) with
        {
            Description = trimmedDescription,
            Amount = amount,
            ExactDate = ExactDate
        };

        SetBusy(true);
        try
        {
            await _incomeService.SaveAdHocIncomeAsync(target);
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdHocIncomeFormViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private string? Validate(out string trimmedDescription, out decimal amount)
    {
        trimmedDescription = Description?.Trim() ?? string.Empty;
        amount = 0m;
        if (string.IsNullOrWhiteSpace(trimmedDescription))
        {
            return DescriptionEmptyMessage;
        }

        if (!StatementEntryViewModel.TryParseAmount(AmountInput, out amount) || amount <= 0m)
        {
            return AmountInvalidMessage;
        }

        if (ExactDate < _clock.Today)
        {
            return DateInvalidMessage;
        }

        return null;
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

    private (string, string, DateOnly) Current() => (Description, AmountInput, ExactDate);
}
