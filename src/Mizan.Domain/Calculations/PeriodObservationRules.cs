using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Dönem içindeki bakiye gözlemlerinin üç kuralı tek yerde (S68): bir gözlem hangi güne yazılabilir,
/// aynı güne ikinci giriş ne olur, gidişat hangi gözlemden hesaplanır. Ana sayfa grafiği her gözlemi
/// bir nokta olarak çizer; kaydetme, kaydetmeden önizleme ve gidişat aynı kurala bakmazsa grafikteki
/// noktalar ile ekrandaki tahmin birbirini tutmaz. Eskiden dönem başına tek kayıt vardı ve her giriş
/// öncekinin üzerine yazıyordu; bu kuralların hiçbirine gerek yoktu.
/// </summary>
public static class PeriodObservationRules
{
    /// <summary>
    /// Verilen güne gözlem yazılıp yazılamayacağını söyler: gün dönemin içinde olmalı ve bugünden sonra
    /// olamaz. Dönem bitmiş ama kapanmamışsa (kapanış ertelendi) hiçbir güne yazılamaz; o bakiyede yeni
    /// dönemin hareketleri vardır ve biten dönemin sonunu kapanış belirler.
    /// </summary>
    /// <param name="period">Açık dönemin yarı açık aralığı.</param>
    /// <param name="observedOn">Gözlemin yazılmak istendiği gün.</param>
    /// <param name="today">Bugün.</param>
    public static bool CanObserveOn(CashFlowPeriod period, DateOnly observedOn, DateOnly today) =>
        today < period.End && period.Contains(observedOn) && observedOn <= today;

    /// <summary>
    /// Yeni gözlemi dönemin gözlemlerine ekler. Aynı günün gözlemi varsa yeni giriş onun yerine geçer:
    /// gün başına tek gözlem vardır, yanlış girilen tutar aynı gün düzeltilir. Sonuç güne göre sıralıdır.
    /// </summary>
    /// <param name="observations">Aynı dönemin kayıtlı gözlemleri.</param>
    /// <param name="entry">Yeni giriş.</param>
    public static IReadOnlyList<PeriodObservation> Record(
        IReadOnlyList<PeriodObservation> observations,
        PeriodObservation entry) =>
        observations
            .Where(x => x.ObservedOn != entry.ObservedOn)
            .Append(entry)
            .OrderBy(x => x.ObservedOn)
            .ToArray();

    /// <summary>
    /// Gidişatın hesaplanacağı gözlemi, yani en geç tarihli olanı döner; gözlem yoksa <c>null</c>.
    /// Giriş sırasına bakılmaz: sonradan girilen geriye tarihli bir bakiye son gözlemi değiştirmez.
    /// </summary>
    /// <param name="observations">Aynı dönemin gözlemleri, herhangi bir sırada.</param>
    public static PeriodObservation? Latest(IEnumerable<PeriodObservation> observations) =>
        observations.MaxBy(x => x.ObservedOn);
}
