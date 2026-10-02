using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Charts;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// "12 Dönem" ekranının görünüm modeli (EK-V8, S74): ana sayfanın dönem sonundan başlayan 12 dönemde bakiyenin
/// en çok nereye indiği, bir yıl sonra nerede olduğu, her dönemin sonu, bu gidişatın faizi ve kredileri erken
/// kapatmanın kazandırıp kazandırmadığı. Eskide aynı ekran tanrı cepheden beş çağrıyla kuruluyor ve metin
/// üretiyordu; burada tek okuma portundan ham değer sunar.
/// </summary>
public sealed partial class FuturePeriodsViewModel : ViewModelBase
{
    // Rol anahtarı (TASARIM-SISTEMI § Rol → token): tek seri düz çizgi.
    private const string ActualKey = "actual";

    private readonly IFutureProjectionService _projectionService;
    private readonly INavigationService _navigationService;

    /// <summary>Izgaranın 12 karosu, zincir sırasıyla.</summary>
    public ObservableCollection<FuturePeriodTile> Periods { get; } = [];

    /// <summary>Erken kapama kartının satırları, kredi sırasıyla; öneri yoksa ya da hesaplanamadıysa boş (S74-7).</summary>
    public ObservableCollection<LoanPayoffRow> PayoffRows { get; } = [];

    [ObservableProperty] private bool hasPayoffRows;

    [ObservableProperty] private decimal? lowestEndingBalance;
    [ObservableProperty] private DateOnly? lowestPeriodStart;
    [ObservableProperty] private decimal? finalEndingBalance;
    [ObservableProperty] private bool isFinalNegative;
    [ObservableProperty] private decimal? totalInterest;
    [ObservableProperty] private DateOnly? chainStart;
    [ObservableProperty] private DateOnly? horizonLastDay;
    [ObservableProperty] private ChartTrend? trend;

    /// <summary>12 dönem görünüm modelini projeksiyon okuma portu ve gezinme portuyla başlatır.</summary>
    public FuturePeriodsViewModel(IFutureProjectionService projectionService, INavigationService navigationService)
    {
        _projectionService = projectionService ?? throw new ArgumentNullException(nameof(projectionService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>
    /// 12 dönemi yükler; açık dönem ya da kurulabilir plan yoksa ekran boştur (S74-2). Erken kapama önerisi listeden
    /// sonra gelir: birkaç düzine projeksiyon koşar, liste onu beklemez ve kart hazır olunca görünür.
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        await LoadProjectionAsync();
        if (State == ScreenState.Content)
        {
            await LoadPayoffAsync();
        }
    }

    /// <summary>Boş hâlin aksiyonu: gelir ya da bakiye bilgisinin girildiği Finansal Yapı'yı açar.</summary>
    [RelayCommand]
    public Task OpenFinancialStructureAsync() => _navigationService.NavigateToAsync(Routes.FinancialStructure);

    /// <summary>Erken kapama satırının krediyi açması: kapama kredi formunda erken ödeme olarak planlanır (S74-7).</summary>
    [RelayCommand]
    public Task OpenLoanAsync(LoanPayoffRow row) =>
        _navigationService.NavigateToAsync(Routes.LoanForm, new Dictionary<string, object> { [Routes.LoanIdParameter] = row.LoanId });

    private async Task LoadProjectionAsync()
    {
        SetBusy(true);
        State = ScreenState.Loading;
        PayoffRows.Clear();
        HasPayoffRows = false;
        try
        {
            var projection = await _projectionService.GetAsync();
            if (projection is null)
            {
                ShowEmptyState();
                return;
            }

            ApplyPeriods(projection.Periods);
            ApplySummary(projection.Periods, projection.TotalInterestCost);
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FuturePeriodsViewModel ERROR] {ex}");
            // Okuma ya da hesap hatası ekranda StateBlock (Hata) olarak görünür; "Tekrar dene" bu komutu çalıştırır.
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    // Önerinin hatası sayfayı düşürmez, yalnız kartı gizler (S74-7). Satırlar sonuç gelince bir arada yazılır; araya
    // giren ikinci yükleme satırları çiftleyemez.
    private async Task LoadPayoffAsync()
    {
        try
        {
            var advice = await _projectionService.GetPayoffAdviceAsync();
            PayoffRows.Clear();
            foreach (var item in advice)
            {
                PayoffRows.Add(ToRow(item));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FuturePeriodsViewModel ERROR] Erken kapama: {ex}");
            PayoffRows.Clear();
        }
        HasPayoffRows = PayoffRows.Count > 0;
    }

    private static LoanPayoffRow ToRow(LoanPayoffAdvice advice) => new()
    {
        LoanId = advice.LoanId,
        LoanName = advice.LoanName,
        Status = advice.Status,
        Date = advice.Date,
        PayoffAmount = advice.PayoffAmount,
        NetGain = advice.NetGain
    };

    private void ApplyPeriods(IReadOnlyList<CashFlowPeriodProjection> periods)
    {
        var lowest = LowestIndex(periods);
        Periods.Clear();
        for (var index = 0; index < periods.Count; index++)
        {
            var start = periods[index].PeriodStart;
            // Yıl ilk karoda ve yılın ilk döneminde yazılır; aradaki karolar yalnız ay adını taşır (GS26-1).
            var showsYear = index == 0 || start.Month == 1;
            Periods.Add(new FuturePeriodTile(index, start, periods[index].EndingBalance, index == lowest, showsYear));
        }
    }

    private void ApplySummary(IReadOnlyList<CashFlowPeriodProjection> periods, decimal totalInterest)
    {
        var lowest = periods[LowestIndex(periods)];
        LowestEndingBalance = lowest.EndingBalance;
        LowestPeriodStart = lowest.PeriodStart;
        FinalEndingBalance = periods[^1].EndingBalance;
        IsFinalNegative = FinalEndingBalance < 0m;
        TotalInterest = totalInterest;
        ChainStart = periods[0].PeriodStart;
        HorizonLastDay = periods[^1].PeriodEnd.AddDays(-1); // PeriodEnd sonraki dönemin ilk günü (S72-7 ile aynı)
        Trend = BuildTrend(periods);
    }

    private void ShowEmptyState()
    {
        Periods.Clear();
        LowestEndingBalance = FinalEndingBalance = TotalInterest = null;
        LowestPeriodStart = ChainStart = HorizonLastDay = null;
        IsFinalNegative = false;
        Trend = null;
        State = ScreenState.Empty;
    }

    // Eşitlikte ilk dönem: kullanıcı en erken sıkışacağı anı görmeli.
    private static int LowestIndex(IReadOnlyList<CashFlowPeriodProjection> periods)
    {
        var lowest = 0;
        for (var index = 1; index < periods.Count; index++)
        {
            if (periods[index].EndingBalance < periods[lowest].EndingBalance)
            {
                lowest = index;
            }
        }
        return lowest;
    }

    // Zincirin başı + 12 dönem sonu; sıfır eşiği her zaman ölçekte, dolgu ona iner (GS26-3, 4).
    private static ChartTrend BuildTrend(IReadOnlyList<CashFlowPeriodProjection> periods)
    {
        var points = new List<ChartPoint> { new(periods[0].PeriodStart, periods[0].OpeningBalance) };
        points.AddRange(periods.Select(period => new ChartPoint(period.PeriodEnd, period.EndingBalance)));
        return new ChartTrend(new ChartSeries(ActualKey, points), null, null, null, null) { Threshold = new ChartThreshold(0m) };
    }
}
