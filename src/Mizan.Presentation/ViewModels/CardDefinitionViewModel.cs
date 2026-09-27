using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Onboarding.Support;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kart formunun çocuğu: kartın tanımını (ad, banka, limit, kesim ve son ödeme günü, güncel borç)
/// taşır, doğrular ve karta çevirir. Formun dokunmadığı her alan korunur; asgari oran limitten,
/// güncel borç faizsiz harcama olarak yazılır (EK-V6b, S63-2..4).
/// </summary>
public sealed partial class CardDefinitionViewModel : ObservableObject
{
    private const string InvalidDebtMessage = "Güncel borcu sayı olarak gir.";

    private (string Name, string Bank, string Limit, string Debt, string ClosingDay, string DueDay) _loaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowsDebt))]
    private bool hasStatement;

    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string bank = string.Empty;
    [ObservableProperty] private string limitInput = string.Empty;
    [ObservableProperty] private string debtInput = string.Empty;
    [ObservableProperty] private string closingDayInput = string.Empty;
    [ObservableProperty] private string dueDayInput = string.Empty;

    /// <summary>Alanları boş yeni kart için başlatır.</summary>
    public CardDefinitionViewModel() => Fill(null);

    /// <summary>Güncel borç alanı yalnız kesilmiş ekstresi olmayan kartta görünür (S63-4).</summary>
    public bool ShowsDebt => !HasStatement;

    /// <summary>Alanlar dolduruldukları andan beri değişti mi.</summary>
    public bool HasChanges => Current() != _loaded;

    /// <summary>Alanları karttan doldurur; kart yoksa boş yeni kart formu.</summary>
    public void Fill(CreditCard? card)
    {
        HasStatement = card?.CurrentStatement is not null;
        Name = card?.Name ?? string.Empty;
        Bank = card?.Bank ?? string.Empty;
        LimitInput = card is null ? string.Empty : StatementEntryViewModel.FormatAmount(card.Limit);
        DebtInput = card is null || CurrentDebt(card) == 0m ? string.Empty : StatementEntryViewModel.FormatAmount(CurrentDebt(card));
        ClosingDayInput = card?.StatementClosingDay.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        DueDayInput = card?.PaymentDueDay.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _loaded = Current();
    }

    /// <summary>
    /// Alanları doğrulayıp kartı kurar. Geçersizse kullanıcıya gösterilecek mesajı döner ve kart
    /// kurulmaz. Ad, limit ve günler kurulumla aynı kurallardan geçer; boş borç sıfırdır.
    /// </summary>
    public string? TryBuild(CreditCard? existing, DateOnly today, out CreditCard? card)
    {
        card = null;
        var hasLimit = StatementEntryViewModel.TryParseAmount(LimitInput, out var limit);
        var closingDay = ParseDay(ClosingDayInput);
        var dueDay = ParseDay(DueDayInput);
        var error = OnboardingValidator.ValidateCard(Name, hasLimit ? limit : null, closingDay, dueDay);
        var debt = 0m;
        if (error is null && ShowsDebt && !string.IsNullOrWhiteSpace(DebtInput) &&
            !StatementEntryViewModel.TryParseAmount(DebtInput, out debt))
        {
            error = InvalidDebtMessage;
        }

        if (error is null)
        {
            card = Build(existing, today, limit, debt, closingDay, dueDay);
        }

        return error;
    }

    private CreditCard Build(CreditCard? existing, DateOnly today, decimal limit, decimal debt, int closingDay, int dueDay)
    {
        var card = (existing ?? new CreditCard { PaymentStrategy = CreditCardPaymentStrategy.FullStatement, BalanceAsOfDate = today }) with
        {
            Name = Name.Trim(),
            Bank = Bank.Trim(),
            Limit = limit,
            StatementClosingDay = closingDay,
            PaymentDueDay = dueDay,
            MinimumPaymentRate = existing is null || existing.Limit != limit || existing.MinimumPaymentRate == 0m
                ? CreditCardRules.ResolveMinimumPaymentRate(limit)
                : existing.MinimumPaymentRate
        };

        // S63-4: borç değiştiyse faizsiz ekstreleşmemiş harcama olur; değişmediyse devreden ve tarih korunur.
        return ShowsDebt && (existing is null || CurrentDebt(existing) != debt)
            ? card with { CarriedBalance = 0m, UnbilledSpending = debt, BalanceAsOfDate = today }
            : card;
    }

    // Sayı olmayan gün 0 sayılır; aralık dışı değer doğrulamada yakalanır.
    private static int ParseDay(string text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var day) ? day : 0;

    private static decimal CurrentDebt(CreditCard card) => card.CarriedBalance + card.UnbilledSpending;

    private (string, string, string, string, string, string) Current() =>
        (Name, Bank, LimitInput, DebtInput, ClosingDayInput, DueDayInput);
}
