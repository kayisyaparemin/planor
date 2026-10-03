using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Simülatörün deneme formu (EK-V10, S76-3, 7, 8). Bir denemeyi türünün alanlarıyla ekler ya da düzenler ve
/// çalışma listesine yazar; gerçek kayıtlara dokunmaz. Tür "Ekle" seçicisinde seçilir, düzenlemede değişmez.
/// Ad zorunludur çünkü deneme uygulanınca kaydın adı olur; tarih en erken bugündür çünkü geçmiş kaydın işidir.
/// </summary>
public sealed partial class SimulationConditionViewModel : ViewModelBase
{
    private const string SaveFailedTitle = "Deneme kaydedilemedi";
    private const string UnexpectedErrorMessage = "Deneme şu an kaydedilemedi. Tekrar dene.";
    private const string NotFoundTitle = "Deneme bulunamadı";
    private const string NotFoundMessage = "Deneme silinmiş olabilir.";
    private const string ExitConfirmTitle = "Kaydetmeden çık";
    private const string ExitConfirmMessage = "Yaptığın değişiklikler kaydedilmeyecek.";

    private readonly ISimulationWorkflowService _simulationService;
    private readonly IClock _clock;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IPlanReader _planReader;

    private ScenarioOption _option = SimulationScenarioCatalog.CashPayment;
    private Guid? _conditionId;
    private SimulationRequest? _loaded;

    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private SimulationScenarioType scenarioType;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private DateOnly date;
    [ObservableProperty] private Guid? cardId;
    [ObservableProperty] private string cardName = string.Empty;
    [ObservableProperty] private string paymentCountInput = string.Empty;
    [ObservableProperty] private CreditCardPaymentType selectedCardPaymentMode = CreditCardPaymentType.FullStatement;
    [ObservableProperty] private bool selectedCardPaymentScope;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public SimulationConditionViewModel(
        ISimulationWorkflowService simulationService, IClock clock,
        INavigationService navigationService, IDialogService dialogService, IPlanReader planReader)
    {
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _planReader = planReader ?? throw new ArgumentNullException(nameof(planReader));
        Date = _clock.Today;
    }

    /// <summary>Tarih seçicisinin en erken günü: bugün (S76-3).</summary>
    public DateOnly MinimumDate => _clock.Today;
    /// <summary>Deneme kartla ilişkili mi.</summary>
    public bool IsCard => _option.Key is "card" or "card-payment-mode";
    /// <summary>Deneme kartla harcama mı.</summary>
    public bool IsCardSpending => _option.Key == "card";
    /// <summary>Deneme düzenli ödeme mi.</summary>
    public bool IsRecurring => _option.Key == "recurring";
    /// <summary>Deneme kart ödeme şekli mi.</summary>
    public bool IsCardPaymentMode => _option.Key == "card-payment-mode";
    /// <summary>Tutar alanı gerekiyor mu.</summary>
    public bool NeedsAmount => _option.Key != "card-payment-mode";
    /// <summary>Taksit veya ödeme adedi gerekiyor mu.</summary>
    public bool NeedsPaymentCount => _option.Key is "card" or "recurring";
    /// <summary>Kart ödeme şekli seçenekleri.</summary>
    public IReadOnlyList<CreditCardPaymentType> CardPaymentModes { get; } = [CreditCardPaymentType.FullStatement, CreditCardPaymentType.Minimum];
    /// <summary>Kart ödeme kapsamı seçenekleri.</summary>
    public IReadOnlyList<bool> CardPaymentScopes { get; } = [false, true];

    /// <summary>Formda kaydedilmemiş bir değişiklik var mı.</summary>
    public bool HasChanges => _loaded is { } l
        ? Name.Trim() != l.Name || AmountInput != (NeedsAmount ? StatementEntryViewModel.FormatAmount(l.Amount) : string.Empty) ||
          Date != l.StartDate || PaymentCountInput != (l.PaymentCount > 1 ? l.PaymentCount.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty) ||
          SelectedCardPaymentMode != (l.CardPaymentType ?? CreditCardPaymentType.FullStatement) || SelectedCardPaymentScope != l.AppliesToAllStatements
        : !string.IsNullOrWhiteSpace(Name) || (NeedsAmount && !string.IsNullOrWhiteSpace(AmountInput)) || Date != _clock.Today ||
          !string.IsNullOrWhiteSpace(PaymentCountInput) || (IsCardPaymentMode && (SelectedCardPaymentMode != CreditCardPaymentType.FullStatement || SelectedCardPaymentScope));

