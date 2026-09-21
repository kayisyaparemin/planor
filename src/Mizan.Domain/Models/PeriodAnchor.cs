using Mizan.Domain.Calculations;

namespace Mizan.Domain.Models;

/// <summary>
/// Nakit akış döneminin hangi takvim kuralına göre döndüğünü belirleyen dönem çapası.
/// Gelir gününden bağımsız bir değer nesnesidir; mevcut aşamada ayın belirli bir gününü (DayOfMonth)
/// temsil ederken, ileride haftalık veya iki haftalık periyotları destekleyecek esnekliği sağlar (S1).
/// </summary>
public sealed record PeriodAnchor
{
    /// <summary>
    /// Dönemin başladığı ayın gününü (1-31) alır.
    /// </summary>
    public int DayOfMonth { get; }

    /// <summary>
    /// Belirtilen ayın günüyle yeni bir dönem çapası oluşturur.
    /// </summary>
    /// <param name="dayOfMonth">Dönem başlangıç günü (1-31).</param>
    public PeriodAnchor(int dayOfMonth)
    {
        CalendarRules.ValidateDay(dayOfMonth);
        DayOfMonth = dayOfMonth;
    }
}
