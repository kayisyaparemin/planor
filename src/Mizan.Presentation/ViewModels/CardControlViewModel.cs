using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kart kontrol ekranının görünüm modelidir: hangi kartın açık olduğunu, kartın döngüsünü ve
/// limitini sunar; sıradaki ödeme <see cref="NextPaymentViewModel"/>, sonraki ödemeler ve
/// varsayılan ödeme şekli <see cref="UpcomingPaymentsViewModel"/>, ekstre girişi
/// <see cref="StatementEntryViewModel"/> çocuğundadır (EK-V7, S61).
/// </summary>
public sealed partial class CardControlViewModel : ViewModelBase
{
    // Sıradaki ödeme + dört sonraki vade; sıfır tutarlı vadeler atlandığı için pay bırakılır.
    private const int ProjectedStatementCount = 8;
    private const string SelectCardTitle = "Kart seç";
    private const string CancelText = "Vazgeç";

    private readonly ICreditCardRepository _cardRepository;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly CreditCardStatementCalculator _calculator;
    private readonly IDialogService _dialogService;
    private IReadOnlyList<CreditCard> _cards = [];
    private CreditCard? _card;
    private CreditCardStatementProjection? _entryDefaults;

    [ObservableProperty] private Guid cardId;
    [ObservableProperty] private string cardName = string.Empty;
    [ObservableProperty] private string bankName = string.Empty;
    [ObservableProperty] private int statementClosingDay;
    [ObservableProperty] private int paymentDueDay;
    [ObservableProperty] private decimal limit;
    [ObservableProperty] private decimal availableLimit;
    [ObservableProperty] private bool hasMultipleCards;
    [ObservableProperty] private bool hasStatement;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPaymentView), nameof(IsNoPaymentView))]
    private bool hasUpcomingPayment;

    /// <summary>Sıradaki ödeme ve kararı.</summary>
    public NextPaymentViewModel NextPayment { get; }

    /// <summary>Sonraki ödemeler ve kartın varsayılan ödeme şekli.</summary>
    public UpcomingPaymentsViewModel Upcoming { get; }

    /// <summary>Kesilmiş ekstreyi elle giren ya da düzenleyen form.</summary>
    public StatementEntryViewModel Entry { get; }

    /// <summary>Sıradaki ödeme kartı ödemeyi mi gösteriyor (form kapalıyken).</summary>
    public bool IsPaymentView => HasUpcomingPayment && !Entry.IsOpen;

    /// <summary>Sıradaki ödeme kartı "önümüzdeki ödeme yok" hâlinde mi (form kapalıyken).</summary>
    public bool IsNoPaymentView => !HasUpcomingPayment && !Entry.IsOpen;

    /// <summary>Görünüm modelini beş dar portla başlatır (Kural M3).</summary>
    public CardControlViewModel(
        ICreditCardRepository cardRepository, ICreditCardObligationService cardService,
        IUserSettingsRepository settingsRepository, CreditCardStatementCalculator calculator,
        IDialogService dialogService)
    {
        ArgumentNullException.ThrowIfNull(cardService);
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        NextPayment = new NextPaymentViewModel(cardService, dialogService, () => ReloadAsync(CardId));
        Upcoming = new UpcomingPaymentsViewModel(cardService, dialogService, () => ReloadAsync(CardId));
        Entry = new StatementEntryViewModel(cardService, dialogService, () => ReloadAsync(CardId));
        Entry.PropertyChanged += OnEntryPropertyChanged;
    }

    /// <summary>
    /// Kartı yükler. Kimlik verilmezse açık kart yenilenir; ilk açılışta ya da kart
    /// bulunamazsa en yakın ödemesi olan karta, kart yoksa boş duruma düşülür.
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync(Guid? targetCardId)
    {
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            var target = targetCardId ?? (CardId == Guid.Empty ? null : CardId);
            State = await ReloadAsync(target) ? ScreenState.Content : ScreenState.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CardControlViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Birden çok kart varsa hangi kartın açılacağını sorar.</summary>
    [RelayCommand]
    private async Task SelectCardAsync()
    {
        var names = _cards.Select(c => string.IsNullOrWhiteSpace(c.Name) ? c.Bank : c.Name).ToArray();
        var index = Array.IndexOf(names, await _dialogService.ChooseAsync(SelectCardTitle, CancelText, null, names));
        if (index < 0)
        {
            return;
        }

        Entry.CancelCommand.Execute(null);
        await LoadAsync(_cards[index].Id);
    }

    /// <summary>Ekstre giriş formunu açar; tarihler sıradaki vadeden dolar.</summary>
    [RelayCommand]
    private void StartStatementEntry()
    {
        if (_card is null)
        {
            return;
        }

        var closeDate = _entryDefaults?.StatementCloseDate ?? _card.BalanceAsOfDate;
        Entry.Open(_card, closeDate, _entryDefaults?.PaymentDueDate ?? closeDate);
    }

    private async Task<bool> ReloadAsync(Guid? targetCardId)
    {
        _cards = await _cardRepository.GetCreditCardsAsync();
        HasMultipleCards = _cards.Count > 1;
        var rate = (await _settingsRepository.GetSettingsAsync()).CreditCardCarryInterestRate;
        var projected = _cards.Select(c => (Card: c, Projections: _calculator.Project(c, ProjectedStatementCount, true, rate))).ToList();
        var selected = projected.FirstOrDefault(p => p.Card.Id == targetCardId);
        if (selected.Card is null)
        {
            selected = projected.OrderBy(p => NextPaymentViewModel.NextPaymentDate(p.Projections)).FirstOrDefault();
        }

        _card = selected.Card;
        if (_card is null)
        {
            CardId = Guid.Empty;
            return false;
        }

        ApplyCard(_card);
        ApplyPayments(_card, selected.Projections);
        return true;
    }

    private void ApplyPayments(CreditCard card, IReadOnlyList<CreditCardStatementProjection> projections)
    {
        var index = NextPaymentViewModel.FindPaymentIndex(projections);
        HasUpcomingPayment = index >= 0;
        _entryDefaults = projections.Count > 0 ? projections[Math.Max(index, 0)] : null;
        if (index >= 0)
        {
            NextPayment.Apply(card, projections[index], index + 1 < projections.Count ? projections[index + 1] : null);
        }

        Upcoming.Apply(card, projections.Skip(index + 1));
    }

    private void ApplyCard(CreditCard card)
    {
        CardId = card.Id;
        CardName = card.Name;
        BankName = card.Bank;
        StatementClosingDay = card.StatementClosingDay;
        PaymentDueDay = card.PaymentDueDay;
        Limit = card.Limit;
        AvailableLimit = card.AvailableLimit;
        HasStatement = card.CurrentStatement is not null;
    }

    private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StatementEntryViewModel.IsOpen))
        {
            OnPropertyChanged(nameof(IsPaymentView));
            OnPropertyChanged(nameof(IsNoPaymentView));
        }
    }
}
