using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Finansal Yapı'nın "Ekle"siyle açılan kayıt türü seçicinin görünüm modelidir (EK-V6f, S77). Düz sistem
/// diyaloğu altı türü ayrımsız alt alta diziyordu; seçici türleri listenin dört grubunda, hepsi aynı anda
/// görünür biçimde sunar ve her türün ne işe yaradığını seçimden önce söyler. Seçilen tür, seçicinin yerine
/// geçen formu açar: formdan dönüş doğrudan listeye iner. Var olan bir kayda eklenen türlerde (kartla
/// harcama, erken ödeme, gelir değişikliği) önce kayıt çözülür: hiç yoksa eklemesi önerilir, tekse
/// sorulmaz, birden fazlaysa aynı sayfada "Hangi kart?" sorulur (S77-5). Eski ortak form bunu formun
/// içindeki açılır listeyle soruyor, kaydın olmadığını ancak form açıldıktan sonra söylüyordu.
/// </summary>
public sealed partial class RecordEntryPickerViewModel : ViewModelBase
{
    private const string CancelText = "Vazgeç";
    private const string ReadFailedTitle = "Kayıtlar okunamadı";
    private const string ReadFailedMessage = "Kayıtların okunurken bir sorun oluştu. Tekrar dene.";

    // Var olan kayda eklenen türlerin üst kaydı: listede hangi satırlar aday, form hangi kimlikle açılır,
    // aday yoksa ne önerilir. Kart ve kredi formunda harcama / erken ödeme yeni kayıtta da girilir.
    private static readonly Dictionary<RecordEntryForm, ParentRecord> Parents = new()
    {
        [RecordEntryForm.CreditCard] = new(
            FinancialRecordKind.CreditCard, Routes.CardIdParameter, "Henüz kartın yok",
            "Kartla harcama için önce kartını ekle; harcamayı aynı formda girebilirsin.", "Kart ekle"),
        [RecordEntryForm.Loan] = new(
            FinancialRecordKind.Loan, Routes.LoanIdParameter, "Henüz kredin yok",
            "Erken ödeme için önce kredini ekle; erken ödemeyi aynı formda planlayabilirsin.", "Kredi ekle"),
        [RecordEntryForm.RecurringIncome] = new(
            FinancialRecordKind.RecurringIncome, Routes.IncomeIdParameter, "Henüz düzenli gelirin yok",
            "Tutarı değişecek bir gelir yok. Önce düzenli gelirini ekle.", "Gelir ekle")
    };

    private readonly INavigationService _navigationService;
    private readonly RecordCandidateResolver _candidateResolver;
    private readonly IDialogService _dialogService;
    private RecordEntryForm? _choiceForm;

    /// <summary>Hangi grubun kaydı soruluyor; null ise dört grubun karoları görünür.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChoosing))]
    private RecordEntryGroup? choiceGroup;

