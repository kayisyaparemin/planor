using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Taksitli ödeme planı ekleme ve düzenleme formu görünüm modelidir (EK-V6e, S65-3).
/// </summary>
public sealed partial class PaymentPlanFormViewModel : ViewModelBase
{
    private const string ExitConfirmTitle = "Kaydetmeden çık";
    private const string ExitConfirmMessage = "Yaptığın değişiklikler kaydedilmeyecek.";
    private const string ExitConfirmLeave = "Çık";
    private const string ExitConfirmStay = "Kal";
    private const string SaveFailedTitle = "Kayıt başarısız";
    private const string MissingNameMessage = "Ödeme planı adı boş bırakılamaz.";
    private const string MissingInstallmentMessage = "Ödeme planında en az bir taksit bulunmalıdır.";
    private const string NotFoundMessage = "Düzenlenecek ödeme planı bulunamadı.";

    private readonly IObligationManagementService _obligationService;
    private readonly ITemporaryPaymentPlanRepository _repository;
    private readonly IClock _clock;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private Guid? _planId;
    private TemporaryPaymentPlan? _loadedPlan;

    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string name = string.Empty;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public PaymentPlanFormViewModel(
        IObligationManagementService obligationService,
        ITemporaryPaymentPlanRepository repository,
        IClock clock,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _obligationService = obligationService ?? throw new ArgumentNullException(nameof(obligationService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Installments = new PaymentPlanInstallmentsViewModel(_dialogService, _clock);
    }

    /// <summary>Taksitler listesi ve girişi yöneten çocuk ViewModel.</summary>
    public PaymentPlanInstallmentsViewModel Installments { get; }

    /// <summary>Formda kaydedilmemiş bir değişiklik var mı.</summary>
    public bool HasChanges =>
        IsEditing
            ? Name.Trim() != (_loadedPlan?.Name ?? string.Empty) || Installments.HasChanges
            : !string.IsNullOrWhiteSpace(Name) || Installments.HasChanges || Installments.TotalCount > 0;


    /// <summary>Düzenlenecek planı okur veya yeni plan için formu açar.</summary>
    [RelayCommand]
    public async Task LoadAsync(Guid? planId = null)
    {
        if (planId.HasValue) { _planId = planId; IsEditing = true; }
        if (!_planId.HasValue) { ResetForm(); return; }

        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            var plans = await _repository.GetPaymentPlansAsync();
            _loadedPlan = plans.FirstOrDefault(x => x.Id == _planId.Value);
            if (_loadedPlan is null)
            {
                await _dialogService.ShowAlertAsync(SaveFailedTitle, NotFoundMessage);
                await _navigationService.NavigateBackAsync();
                return;
            }
            Name = _loadedPlan.Name;
            Installments.Load(_loadedPlan.Installments);
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
        _loadedPlan = null;
        Name = string.Empty;
        Installments.Load([]);
        State = ScreenState.Content;
    }

    /// <summary>Okuma hatasında planı tekrar yükler.</summary>
    [RelayCommand]
    public Task RetryAsync() => LoadAsync();

    /// <summary>Ödeme planını taksitleriyle birlikte tek işlemde kaydeder.</summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (IsBusy) { return; }
        if (!TryValidate(out var error))
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, error!);
            return;
        }

        var planId = _loadedPlan?.Id ?? Guid.NewGuid();
        var installments = Installments.ToInstallments(planId);
        var plan = _loadedPlan is not null
            ? _loadedPlan with { Name = Name.Trim(), Installments = installments }
            : new TemporaryPaymentPlan { Id = planId, Name = Name.Trim(), Installments = installments };

        try
        {
            SetBusy(true);
            await _obligationService.SavePaymentPlanAsync(plan);
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

    private bool TryValidate(out string? error)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            error = MissingNameMessage;
            return false;
        }
        if (!Installments.TryCommitPendingEntry(out error))
        {
            return false;
        }
        if (Installments.TotalCount == 0)
        {
            error = MissingInstallmentMessage;
            return false;
        }
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
