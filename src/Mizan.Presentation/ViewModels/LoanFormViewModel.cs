using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kredi formunun görünüm modelidir: krediyi ekler ya da düzenler, kaydedip listeye döner,
/// kaydedilmemiş değişiklikte çıkmadan önce onay sorar. Kredinin tanımı
/// <see cref="LoanDefinitionViewModel"/>, faiz ve bugün kapatma bedeli <see cref="LoanPayoffViewModel"/>
/// çocuğundadır (EK-V6c, S64).
/// </summary>
public sealed partial class LoanFormViewModel : ViewModelBase
{
    private const string SaveFailedTitle = "Kredi kaydedilemedi";
    private const string NotFoundTitle = "Kredi bulunamadı";
    private const string NotFoundMessage = "Kredi silinmiş olabilir.";
    private const string UnexpectedErrorMessage = "Kredi kaydedilirken bir sorun oluştu. Tekrar dene.";
    private const string DiscardTitle = "Kaydetmeden çık";
    private const string DiscardMessage = "Yaptığın değişiklikler kaydedilmeyecek.";
    private const string DiscardAccept = "Çık";
    private const string DiscardCancel = "Kal";

    private readonly ILoanRepository _loanRepository;
    private readonly IObligationManagementService _obligationService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IClock _clock;
    private Guid? _requestedLoanId;
    private Loan? _loan;

    [ObservableProperty] private bool isEditing;

    /// <summary>Görünüm modelini beş dar bağımlılıkla başlatır (Kural M3).</summary>
    public LoanFormViewModel(
        ILoanRepository loanRepository, IObligationManagementService obligationService,
        INavigationService navigationService, IDialogService dialogService, IClock clock)
    {
        _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        _obligationService = obligationService ?? throw new ArgumentNullException(nameof(obligationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        Payoff = new LoanPayoffViewModel(_obligationService);

        // Faiz kartı canlıdır: tanımdaki her değişiklik kartı formun o anki taslağıyla yeniden çözer.
        Fields.PropertyChanged += (_, _) => Payoff.Refresh(Fields.Draft(_loan));
    }

    /// <summary>Kredinin tanımı: ad, banka, aylık taksit, kalan taksit, sonraki taksit tarihi, tür.</summary>
    public LoanDefinitionViewModel Fields { get; } = new();

    /// <summary>Faiz kartı: kalan anapara, bankanın kapatma tutarı ve bugün kapatmanın bedeli.</summary>
    public LoanPayoffViewModel Payoff { get; }

    /// <summary>Form açıldığından beri bir alan ya da faiz kartındaki bir tutar değişti mi.</summary>
    public bool HasChanges => Fields.HasChanges || Payoff.HasChanges;

    /// <summary>Kimlik verilmezse boş yeni kredi formu, verilirse o kredinin düzenlemesi açılır.</summary>
    [RelayCommand]
    public async Task LoadAsync(Guid? loanId)
    {
        _requestedLoanId = loanId;
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            _loan = loanId is { } id ? (await _loanRepository.GetLoansAsync()).FirstOrDefault(l => l.Id == id) : null;
            if (loanId is not null && _loan is null)
            {
                await _dialogService.ShowAlertAsync(NotFoundTitle, NotFoundMessage);
                await _navigationService.NavigateBackAsync();
                return;
            }

            IsEditing = _loan is not null;
            Fields.Fill(_loan, _clock.Today);
            Payoff.Fill(_loan);
            Payoff.Refresh(Fields.Draft(_loan));
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoanFormViewModel ERROR] {ex}");
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Okuma hatasından sonra aynı krediyi (ya da yeni kredi formunu) yeniden yükler.</summary>
    [RelayCommand]
    private Task RetryAsync() => LoadAsync(_requestedLoanId);

    /// <summary>Formu doğrular, krediyi kaydeder ve bir önceki sayfaya döner.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (Fields.TryBuild(_loan, out var defined) is { } error)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, error);
            return;
        }

        if (Payoff.TryApply(defined!, out var loan) is { } amountError)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, amountError);
            return;
        }

        SetBusy(true);
        try
        {
            await _obligationService.SaveLoanAsync(loan);
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoanFormViewModel ERROR] {ex}");
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
