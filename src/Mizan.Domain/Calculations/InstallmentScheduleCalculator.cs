using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Toplam bir harcama veya borç tutarını takvim ve kuruş korunum kurallarına uygun olarak
/// aylık taksitlere bölen saf hesaplayıcı.
/// </summary>
public sealed class InstallmentScheduleCalculator
{
    /// <summary>
    /// Verilen toplam tutarı belirtilen taksit adedine böler; kuruş artığını son taksite ekler ve
    /// ay sonu kenetlenmesini koruyarak her taksitin vade tarihini belirler.
    /// </summary>
    /// <param name="total">Bölüştürülecek toplam tutar (sıfırdan büyük olmalıdır).</param>
    /// <param name="count">Taksit sayısı (1 ile 120 arasında olmalıdır).</param>
    /// <param name="firstDate">İlk taksitin vadesi.</param>
    /// <returns>Tarih ve tutar ikililerinden oluşan takvimli taksit listesi.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Tutar sıfır/negatifse veya taksit sayısı 1-120 aralığı dışındaysa fırlatılır.</exception>
    /// <exception cref="ArgumentException">İlk ödeme tarihi varsayılan (default) ise fırlatılır.</exception>
    public IReadOnlyList<ScheduledAmount> Split(
        decimal total,
        int count,
        DateOnly firstDate)
    {
        if (firstDate == default)
        {
            throw new ArgumentException("İlk ödeme tarihi gereklidir.", nameof(firstDate));
        }

        var amounts = MoneyRules.Distribute(total, count);

        return Enumerable.Range(0, count)
            .Select(index => new ScheduledAmount(
                CalendarRules.AddMonthsKeepingDay(firstDate, index, firstDate.Day),
                amounts[index]))
            .ToArray();
    }
}
