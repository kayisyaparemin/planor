using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Geçmiş dönemler listesinin görünüm modeli (EK-V12, S29, GS32): Kapanmış tüm dönemleri
/// en yeniden eskiye listeler; son dönemlerin net değişim özetini (planlanan vs gerçekleşen net değişim)
/// sunar. Satıra dokunulduğunda o dönemin ayrıntı ekranını açar.
/// </summary>
public sealed partial class HistoryViewModel : ViewModelBase
{
    private readonly HistoryQueryService _historyQueryService;
    private readonly INavigationService _navigationService;

    /// <summary>Kapanmış dönemlerin listesi, en yeni en başta.</summary>
    public ObservableCollection<HistoryPeriodRow> Periods { get; } = [];

    /// <summary>Özet kartının görünüp görünmeyeceği (en az 1 kapanmış dönem varsa).</summary>
    [ObservableProperty] private bool hasSummary;

    /// <summary>Özetin kapsadığı dönem sayısı.</summary>
    [ObservableProperty] private int summaryPeriodCount;

    /// <summary>Son dönemlerde planlanan net tasarruf/değişim toplamı.</summary>
    [ObservableProperty] private decimal? summaryPlannedNetChange;

    /// <summary>Son dönemlerde fiilen gerçekleşen net tasarruf/değişim toplamı.</summary>
    [ObservableProperty] private decimal? summaryActualNetChange;

    /// <summary>Planlanan ile gerçekleşen net değişim arasındaki fark.</summary>
    [ObservableProperty] private decimal? summaryDifference;

    /// <summary>Özet farkın pozitif olup olmadığı.</summary>
    [ObservableProperty] private bool isSummaryDifferencePositive;

    /// <summary>Özet farkın negatif olup olmadığı.</summary>
    [ObservableProperty] private bool isSummaryDifferenceNegative;

    /// <summary>Geçmiş görünüm modelini servis ve gezinme bağımlılıklarıyla başlatır.</summary>
    public HistoryViewModel(HistoryQueryService historyQueryService, INavigationService navigationService)
    {
        _historyQueryService = historyQueryService ?? throw new ArgumentNullException(nameof(historyQueryService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>Kapanmış dönemleri ve son dönemler özetini yükler.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        SetBusy(true);
        State = ScreenState.Loading;

        try
        {
            var periods = await _historyQueryService.GetPeriodsAsync();
            Periods.Clear();

            if (periods.Count == 0)
            {
                HasSummary = false;
                State = ScreenState.Empty;
                return;
            }

            foreach (var period in periods)
            {
                Periods.Add(MapRow(period));
            }

            var summary = await _historyQueryService.GetRecentSummaryAsync();
            ApplySummary(summary);
            State = ScreenState.Content;
        }
        catch
        {
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Ana sayfaya döner.</summary>
    [RelayCommand]
    public Task GoBackAsync() => _navigationService.NavigateToAsync(Routes.Dashboard);

    /// <summary>Seçilen dönemin ayrıntı ekranına gider.</summary>
    [RelayCommand]
    public Task OpenDetailAsync(HistoryPeriodRow? row)
    {
        if (row is null)
        {
            return Task.CompletedTask;
        }

        var parameters = new Dictionary<string, object>
        {
            [Routes.HistoryActualIdParameter] = row.ActualId
        };
        return _navigationService.NavigateToAsync(Routes.HistoryDetail, parameters);
    }

    private void ApplySummary(HistorySummary? summary)
    {
        if (summary is null)
        {
            HasSummary = false;
            return;
        }

        HasSummary = true;
        SummaryPeriodCount = summary.PeriodCount;
        SummaryPlannedNetChange = summary.PlannedNetChange;
        SummaryActualNetChange = summary.ActualNetChange;
        SummaryDifference = summary.Difference;
        IsSummaryDifferencePositive = summary.Difference > 0m;
        IsSummaryDifferenceNegative = summary.Difference < 0m;
    }

    private static HistoryPeriodRow MapRow(HistoryPeriod period)
    {
        var diff = period.Comparison.Difference;
        var (statusText, semantic) = diff switch
        {
            > 0m => ("Planın üzerinde", "Positive"),
            < 0m => ("Planın altında", "Negative"),
            _ => ("Planlandığı gibi", "Default")
        };

        return new HistoryPeriodRow
        {
            ActualId = period.Actual.Id,
            PeriodStart = period.Actual.PeriodStart,
            PeriodLastDay = period.Actual.PeriodEnd.AddDays(-1),
            ActualEndingBalance = period.Comparison.ActualEndingBalance,
            PlannedEndingBalance = period.Comparison.PlannedEndingBalance,
            Difference = diff,
            StatusText = statusText,
            StatusSemantic = semantic
        };
    }
}
