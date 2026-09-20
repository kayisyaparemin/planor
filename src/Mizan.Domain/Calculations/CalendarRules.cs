namespace Mizan.Domain.Calculations;

/// <summary>
/// Ay sonu ve artık yıl kaynaklı tarih kaymalarını (drift) önleyen takvim kuralları.
/// Kullanıcının tercih ettiği ödeme veya kesim gününü hafızada tutarak kısa aylarda ay sonuna
/// kenetler, uzun aylarda ise asıl günü geri kazanır (BR-CALENDAR-01).
/// </summary>
public static class CalendarRules
{
    /// <summary>
    /// Belirtilen yıl ve ay için kullanıcının tercih ettiği günü takvim sınırlarına göre çözer.
    /// Ayın gün sayısını aşan tercihler ayın son gününe kenetlenir.
    /// </summary>
    /// <param name="year">Hedef takvim yılı.</param>
    /// <param name="month">Hedef takvim ayı (1-12).</param>
    /// <param name="preferredDay">Kullanıcının tercih ettiği gün (1-31).</param>
    /// <returns>Geçerli takvim tarihi.</returns>
    public static DateOnly ResolveDay(int year, int month, int preferredDay)
    {
        ValidateDay(preferredDay);
        return new DateOnly(year, month, Math.Min(preferredDay, DateTime.DaysInMonth(year, month)));
    }

    /// <summary>
    /// Verilen tarihe belirtilen ay sayısını eklerken kullanıcının asıl tercih ettiği günü korur.
    /// Standart AddMonths metodunun aksine, kısa aydan sonra uzun aya geçildiğinde tercih edilen güne geri döner.
    /// </summary>
    /// <param name="date">Başlangıç tarihi.</param>
    /// <param name="months">Eklenecek ay sayısı.</param>
    /// <param name="preferredDay">Kullanıcının orijinal tercih ettiği gün (1-31).</param>
    /// <returns>Tercih edilen gün korunarak hesaplanmış hedef tarih.</returns>
    public static DateOnly AddMonthsKeepingDay(DateOnly date, int months, int preferredDay)
    {
        var targetDate = date.AddMonths(months);
        return ResolveDay(targetDate.Year, targetDate.Month, preferredDay);
    }

    /// <summary>
    /// Tercih edilen günün geçerli bir takvim günü (1-31) olup olmadığını doğrular.
    /// Geçersiz değerlerde anında hata fırlatarak hatalı durumun sisteme yayılmasını engeller.
    /// </summary>
    /// <param name="day">Doğrulanacak gün değeri.</param>
    /// <exception cref="ArgumentOutOfRangeException">Gün 1 ile 31 arasında değilse fırlatılır.</exception>
    public static void ValidateDay(int day)
    {
        if (day is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(day), "Gün 1 ile 31 arasında olmalıdır.");
        }
    }
}
