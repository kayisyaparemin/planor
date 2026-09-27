using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kart formunun çocuğu: karta henüz yansımamış gelecek harcamaları (taksitler) listeler, aylık
/// tutar × taksit sayısıyla ekler ve satıra dokununca siler. Değişiklikler kartla birlikte
/// kaydedilir (EK-V6b, S61-7, S63-5).
/// </summary>
public sealed partial class CardChargesViewModel : ObservableObject
{
    /// <summary>Liste kapalıyken gösterilen en fazla satır sayısı (GS12, GS21).</summary>
    public const int CollapsedRowLimit = 4;

    private const int MaxInstallmentCount = 120;
    private const string AddFailedTitle = "Harcama eklenemedi";
    private const string MissingDescriptionMessage = "Harcamaya bir ad ver.";
    private const string InvalidAmountMessage = "Aylık tutarı sayı olarak gir.";
    private const string InvalidCountMessage = "Taksit sayısı 1 ile 120 arasında olmalı.";
    private const string PastDateMessage = "İlk taksit tarihi bugünden önce olamaz.";
    private const string CancelText = "Vazgeç";
    private const string DeleteText = "Sil";

    private readonly IDialogService _dialogService;
    private readonly IClock _clock;
    private List<CardChargeRow> _rows = [];
    private Guid[] _loadedIds = [];
    private bool _isExpanded;

    [ObservableProperty] private bool hasItems;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    private int hiddenCount;

    [ObservableProperty] private bool isEntryOpen;
    [ObservableProperty] private string descriptionInput = string.Empty;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private string countInput = string.Empty;
    [ObservableProperty] private DateOnly firstDate;

    /// <summary>Çocuğu diyalog servisi ve saatle başlatır.</summary>
    public CardChargesViewModel(IDialogService dialogService, IClock clock)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Ekranda görünen satırlar; tarihe göre sıralı.</summary>
    public ObservableCollection<CardChargeRow> Items { get; } = [];

    /// <summary>Gizli satır varsa taşma satırı görünür.</summary>
    public bool HasOverflow => HiddenCount > 0;

    /// <summary>İlk taksit tarihi seçicisinin en erken günü: bugün.</summary>
    public DateOnly MinimumDate => _clock.Today;

    /// <summary>Liste yüklendiği andan beri değişti mi.</summary>
    public bool HasChanges => !_rows.Select(r => r.Id).Order().SequenceEqual(_loadedIds);

    /// <summary>Kartın kayıtlı harcamalarını yükler; giriş kapanır, liste daralır.</summary>
    public void Load(IEnumerable<CardCharge> charges)
    {
        ArgumentNullException.ThrowIfNull(charges);
        _rows = charges.Select(c => new CardChargeRow(c.Id, c.Description, c.PostingDate, c.Amount)).ToList();
        _loadedIds = _rows.Select(r => r.Id).Order().ToArray();
        _isExpanded = false;
        IsEntryOpen = false;
        Refresh();
    }

    /// <summary>Listedeki harcamaları kartla birlikte kaydedilecek biçimde döner.</summary>
    public IReadOnlyList<CardCharge> ToCharges() =>
        _rows.Select(r => new CardCharge { Id = r.Id, Description = r.Description, PostingDate = r.PostingDate, Amount = r.Amount })
            .ToArray();

    /// <summary>Giriş bloğunu varsayılanlarla açar: tek taksit, ilk tarih bir ay sonra.</summary>
    [RelayCommand]
    private void OpenEntry()
    {
        DescriptionInput = string.Empty;
        AmountInput = string.Empty;
        CountInput = "1";
        FirstDate = _clock.Today.AddMonths(1);
        IsEntryOpen = true;
    }

    /// <summary>Giriş bloğunu eklemeden kapatır.</summary>
    [RelayCommand]
    private void CancelEntry() => IsEntryOpen = false;

    /// <summary>Girişi doğrular ve taksit sayısı kadar aylık harcamayı listeye ekler.</summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        var description = DescriptionInput.Trim();
        var hasAmount = StatementEntryViewModel.TryParseAmount(AmountInput, out var amount) && amount > 0m;
        var hasCount = int.TryParse(CountInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) &&
                       count is >= 1 and <= MaxInstallmentCount;
        var error = description.Length == 0 ? MissingDescriptionMessage
            : !hasAmount ? InvalidAmountMessage
            : !hasCount ? InvalidCountMessage
            : FirstDate < _clock.Today ? PastDateMessage
            : null;
        if (error is not null)
        {
            await _dialogService.ShowAlertAsync(AddFailedTitle, error);
            return;
        }

        // S63-5: her ay aynı tutar; ay sonu günleri ayın son gününe kenetlenir.
        _rows.AddRange(Enumerable.Range(0, count).Select(i => new CardChargeRow(
            Guid.NewGuid(),
            count == 1 ? description : $"{description} ({i + 1}/{count})",
            CalendarRules.AddMonthsKeepingDay(FirstDate, i, FirstDate.Day),
            amount)));
        IsEntryOpen = false;
        Refresh();
    }

    /// <summary>Satıra dokununca silmeyi sorar; silme kartla birlikte kaydedilir.</summary>
    [RelayCommand]
    private async Task SelectAsync(CardChargeRow? row)
    {
        if (row is not null && await _dialogService.ChooseAsync(row.Description, CancelText, DeleteText) == DeleteText)
        {
            _rows.Remove(row);
            Refresh();
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
        _rows = _rows.OrderBy(r => r.PostingDate).ToList();
        Items.Clear();
        foreach (var row in _isExpanded ? _rows : _rows.Take(CollapsedRowLimit))
        {
            Items.Add(row);
        }

        HasItems = Items.Count > 0;
        HiddenCount = _rows.Count - Items.Count;
    }
}
