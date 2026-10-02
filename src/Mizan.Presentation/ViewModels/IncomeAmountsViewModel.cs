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
/// Gelir formunun çocuğu: gelirin bugün yürürlükteki tutarını ve ileri tarihli değişikliklerini
/// (simülatörden gelenler dahil) listeler, yeni tutarı tarihiyle ekler ve henüz yürürlüğe girmemiş
/// değişikliği satıra dokununca siler. Kurallar servisle ortak <see cref="IncomeAmountRules"/>'tadır;
/// değişiklikler gelirle birlikte kaydedilir (EK-V6d, S67-5).
/// </summary>
public sealed partial class IncomeAmountsViewModel : ObservableObject
{
    /// <summary>Liste kapalıyken gösterilen en fazla satır sayısı (GS12, GS21).</summary>
    public const int CollapsedRowLimit = 4;

    private const string AddFailedTitle = "Tutar eklenemedi";
    private const string InvalidAmountMessage = "Yeni tutarı sayı olarak gir.";
    private const string RemoveTitle = "Tutar değişikliği";
    private const string EffectiveTitle = "Yürürlükteki tutar";
    private const string EffectiveMessage = "Yürürlüğe girmiş tutar silinmez. Değiştirmek için yeni tutar ekle.";
    private const string CancelText = "Vazgeç";
    private const string DeleteText = "Sil";

    private readonly IDialogService _dialogService;
    private readonly IClock _clock;
    private List<IncomeAmountHistory> _amounts = [];
    private Guid[] _loadedIds = [];
    private int? _paymentDay;
    private bool _isExpanded;

    [ObservableProperty] private bool hasItems;
    [ObservableProperty] private bool isEntryOpen;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private DateOnly entryDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    private int hiddenCount;

    /// <summary>Çocuğu diyalog servisi ve saatle başlatır.</summary>
    public IncomeAmountsViewModel(IDialogService dialogService, IClock clock)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Ekranda görünen satırlar: yürürlükteki tutar ve ileri tarihliler, tarihe göre.</summary>
    public ObservableCollection<IncomeAmountRow> Items { get; } = [];

    /// <summary>Gizli satır varsa taşma satırı görünür.</summary>
    public bool HasOverflow => HiddenCount > 0;

    /// <summary>Geçerlilik tarihi seçicisinin en erken günü: bugün.</summary>
    public DateOnly MinimumDate => _clock.Today;

    /// <summary>Tutarlar yüklendiği andan beri bir tutar eklendi ya da silindi mi.</summary>
    public bool HasChanges => !_amounts.Select(x => x.Id).Order().SequenceEqual(_loadedIds);

    /// <summary>
    /// Gelirin kayıtlı tutarlarını yükler; gelir yoksa (yeni gelir) liste boştur. Giriş kapanır, liste daralır.
    /// </summary>
    public void Load(RecurringIncome? income, IEnumerable<IncomeAmountHistory> allAmounts)
    {
        ArgumentNullException.ThrowIfNull(allAmounts);
        _paymentDay = income?.PaymentDay;
        _amounts = income is null ? [] : allAmounts.Where(x => x.RecurringIncomeId == income.Id).ToList();
        _loadedIds = _amounts.Select(x => x.Id).Order().ToArray();
        _isExpanded = false;
        IsEntryOpen = false;
        Rebuild();
    }

    /// <summary>Listeye eklenmiş, henüz kaydedilmemiş tutarlar.</summary>
    public IReadOnlyList<IncomeAmountHistory> ToAdded() => _amounts.Where(x => !_loadedIds.Contains(x.Id)).ToArray();

    /// <summary>Kayıtlıyken listeden silinmiş tutarların kimlikleri.</summary>
    public IReadOnlyList<Guid> ToRemovedIds() => _loadedIds.Where(id => _amounts.All(x => x.Id != id)).ToArray();

    /// <summary>Giriş bloğunu boş tutar ve bugünden sonraki ilk ödeme günüyle açar (S67 V6d2 notları b).</summary>
    [RelayCommand]
    private void OpenEntry()
    {
        AmountInput = string.Empty;
        EntryDate = DefaultDate();
        IsEntryOpen = true;
    }

    /// <summary>Giriş bloğunu eklemeden kapatır.</summary>
    [RelayCommand]
    private void CancelEntry() => IsEntryOpen = false;

    /// <summary>Girişi tutar kurallarıyla doğrular ve tutarı listeye ekler; hata varsa giriş açık kalır.</summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        var candidate = StatementEntryViewModel.TryParseAmount(AmountInput, out var amount)
            ? new IncomeAmountHistory { Amount = amount, EffectiveDate = EntryDate }
            : null;
        var error = candidate is null
            ? InvalidAmountMessage
            : IncomeAmountRules.CheckAddition(_amounts, candidate, _clock.Today);
        if (error is not null)
        {
            await _dialogService.ShowAlertAsync(AddFailedTitle, error);
            return;
        }

        _amounts.Add(candidate!);
        IsEntryOpen = false;
        Rebuild();
    }

    /// <summary>
    /// Satıra dokununca henüz yürürlüğe girmemiş tutarın silinmesini sorar; yürürlüğe girmiş tutarda
    /// neden silinemediğini söyler. Silme gelirle birlikte kaydedilir.
    /// </summary>
    [RelayCommand]
    private async Task SelectAsync(IncomeAmountRow? row)
    {
        if (row is null)
        {
            return;
        }

        if (!row.IsRemovable)
        {
            await _dialogService.ShowAlertAsync(EffectiveTitle, EffectiveMessage);
            return;
        }

        if (await _dialogService.ChooseAsync(RemoveTitle, CancelText, DeleteText) == DeleteText)
        {
            _amounts.RemoveAll(x => x.Id == row.Id);
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

    // Ödeme günü bugünse sıradaki yatış gelecek aydadır; kısa ayda gün ay sonuna kenetlenir.
    private DateOnly DefaultDate()
    {
        var today = _clock.Today;
        if (_paymentDay is not { } day)
        {
            return today;
        }

        var thisMonth = CalendarRules.ResolveDay(today.Year, today.Month, day);
        return thisMonth > today ? thisMonth : CalendarRules.AddMonthsKeepingDay(thisMonth, 1, day);
    }

    private void Rebuild()
    {
        var today = _clock.Today;
        var rows = IncomeAmountRules.Upcoming(_amounts, today)
            .Select(x => new IncomeAmountRow(x.Id, x.EffectiveDate, x.Amount, IncomeAmountRules.IsRemovable(x, today)))
            .ToArray();

        Items.Clear();
        foreach (var row in _isExpanded ? rows : rows.Take(CollapsedRowLimit))
        {
            Items.Add(row);
        }

        HasItems = Items.Count > 0;
        HiddenCount = rows.Length - Items.Count;
    }
}
