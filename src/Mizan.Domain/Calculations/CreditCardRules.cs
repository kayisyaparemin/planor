namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi kartı iş kuralları, BDDK mevzuatına dayalı yasal asgari ödeme oranları ve
/// standart bankacılık vade teamüllerini hesaplayan saf kural sınıfı.
/// </summary>
public static class CreditCardRules
{
    private const decimal BddkLimitThreshold = 25_000m;
    private const decimal LowLimitMinimumRate = 0.20m;
    private const decimal HighLimitMinimumRate = 0.40m;

    /// <summary>
    /// BDDK mevzuatına göre kredi kartı limitine bağlı olarak uygulanması gereken asgari ödeme oranını belirler.
    /// Limit 25.000 TL ve altında ise %20 (0.20), 25.000 TL üzerinde ise %40 (0.40) döner.
    /// </summary>
    /// <param name="limit">Kredi kartı limiti.</param>
    /// <returns>Geçerli asgari ödeme oranı.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Limit negatif olduğunda fırlatılır.</exception>
    public static decimal ResolveMinimumPaymentRate(decimal limit)
    {
        if (limit < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Kredi kartı limiti negatif olamaz.");
        }

        return limit <= BddkLimitThreshold ? LowLimitMinimumRate : HighLimitMinimumRate;
    }

    /// <summary>
    /// Hesap kesim gününe göre standart bankacılık teamülü olan 10 gün sonrasını hesaplar (1-31 aralığında).
    /// </summary>
    /// <param name="statementClosingDay">Hesap kesim günü (1-31).</param>
    /// <returns>Varsayılan son ödeme günü.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Hesap kesim günü 1-31 aralığında değilse fırlatılır.</exception>
    public static int ResolveDefaultPaymentDueDay(int statementClosingDay)
    {
        CalendarRules.ValidateDay(statementClosingDay);

        var dueDay = statementClosingDay + 10;
        return dueDay > 31 ? dueDay - 31 : dueDay;
    }
}
