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
/// Simülatör sayfasının görünüm modeli (EK-V10, S76). Kullanıcının "şunu yaparsam ne olur?" diye kurduğu
/// denemeleri tek çalışma listesinde tutar: listeler, açıp kapatır, düzenlemeye gönderir, siler. Liste
/// kendiliğinden saklanır (S76-4); eskiden ekranda yaşıyor ve uygulama kapanınca kayboluyordu. Gerçek kayıtlara
/// dokunmaz. Sonuç tarafı (grafik, kıyas, dönem sonları) V10b'de gelir.
/// </summary>
public sealed partial class SimulatorViewModel : ViewModelBase
{
    /// <summary>Liste kapalıyken gösterilen en fazla satır sayısı; fazlası "+N daha" ile yerinde açılır (GS21).</summary>
    public const int CollapsedRowLimit = 4;

    private const string AddTitle = "Ne denemek istiyorsun?";
    private const string CancelText = "Vazgeç";
    private const string EditText = "Düzenle";
    private const string DeleteText = "Sil";
    private const string SaveFailedTitle = "Liste kaydedilemedi";
    private const string SaveFailedMessage = "Değişiklik şu an kaydedilemedi. Tekrar dene.";

    // "Ekle" seçicisinde sunulan türler, katalog sırasıyla; her alt adım kendi türünü ekler (V10a: nakit ödeme).
    private static readonly IReadOnlyList<ScenarioOption> AddableOptions = [SimulationScenarioCatalog.CashPayment];

    private readonly ISimulationWorkflowService _simulationService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private List<SimulationWorkingCondition> _conditions = [];
    private bool _isExpanded;

    [ObservableProperty] private bool hasConditions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    private int hiddenCount;

    /// <summary>Görünüm modelini üç dar bağımlılıkla başlatır (Kural M3).</summary>
    public SimulatorViewModel(
        ISimulationWorkflowService simulationService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>Ekranda görünen deneme satırları, kurulduğu sırayla.</summary>
    public ObservableCollection<SimulationConditionRow> Conditions { get; } = [];

    /// <summary>Gizli satır varsa taşma satırı görünür.</summary>
    public bool HasOverflow => HiddenCount > 0;

    /// <summary>Çalışma listesini okur; okuma hatasında hata durumuna düşer.</summary>
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
    }

    /// <summary>Denenebilecek türleri sorar ve seçilen türün formunu açar.</summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        var titles = AddableOptions.Select(x => x.Title).ToArray();
        var choice = await _dialogService.ChooseAsync(AddTitle, CancelText, null, titles);
        var option = AddableOptions.FirstOrDefault(x => x.Title == choice);
        if (option is null)
        {
            return;
        }

        await _navigationService.NavigateToAsync(
            Routes.SimulationCondition,
            new Dictionary<string, object> { [Routes.ScenarioOptionParameter] = option.Key });
    }

    /// <summary>Satırın seçeneklerini tek diyalogda sunar: düzenle, sil.</summary>
    [RelayCommand]
    private async Task SelectConditionAsync(SimulationConditionRow? row)
    {
        if (row is null)
        {
            return;
        }

        var choice = await _dialogService.ChooseAsync(row.Name, CancelText, DeleteText, EditText);
        if (choice == EditText)
        {
            await _navigationService.NavigateToAsync(
                Routes.SimulationCondition,
                new Dictionary<string, object> { [Routes.ConditionIdParameter] = row.Id });
        }
        else if (choice == DeleteText)
        {
            _conditions.RemoveAll(x => x.Request.ScenarioId == row.Id);
            Refresh();
            await PersistAsync();
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
    }

    // Anahtar satıra iki yönlü bağlı: dokunuş satırı değiştirir, değişiklik buradan listeye yazılır (S76-4).
    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not SimulationConditionRow row || e.PropertyName != nameof(SimulationConditionRow.IsEnabled))
        {
            return;
        }

        var index = _conditions.FindIndex(x => x.Request.ScenarioId == row.Id);
        if (index < 0)
        {
            return;
        }

        _conditions[index] = _conditions[index] with { IsEnabled = row.IsEnabled };
        _ = PersistAsync();
    }

    // Yazma düşerse ekran diskteki listeye döner; kullanıcı kaydedilmemiş bir hâli görmeye devam etmez.
    private async Task PersistAsync()
    {
        try
        {
            await _simulationService.SaveWorkingListAsync(
                _conditions.Select(x => new SimulationDraftCondition(x.Request, x.IsEnabled)).ToArray());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulatorViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, SaveFailedMessage);
            await LoadAsync();
        }
    }
}
