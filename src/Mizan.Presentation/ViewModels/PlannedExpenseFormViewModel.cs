using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Planlı büyük harcama ekleme ve düzenleme formu görünüm modelidir (EK-V6e, S65-4).
/// </summary>
public sealed partial class PlannedExpenseFormViewModel : ViewModelBase
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    private const string ExitConfirmTitle = "Kaydetmeden çık";
    private const string ExitConfirmMessage = "Yaptığın değişiklikler kaydedilmeyecek.";
    private const string ExitConfirmLeave = "Çık";
    private const string ExitConfirmStay = "Kal";
    private const string SaveFailedTitle = "Kayıt başarısız";
    private const string MissingNameMessage = "Harcama adı boş bırakılamaz.";
    private const string InvalidAmountMessage = "Tutarı sıfırdan büyük bir sayı olarak gir.";
    private const string PastDateMessage = "Harcama tarihi bugünden önce olamaz.";
    private const string NotFoundMessage = "Düzenlenecek harcama bulunamadı.";

    private readonly IObligationManagementService _obligationService;
    private readonly IPlannedLargeExpenseRepository _repository;
    private readonly IClock _clock;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private Guid? _expenseId;
    private PlannedLargeExpense? _loadedExpense;

    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private DateOnly exactDate;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public PlannedExpenseFormViewModel(
        IObligationManagementService obligationService,
        IPlannedLargeExpenseRepository repository,
        IClock clock,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _obligationService = obligationService ?? throw new ArgumentNullException(nameof(obligationService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        ExactDate = _clock.Today;
    }

    /// <summary>Tarih seçicisinin en erken günü: bugün.</summary>
    public DateOnly MinimumDate => _clock.Today;

    /// <summary>Formda kaydedilmemiş bir değişiklik var mı.</summary>
    public bool HasChanges =>
        IsEditing
            ? Name.Trim() != (_loadedExpense?.Name ?? string.Empty) ||
              AmountInput != (_loadedExpense?.Amount.ToString("N2", TurkishCulture) ?? string.Empty) ||
              ExactDate != (_loadedExpense?.ExactDate ?? _clock.Today)
            : !string.IsNullOrWhiteSpace(Name) ||
              !string.IsNullOrWhiteSpace(AmountInput) ||
              ExactDate != _clock.Today;


    /// <summary>Düzenlenecek harcamayı okur veya yeni harcama için formu açar.</summary>
    [RelayCommand]
    public async Task LoadAsync(Guid? expenseId = null)
    {
        if (expenseId.HasValue) { _expenseId = expenseId; IsEditing = true; }
        if (!_expenseId.HasValue) { ResetForm(); return; }

        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            var expenses = await _repository.GetPlannedLargeExpensesAsync();
            _loadedExpense = expenses.FirstOrDefault(x => x.Id == _expenseId.Value);
            if (_loadedExpense is null)
            {
                await _dialogService.ShowAlertAsync(SaveFailedTitle, NotFoundMessage);
                await _navigationService.NavigateBackAsync();
                return;
            }
            Name = _loadedExpense.Name;
            AmountInput = _loadedExpense.Amount.ToString("N2", TurkishCulture);
            ExactDate = _loadedExpense.ExactDate;
            State = ScreenState.Content;
        }
        catch (Exception)
        {
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ResetForm()
    {
        _loadedExpense = null;
        Name = string.Empty;
        AmountInput = string.Empty;
        ExactDate = _clock.Today;
        State = ScreenState.Content;
    }

    /// <summary>Okuma hatasında harcamayı tekrar yükler.</summary>
    [RelayCommand]
    public Task RetryAsync() => LoadAsync();

    /// <summary>Planlanan büyük harcamayı tek işlemde kaydeder.</summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (IsBusy) { return; }
        if (!TryValidate(out var amount, out var error))
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, error!);
            return;
        }

        var expense = _loadedExpense is not null
            ? _loadedExpense with { Name = Name.Trim(), Amount = amount, ExactDate = ExactDate }
            : new PlannedLargeExpense { Name = Name.Trim(), Amount = amount, ExactDate = ExactDate };

        try
        {
            SetBusy(true);
            await _obligationService.SavePlannedLargeExpenseAsync(expense);
            await _navigationService.NavigateBackAsync();
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool TryValidate(out decimal amount, out string? error)
    {
        amount = 0m;
        if (string.IsNullOrWhiteSpace(Name)) { error = MissingNameMessage; return false; }
        if (!StatementEntryViewModel.TryParseAmount(AmountInput, out amount) || amount <= 0m)
        {
            error = InvalidAmountMessage; return false;
        }
        if (ExactDate < _clock.Today) { error = PastDateMessage; return false; }
        error = null;
        return true;
    }

    /// <summary>Değişiklik varsa onay sorup çıkar, yoksa doğrudan döner.</summary>
    [RelayCommand]
    public async Task CancelAsync()
    {
        if (HasChanges && !await _dialogService.ConfirmAsync(ExitConfirmTitle, ExitConfirmMessage, ExitConfirmLeave, ExitConfirmStay))
        {
            return;
        }

        await _navigationService.NavigateBackAsync();
    }
}