    /// <summary>Görünüm modelini üç dar bağımlılıkla başlatır (Kural M3).</summary>
    public RecordEntryPickerViewModel(
        INavigationService navigationService, RecordCandidateResolver candidateResolver, IDialogService dialogService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _candidateResolver = candidateResolver ?? throw new ArgumentNullException(nameof(candidateResolver));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>Gelir bölümü: Finansal Yapı'daki "Gelirler" grubuna düşen türler.</summary>
    public EntryTypeSection IncomeSection { get; } = SectionOf(RecordEntryGroup.Income);

    /// <summary>Kart bölümü: "Kartlar" grubuna düşen türler.</summary>
    public EntryTypeSection CardSection { get; } = SectionOf(RecordEntryGroup.Card);

    /// <summary>Kredi bölümü: "Krediler" grubuna düşen türler.</summary>
    public EntryTypeSection LoanSection { get; } = SectionOf(RecordEntryGroup.Loan);

    /// <summary>Ödeme bölümü: "Ödemeler" grubuna düşen türler.</summary>
    public EntryTypeSection PaymentSection { get; } = SectionOf(RecordEntryGroup.Payment);

    /// <summary>Sayfa ikinci seviyede mi: karolar yerine "Hangi kart?" adayları görünür.</summary>
    public bool IsChoosing => ChoiceGroup is not null;

    /// <summary>İkinci seviyenin adayları, Finansal Yapı listesindeki sırasıyla ve süzgeciyle.</summary>
    public ObservableCollection<FinancialRecordRow> Choices { get; } = [];

    /// <summary>
    /// Seçeneğin formunu seçicinin yerine açar. Var olan kayda eklenen türde önce adaylar okunur: aday yoksa
    /// eklemesi önerilir, tek aday sorulmaz, birden fazlası ikinci seviyede sorulur.
    /// </summary>
    [RelayCommand]
    private async Task SelectOptionAsync(EntryTypeOptionItem? option)
    {
        if (option is null)
        {
            return;
        }

        var entry = FinancialRecordEntryCatalog.For(option.Key);
        if (!entry.EditsExisting)
        {
            await _navigationService.NavigateToAsync(Routes.ReplacingCurrent(RouteOf(entry.Form)));
            return;
        }

        switch (await ReadCandidatesAsync(entry.Form))
        {
            case null:
                return; // Okunamadı; kullanıcı uyarıldı, seçici yerinde kalır.
            case []:
                await OfferToAddAsync(entry.Form);
                return;
            case [var only]:
                await OpenAsync(entry.Form, only.Id);
                return;
            case var candidates:
                ShowChoices(entry, candidates);
                return;
        }
    }

    /// <summary>İkinci seviyede seçilen kaydın formunu, kaydın kimliğiyle seçicinin yerine açar.</summary>
    [RelayCommand]
    private Task ChooseRecordAsync(FinancialRecordRow? row) =>
        row is not null && _choiceForm is { } form ? OpenAsync(form, row.Id) : Task.CompletedTask;

    /// <summary>Geri: ikinci seviyedeyse karolara, değilse Finansal Yapı'ya döner.</summary>
    [RelayCommand]
    private Task BackAsync()
    {
        if (!IsChoosing)
        {
            return _navigationService.NavigateBackAsync();
        }

        _choiceForm = null;
        Choices.Clear();
        ChoiceGroup = null;
        return Task.CompletedTask;
    }

    // Adaylar Finansal Yapı listesinin süzgecinden geçer: listede görünmeyen kayıt seçicide de çıkmaz (S62-5).
    private async Task<IReadOnlyList<FinancialRecordRow>?> ReadCandidatesAsync(RecordEntryForm form)
    {
        var candidates = await _candidateResolver.GetCandidatesAsync(Parents[form].Kind);
        if (candidates is null)
        {
            await _dialogService.ShowAlertAsync(ReadFailedTitle, ReadFailedMessage);
        }

        return candidates;
    }

    private async Task OfferToAddAsync(RecordEntryForm form)
    {
        var parent = Parents[form];
        if (await _dialogService.ConfirmAsync(parent.MissingTitle, parent.MissingMessage, parent.AddText, CancelText))
        {
            await _navigationService.NavigateToAsync(Routes.ReplacingCurrent(RouteOf(form)));
        }
    }

    private Task OpenAsync(RecordEntryForm form, Guid recordId) =>
        _navigationService.NavigateToAsync(
            Routes.ReplacingCurrent(RouteOf(form)), new Dictionary<string, object> { [Parents[form].IdParameter] = recordId });

    private void ShowChoices(RecordEntryOption entry, IReadOnlyList<FinancialRecordRow> candidates)
    {
        _choiceForm = entry.Form;
        Choices.Clear();
        foreach (var candidate in candidates)
        {
            Choices.Add(candidate);
        }

        ChoiceGroup = entry.Group;
    }

    private static EntryTypeSection SectionOf(RecordEntryGroup group) => new(
        group,
        FinancialRecordEntryCatalog.OptionsIn(group).Select((option, index) => new EntryTypeOptionItem(option.Key, index)).ToArray());

    private static string RouteOf(RecordEntryForm form) => form switch
    {
        RecordEntryForm.PlannedExpense => Routes.PlannedExpenseForm,
        RecordEntryForm.PaymentPlan => Routes.PaymentPlanForm,
        RecordEntryForm.CreditCard => Routes.CardForm,
        RecordEntryForm.Loan => Routes.LoanForm,
        RecordEntryForm.RecurringIncome => Routes.IncomeForm,
        RecordEntryForm.AdHocIncome => Routes.AdHocIncomeForm,
        _ => throw new ArgumentOutOfRangeException(nameof(form), form, "Formun rotası tanımlı değil.")
    };

    private sealed record ParentRecord(
        FinancialRecordKind Kind, string IdParameter, string MissingTitle, string MissingMessage, string AddText);
}
