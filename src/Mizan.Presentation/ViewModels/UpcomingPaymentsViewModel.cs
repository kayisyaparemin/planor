using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kart kontrol ekranının çocuğu: sıradaki ödemeden sonraki vadeleri (EK-V7 S3) ve kartın ayrı
/// karar verilmeyen ekstreler için varsayılan ödeme şeklini (S4) sunar ve değiştirir.
/// </summary>
public sealed partial class UpcomingPaymentsViewModel : ObservableObject
{
    // GS12: ListCard en fazla dört satır.
    private const int VisibleLimit = 4;
    private const string DecideTitle = "Bu vade nasıl ödensin?";
    private const string DefaultTitle = "Ayrı karar vermediğin ekstreler";
    private const string FallbackTitle = "Karar verene kadar hesap neyi varsaysın?";
    private const string SaveFailedTitle = "Ödeme kaydedilemedi";
    private const string UnexpectedErrorMessage = "Ödeme kaydedilirken bir sorun oluştu. Tekrar dene.";
    private const string CancelText = "Vazgeç";
    private const string MinimumText = "Asgari";
    private const string FullText = "Tamamı";
    private const string AskText = "Her ekstrede sor";
    private const string ResetText = "Kartın varsayılanına dön";

    private readonly ICreditCardObligationService _cardService;
    private readonly IDialogService _dialogService;
    private readonly Func<Task> _onSaved;
    private CreditCard? _card;

    [ObservableProperty] private bool hasItems;
    [ObservableProperty] private CardPaymentDefault? defaultRule;

    /// <summary>Sıradaki ödemeden sonraki, tutarı sıfırdan büyük en fazla dört vade.</summary>
    public ObservableCollection<UpcomingPaymentItem> Items { get; } = [];

    /// <summary>Kaydetme portu, diyalog servisi ve kayıt sonrası geri çağrıyla başlatır.</summary>
    public UpcomingPaymentsViewModel(ICreditCardObligationService cardService, IDialogService dialogService, Func<Task> onSaved)
    {
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
    }

    /// <summary>Sıradaki ödemeden sonraki ekstre projeksiyonlarını ve kartın varsayılanını gösterir.</summary>
    public void Apply(CreditCard card, IEnumerable<CreditCardStatementProjection> following)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        Items.Clear();
        foreach (var projection in following.Where(p => p.StatementBalance > 0m).Take(VisibleLimit))
        {
            Items.Add(new UpcomingPaymentItem
            {
                DueDate = projection.PaymentDueDate,
                PaymentAmount = projection.Payment,
                StatementBalance = projection.StatementBalance,
                AppliedPaymentType = projection.AppliedPaymentType,
                Resolution = projection.PaymentResolution
            });
        }

        HasItems = Items.Count > 0;
        DefaultRule = new CardPaymentDefault(card.PaymentStrategy, card.ProjectionFallbackStrategy);
    }

    /// <summary>Bir vade için Asgari / Tamamı kararı verir ya da kartın varsayılanına döndürür.</summary>
    [RelayCommand]
    private async Task DecideAsync(UpcomingPaymentItem? item)
    {
        if (_card is null || item is null)
        {
            return;
        }

        string[] options = item.IsDueDateOverride ? [MinimumText, FullText, ResetText] : [MinimumText, FullText];
        var choice = await _dialogService.ChooseAsync(DecideTitle, CancelText, null, options);
        var cardId = _card.Id;
        Func<Task>? save = choice switch
        {
            MinimumText => () => _cardService.SetStatementPaymentModeAsync(cardId, item.DueDate, CreditCardPaymentType.Minimum),
            FullText => () => _cardService.SetStatementPaymentModeAsync(cardId, item.DueDate, CreditCardPaymentType.FullStatement),
            ResetText => () => _cardService.RemoveCreditCardPaymentPlanAsync(cardId, item.DueDate),
            _ => null
        };

        if (save is not null)
        {
            await RunSaveAsync(save);
        }
    }

    /// <summary>Kartın varsayılan ödeme şeklini (ve "her ekstrede sor"da varsayımı) değiştirir.</summary>
    [RelayCommand]
    private async Task ChangeDefaultAsync()
    {
        if (_card is null)
        {
            return;
        }

        CreditCardPaymentStrategy? strategy = await _dialogService.ChooseAsync(DefaultTitle, CancelText, null, MinimumText, FullText, AskText) switch
        {
            MinimumText => CreditCardPaymentStrategy.Minimum,
            FullText => CreditCardPaymentStrategy.FullStatement,
            AskText => CreditCardPaymentStrategy.AskEachStatement,
            _ => null
        };
        if (strategy is null)
        {
            return;
        }

        var card = _card with { PaymentStrategy = strategy.Value, FixedPaymentAmount = null };
        if (strategy == CreditCardPaymentStrategy.AskEachStatement)
        {
            ProjectionFallbackStrategy? fallback = await _dialogService.ChooseAsync(FallbackTitle, CancelText, null, MinimumText, FullText) switch
            {
                MinimumText => ProjectionFallbackStrategy.Minimum,
                FullText => ProjectionFallbackStrategy.FullStatement,
                _ => null
            };
            if (fallback is null)
            {
                return;
            }

            card = card with { ProjectionFallbackStrategy = fallback.Value, ProjectionFallbackFixedAmount = null };
        }

        await RunSaveAsync(() => _cardService.SaveCreditCardAsync(card));
    }

    private async Task RunSaveAsync(Func<Task> save)
    {
        try
        {
            await save();
            await _onSaved();
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UpcomingPaymentsViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
    }
}
