using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;
using Mizan.Presentation.Charts;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kapanmış bir dönemin ayrıntı görünüm modeli (EK-V12, S68-6, 7, GS32): Dönemin dondurulmuş planına göre
/// gerçekleşen kapanış bakiyesi, bakiye çizgisi (ChartCard trendi), farkın kaynağı (yaşam harcaması,
/// borç ödemeleri, KMH, kasa düzeltmesi) ve gerçekleşen ödemeler listesini sunar.
/// </summary>
public sealed partial class HistoryDetailViewModel(
    HistoryQueryService historyQueryService,
    IPeriodObservationRepository observationRepository,
    INavigationService navigationService) : ViewModelBase
{
    private const int CollapsedPaymentLimit = 4;
    private readonly HistoryQueryService _historyQueryService = historyQueryService ?? throw new ArgumentNullException(nameof(historyQueryService));
    private readonly IPeriodObservationRepository _observationRepository = observationRepository ?? throw new ArgumentNullException(nameof(observationRepository));
    private readonly INavigationService _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    private readonly List<HistoryPaymentRow> _allPayments = [];
    private bool _isPaymentListExpanded;
    private Guid? _currentActualId;

    /// <summary>Görüntülenen ödemeler listesi (daraltılmış veya tümü).</summary>
    public ObservableCollection<HistoryPaymentRow> Payments { get; } = [];

    [ObservableProperty] private DateOnly? periodStart;
    [ObservableProperty] private DateOnly? periodLastDay;
    [ObservableProperty] private decimal? actualEndingBalance;
    [ObservableProperty] private decimal? plannedEndingBalance;
    [ObservableProperty] private decimal? difference;
    [ObservableProperty] private bool isDifferencePositive;
    [ObservableProperty] private bool isDifferenceNegative;
    [ObservableProperty] private decimal? openingBalance;
    [ObservableProperty] private ChartTrend? trend;
    [ObservableProperty] private decimal? plannedLivingSpend;
    [ObservableProperty] private decimal? actualLivingSpend;
    [ObservableProperty] private decimal? livingDifference;
    [ObservableProperty] private bool isLivingDifferencePositive;
    [ObservableProperty] private bool isLivingDifferenceNegative;
    [ObservableProperty] private decimal? plannedMandatory;
    [ObservableProperty] private decimal? actualMandatory;
    [ObservableProperty] private decimal? mandatoryDifference;
    [ObservableProperty] private bool isMandatoryDifferencePositive;
    [ObservableProperty] private bool isMandatoryDifferenceNegative;
    [ObservableProperty] private int paidPaymentCount;
    [ObservableProperty] private int totalPaymentCount;
    [ObservableProperty] private decimal? actualInterest;
    [ObservableProperty] private bool hasActualInterest;
    [ObservableProperty] private decimal? reconciliationAdjustment;
    [ObservableProperty] private bool hasReconciliationAdjustment;
    [ObservableProperty] private string summaryText = string.Empty;
    [ObservableProperty] private bool hasPayments;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasHiddenPayments))] private int hiddenPaymentCount;
    [ObservableProperty] private bool isExpanded;

    /// <summary>Gizli ödeme satırları var mı.</summary>
    public bool HasHiddenPayments => HiddenPaymentCount > 0;

    /// <summary>Gerçekleşme kaydını kimliğiyle yükler.</summary>
    [RelayCommand]
    public async Task LoadAsync(Guid? actualId = null)
    {
        SetBusy(true);
        State = ScreenState.Loading;
        _currentActualId = actualId ?? _currentActualId;
        if (!_currentActualId.HasValue)
        {
            State = ScreenState.Empty;
            SetBusy(false);
            return;
        }

        try
        {
            var period = await _historyQueryService.GetPeriodAsync(_currentActualId.Value);
            if (period is null)
            {
                State = ScreenState.Empty;
                return;
            }

            ApplyMetrics(period);
            await ApplyTrendAsync(period);
            ApplyPayments(period);
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

    /// <summary>Ödemeler listesini yerinde genişletir veya daraltır.</summary>
    [RelayCommand]
    public void TogglePayments()
    {
        _isPaymentListExpanded = !_isPaymentListExpanded;
        ApplyPaymentVisibility();
    }

    /// <summary>Önceki sayfaya döner.</summary>
    [RelayCommand]
    public Task GoBackAsync() => _navigationService.NavigateBackAsync();

    private void ApplyMetrics(HistoryPeriod period)
    {
        var (rev, orig, actual) = (period.Revision, period.OriginalPlan, period.Actual);
        PeriodStart = actual.PeriodStart;
        PeriodLastDay = actual.PeriodEnd.AddDays(-1);
        ActualEndingBalance = period.Comparison.ActualEndingBalance;
        PlannedEndingBalance = period.Comparison.PlannedEndingBalance;
        Difference = period.Comparison.Difference;
        IsDifferencePositive = Difference > 0m;
        IsDifferenceNegative = Difference < 0m;
        OpeningBalance = orig.OpeningBalance;

        PlannedLivingSpend = rev?.PlannedVariableExpenseAllowance ?? orig.PlannedVariableExpenseAllowance;
        ActualLivingSpend = actual.ActualLivingSpend;
        LivingDifference = PlannedLivingSpend - ActualLivingSpend;
        IsLivingDifferencePositive = LivingDifference > 0m;
        IsLivingDifferenceNegative = LivingDifference < 0m;

        PlannedMandatory = rev?.PlannedMandatoryPayments ?? orig.PlannedMandatoryPayments;
        ActualMandatory = actual.ActualMandatoryPayments;
        MandatoryDifference = ActualMandatory - PlannedMandatory;
        IsMandatoryDifferencePositive = MandatoryDifference > 0m;
        IsMandatoryDifferenceNegative = MandatoryDifference < 0m;
        TotalPaymentCount = actual.Payments.Count;
        PaidPaymentCount = actual.Payments.Count(p => p.Status != ActualPaymentStatus.Unpaid);

        ActualInterest = actual.ActualInterest;
        HasActualInterest = ActualInterest > 0m;
        ReconciliationAdjustment = actual.ReconciliationAdjustment;
        HasReconciliationAdjustment = ReconciliationAdjustment != 0m;
        SummaryText = period.Comparison.Summary;
    }

    private async Task ApplyTrendAsync(HistoryPeriod period)
    {
        var obs = await _observationRepository.GetPeriodObservationsAsync(period.OriginalPlan.Id);
        var lastDay = period.Actual.PeriodEnd.AddDays(-1);
        var points = new List<ChartPoint> { new(period.Actual.PeriodStart, period.OriginalPlan.OpeningBalance) };
        points.AddRange(obs.Where(o => o.ObservedOn > period.Actual.PeriodStart && o.ObservedOn < lastDay)
            .OrderBy(o => o.ObservedOn).Select(o => new ChartPoint(o.ObservedOn, o.ObservedBalance)));
        points.Add(new ChartPoint(lastDay, period.Comparison.ActualEndingBalance));

        var markers = new ChartSeries("observation", obs.Select(o => new ChartPoint(o.ObservedOn, o.ObservedBalance)).ToList());
        Trend = new ChartTrend(new ChartSeries("actual", points), null, null, new ChartThreshold(period.Comparison.PlannedEndingBalance), markers)
        {
            Threshold = new ChartThreshold(0m)
        };
    }

    private void ApplyPayments(HistoryPeriod period)
    {
        _allPayments.Clear();
        foreach (var p in period.Actual.Payments.OrderBy(x => x.PlannedDate))
        {
            var (text, sem) = p.Status == ActualPaymentStatus.Paid ? ("Ödendi", "Positive")
                : p.Status == ActualPaymentStatus.DifferentAmount ? ("Farklı tutar", "Warning")
                : ("Ödenmedi", "Negative");

            _allPayments.Add(new HistoryPaymentRow
            {
                Name = p.Name, PlannedDate = p.PlannedDate, PlannedAmount = p.PlannedAmount,
                ActualAmount = p.ActualAmount, StatusText = text, StatusSemantic = sem
            });
        }
        HasPayments = _allPayments.Count > 0;
        ApplyPaymentVisibility();
    }

    private void ApplyPaymentVisibility()
    {
        Payments.Clear();
        var showAll = _isPaymentListExpanded || _allPayments.Count <= CollapsedPaymentLimit;
        var display = showAll ? _allPayments : _allPayments.Take(CollapsedPaymentLimit);
        foreach (var p in display)
        {
            Payments.Add(p);
        }
        HiddenPaymentCount = showAll ? 0 : _allPayments.Count - CollapsedPaymentLimit;
        IsExpanded = showAll && _allPayments.Count > CollapsedPaymentLimit;
    }
}
