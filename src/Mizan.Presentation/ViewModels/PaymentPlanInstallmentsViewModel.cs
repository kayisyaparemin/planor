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
/// Ödeme planı formunun çocuğu: taksitleri listeler, yeni taksit veya aylık taksit serisi
/// ekler, ödenmemiş taksiti siler (EK-V6e, S65-3).
/// </summary>
public sealed partial class PaymentPlanInstallmentsViewModel : ObservableObject
{
    /// <summary>Liste kapalıyken gösterilen en fazla satır sayısı (GS21).</summary>
    public const int CollapsedRowLimit = 4;
    private const int MaxInstallmentCount = 120;
    private const string AddFailedTitle = "Taksit eklenemedi";
    private const string InvalidAmountMessage = "Taksit tutarını sıfırdan büyük bir sayı olarak gir.";
    private const string InvalidCountMessage = "Taksit sayısını 1 ile 120 arasında bir tam sayı olarak gir.";
    private const string PastDateMessage = "İlk taksit tarihi bugünden önce olamaz.";
    private const string PaidInstallmentTitle = "Ödenmiş taksit";
    private const string PaidInstallmentMessage = "Ödenmiş taksitler silinemez.";
    private const string RemoveTitle = "Taksit";
    private const string CancelText = "Vazgeç";
    private const string DeleteText = "Sil";

    private readonly IDialogService _dialogService;
    private readonly IClock _clock;
    private readonly List<PaymentInstallmentRow> _rows = [];
    private List<PaymentInstallmentRow> _loadedRows = [];
    private bool _isExpanded;

    [ObservableProperty] private bool hasItems;
    [ObservableProperty] private bool isEntryOpen;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private string countInput = "1";
    [ObservableProperty] private DateOnly entryDate;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasOverflow))] private int hiddenCount;

    /// <summary>Çocuğu diyalog servisi ve saatle başlatır.</summary>
    public PaymentPlanInstallmentsViewModel(IDialogService dialogService, IClock clock)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>Ekranda görünen taksit satırları.</summary>
    public ObservableCollection<PaymentInstallmentRow> Items { get; } = [];
    /// <summary>Gizli satır varsa taşma satırı görünür (GS21).</summary>
    public bool HasOverflow => HiddenCount > 0;
    /// <summary>İlk taksit tarihi seçicisinin en erken günü: bugün.</summary>
    public DateOnly MinimumDate => _clock.Today;
    /// <summary>Taksitler yüklendiği andan beri değişti mi.</summary>
    public bool HasChanges => !_rows.SequenceEqual(_loadedRows);
    /// <summary>Listedeki toplam taksit adedi.</summary>
    public int TotalCount => _rows.Count;

    /// <summary>Planın kayıtlı taksitlerini yükler.</summary>
    public void Load(IEnumerable<TemporaryPaymentInstallment> installments)
    {
        ArgumentNullException.ThrowIfNull(installments);
        _rows.Clear();
        _rows.AddRange(installments.OrderBy(x => x.DueDate).Select(x => new PaymentInstallmentRow(x.Id, x.DueDate, x.Amount, x.IsPaid)));
        _loadedRows = [.. _rows];
        _isExpanded = false;
        IsEntryOpen = _rows.Count == 0;
        AmountInput = string.Empty;
        CountInput = "1";
        EntryDate = _clock.Today;
        Refresh();
    }

    /// <summary>Listedeki taksitleri domain modeli biçiminde döner.</summary>
    public IReadOnlyList<TemporaryPaymentInstallment> ToInstallments(Guid planId) =>
        _rows.Select(r => new TemporaryPaymentInstallment
        {
            Id = r.Id, PlanId = planId, DueDate = r.DueDate, Amount = r.Amount, IsPaid = r.IsPaid
        }).ToArray();

    /// <summary>Giriş bloğunu varsayılanlarla açar.</summary>
    [RelayCommand]
    private void OpenEntry()
    {
        AmountInput = string.Empty;
        CountInput = "1";
        EntryDate = _clock.Today;
        IsEntryOpen = true;
    }

    /// <summary>Giriş bloğunu kapatır.</summary>
    [RelayCommand]
    private void CancelEntry() => IsEntryOpen = false;

    /// <summary>Girişi doğrular ve taksit sayısı kadar aylık taksiti listeye ekler.</summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        var error = ValidateCandidate(out var amount, out var count);
        if (error is not null)
        {
            await _dialogService.ShowAlertAsync(AddFailedTitle, error);
            return;
        }

        AddGeneratedInstallments(amount, count, EntryDate);
        IsEntryOpen = false;
        Refresh();
    }

    /// <summary>Açık giriş bloğundaki veriyi kaydetmeden önce otomatik eklemeyi dener.</summary>
    public bool TryCommitPendingEntry(out string? error)
    {
        error = null;
        if (!IsEntryOpen || string.IsNullOrWhiteSpace(AmountInput))
        {
            return true;
        }

        error = ValidateCandidate(out var amount, out var count);
        if (error is not null)
        {
            return false;
        }

        AddGeneratedInstallments(amount, count, EntryDate);
        IsEntryOpen = false;
        Refresh();
        return true;
    }

    [RelayCommand]
    private async Task SelectAsync(PaymentInstallmentRow? row)
    {
        if (row is null)
        {
            return;
        }

        if (row.IsPaid)
        {
            await _dialogService.ShowAlertAsync(PaidInstallmentTitle, PaidInstallmentMessage);
            return;
        }

        if (await _dialogService.ChooseAsync(RemoveTitle, CancelText, DeleteText) == DeleteText)
        {
            _rows.Remove(row);
            Refresh();
        }
    }

    [RelayCommand]
    private void Expand()
    {
        _isExpanded = true;
        Refresh();
    }

    private string? ValidateCandidate(out decimal amount, out int count)
    {
        count = 1;
        var hasAmount = StatementEntryViewModel.TryParseAmount(AmountInput, out amount) && amount > 0m;
        var hasCount = int.TryParse(CountInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out count) &&
                       count is >= 1 and <= MaxInstallmentCount;

        if (!hasAmount) { return InvalidAmountMessage; }
        if (!hasCount) { return InvalidCountMessage; }
        if (EntryDate < _clock.Today) { return PastDateMessage; }
        return null;
    }

    private void AddGeneratedInstallments(decimal amount, int count, DateOnly startDate)
    {
        var generated = Enumerable.Range(0, count).Select(i => new PaymentInstallmentRow(
            Guid.NewGuid(),
            CalendarRules.AddMonthsKeepingDay(startDate, i, startDate.Day),
            amount,
            false));
        _rows.AddRange(generated);
        _rows.Sort((a, b) => a.DueDate.CompareTo(b.DueDate));
    }

    private void Refresh()
    {
        Items.Clear();
        foreach (var row in _isExpanded ? _rows : _rows.Take(CollapsedRowLimit)) { Items.Add(row); }
        HasItems = Items.Count > 0;
        HiddenCount = _rows.Count - Items.Count;
    }
}
