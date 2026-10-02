using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Dönem ayrıntısının görünüm modeli (EK-V9, S75): 12 Dönem'deki bir dönemin sonunda ne kalacağı, bu rakamın
/// nereden çıktığı, o dönem neyin hangi gün ödeneceği ve hangi kartta faiz doğduğu. Dönemi 12 Dönem'le aynı
/// zincirden seçer, böylece karodaki rakamla aynı sonucu söyler. Eskide dönem nesne olarak geliyor ve metni bir
/// sunucu üretiyordu; burada dönemin ilk günüyle yeniden yüklenebilir ve yalnız ham değer sunar.
/// </summary>
public sealed partial class PeriodDetailViewModel : ViewModelBase
{
    // GS12: liste dört satır gösterir; fazlası "+N daha" ile yerinde açılır (GS21).
    private const int CollapsedPaymentLimit = 4;

    private readonly IFutureProjectionService _projectionService;
    private readonly INavigationService _navigationService;
    private List<PeriodPaymentRow> _allPayments = [];
    private bool _isPaymentListExpanded;
    private DateOnly? _requestedPeriodStart;

    /// <summary>Ödeme listesinde görünen satırlar; gün sonra ad sırasıyla.</summary>
    public ObservableCollection<PeriodPaymentRow> Payments { get; } = [];

    /// <summary>Faiz doğan kartlar; dönemde kart faizi yoksa boş.</summary>
    public ObservableCollection<PeriodCardInterestRow> CardInterests { get; } = [];

    [ObservableProperty] private DateOnly? periodStart; [ObservableProperty] private DateOnly? periodLastDay;

    [ObservableProperty] private decimal? endingBalance; [ObservableProperty] private decimal? netChange;
    [ObservableProperty] private bool isNetChangeNegative; [ObservableProperty] private bool isNetChangePositive;

    [ObservableProperty] private decimal? openingBalance; [ObservableProperty] private bool isOpeningNegative;
    [ObservableProperty] private decimal? income; [ObservableProperty] private decimal? paymentsTotal;
    [ObservableProperty] private decimal? variableExpenseAllowance;
    [ObservableProperty] private decimal? deficitInterest; [ObservableProperty] private bool hasDeficitInterest;

