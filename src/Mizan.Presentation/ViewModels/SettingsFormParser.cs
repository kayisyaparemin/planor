using System.Globalization;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Ayarlar formundaki sayısal ve ondalık girdileri Türkçe ve evrensel kültür kurallarıyla ayrıştıran ve doğrulayan yardımcı sınıf.
/// </summary>
internal static class SettingsFormParser
{
    /// <summary>Girdi metnini güvenli şekilde decimal sayıya dönüştürür.</summary>
    public static bool TryParseDecimal(string? text, out decimal value)
    {
        return StatementEntryViewModel.TryParseAmount(text, out value);
    }

    /// <summary>Ayarlar form girdilerini ayrıştırır ve doğrular; hata varsa hata iletisini döner.</summary>
    public static string? TryBuildSettings(
        UserSettings existing,
        string anchorDayText,
        string allowanceText,
        string cardRateText,
        string deficitRateText,
        out UserSettings settings)
    {
        settings = existing;
        if (!int.TryParse(anchorDayText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var day) || day is < 1 or > 31)
        {
            return "Dönem başlangıç günü 1 ile 31 arasında olmalıdır.";
        }

        var allowance = 0m;
        if (!string.IsNullOrWhiteSpace(allowanceText) && !TryParseDecimal(allowanceText, out allowance))
        {
            return "Yaşam gideri tutarını sayı olarak gir.";
        }

        var rateError = TryParseRates(cardRateText, deficitRateText, out var cardRate, out var deficitRate);
        if (rateError is not null)
        {
            return rateError;
        }

        settings = existing with
        {
            PeriodAnchor = new PeriodAnchor(day),
            PeriodVariableExpenseAllowance = allowance,
            CreditCardCarryInterestRate = cardRate,
            DeficitFinancingInterestRate = deficitRate
        };

        return ValidateSettings(settings);
    }

    private static string? TryParseRates(string cardRateText, string deficitRateText, out decimal cardRate, out decimal deficitRate)
    {
        cardRate = 0m;
        deficitRate = 0m;
        if (!TryParseDecimal(cardRateText, out var cardRatePct) || cardRatePct is < 0m or > 100m)
        {
            return "Kredi kartı faizi %0 ile %100 arasında olmalıdır.";
        }
        if (!TryParseDecimal(deficitRateText, out var deficitRatePct) || deficitRatePct is < 0m or > 100m)
        {
            return "Finansman açığı faizi %0 ile %100 arasında olmalıdır.";
        }
        cardRate = cardRatePct / 100m;
        deficitRate = deficitRatePct / 100m;
        return null;
    }

    private static string? ValidateSettings(UserSettings settings)
    {
        try
        {
            UserSettingsValidator.Validate(settings);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
