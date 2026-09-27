using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kart kontrol ekranının merkezi: sıradaki vadede karttan ne kadar çıkacağını, bunun nasıl
/// ödeneceği kararını (Asgari / Tamamı / Özel) ve kararın bedelini sunar (EK-V7 S1, S2).
/// Kesilmiş ekstre yoksa tahmini ekstreye bakar; karar o vadeye özel plana yazılır (S61).
/// </summary>
public sealed partial class NextPaymentViewModel : ObservableObject
{
    private const string SaveFailedTitle = "Ödeme kaydedilemedi";
    private const string InvalidAmountMessage = "Ödeyeceğin tutarı sayı olarak gir.";
    private const string UnexpectedErrorMessage = "Ödeme kaydedilirken bir sorun oluştu. Tekrar dene.";

    private readonly ICreditCardObligationService _cardService;
    private readonly IDialogService _dialogService;
    private readonly Func<Task> _onSaved;
    private CreditCard? _card;

    [ObservableProperty] private DateOnly? dueDate;
    [ObservableProperty] private bool isEstimate;
    [ObservableProperty] private decimal? statementBalance;
    [ObservableProperty] private decimal? minimumPayment;
    [ObservableProperty] private decimal? paymentAmount;
    [ObservableProperty] private decimal? carriedAmount;
    [ObservableProperty] private decimal? nextInterest;
    [ObservableProperty] private bool hasCarry;
    [ObservableProperty] private bool isCustomAmountOpen;
    [ObservableProperty] private string customAmountInput = string.Empty;
    [ObservableProperty] private bool isSaving;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMinimumSelected), nameof(IsFullSelected), nameof(IsCustomSelected))]
    private CurrentStatementPaymentMode? selectedPaymentMode;

    /// <summary>Asgari seçili mi.</summary>
    public bool IsMinimumSelected => SelectedPaymentMode == CurrentStatementPaymentMode.Minimum;

    /// <summary>Tamamı seçili mi.</summary>
    public bool IsFullSelected => SelectedPaymentMode == CurrentStatementPaymentMode.Full;

    /// <summary>Özel tutar seçili mi.</summary>
    public bool IsCustomSelected => SelectedPaymentMode == CurrentStatementPaymentMode.Custom;

    /// <summary>Kararı kaydetme portu, diyalog servisi ve kayıt sonrası geri çağrıyla başlatır.</summary>
    public NextPaymentViewModel(ICreditCardObligationService cardService, IDialogService dialogService, Func<Task> onSaved)
    {
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
    }

    /// <summary>
    /// Sıradaki ödemeyi gösterir. <paramref name="following"/> bir sonraki ekstredir; devreden
    /// tutara binecek faiz ondan okunur.
    /// </summary>
    public void Apply(CreditCard card, CreditCardStatementProjection payment, CreditCardStatementProjection? following)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        ArgumentNullException.ThrowIfNull(payment);
        DueDate = payment.PaymentDueDate;
        IsEstimate = !payment.IsActualStatement;
        StatementBalance = payment.StatementBalance;
        MinimumPayment = payment.MinimumPayment;
        PaymentAmount = payment.Payment;
        CarriedAmount = payment.CarriedAfterPayment;
        HasCarry = payment.CarriedAfterPayment > 0m;
        NextInterest = following?.CarryInterest;
        SelectedPaymentMode = ToDecidedMode(payment);
        IsCustomAmountOpen = SelectedPaymentMode == CurrentStatementPaymentMode.Custom;
        CustomAmountInput = IsCustomAmountOpen && payment.Payment is { } paid
            ? StatementEntryViewModel.FormatAmount(paid)
            : string.Empty;
    }

    /// <summary>
    /// Sıradaki ödemenin sırası: kesilmiş ekstre, yoksa tutarı sıfırdan büyük ilk tahmini ekstre;
    /// hiçbiri yoksa -1.
    /// </summary>
    public static int FindPaymentIndex(IReadOnlyList<CreditCardStatementProjection> projections)
    {
        ArgumentNullException.ThrowIfNull(projections);
        for (var i = 0; i < projections.Count; i++)
        {
            if (projections[i].IsActualStatement || projections[i].StatementBalance > 0m)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Sıradaki ödemenin vadesi; ödeme yoksa kart sıralamada sona düşsün diye en büyük tarih.</summary>
    public static DateOnly NextPaymentDate(IReadOnlyList<CreditCardStatementProjection> projections) =>
        FindPaymentIndex(projections) is var index && index >= 0 ? projections[index].PaymentDueDate : DateOnly.MaxValue;

    /// <summary>Sıradaki vadeyi asgari ödeme olarak kaydeder.</summary>
    [RelayCommand]
    private Task SetMinimumAsync() => SaveModeAsync(CreditCardPaymentType.Minimum);

    /// <summary>Sıradaki vadeyi ekstrenin tamamı olarak kaydeder.</summary>
    [RelayCommand]
    private Task SetFullAsync() => SaveModeAsync(CreditCardPaymentType.FullStatement);

    /// <summary>Özel tutar giriş satırını açar.</summary>
    [RelayCommand]
    private void OpenCustomAmount() => IsCustomAmountOpen = true;

    /// <summary>
    /// Girilen özel tutarı kaydeder: kesilmiş ekstrede ekstrenin planına, tahmini vadede
    /// sabit tutarlı vade planına.
    /// </summary>
    [RelayCommand]
    private async Task SaveCustomAmountAsync()
    {
        if (_card is null || DueDate is not { } due)
        {
            return;
        }

        if (!StatementEntryViewModel.TryParseAmount(CustomAmountInput, out var amount))
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, InvalidAmountMessage);
            return;
        }

        var card = _card;
        await RunSaveAsync(!IsEstimate && card.CurrentStatement is { } statement
            ? () => _cardService.SaveCreditCardStatementAsync(card.Id, statement,
                new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Custom, CustomAmount = amount })
            : () => _cardService.SaveCreditCardPaymentPlanAsync(card.Id, due, CreditCardPaymentType.FixedAmount, amount));
    }

    // Servis, vade kesilmiş ekstreninse onun planını, değilse vadeye özel planı yazar.
    private Task SaveModeAsync(CreditCardPaymentType type)
    {
        if (_card is null || DueDate is not { } due)
        {
            return Task.CompletedTask;
        }

        var cardId = _card.Id;
        return RunSaveAsync(() => _cardService.SetStatementPaymentModeAsync(cardId, due, type));
    }

    // Tutar kuralları CreditCardValidator ve karar çözümleyicidedir; ihlal diyalogla gösterilir.
    private async Task RunSaveAsync(Func<Task> save)
    {
        if (IsSaving)
        {
            return;
        }

        IsSaving = true;
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
            System.Diagnostics.Debug.WriteLine($"[NextPaymentViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Varsayım (kart "her ekstrede sor") bir karar değildir; o durumda hiçbir seçenek seçili görünmez.
    private static CurrentStatementPaymentMode? ToDecidedMode(CreditCardStatementProjection payment) =>
        payment.PaymentResolution is CreditCardPaymentResolution.CurrentStatementPlan
            or CreditCardPaymentResolution.DueDateOverride
            or CreditCardPaymentResolution.GeneralStrategy
            ? payment.AppliedPaymentType switch
            {
                CreditCardPaymentType.Minimum => CurrentStatementPaymentMode.Minimum,
                CreditCardPaymentType.FullStatement => CurrentStatementPaymentMode.Full,
                CreditCardPaymentType.FixedAmount => CurrentStatementPaymentMode.Custom,
                _ => null
            }
            : null;
}
