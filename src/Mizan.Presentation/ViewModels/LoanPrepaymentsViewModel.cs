using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kredi formunun çocuğu: kredinin planlı erken ödemelerini (simülatörden uygulananlar dahil) o gün
/// ödenecek tutarlarıyla listeler, yenisini ekler ve satıra dokununca siler. Tutarlar formun o anki
/// taslak kredisinden kaydın kurallarıyla canlı hesaplanır; değişiklikler krediyle birlikte
/// kaydedilir (EK-V6c, S64-6, S64-12–15).
/// </summary>
public sealed partial class LoanPrepaymentsViewModel : ObservableObject
{
    /// <summary>Liste kapalıyken gösterilen en fazla satır sayısı (GS12, GS21).</summary>
    public const int CollapsedRowLimit = 4;

    private const string AddFailedTitle = "Erken ödeme eklenemedi";
    private const string InvalidAmountMessage = "Anaparadan düşecek tutarı sayı olarak gir.";
    private const string PastDateMessage = "Ödeme tarihi bugünden önce olamaz.";
    private const string MissingTermsMessage = "Önce aylık taksiti ve kalan taksiti gir.";
    private const string CancelText = "Vazgeç";
    private const string DeleteText = "Sil";

    // Silme diyaloğunun başlığı; ekrandaki satırla aynı ad (App'teki ErkenOdemeTuruConverter).
    private static readonly Dictionary<LoanPrepaymentMode, string> ModeTitles = new()
    {
        [LoanPrepaymentMode.FullClosure] = "Tamamen kapatma",
        [LoanPrepaymentMode.ReduceTerm] = "Ara ödeme · vade kısalır",
        [LoanPrepaymentMode.ReduceInstallment] = "Ara ödeme · taksit düşer"
    };

    private readonly IObligationManagementService _service;
    private readonly IDialogService _dialogService;
    private readonly IClock _clock;
    private List<LoanPrepayment> _prepayments = [];
    private Guid[] _loadedIds = [];
    private Loan? _terms;
    private bool _isExpanded;

