namespace Mizan.Presentation.Onboarding.Support;

/// <summary>
/// Kurulum sihirbazı form girdilerini doğrulayan saf yardımcıdır.
/// </summary>
internal static class OnboardingValidator
{
    public static string? ValidatePeriodDay(int day)
    {
        if (day is < 1 or > 31) { return "Dönem başlangıç günü 1 ile 31 arasında olmalıdır."; }
        return null;
    }

    public static string? ValidateIncome(string? name, decimal? amount, int day)
    {
        if (string.IsNullOrWhiteSpace(name)) { return "Lütfen gelirin adını gir."; }
        if (amount is null or <= 0m) { return "Lütfen geçerli bir aylık net tutar gir."; }
        if (day is < 1 or > 31) { return "Ödeme günü 1 ile 31 arasında olmalıdır."; }
        return null;
    }

    public static string? ValidateCard(string? name, decimal? limit, int closeDay, int dueDay)
    {
        if (string.IsNullOrWhiteSpace(name)) { return "Lütfen kartın adını gir."; }
        if (limit is null or <= 0m) { return "Lütfen kart limitini gir."; }
        if (closeDay is < 1 or > 31 || dueDay is < 1 or > 31) { return "Hesap kesim ve son ödeme günleri 1 ile 31 arasında olmalıdır."; }
        return null;
    }

    public static string? ValidateLoan(string? name, decimal? payment, int? installments, int day)
    {
        if (string.IsNullOrWhiteSpace(name)) { return "Lütfen kredinin adını gir."; }
        if (payment is null or <= 0m) { return "Lütfen aylık taksit tutarını gir."; }
        if (installments is null or <= 0) { return "Lütfen kalan taksit sayısını gir."; }
        if (day is < 1 or > 31) { return "Taksit günü 1 ile 31 arasında olmalıdır."; }
        return null;
    }

    public static string? ValidateExpense(string? name, decimal? amount)
    {
        if (string.IsNullOrWhiteSpace(name)) { return "Lütfen ödeme adını gir."; }
        if (amount is null or <= 0m) { return "Lütfen ödeme tutarını gir."; }
        return null;
    }
}
