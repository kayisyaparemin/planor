using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Domain.Models;
using Mizan.Presentation.Onboarding.Support;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Gelir formunun çocuğu: düzenli gelirin tanımını (ad, ödeme günü; yeni gelirde aylık net tutar)
/// taşır, doğrular ve gelire çevirir. Tutar yalnız yeni gelirde sorulur: kayıtlı gelirin tutarı
/// etkin tarihli geçmiştir, üzerine yazılmaz (EK-V6d, S67-2, kural 05).
/// </summary>
public sealed partial class IncomeDefinitionViewModel : ObservableObject
{
    private (string Name, string Amount, string PaymentDay) _loaded;

    [ObservableProperty] private bool showsAmount = true;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string amountInput = string.Empty;
    [ObservableProperty] private string paymentDayInput = string.Empty;

    /// <summary>Alanlar dolduruldukları andan beri değişti mi.</summary>
    public bool HasChanges => Current() != _loaded;

    /// <summary>Alanları gelirden doldurur; gelir yoksa boş yeni gelir formu.</summary>
    public void Fill(RecurringIncome? income)
    {
        ShowsAmount = income is null;
        Name = income?.Name ?? string.Empty;
        AmountInput = string.Empty;
        PaymentDayInput = income?.PaymentDay.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _loaded = Current();
    }

    /// <summary>
    /// Alanları doğrulayıp geliri kurar. Geçersizse kullanıcıya gösterilecek mesajı döner ve gelir
    /// kurulmaz. Ad, tutar ve gün kurulumla aynı kurallardan geçer; düzenleme yüklenen gelirin üstüne
    /// kurulur, aktiflik korunur.
    /// </summary>
    public string? TryBuild(RecurringIncome? existing, out RecurringIncome? income)
    {
        income = null;
        var day = ParseDay(PaymentDayInput);
        var error = ShowsAmount
            ? OnboardingValidator.ValidateIncome(Name, ParsedAmount(), day)
            : OnboardingValidator.ValidateIncomeDefinition(Name, day);
        if (error is null)
        {
            income = (existing ?? new RecurringIncome()) with { Name = Name.Trim(), PaymentDay = day };
        }

        return error;
    }

    /// <summary>Yeni gelirin bugünden yürürlüğe giren ilk tutarını kurar (S67-2).</summary>
    public IncomeAmountHistory BuildFirstAmount(Guid incomeId, DateOnly today) =>
        new() { RecurringIncomeId = incomeId, Amount = ParsedAmount() ?? 0m, EffectiveDate = today };

    private decimal? ParsedAmount() =>
        StatementEntryViewModel.TryParseAmount(AmountInput, out var amount) ? amount : null;

    // Sayı olmayan gün 0 sayılır; aralık dışı değer doğrulamada yakalanır.
    private static int ParseDay(string text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var day) ? day : 0;

    private (string, string, string) Current() => (Name, AmountInput, PaymentDayInput);
}
