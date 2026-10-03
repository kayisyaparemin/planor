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
    [ObservableProperty] private Guid? loanId;
    [ObservableProperty] private string loanName = string.Empty;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(NeedsAmount))]
    private LoanPrepaymentMode selectedPrepaymentMode = LoanPrepaymentMode.FullClosure;
    [ObservableProperty] private string totalRepaymentAmountInput = string.Empty;
    [ObservableProperty] private DateOnly firstPaymentDate;

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
        Date = FirstPaymentDate = _clock.Today;
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
    /// <summary>Deneme kredi çekme mi.</summary>
    public bool IsFinancing => _option.Key == "financing";
    /// <summary>Deneme taksitli nakit borç mu.</summary>
    public bool IsCashDebt => _option.Key == "cash-debt";
    /// <summary>Deneme krediye erken ödeme mi.</summary>
    public bool IsLoanPrepayment => _option.Key == "loan-prepayment";
    /// <summary>Kredi adı alanı görünür mü.</summary>
    public bool IsLoan => _option.Key == "loan-prepayment";
    /// <summary>İlk ödeme tarihi alanı gerekiyor mu.</summary>
    public bool NeedsFirstPaymentDate => _option.Key == "financing";
    /// <summary>Toplam geri ödeme alanı gerekiyor mu.</summary>
    public bool NeedsTotalRepayment => _option.Key == "financing";
    /// <summary>Taksit veya ödeme adedi gerekiyor mu.</summary>
    public bool NeedsPaymentCount => _option.Key is "card" or "recurring" or "financing" or "cash-debt";
    /// <summary>Tutar alanı gerekiyor mu.</summary>
    public bool NeedsAmount => _option.Key switch
    {
        "card-payment-mode" => false,
        "loan-prepayment" => SelectedPrepaymentMode != LoanPrepaymentMode.FullClosure,
        _ => true
    };

    /// <summary>Kart ödeme şekli seçenekleri.</summary>
    public IReadOnlyList<CreditCardPaymentType> CardPaymentModes { get; } = [CreditCardPaymentType.FullStatement, CreditCardPaymentType.Minimum];
    /// <summary>Kart ödeme kapsamı seçenekleri.</summary>
    public IReadOnlyList<bool> CardPaymentScopes { get; } = [false, true];
    /// <summary>Erken ödeme şekli seçenekleri.</summary>
    public IReadOnlyList<LoanPrepaymentMode> PrepaymentModes { get; } =
        [LoanPrepaymentMode.FullClosure, LoanPrepaymentMode.ReduceTerm, LoanPrepaymentMode.ReduceInstallment];

    /// <summary>Formda kaydedilmemiş bir değişiklik var mı.</summary>
    public bool HasChanges => SimulationConditionDraftBuilder.HasChanges(this, _loaded, _clock.Today);

    internal ScenarioOption Option => _option;
    internal void SetLoadedOption(ScenarioOption option) => _option = option;

    /// <summary>Formun neyi açacağını gezinmeden alır.</summary>
    public void Prepare(string? scenarioOptionKey, Guid? conditionId, Guid? cardId = null, Guid? loanId = null)
    {
        _conditionId = conditionId;
        IsEditing = conditionId.HasValue;
        CardId = cardId;
        LoanId = loanId;
        _option = SimulationScenarioCatalog.Options.FirstOrDefault(x => x.Key == scenarioOptionKey)
                  ?? SimulationScenarioCatalog.CashPayment;
        ScenarioType = _option.DefaultType;
    }

    /// <summary>Düzenlenecek denemeyi çalışma listesinden okur ya da yeni deneme için formu açar.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (_conditionId is not { } id) { await ResolveNamesAsync(); State = ScreenState.Content; return; }
        SetBusy(true); State = ScreenState.Loading;
        try
        {
            var list = await _simulationService.GetWorkingListAsync();
            _loaded = list.FirstOrDefault(x => x.Request.ScenarioId == id)?.Request;
            if (_loaded is null)
            {
                await _dialogService.ShowAlertAsync(NotFoundTitle, NotFoundMessage);
                await _navigationService.NavigateBackAsync();
                return;
            }
            SimulationConditionDraftBuilder.Populate(this, _loaded);
            await ResolveNamesAsync();
            State = ScreenState.Content;
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[SimulationConditionViewModel ERROR] {ex}"); State = ScreenState.Error; }
        finally { SetBusy(false); }
    }

    private async Task ResolveNamesAsync()
    {
        var plan = await _planReader.GetPlanAsync();
        if (CardId is { } cid && plan.CreditCards.FirstOrDefault(x => x.Id == cid) is { } c) { CardName = $"{c.Bank} {c.Name}".Trim(); }
        if (LoanId is { } lid && plan.Loans.FirstOrDefault(x => x.Id == lid) is { } l) { LoanName = $"{l.Bank} {l.Name}".Trim(); }
    }

    /// <summary>Denemeyi doğrular ve çalışma listesine yazar; yeni deneme açık olarak sona eklenir.</summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (IsBusy) { return; }
        if (!TryBuildRequest(out var request, out var error)) { await _dialogService.ShowAlertAsync(SaveFailedTitle, error); return; }
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
        if (HasChanges && !await _dialogService.ConfirmAsync(ExitConfirmTitle, ExitConfirmMessage, "Çık", "Kal")) { return; }
        await _navigationService.NavigateBackAsync();
    }

    private bool TryBuildRequest(out SimulationRequest request, out string error) =>
        SimulationConditionDraftBuilder.TryBuild(this, _loaded, _clock.Today, out request, out error);
}
