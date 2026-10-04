using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Dönem kapanışı ve mutabakat özet ekranının görünüm modeli (EK-V11, GS31, S78).
/// Dönem sonu bakiyesini, plana göre sapmayı, farkın kaynaklarını sunar; kapanış bakiyesini
/// teyit edip ya da revize edip tek dokunuşla yeni dönemi başlatır.
/// </summary>
public sealed partial class PeriodSettlementViewModel : ViewModelBase
{
    private readonly IPeriodWorkflowService _workflowService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private PeriodSettlementDraft? _draft;

    [ObservableProperty] private DateOnly? periodStart; [ObservableProperty] private DateOnly? periodLastDay;
    [ObservableProperty] private DateOnly? nextPeriodStart;
    [ObservableProperty] private decimal? endingBalance; [ObservableProperty] private decimal? plannedEndingBalance;
    [ObservableProperty] private decimal? endingDeviation;
    [ObservableProperty] private bool isBehindPlan; [ObservableProperty] private bool isAheadOfPlan;
    [ObservableProperty] private decimal plannedLivingSpend; [ObservableProperty] private decimal actualLivingSpend;
    [ObservableProperty] private decimal livingDifference;
    [ObservableProperty] private bool isLivingSaved; [ObservableProperty] private bool isLivingOverspent;
    [ObservableProperty] private int totalPaymentsCount; [ObservableProperty] private int paidPaymentsCount;
    [ObservableProperty] private decimal paymentsDifference;
    [ObservableProperty] private bool hasDeficitInterest; [ObservableProperty] private decimal actualDeficitInterest;
    [ObservableProperty] private decimal? confirmedBalance; [ObservableProperty] private DateOnly? confirmedDate;
    [ObservableProperty] private bool canFinalize;

    /// <summary>Dönem kapanışı görünüm modelini başlatır.</summary>
    public PeriodSettlementViewModel(
        IPeriodWorkflowService workflowService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>Dönem kapanış özetini ve taslağını yükler.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        SetBusy(true);
        State = ScreenState.Loading;
        try
        {
            var availability = await _workflowService.GetSettlementAvailabilityAsync();
            if (!availability.IsDue || availability.PendingPlan is null)
            {
                CanFinalize = false;
                State = ScreenState.Empty;
                return;
            }

            await LoadPlanInternalAsync(availability.PendingPlan);
            CanFinalize = true;
            State = ScreenState.Content;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PeriodSettlementViewModel ERROR] {ex}");
            CanFinalize = false;
            State = ScreenState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadPlanInternalAsync(PeriodPlanSnapshot plan)
    {
        PeriodStart = plan.PeriodStart;
        PeriodLastDay = plan.PeriodEnd.AddDays(-1);
        NextPeriodStart = plan.PeriodEnd;

        var context = await _workflowService.GetSettlementContextAsync(plan.Id);
        var observed = await _workflowService.GetObservedSettlementDraftAsync(plan.Id);

        _draft = observed ?? new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            Payments = plan.PaymentLines.Select(DefaultPaymentDraft).ToArray(),
            ActualLivingSpend = plan.PlannedVariableExpenseAllowance,
            ActualInterest = 0m,
            ConfirmedEndingBalance = context.SuggestedStartingBalance
        };

        await ApplyDraftAndPreviewAsync(_draft);
    }

    /// <summary>Kapanış bakiyesini diyalog üzerinden günceller (S68-5).</summary>
    [RelayCommand]
    public async Task ChangeBalanceAsync()
    {
        if (_draft is null)
        {
            return;
        }

        var input = await _dialogService.PromptAsync(
            "Kapanış Bakiyesi",
            "Dönem sonundaki kesinleşen banka bakiyesini girin:",
            "Tamam",
            "Vazgeç",
            ConfirmedBalance?.ToString("N2", CultureInfo.InvariantCulture) ?? string.Empty);

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        if (decimal.TryParse(input.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var newBalance) ||
            decimal.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out newBalance))
        {
            _draft = _draft with { ConfirmedEndingBalance = newBalance };
            await ApplyDraftAndPreviewAsync(_draft);
        }
    }

    /// <summary>Dönemi kesinleştirerek kapatır ve yeni dönemi başlatır.</summary>
    [RelayCommand]
    public async Task ClosePeriodAsync()
    {
        if (_draft is null || !CanFinalize)
        {
            return;
        }

        SetBusy(true);
        try
        {
            await _workflowService.FinalizeSettlementAsync(_draft);
            await _navigationService.NavigateToAsync(Routes.Dashboard);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ClosePeriodAsync ERROR] {ex}");
            await _dialogService.ShowAlertAsync("Hata", "Dönem kapatılırken bir hata oluştu.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Kapanışı erteler ve ana sayfaya döner.</summary>
    [RelayCommand]
    public Task DismissAsync() => _navigationService.NavigateToAsync(Routes.Dashboard);

    private async Task ApplyDraftAndPreviewAsync(PeriodSettlementDraft draft)
    {
        var preview = await _workflowService.PreviewSettlementAsync(draft);
        EndingBalance = preview.ConfirmedEndingBalance;
        PlannedEndingBalance = preview.Comparison.PlannedEndingBalance;
        EndingDeviation = preview.Comparison.Difference;
        IsBehindPlan = EndingDeviation < 0m;
        IsAheadOfPlan = EndingDeviation > 0m;

        var comparison = preview.Comparison;
        var livingLine = comparison.Lines.FirstOrDefault(x => x.Category.Contains("Yaşam"));
        PlannedLivingSpend = livingLine?.Planned ?? draft.ActualLivingSpend;
        ActualLivingSpend = livingLine?.Actual ?? draft.ActualLivingSpend;
        LivingDifference = PlannedLivingSpend - ActualLivingSpend;
        IsLivingSaved = LivingDifference > 0m;
        IsLivingOverspent = LivingDifference < 0m;

        TotalPaymentsCount = draft.Payments.Count;
        PaidPaymentsCount = draft.Payments.Count(p => p.Status is ActualPaymentStatus.Paid or ActualPaymentStatus.DifferentAmount);
        var paymentLines = comparison.Lines.Where(x => x.Category is "Krediler" or "Kredi kartları" or "Zorunlu ödemeler" or "Diğer planlı ödemeler" or "Geçici ödemeler" or "Taksitli ödemeler").ToList();
        PaymentsDifference = paymentLines.Sum(x => x.Difference);

        var interestLine = comparison.Lines.FirstOrDefault(x => x.Category.Contains("Faiz"));
        ActualDeficitInterest = interestLine?.Actual ?? 0m;
        HasDeficitInterest = ActualDeficitInterest > 0m;
        ConfirmedBalance = preview.ConfirmedEndingBalance;
        ConfirmedDate = PeriodLastDay;
    }

    private static ActualPaymentDraft DefaultPaymentDraft(PeriodPlanPaymentLine line) => new()
    {
        PeriodPlanPaymentLineId = line.Id,
        Status = line.PlannedAmount is null ? ActualPaymentStatus.Unpaid : ActualPaymentStatus.Paid,
        ActualAmount = line.PlannedAmount.GetValueOrDefault(),
        ActualPaymentDate = line.PlannedDate
    };
}
