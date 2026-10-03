using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Charts;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Simülatörün sonuç kartının görünüm modeli (EK-V10, S76): denemelerle önümüzdeki 12 dönemde en çok nerede
/// sıkışılacağı, o dönemde şu anki gidişata göre ne kadar aşağıda kalınacağı ve iki çizgili yolu. Deneme listesi
/// <see cref="SimulatorViewModel"/>'dedir; liste her değiştiğinde buraya verilir ve sonuç yeniden hesaplanır. Ayrı
/// bir çocuk ViewModel'dir çünkü liste yönetimi dosya sınırına dayandı (K3); sonucun kendi durumu da (yükleniyor,
/// boş, hata) listeninkinden ayrıdır: hesap düşerse liste kullanılabilir kalır.
/// </summary>
public sealed partial class SimulationResultViewModel : ViewModelBase
{
    [ObservableProperty] private decimal? lowestEndingBalance;
    [ObservableProperty] private DateOnly? lowestPeriodStart;
    [ObservableProperty] private DateOnly? chainStart;
    [ObservableProperty] private DateOnly? horizonLastDay;
    [ObservableProperty] private ChartTrend? trend;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDifference))]
    [NotifyPropertyChangedFor(nameof(IsDifferenceNegative))]
    [NotifyPropertyChangedFor(nameof(IsDifferencePositive))]
    private decimal? differenceAtLowest;

    private readonly ISimulationResultService _resultService;
    private readonly INavigationService _navigationService;

    private IReadOnlyList<SimulationDraftCondition> _conditions = [];
    private int _request;

    /// <summary>Sonuç görünüm modelini hesap portu ve gezinme portuyla başlatır.</summary>
    public SimulationResultViewModel(ISimulationResultService resultService, INavigationService navigationService)
    {
        _resultService = resultService ?? throw new ArgumentNullException(nameof(resultService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        State = ScreenState.Loading;
    }

    /// <summary>Açık deneme varsa en düşük dönemdeki fark gösterilir; yoksa yalnız şu anki gidişat vardır.</summary>
    public bool HasDifference => DifferenceAtLowest is not null;

    /// <summary>Deneme o dönemde şu anki gidişatın altına çekiyor.</summary>
    public bool IsDifferenceNegative => DifferenceAtLowest < 0m;

    /// <summary>Deneme o dönemde şu anki gidişatın üstüne çıkarıyor.</summary>
    public bool IsDifferencePositive => DifferenceAtLowest > 0m;

    /// <summary>
    /// Verilen liste için sonucu yeniden hesaplar; üst üste binen isteklerde son istek kazanır, eski cevap atılır
    /// (S73-2). Sonuç varken iskelet gösterilmez, eski sonuç yenisi gelene kadar yerinde kalır (EK-V10 § 5). Hesap
    /// düşerse yalnız bu kart hata gösterir, deneme listesi etkilenmez.
    /// </summary>
    /// <param name="conditions">Çalışma listesinin o anki hâli.</param>
    public async Task RefreshAsync(IReadOnlyList<SimulationDraftCondition> conditions)
    {
        var request = ++_request;
        _conditions = conditions;
        if (State != ScreenState.Content)
        {
            State = ScreenState.Loading;
        }

        try
        {
            var outcome = await _resultService.CalculateAsync(conditions);
            if (request == _request)
            {
                Apply(outcome);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SimulationResultViewModel ERROR] {ex}");
            if (request == _request)
            {
                State = ScreenState.Error;
            }
        }
    }

    /// <summary>Hata durumunun aksiyonu: son verilen listeyle yeniden hesaplar.</summary>
    [RelayCommand]
    public Task RetryAsync() => RefreshAsync(_conditions);

    /// <summary>Boş hâlin aksiyonu: gelir ya da bakiye bilgisinin girildiği Finansal Yapı'yı açar.</summary>
    [RelayCommand]
    public Task OpenFinancialStructureAsync() => _navigationService.NavigateToAsync(Routes.FinancialStructure);

    // Açık deneme varsa hero denemeli serinin en düşüğüdür; fark o dönemin denemesiz sonuyla alınır (V10b notları c).
    private void Apply(SimulationOutcome? outcome)
    {
        if (outcome is null)
        {
            ShowEmptyState();
            return;
        }

        var series = outcome.Scenario ?? outcome.Baseline;
        var lowest = ProjectionSummary.LowestIndex(series);
        LowestEndingBalance = series[lowest].EndingBalance;
        LowestPeriodStart = series[lowest].PeriodStart;
        DifferenceAtLowest = outcome.Scenario is null ? null : series[lowest].EndingBalance - outcome.Baseline[lowest].EndingBalance;
        ChainStart = series[0].PeriodStart;
        HorizonLastDay = series[^1].PeriodEnd.AddDays(-1); // PeriodEnd sonraki dönemin ilk günü (S72-7 ile aynı)
        Trend = ProjectionTrend.Build(series, outcome.Scenario is null ? null : outcome.Baseline);
        State = ScreenState.Content;
    }

    private void ShowEmptyState()
    {
        LowestEndingBalance = DifferenceAtLowest = null;
        LowestPeriodStart = ChainStart = HorizonLastDay = null;
        Trend = null;
        State = ScreenState.Empty;
    }
}