    [ObservableProperty] private bool hasItems;
    [ObservableProperty] private bool isEntryOpen;
    [ObservableProperty] private DateOnly entryDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    private int hiddenCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPartial))]
    private LoanPrepaymentMode entryMode;

    [ObservableProperty] private string amountInput = string.Empty;

    /// <summary>Çocuğu kredi yazma portunun önizlemesi, diyalog servisi ve saatle başlatır.</summary>
    public LoanPrepaymentsViewModel(IObligationManagementService service, IDialogService dialogService, IClock clock)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Ekranda görünen satırlar; tarihe göre sıralı.</summary>
    public ObservableCollection<LoanPrepaymentRow> Items { get; } = [];

    /// <summary>Şekil seçicisinin seçenekleri; görünen adlar App'teki çeviricidedir.</summary>
    public IReadOnlyList<LoanPrepaymentMode> Modes { get; } = Enum.GetValues<LoanPrepaymentMode>();

    /// <summary>Seçilen şekil ara ödemeyse anaparadan düşecek tutar sorulur.</summary>
    public bool IsPartial => EntryMode != LoanPrepaymentMode.FullClosure;

    /// <summary>Gizli satır varsa taşma satırı görünür.</summary>
    public bool HasOverflow => HiddenCount > 0;

    /// <summary>Ödeme tarihi seçicisinin en erken günü: bugün.</summary>
    public DateOnly MinimumDate => _clock.Today;

    /// <summary>Liste yüklendiği andan beri değişti mi.</summary>
    public bool HasChanges => !_prepayments.Select(p => p.Id).Order().SequenceEqual(_loadedIds);

    /// <summary>Kredinin kayıtlı erken ödemelerini yükler; giriş kapanır, liste daralır.</summary>
    public void Load(IEnumerable<LoanPrepayment> prepayments)
    {
        ArgumentNullException.ThrowIfNull(prepayments);
        _prepayments = prepayments.ToList();
        _loadedIds = _prepayments.Select(p => p.Id).Order().ToArray();
        _isExpanded = false;
        IsEntryOpen = false;
        Rebuild();
    }

    /// <summary>Formun taslak kredisi değişince tutarları yeniden hesaplar; taslak null ise hesaplanmaz.</summary>
    public void Refresh(Loan? terms)
    {
        _terms = terms;
        Rebuild();
    }

    /// <summary>Listedeki erken ödemeleri krediyle birlikte kaydedilecek biçimde döner.</summary>
    public IReadOnlyList<LoanPrepayment> ToPrepayments() => _prepayments.ToArray();

    /// <summary>Giriş bloğunu varsayılanlarla açar: tamamen kapatma, bugünden sonraki ilk taksit günü.</summary>
    [RelayCommand]
    private void OpenEntry()
    {
        EntryMode = LoanPrepaymentMode.FullClosure;
        EntryDate = DefaultDate();
        AmountInput = string.Empty;
        IsEntryOpen = true;
    }

    /// <summary>Giriş bloğunu eklemeden kapatır.</summary>
    [RelayCommand]
    private void CancelEntry() => IsEntryOpen = false;

    /// <summary>Girişi kaydın kurallarıyla doğrular ve erken ödemeyi listeye ekler.</summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        decimal? principal = null;
        if (IsPartial)
        {
            if (!StatementEntryViewModel.TryParseAmount(AmountInput, out var amount) || amount <= 0m)
            {
                await _dialogService.ShowAlertAsync(AddFailedTitle, InvalidAmountMessage);
                return;
            }

            principal = amount;
        }

        var candidate = new LoanPrepayment { Date = EntryDate, Mode = EntryMode, PrincipalAmount = principal };
        var error = EntryDate < _clock.Today ? PastDateMessage
            : _terms is null ? MissingTermsMessage
            : _service.ValidateLoanPrepayment(_terms, _prepayments, candidate);
        if (error is not null)
        {
            await _dialogService.ShowAlertAsync(AddFailedTitle, error);
            return;
        }

        _prepayments.Add(candidate);
        IsEntryOpen = false;
        Rebuild();
    }

    /// <summary>Satıra dokununca silmeyi sorar; silme krediyle birlikte kaydedilir.</summary>
    [RelayCommand]
    private async Task SelectAsync(LoanPrepaymentRow? row)
    {
        if (row is not null && await _dialogService.ChooseAsync(ModeTitles[row.Mode], CancelText, DeleteText) == DeleteText)
        {
            _prepayments.RemoveAll(p => p.Id == row.Id);
            Rebuild();
        }
    }

    /// <summary>Gizli satırları da gösterir.</summary>
    [RelayCommand]
    private void Expand()
    {
        _isExpanded = true;
        Rebuild();
    }

    // S64-15: bugünden sonraki ilk taksit günü; kapatılacak başka taksit yoksa ya da taslak yoksa bugün.
    private DateOnly DefaultDate()
    {
        var today = _clock.Today;
        return _terms is { RemainingInstallmentCount: > 1 } terms
            ? Enumerable.Range(0, terms.RemainingInstallmentCount)
                .Select(i => CalendarRules.AddMonthsKeepingDay(terms.NextPaymentDate, i, terms.PaymentDay))
                .FirstOrDefault(date => date >= today, today)
            : today;
    }

    // Tutarlar her seferinde taslak krediyle kaydın kurallarından gelir (S64-14); taslak yoksa "—".
    private void Rebuild()
    {
        _prepayments = _prepayments.OrderBy(p => p.Date).ToList();
        var amounts = _terms is null
            ? null
            : _service.PreviewLoanPrepayments(_terms, _prepayments).ToDictionary(x => x.Prepayment.Id, x => x.Amount);
        var rows = _prepayments.Select(p => new LoanPrepaymentRow(p.Id, p.Mode, p.Date, p.PrincipalAmount, amounts?.GetValueOrDefault(p.Id)));

        Items.Clear();
        foreach (var row in _isExpanded ? rows : rows.Take(CollapsedRowLimit))
        {
            Items.Add(row);
        }

        HasItems = Items.Count > 0;
        HiddenCount = _prepayments.Count - Items.Count;
    }
}