    [ObservableProperty] private bool hasPayments; [ObservableProperty] private int paymentCount;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasHiddenPayments))] private int hiddenPaymentCount;

    [ObservableProperty] private bool hasCardInterest; [ObservableProperty] private decimal? cardInterestTotal;

    /// <summary>Dönem ayrıntısını 12 dönem okuma portu ve gezinme portuyla başlatır.</summary>
    public PeriodDetailViewModel(IFutureProjectionService projectionService, INavigationService navigationService)
    {
        _projectionService = projectionService ?? throw new ArgumentNullException(nameof(projectionService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>Ödeme listesinde gizli satır var mı; taşma satırı yalnız o zaman görünür.</summary>
    public bool HasHiddenPayments => HiddenPaymentCount > 0;

    /// <summary>
    /// Dönemi ilk günüyle 12 dönem zincirinden yükler (S75-1). Parametresiz çağrı ("Tekrar dene", sayfaya geri
    /// dönüş) son istenen dönemi yeniden yükler. Dönem zincirde yoksa ekran boştur (S75-2).
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync(DateOnly? requestedPeriodStart)
    {
        _requestedPeriodStart = requestedPeriodStart ?? _requestedPeriodStart;
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            var projection = await _projectionService.GetAsync();
            var period = projection?.Periods.FirstOrDefault(x => x.PeriodStart == _requestedPeriodStart);
            if (period is null)
            {
                ShowEmptyState();
                return;
            }

            ApplyFlow(period);
            ApplyPayments(period);
            ApplyCardInterest(period);
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PeriodDetailViewModel ERROR] {ex}");
            // Okuma ya da hesap hatası ekranda StateBlock (Hata) olarak görünür; "Tekrar dene" aynı dönemi yeniden yükler.
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Ödeme listesinin gizli satırlarını yerinde açar (GS21).</summary>
    [RelayCommand]
    public void ExpandPayments()
    {
        _isPaymentListExpanded = true;
        RefreshPayments();
    }

    /// <summary>Kart ekstresi satırının o kartın Kart Kontrol'ünü açması; ödeme şekli orada seçilir (S75-6).</summary>
    [RelayCommand]
    public Task OpenCardControlAsync(PeriodPaymentRow row) =>
        row.CardId is { } cardId
            ? _navigationService.NavigateToAsync(Routes.CardControl, new Dictionary<string, object> { [Routes.CardIdParameter] = cardId })
            : Task.CompletedTask;

    /// <summary>Boş hâlin aksiyonu: dönemin açıldığı 12 Dönem'e döner.</summary>
    [RelayCommand]
    public Task GoBackAsync() => _navigationService.NavigateBackAsync();

    // Akışın satırları dönem sonuna kuruşu kuruşuna iner: başı + gelir − ödemeler − yaşam gideri − KMH faizi (S75-3).
    private void ApplyFlow(CashFlowPeriodProjection period)
    {
        PeriodStart = period.PeriodStart;
        PeriodLastDay = period.PeriodEnd.AddDays(-1); // PeriodEnd sonraki dönemin ilk günü (S72-7 ile aynı)
        EndingBalance = period.EndingBalance;
        NetChange = period.EndingBalance - period.OpeningBalance;
        IsNetChangeNegative = NetChange < 0m;
        IsNetChangePositive = NetChange > 0m;
        OpeningBalance = period.OpeningBalance;
        IsOpeningNegative = period.OpeningBalance < 0m;
        Income = period.TotalIncome;
        // Büyük harcama ayrı satır değil: listede adıyla durur, "Ödemeler" listenin notuyla aynı toplamı söyler.
        PaymentsTotal = period.MandatoryOutflow + period.PlannedLargeCashExpenses;
        VariableExpenseAllowance = period.VariableExpenseAllowance;
        DeficitInterest = period.DeficitFinancingInterest;
        HasDeficitInterest = period.DeficitFinancingInterest > 0m;
    }

    private void ApplyPayments(CashFlowPeriodProjection period)
    {
        _allPayments = PaymentRowsOf(period);
        _isPaymentListExpanded = false;
        PaymentCount = _allPayments.Count;
        HasPayments = PaymentCount > 0;
        RefreshPayments();
    }

    // Zorunlu kalemler, büyük harcamalar ve tutarı belirlenemeyen kart ekstreleri (S75-4). Kart ödemesi kartın
    // kimliğini PaymentId'de taşır; belirlenemeyen ekstrenin ödeme kalemi yoktur, satırı ekstre durumundan kurulur.
    private static List<PeriodPaymentRow> PaymentRowsOf(CashFlowPeriodProjection period)
    {
        var rows = period.MandatoryItems.Select(item => new PeriodPaymentRow
        {
            DueDate = item.DueDate, Name = item.Name, Amount = item.Amount, IsEstimate = item.IsEstimate,
            CardId = item.Type == ObligationType.CreditCard ? item.PaymentId : null
        }).ToList();
        rows.AddRange(period.LargeExpenseItems.Select(expense =>
            new PeriodPaymentRow { DueDate = expense.ExactDate, Name = expense.Name, Amount = expense.Amount }));
        rows.AddRange(period.CardPaymentStatuses.Where(card => card.Payment is null).Select(card =>
            new PeriodPaymentRow { DueDate = card.PaymentDueDate, Name = card.CardName, CardId = card.CardId }));
        return rows.OrderBy(x => x.DueDate).ThenBy(x => x.Name, StringComparer.Ordinal).ToList();
    }

    private void RefreshPayments()
    {
        Payments.Clear();
        foreach (var row in _isPaymentListExpanded ? _allPayments : _allPayments.Take(CollapsedPaymentLimit))
        {
            Payments.Add(row);
        }

        HiddenPaymentCount = _allPayments.Count - Payments.Count;
    }

    // Kart faizi borca eklenir, akışta yoktur (S75-5); toplam 12 Dönem'in faiz toplamıyla aynı kaynaktan.
    private void ApplyCardInterest(CashFlowPeriodProjection period)
    {
        CardInterests.Clear();
        foreach (var card in period.CardPaymentStatuses.Where(x => x.CarryInterest > 0m))
        {
            CardInterests.Add(new PeriodCardInterestRow(card.CardName, card.CarryInterest));
        }

        CardInterestTotal = period.CardInterestGenerated;
        HasCardInterest = CardInterests.Count > 0;
    }

    private void ShowEmptyState()
    {
        _allPayments = [];
        Payments.Clear();
        CardInterests.Clear();
        PeriodStart = PeriodLastDay = null;
        EndingBalance = NetChange = OpeningBalance = Income = PaymentsTotal = null;
        VariableExpenseAllowance = DeficitInterest = CardInterestTotal = null;
        IsNetChangeNegative = IsNetChangePositive = IsOpeningNegative = HasDeficitInterest = false;
        HasPayments = HasCardInterest = false;
        PaymentCount = HiddenPaymentCount = 0;
        State = ScreenState.Empty;
    }
}
