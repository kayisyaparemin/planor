using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Simülatör sayfasının görünüm modeli (EK-V10, S76). Denemeleri çalışma listesinde
/// yönetir ve onaylanan denemeleri tek işlemde canlı plana aktarır (V10f, S76-12).
/// </summary>
public sealed partial class SimulatorViewModel(
    ISimulationWorkflowService simulationService, INavigationService navigationService,
    IDialogService dialogService, SimulationResultViewModel result) : ViewModelBase
{
    /// <summary>Liste kapalıyken gösterilen en fazla satır sayısı; fazlası "+N daha" ile yerinde açılır (GS21).</summary>
    public const int CollapsedRowLimit = 4;

    private const string CancelText = "Vazgeç", EditText = "Düzenle", DeleteText = "Sil";
    private const string SaveFailedTitle = "Liste kaydedilemedi", SaveFailedMessage = "Değişiklik şu an kaydedilemedi. Tekrar dene.";
    private const string ApplyConfirmTitle = "Planıma ekle", ApplyActionText = "Planıma ekle",
        ApplyConfirmMessage = "Açık denemeler finans planına eklenecek ve gerçek kayıtlara dönüştürülecek. Onaylıyor musun?",
        ApplyFailedTitle = "Plan güncellenemedi", ApplyFailedMessage = "Denemeler plana eklenirken bir hata oluştu. Tekrar dene.";

    private readonly ISimulationWorkflowService _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
    private readonly INavigationService _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    private readonly IDialogService _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

    private List<SimulationWorkingCondition> _conditions = [];
    private bool _isExpanded;

    [ObservableProperty] private bool hasConditions;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasOverflow))] private int hiddenCount;

    /// <summary>Sonuç kartı; liste her değiştiğinde yeniden hesaplanır (S76-6).</summary>
    public SimulationResultViewModel Result { get; } = result ?? throw new ArgumentNullException(nameof(result));
    /// <summary>Ekranda görünen deneme satırları, kurulduğu sırayla.</summary>
    public ObservableCollection<SimulationConditionRow> Conditions { get; } = [];
    /// <summary>Gizli satır varsa taşma satırı görünür.</summary>
    public bool HasOverflow => HiddenCount > 0;

    /// <summary>Açık ve geçerli en az bir deneme varsa buton görünür (EK-V10 S5).</summary>
    public bool CanApply => _conditions.Any(x => x.IsEnabled && x.Issue == SimulationConditionIssue.None);

    /// <summary>Çalışma listesini okur, sonucu o listeyle hesaplatır; okuma hatasında hata durumuna düşer (S76-6).</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            _conditions = [.. await _simulationService.GetWorkingListAsync()];
            Refresh();
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulatorViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }

        if (State == ScreenState.Content)
        {
            await Recalculate();
        }
    }

    /// <summary>Deneme türü seçicisini açar (EK-V10, S77 V10 notları i).</summary>
    [RelayCommand]
    private Task AddAsync() => _navigationService.NavigateToAsync(Routes.SimulationConditionPicker);

    /// <summary>Açık ve geçerli denemeleri kullanıcı onayıyla tek işlemde plana ekler (S76-12, EK-V10 S5).</summary>
    [RelayCommand]
    private async Task ApplyAsync()
    {
        var applicable = _conditions.Where(x => x.IsEnabled && x.Issue == SimulationConditionIssue.None).ToList();
        if (applicable.Count == 0) { return; }

        var confirmed = await _dialogService.ConfirmAsync(ApplyConfirmTitle, ApplyConfirmMessage, ApplyActionText, CancelText);
        if (!confirmed) { return; }

        SetBusy(true);
        try
        {
            var requests = applicable.Select(x => x.Request).ToList();
            await _simulationService.ApplySimulationAsync(requests, confirmed: true);

            var appliedIds = requests.Select(x => x.ScenarioId).ToHashSet();
            _conditions.RemoveAll(x => appliedIds.Contains(x.Request.ScenarioId));

            await _simulationService.SaveWorkingListAsync(CurrentDrafts());
            Refresh();
            await Recalculate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulatorViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(ApplyFailedTitle, ApplyFailedMessage);
            await LoadAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Satırın seçeneklerini tek diyalogda sunar: düzenle, sil.</summary>
    [RelayCommand]
    private async Task SelectConditionAsync(SimulationConditionRow? row)
    {
        if (row is null) { return; }

        var choice = await _dialogService.ChooseAsync(row.Name, CancelText, DeleteText, EditText);
        if (choice == EditText)
        {
            await _navigationService.NavigateToAsync(Routes.SimulationCondition,
                new Dictionary<string, object> { [Routes.ConditionIdParameter] = row.Id });
        }
        else if (choice == DeleteText)
        {
            _conditions.RemoveAll(x => x.Request.ScenarioId == row.Id);
            Refresh();
            await Task.WhenAll(PersistAsync(), Recalculate());
        }
    }

    /// <summary>Gizli satırları da gösterir.</summary>
    [RelayCommand]
    private void Expand()
    {
        _isExpanded = true;
        Refresh();
    }

    private void Refresh()
    {
        foreach (var row in Conditions)
        {
            row.PropertyChanged -= OnRowChanged;
        }

        Conditions.Clear();
        foreach (var condition in _isExpanded ? _conditions : _conditions.Take(CollapsedRowLimit))
        {
            var row = new SimulationConditionRow(condition);
            row.PropertyChanged += OnRowChanged;
            Conditions.Add(row);
        }

        HasConditions = _conditions.Count > 0;
        HiddenCount = _conditions.Count - Conditions.Count;
        OnPropertyChanged(nameof(CanApply));
    }

    // Anahtar satıra iki yönlü bağlı: dokunuş satırı değiştirir, değişiklik buradan listeye yazılır (S76-4).
    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not SimulationConditionRow row || e.PropertyName != nameof(SimulationConditionRow.IsEnabled)) { return; }

        var index = _conditions.FindIndex(x => x.Request.ScenarioId == row.Id);
        if (index >= 0)
        {
            _conditions[index] = _conditions[index] with { IsEnabled = row.IsEnabled };
            OnPropertyChanged(nameof(CanApply));
            _ = PersistAsync();
            _ = Recalculate();
        }
    }

    private SimulationDraftCondition[] CurrentDrafts() =>
        _conditions.Select(x => new SimulationDraftCondition(x.Request, x.IsEnabled)).ToArray();

    private Task Recalculate() => Result.RefreshAsync(CurrentDrafts());

    private async Task PersistAsync()
    {
        try
        {
            await _simulationService.SaveWorkingListAsync(CurrentDrafts());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulatorViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, SaveFailedMessage);
            await LoadAsync();
        }
    }
}
