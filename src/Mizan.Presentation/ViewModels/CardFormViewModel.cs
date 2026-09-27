using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kart formunun görünüm modelidir: kartı ekler ya da düzenler, kaydedip listeye döner, kaydedilmemiş
/// değişiklikte çıkmadan önce onay sorar. Kartın tanımı <see cref="CardDefinitionViewModel"/>
/// çocuğundadır; ekstre ve ödeme kararları kart kontroldedir (EK-V6b, S63).
/// </summary>
public sealed partial class CardFormViewModel : ViewModelBase
{
    private const string SaveFailedTitle = "Kart kaydedilemedi";
    private const string NotFoundTitle = "Kart bulunamadı";
    private const string NotFoundMessage = "Kart silinmiş olabilir.";
    private const string UnexpectedErrorMessage = "Kart kaydedilirken bir sorun oluştu. Tekrar dene.";
    private const string DiscardTitle = "Kaydetmeden çık";
    private const string DiscardMessage = "Yaptığın değişiklikler kaydedilmeyecek.";
    private const string DiscardAccept = "Çık";
    private const string DiscardCancel = "Kal";

    private readonly ICreditCardRepository _cardRepository;
    private readonly ICreditCardObligationService _cardService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IClock _clock;
    private Guid? _requestedCardId;
    private CreditCard? _card;

    [ObservableProperty] private bool isEditing;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public CardFormViewModel(
        ICreditCardRepository cardRepository, ICreditCardObligationService cardService,
        INavigationService navigationService, IDialogService dialogService, IClock clock)
    {
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        Charges = new CardChargesViewModel(dialogService, clock);
    }

    /// <summary>Kartın tanımı: ad, banka, limit, kesim ve son ödeme günü, güncel borç.</summary>
    public CardDefinitionViewModel Fields { get; } = new();

    /// <summary>Karta henüz yansımamış gelecek harcamalar (taksitler).</summary>
    public CardChargesViewModel Charges { get; }

    /// <summary>Form açıldığından beri bir alan ya da harcama listesi değişti mi.</summary>
    public bool HasChanges => Fields.HasChanges || Charges.HasChanges;

    /// <summary>Kimlik verilmezse boş yeni kart formu, verilirse o kartın düzenlemesi açılır.</summary>
    [RelayCommand]
    public async Task LoadAsync(Guid? cardId)
    {
        _requestedCardId = cardId;
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            _card = cardId is { } id ? (await _cardRepository.GetCreditCardsAsync()).FirstOrDefault(c => c.Id == id) : null;
            if (cardId is not null && _card is null)
            {
                await _dialogService.ShowAlertAsync(NotFoundTitle, NotFoundMessage);
                await _navigationService.NavigateBackAsync();
                return;
            }

            IsEditing = _card is not null;
            Fields.Fill(_card);
            Charges.Load(_card?.Charges ?? []);
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CardFormViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Okuma hatasından sonra aynı kartı (ya da yeni kart formunu) yeniden yükler.</summary>
    [RelayCommand]
    private Task RetryAsync() => LoadAsync(_requestedCardId);

    /// <summary>Formu doğrular, kartı kaydeder ve bir önceki sayfaya döner.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (Fields.TryBuild(_card, _clock.Today, out var card) is { } error)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, error);
            return;
        }

        SetBusy(true);
        try
        {
            await _cardService.SaveCreditCardAsync(card! with { Charges = Charges.ToCharges() });
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CardFormViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Kaydedilmemiş değişiklik varsa onay alıp bir önceki sayfaya döner.</summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        if (HasChanges && !await _dialogService.ConfirmAsync(DiscardTitle, DiscardMessage, DiscardAccept, DiscardCancel))
        {
            return;
        }

        await _navigationService.NavigateBackAsync();
    }
}
