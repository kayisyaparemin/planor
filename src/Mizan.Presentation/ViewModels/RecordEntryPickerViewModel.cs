using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Finansal Yapı'nın "Ekle"siyle açılan kayıt türü seçicinin görünüm modelidir (EK-V6f, S77). Düz sistem
/// diyaloğu altı türü ayrımsız alt alta diziyordu; seçici türleri listenin dört grubunda, hepsi aynı anda
/// görünür biçimde sunar ve her türün ne işe yaradığını seçimden önce söyler. Bölümler ayrı ayrı açılır ki
/// sayfa onları konsepteki gibi yerleştirebilsin (tek karolu iki bölüm yan yana, GS29). Seçilen tür,
/// seçicinin yerine geçen formu açar: formdan dönüş doğrudan listeye iner.
/// </summary>
public sealed partial class RecordEntryPickerViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    /// <summary>Görünüm modelini gezinme portuyla başlatır.</summary>
    public RecordEntryPickerViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>Gelir bölümü: Finansal Yapı'daki "Gelirler" grubuna düşen türler.</summary>
    public EntryTypeSection IncomeSection { get; } = SectionOf(RecordEntryGroup.Income);

    /// <summary>Kart bölümü: "Kartlar" grubuna düşen türler.</summary>
    public EntryTypeSection CardSection { get; } = SectionOf(RecordEntryGroup.Card);

    /// <summary>Kredi bölümü: "Krediler" grubuna düşen türler.</summary>
    public EntryTypeSection LoanSection { get; } = SectionOf(RecordEntryGroup.Loan);

    /// <summary>Ödeme bölümü: "Ödemeler" grubuna düşen türler.</summary>
    public EntryTypeSection PaymentSection { get; } = SectionOf(RecordEntryGroup.Payment);

    /// <summary>Seçeneğin formunu seçicinin yerine açar.</summary>
    [RelayCommand]
    private Task SelectOptionAsync(EntryTypeOptionItem? option)
    {
        if (option is null)
        {
            return Task.CompletedTask;
        }

        var form = FinancialRecordEntryCatalog.For(option.Key).Form;
        return _navigationService.NavigateToAsync(Routes.ReplacingCurrent(RouteOf(form)));
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
}
