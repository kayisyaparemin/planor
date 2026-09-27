using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Finansal Yapı ekranının görünüm modelidir: plana giren kayıtları dört grupta sunar ve satıra
/// dokununca tek diyalogla kart kontrolü açar ya da kaydı siler (EK-V6, S62). Formlar ayrı
/// sayfalardır ve V6b–V6e'de gelir.
/// </summary>
public sealed partial class FinancialStructureViewModel : ViewModelBase
{
    private const string CancelText = "Vazgeç";
    private const string DeleteText = "Sil";
    private const string ManagePaymentText = "Ödemeyi yönet";
    private const string DeleteConfirmTitle = "Kaydı sil";
    private const string DeleteFailedTitle = "Kayıt silinemedi";
    private const string UnexpectedErrorMessage = "Kayıt silinirken bir sorun oluştu. Tekrar dene.";

    private readonly IPlanReader _planReader;
    private readonly FinancialRecordRowBuilder _rowBuilder;
    private readonly FinancialRecordRemover _remover;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public FinancialStructureViewModel(
        IPlanReader planReader, FinancialRecordRowBuilder rowBuilder, FinancialRecordRemover remover,
        INavigationService navigationService, IDialogService dialogService)
    {
        _planReader = planReader ?? throw new ArgumentNullException(nameof(planReader));
        _rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
        _remover = remover ?? throw new ArgumentNullException(nameof(remover));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>Düzenli ve tek seferlik gelirler.</summary>
    public FinancialRecordGroup Incomes { get; } = new();

    /// <summary>Kredi kartları.</summary>
    public FinancialRecordGroup Cards { get; } = new();

    /// <summary>Krediler.</summary>
    public FinancialRecordGroup Loans { get; } = new();

    /// <summary>Ödeme planları ve planlı büyük harcamalar.</summary>
    public FinancialRecordGroup Payments { get; } = new();

    /// <summary>Planı okur ve grupları doldurur; kayıt yoksa boş, okuma hatasında hata durumuna düşer.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        SetBusy(true);
        State = ScreenState.Loading;
        await ReloadAsync();
        SetBusy(false);
    }

    /// <summary>Satırın seçeneklerini tek diyalogda sunar: kartta ödemeyi yönetme, her türde silme.</summary>
    [RelayCommand]
    private async Task SelectRecordAsync(FinancialRecordRow? row)
    {
        if (row is null)
        {
            return;
        }

        string[] options = row.Kind == FinancialRecordKind.CreditCard ? [ManagePaymentText] : [];
        var choice = await _dialogService.ChooseAsync(row.Name, CancelText, DeleteText, options);
        if (choice == ManagePaymentText)
        {
            var parameters = new Dictionary<string, object> { [Routes.CardIdParameter] = row.Id };
            await _navigationService.NavigateToAsync(Routes.CardControl, parameters);
        }
        else if (choice == DeleteText)
        {
            await DeleteAsync(row);
        }
    }

    private async Task DeleteAsync(FinancialRecordRow row)
    {
        var message = $"{row.Name} ve ona bağlı kayıtlar kalıcı olarak silinecek.";
        if (!await _dialogService.ConfirmAsync(DeleteConfirmTitle, message, DeleteText, CancelText))
        {
            return;
        }

        try
        {
            await _remover.RemoveAsync(row.Kind, row.Id);
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync(DeleteFailedTitle, ex.Message);
            return;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FinancialStructureViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(DeleteFailedTitle, UnexpectedErrorMessage);
            return;
        }

        await ReloadAsync();
    }

    // Silmeden sonra da çağrılır; iskelet göstermeden listeyi yeniler.
    private async Task ReloadAsync()
    {
        try
        {
            var rows = _rowBuilder.Build(await _planReader.GetPlanAsync());
            Incomes.Apply(rows.Incomes);
            Cards.Apply(rows.Cards);
            Loans.Apply(rows.Loans);
            Payments.Apply(rows.Payments);
            State = rows.IsEmpty ? ScreenState.Empty : ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FinancialStructureViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
    }
}
