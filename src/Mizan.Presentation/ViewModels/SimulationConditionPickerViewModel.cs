using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Simülatör ekranının "Ekle" seçicisi görünüm modelidir (EK-V10, S77 V10 notları i).
/// Dört gruptan simülatörün desteklediği türleri sunar ve var olan kayıt isteyen türlerde
/// (kartla harcama, kart ödeme şekli) kartı yerinde çözer.
/// </summary>
public sealed partial class SimulationConditionPickerViewModel : ViewModelBase
{
    private const string MissingCardTitle = "Kayıtlı kart bulunamadı";
    private const string MissingCardMessage = "Kartla işlem denemek için önce Finansal Yapı'dan bir kredi kartı eklemelisin.";
    private const string ReadFailedTitle = "Kayıtlar okunamadı";
    private const string ReadFailedMessage = "Kayıtların okunurken bir sorun oluştu. Tekrar dene.";

    private readonly INavigationService _navigationService;
    private readonly RecordCandidateResolver _candidateResolver;
    private readonly IDialogService _dialogService;
    private string? _pendingOptionKey;

    /// <summary>Hangi grubun kaydı soruluyor; null ise karolar görünür.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChoosing))]
    private RecordEntryGroup? choiceGroup;

    /// <summary>Görünüm modelini üç dar bağımlılıkla başlatır (Kural M3).</summary>
    public SimulationConditionPickerViewModel(
        INavigationService navigationService,
        RecordCandidateResolver candidateResolver,
        IDialogService dialogService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _candidateResolver = candidateResolver ?? throw new ArgumentNullException(nameof(candidateResolver));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>Ödeme / harcama seçenekleri bölümü.</summary>
    public EntryTypeSection PaymentSection { get; } = new(
        RecordEntryGroup.Payment,
        [
            new(SimulationScenarioCatalog.CashPayment.Key, 0),
            new(SimulationScenarioCatalog.RecurringPayment.Key, 1)
        ]);

    /// <summary>Kart seçenekleri bölümü.</summary>
    public EntryTypeSection CardSection { get; } = new(
        RecordEntryGroup.Card,
        [
            new(SimulationScenarioCatalog.CardSpending.Key, 0),
            new(SimulationScenarioCatalog.CardPaymentMode.Key, 1)
        ]);

    /// <summary>Sayfa ikinci seviyede mi ("Hangi kart?").</summary>
    public bool IsChoosing => ChoiceGroup is not null;

    /// <summary>İkinci seviyedeki aday kartlar.</summary>
    public ObservableCollection<FinancialRecordRow> Choices { get; } = [];

    /// <summary>
    /// Karoya dokunulduğunda seçeneğin formunu açar. Kart gerektiren türlerde önce aday kartlar çözülür.
    /// </summary>
    [RelayCommand]
    private async Task SelectOptionAsync(EntryTypeOptionItem? option)
    {
        if (option is null)
        {
            return;
        }

        var key = option.Key;
        if (key is "card" or "card-payment-mode")
        {
            var candidates = await _candidateResolver.GetCandidatesAsync(FinancialRecordKind.CreditCard);
            if (candidates is null)
            {
                await _dialogService.ShowAlertAsync(ReadFailedTitle, ReadFailedMessage);
                return;
            }

            switch (candidates)
            {
                case []:
                    await _dialogService.ShowAlertAsync(MissingCardTitle, MissingCardMessage);
                    return;
                case [var only]:
                    await OpenFormAsync(key, only.Id);
                    return;
                case var multiple:
                    _pendingOptionKey = key;
                    Choices.Clear();
                    foreach (var card in multiple)
                    {
                        Choices.Add(card);
                    }

                    ChoiceGroup = RecordEntryGroup.Card;
                    return;
            }
        }

        await OpenFormAsync(key, null);
    }

    /// <summary>İkinci seviyede seçilen kartla formu seçicinin yerine açar.</summary>
    [RelayCommand]
    private Task ChooseRecordAsync(FinancialRecordRow? row) =>
        row is not null && _pendingOptionKey is not null
            ? OpenFormAsync(_pendingOptionKey, row.Id)
            : Task.CompletedTask;

    /// <summary>Geri: ikinci seviyedeyse karolara, değilse simülatöre döner.</summary>
    [RelayCommand]
    private Task BackAsync()
    {
        if (!IsChoosing)
        {
            return _navigationService.NavigateBackAsync();
        }

        _pendingOptionKey = null;
        Choices.Clear();
        ChoiceGroup = null;
        return Task.CompletedTask;
    }

    private Task OpenFormAsync(string optionKey, Guid? cardId)
    {
        var parameters = new Dictionary<string, object>
        {
            [Routes.ScenarioOptionParameter] = optionKey
        };
        if (cardId.HasValue)
        {
            parameters[Routes.CardIdParameter] = cardId.Value;
        }

        return _navigationService.NavigateToAsync(
            Routes.ReplacingCurrent(Routes.SimulationCondition),
            parameters);
    }
}