    /// <summary>Formun neyi açacağını gezinmeden alır.</summary>
    public void Prepare(string? scenarioOptionKey, Guid? conditionId, Guid? cardId = null)
    {
        _conditionId = conditionId;
        IsEditing = conditionId.HasValue;
        CardId = cardId;
        _option = SimulationScenarioCatalog.Options.FirstOrDefault(x => x.Key == scenarioOptionKey)
                  ?? SimulationScenarioCatalog.CashPayment;
        ScenarioType = _option.DefaultType;
    }

    /// <summary>Düzenlenecek denemeyi çalışma listesinden okur ya da yeni deneme için formu açar.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_conditionId is not { } id)
        {
            await ResolveCardNameAsync();
            State = ScreenState.Content;
            return;
        }

        SetBusy(true);
        State = ScreenState.Loading;
        try { await LoadConditionAsync(id); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[SimulationConditionViewModel ERROR] {ex}"); State = ScreenState.Error; }
        finally { SetBusy(false); }
    }

    private async Task LoadConditionAsync(Guid id)
    {
        var list = await _simulationService.GetWorkingListAsync();
        _loaded = list.FirstOrDefault(x => x.Request.ScenarioId == id)?.Request;
        if (_loaded is null)
        {
            await _dialogService.ShowAlertAsync(NotFoundTitle, NotFoundMessage);
            await _navigationService.NavigateBackAsync();
            return;
        }

        PopulateLoaded(_loaded);
        await ResolveCardNameAsync();
        State = ScreenState.Content;
    }

    private void PopulateLoaded(SimulationRequest req)
    {
        _option = SimulationScenarioCatalog.For(req.Type);
        ScenarioType = req.Type;
        Name = req.Name;
        AmountInput = NeedsAmount ? StatementEntryViewModel.FormatAmount(req.Amount) : string.Empty;
        Date = req.StartDate;
        CardId = req.CreditCardId;
        PaymentCountInput = req.PaymentCount > 1 ? req.PaymentCount.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
        SelectedCardPaymentMode = req.CardPaymentType ?? CreditCardPaymentType.FullStatement;
        SelectedCardPaymentScope = req.AppliesToAllStatements;
    }

    private async Task ResolveCardNameAsync()
    {
        if (CardId is { } id && (await _planReader.GetPlanAsync()).CreditCards.FirstOrDefault(x => x.Id == id) is { } card)
        {
            CardName = $"{card.Bank} {card.Name}".Trim();
        }
    }

    /// <summary>Denemeyi doğrular ve çalışma listesine yazar; yeni deneme açık olarak sona eklenir.</summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (IsBusy) { return; }
        if (!TryBuildRequest(out var request, out var error))
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, error);
            return;
        }

        try
        {
            SetBusy(true);
            var updated = SimulationConditionDraftBuilder.ApplyCondition(await _simulationService.GetWorkingListAsync(), request);
            await _simulationService.SaveWorkingListAsync(updated);
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulationConditionViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally { SetBusy(false); }
    }

    /// <summary>Değişiklik varsa onay sorup çıkar, yoksa doğrudan döner.</summary>
    [RelayCommand]
    public async Task CancelAsync()
    {
        if (HasChanges && !await _dialogService.ConfirmAsync(ExitConfirmTitle, ExitConfirmMessage, "Çık", "Kal"))
        {
            return;
        }

        await _navigationService.NavigateBackAsync();
    }

    private bool TryBuildRequest(out SimulationRequest request, out string error) =>
        SimulationConditionDraftBuilder.TryBuild(
            _option, _loaded, Name, AmountInput, Date, CardId, PaymentCountInput,
            SelectedCardPaymentMode, SelectedCardPaymentScope, _clock.Today,
            out request, out error);
}
