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
    private const string MissingLoanTitle = "Kayıtlı kredi bulunamadı";
    private const string MissingLoanMessage = "Krediye erken ödeme denemek için önce Finansal Yapı'dan bir kredi eklemelisin.";
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

    /// <summary>Borç seçenekleri bölümü.</summary>
    public EntryTypeSection DebtSection { get; } = new(
        RecordEntryGroup.Loan,
        [
            new(SimulationScenarioCatalog.Financing.Key, 0),
            new(SimulationScenarioCatalog.CashDebt.Key, 1),
            new(SimulationScenarioCatalog.LoanPrepayment.Key, 2)
        ]);

    /// <summary>Sayfa ikinci seviyede mi ("Hangi kart?" veya "Hangi kredi?").</summary>
    public bool IsChoosing => ChoiceGroup is not null;

    /// <summary>İkinci seviyedeki aday kayıtlar.</summary>
    public ObservableCollection<FinancialRecordRow> Choices { get; } = [];

    /// <summary>
    /// Karoya dokunulduğunda seçeneğin formunu açar. Kart ve kredi gerektiren türlerde önce adaylar çözülür.
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
            await HandleCandidateSelectionAsync(key, FinancialRecordKind.CreditCard, RecordEntryGroup.Card, MissingCardTitle, MissingCardMessage);
            return;
        }

        if (key == "loan-prepayment")
        {
            await HandleCandidateSelectionAsync(key, FinancialRecordKind.Loan, RecordEntryGroup.Loan, MissingLoanTitle, MissingLoanMessage);
            return;
        }

        await OpenFormAsync(key, null, null);
    }

    private async Task HandleCandidateSelectionAsync(
        string key, FinancialRecordKind kind, RecordEntryGroup group, string missingTitle, string missingMessage)
    {
        var candidates = await _candidateResolver.GetCandidatesAsync(kind);
        if (candidates is null)
        {
            await _dialogService.ShowAlertAsync(ReadFailedTitle, ReadFailedMessage);
            return;
        }

        switch (candidates)
        {
            case []:
                await _dialogService.ShowAlertAsync(missingTitle, missingMessage);
                return;
            case [var only]:
                await (group == RecordEntryGroup.Card ? OpenFormAsync(key, only.Id, null) : OpenFormAsync(key, null, only.Id));
                return;
            case var multiple:
                _pendingOptionKey = key;
                Choices.Clear();
                foreach (var item in multiple)
                {
                    Choices.Add(item);
                }

                ChoiceGroup = group;
                return;
        }
    }

    /// <summary>İkinci seviyede seçilen adayla formu seçicinin yerine açar.</summary>
    [RelayCommand]
    private Task ChooseRecordAsync(FinancialRecordRow? row) =>
        row is not null && _pendingOptionKey is not null
            ? (ChoiceGroup == RecordEntryGroup.Card
                ? OpenFormAsync(_pendingOptionKey, row.Id, null)
                : OpenFormAsync(_pendingOptionKey, null, row.Id))
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

    private Task OpenFormAsync(string optionKey, Guid? cardId, Guid? loanId)
    {
        var parameters = new Dictionary<string, object>
        {
            [Routes.ScenarioOptionParameter] = optionKey
        };
        if (cardId.HasValue)
        {
            parameters[Routes.CardIdParameter] = cardId.Value;
        }
        if (loanId.HasValue)
        {
            parameters[Routes.LoanIdParameter] = loanId.Value;
        }

        return _navigationService.NavigateToAsync(
            Routes.ReplacingCurrent(Routes.SimulationCondition),
            parameters);
    }
}
