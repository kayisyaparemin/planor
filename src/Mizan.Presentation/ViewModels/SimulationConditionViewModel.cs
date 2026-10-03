using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Simülatörün deneme formu (EK-V10, S76-3, 7, 8). Bir denemeyi türünün alanlarıyla ekler ya da düzenler ve
/// çalışma listesine yazar; gerçek kayıtlara dokunmaz. Tür "Ekle" seçicisinde seçilir, düzenlemede değişmez.
/// Ad zorunludur çünkü deneme uygulanınca kaydın adı olur; tarih en erken bugündür çünkü geçmiş kaydın işidir.
/// V10a'da yalnız nakit ödeme vardır; diğer türlerin alanları V10c–e'de gelir.
/// </summary>
public sealed partial class SimulationConditionViewModel : ViewModelBase
{
    private const string SaveFailedTitle = "Deneme kaydedilemedi";
    private const string MissingNameMessage = "Denemeye bir ad ver.";
    private const string InvalidAmountMessage = "Tutarı sıfırdan büyük bir sayı olarak gir.";
    private const string UnexpectedErrorMessage = "Deneme şu an kaydedilemedi. Tekrar dene.";
    private const string NotFoundTitle = "Deneme bulunamadı";
    private const string NotFoundMessage = "Deneme silinmiş olabilir.";
    private const string ExitConfirmTitle = "Kaydetmeden çık";
    private const string ExitConfirmMessage = "Yaptığın değişiklikler kaydedilmeyecek.";
    private const string ExitConfirmLeave = "Çık";
    private const string ExitConfirmStay = "Kal";

    private readonly ISimulationWorkflowService _simulationService;
    private readonly IClock _clock;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private ScenarioOption _option = SimulationScenarioCatalog.CashPayment;
    private Guid? _conditionId;
    private SimulationRequest? _loaded;

    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private SimulationScenarioType scenarioType;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private DateOnly date;

    /// <summary>Görünüm modelini dört dar bağımlılıkla başlatır (Kural M3).</summary>
    public SimulationConditionViewModel(
        ISimulationWorkflowService simulationService,
        IClock clock,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        Date = _clock.Today;
    }

    /// <summary>Tarih seçicisinin en erken günü: bugün (S76-3).</summary>
    public DateOnly MinimumDate => _clock.Today;

    /// <summary>Formda kaydedilmemiş bir değişiklik var mı.</summary>
    public bool HasChanges => _loaded is { } loaded
        ? Name.Trim() != loaded.Name ||
          AmountInput != StatementEntryViewModel.FormatAmount(loaded.Amount) ||
          Date != loaded.StartDate
        : !string.IsNullOrWhiteSpace(Name) || !string.IsNullOrWhiteSpace(AmountInput) || Date != _clock.Today;

    /// <summary>
    /// Formun neyi açacağını gezinmeden alır: yeni denemede senaryo seçeneğinin anahtarı, düzenlemede denemenin
    /// kimliği. Yükleme <see cref="LoadCommand"/> ile ayrı yapılır; hata hâlinde aynı hedefle tekrar denenir.
    /// </summary>
    public void Prepare(string? scenarioOptionKey, Guid? conditionId)
    {
        _conditionId = conditionId;
        IsEditing = conditionId.HasValue;
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
            State = ScreenState.Content;
            return;
        }

        SetBusy(true);
        State = ScreenState.Loading;
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

            _option = SimulationScenarioCatalog.For(_loaded.Type);
            ScenarioType = _loaded.Type;
            Name = _loaded.Name;
            AmountInput = StatementEntryViewModel.FormatAmount(_loaded.Amount);
            Date = _loaded.StartDate;
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulationConditionViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
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
            await _simulationService.SaveWorkingListAsync(await WithConditionAsync(request));
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulationConditionViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally
        {
            SetBusy(false);
        }
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

    // Düzenlemede istek yüklenenin üstüne kurulur: kimlik ve tür korunur (tek seferlik ödeme nakde dönmez).
    private bool TryBuildRequest(out SimulationRequest request, out string error)
    {
        request = new SimulationRequest();
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(Name)) { error = MissingNameMessage; return false; }
        if (!StatementEntryViewModel.TryParseAmount(AmountInput, out var amount) || amount <= 0m) { error = InvalidAmountMessage; return false; }

        request = (_loaded ?? new SimulationRequest()) with
        {
            Type = SimulationScenarioCatalog.Resolve(_option, 1, null, _loaded?.Type),
            Name = Name.Trim(),
            Amount = amount,
            StartDate = Date
        };
        if (SimulationConditionRules.IsDatePassed(request, _clock.Today)) { error = SimulationConditionRules.PastDateMessage; return false; }
        return true;
    }

    // Liste yazmadan hemen önce yeniden okunur: simülatörde aç/kapa ya da silme form açıkken de olmuş olabilir.
    private async Task<IReadOnlyList<SimulationDraftCondition>> WithConditionAsync(SimulationRequest request)
    {
        var list = (await _simulationService.GetWorkingListAsync())
            .Select(x => new SimulationDraftCondition(x.Request, x.IsEnabled))
            .ToList();
        var index = list.FindIndex(x => x.Request.ScenarioId == request.ScenarioId);
        if (index < 0) { list.Add(new SimulationDraftCondition(request)); }
        else { list[index] = list[index] with { Request = request }; }
        return list;
    }
}
