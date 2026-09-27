using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kart kontrol ekranının çocuğu: bankanın kestiği ekstreyi (tutar, asgari, kesim ve son
/// ödeme tarihi) elle girer ya da düzeltir. PDF'den okuma yoktur (S21); elle giriş kalır (S61).
/// </summary>
public sealed partial class StatementEntryViewModel : ObservableObject
{
    private const string SaveFailedTitle = "Ekstre kaydedilemedi";
    private const string InvalidAmountMessage = "Ekstre tutarını ve asgari ödemeyi sayı olarak gir.";
    private const string UnexpectedErrorMessage = "Ekstre kaydedilirken bir sorun oluştu. Tekrar dene.";
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    private readonly ICreditCardObligationService _cardService;
    private readonly IDialogService _dialogService;
    private readonly Func<Task> _onSaved;
    private CreditCard? _card;

    [ObservableProperty] private bool isOpen;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private string minimumInput = string.Empty;
    [ObservableProperty] private DateOnly statementDate;
    [ObservableProperty] private DateOnly dueDate;

    /// <summary>Formu kaydetme portu, diyalog servisi ve kayıt sonrası geri çağrıyla başlatır.</summary>
    public StatementEntryViewModel(ICreditCardObligationService cardService, IDialogService dialogService, Func<Task> onSaved)
    {
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
    }

    /// <summary>
    /// Formu verilen kart için açar. Kartın kesilmiş ekstresi varsa alanlar ondan, yoksa
    /// verilen varsayılan tarihlerden dolar.
    /// </summary>
    public void Open(CreditCard card, DateOnly defaultStatementDate, DateOnly defaultDueDate)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        var statement = card.CurrentStatement;
        StatementDate = statement?.StatementDate ?? defaultStatementDate;
        DueDate = statement?.DueDate ?? defaultDueDate;
        AmountInput = statement is null ? string.Empty : FormatAmount(statement.StatementAmount);
        MinimumInput = statement is null ? string.Empty : FormatAmount(statement.MinimumPaymentAmount);
        IsOpen = true;
    }

    /// <summary>Formu kaydetmeden kapatır.</summary>
    [RelayCommand]
    private void Cancel() => IsOpen = false;

    /// <summary>Ekstreyi kaydeder; yeni ekstrenin ödeme şekli kartın varsayılanından gelir.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_card is null || IsSaving)
        {
            return;
        }

        if (!TryParseAmount(AmountInput, out var amount) || !TryParseAmount(MinimumInput, out var minimum))
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, InvalidAmountMessage);
            return;
        }

        IsSaving = true;
        try
        {
            await _cardService.SaveCreditCardStatementAsync(_card.Id, BuildStatement(_card, amount, minimum), BuildPlan(_card, amount));
            IsOpen = false;
            await _onSaved();
        }
        catch (InvalidOperationException ex)
        {
            await _dialogService.ShowAlertAsync(SaveFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StatementEntryViewModel ERROR] {ex}");
            await _dialogService.ShowAlertAsync(SaveFailedTitle, UnexpectedErrorMessage);
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Bankanın bildirdiği sonraki tarihler yalnız aynı ekstre düzeltilirken korunur; kesim tarihi
    // değişirse o tarihler başka bir döngüye aittir ve düşürülür.
    private CreditCardStatement BuildStatement(CreditCard card, decimal amount, decimal minimum)
    {
        var existing = card.CurrentStatement;
        var sameCycle = existing?.StatementDate == StatementDate;
        return (existing ?? new CreditCardStatement()) with
        {
            CreditCardId = card.Id,
            StatementDate = StatementDate,
            DueDate = DueDate,
            StatementAmount = amount,
            MinimumPaymentAmount = minimum,
            NextStatementDate = sameCycle ? existing!.NextStatementDate : null,
            NextDueDate = sameCycle ? existing!.NextDueDate : null
        };
    }

    // S61: düzenlenen ekstre planını korur; yeni ekstre kartın varsayılan ödeme şekliyle başlar.
    private static CurrentStatementPaymentPlan BuildPlan(CreditCard card, decimal amount)
    {
        if (card.CurrentStatement is not null && card.CurrentStatementPaymentPlan is { } existing)
        {
            return existing;
        }

        return card.PaymentStrategy switch
        {
            CreditCardPaymentStrategy.FullStatement => new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Full },
            CreditCardPaymentStrategy.FixedAmount => new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Custom,
                CustomAmount = Math.Min(card.FixedPaymentAmount ?? amount, amount)
            },
            _ => new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Minimum }
        };
    }

    // Türkçe biçim ("18.200,50") esastır. Klavye Türkçe değilse tek ondalık işareti nokta olur:
    // virgülsüz ve üçlü gruplara uymayan noktalı giriş ("5000.50") ondalık sayılır.
    internal static bool TryParseAmount(string? text, out decimal amount)
    {
        var trimmed = (text ?? string.Empty).Trim();
        var isPointDecimal = !trimmed.Contains(',', StringComparison.Ordinal) &&
                             trimmed.Contains('.', StringComparison.Ordinal) &&
                             !ThousandsGrouping().IsMatch(trimmed);
        return isPointDecimal
            ? decimal.TryParse(trimmed, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount)
            : decimal.TryParse(trimmed, NumberStyles.Number, Tr, out amount);
    }

    [GeneratedRegex(@"^\d{1,3}(\.\d{3})+$")]
    private static partial Regex ThousandsGrouping();

    internal static string FormatAmount(decimal amount) => amount.ToString("0.##", Tr);
}
