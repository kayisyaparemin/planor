using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Verilen tarih ve dönem çapasına göre nakit akış dönemlerini ve mutabakat tarihlerini
/// takvim kurallarına (BR-CALENDAR-01) ve yarı açık aralık ilkesine uygun olarak hesaplar.
/// </summary>
public sealed class CashFlowPeriodCalculator
{
    /// <summary>
    /// Belirtilen tarihin ait olduğu nakit akış dönemini hesaplar.
    /// </summary>
    /// <param name="date">Dönemi sorgulanan tarih.</param>
    /// <param name="anchor">Dönem çapası.</param>
    /// <returns>Tarihi kapsayan nakit akış dönemi.</returns>
    public CashFlowPeriod GetPeriod(DateOnly date, PeriodAnchor anchor)
    {
        ArgumentNullException.ThrowIfNull(anchor);

        var currentAnchorDate = CalendarRules.ResolveDay(date.Year, date.Month, anchor.DayOfMonth);
        if (date >= currentAnchorDate)
        {
            return new CashFlowPeriod(
                currentAnchorDate,
                CalendarRules.AddMonthsKeepingDay(currentAnchorDate, 1, anchor.DayOfMonth));
        }

        var previousAnchorDate = CalendarRules.AddMonthsKeepingDay(currentAnchorDate, -1, anchor.DayOfMonth);
        return new CashFlowPeriod(previousAnchorDate, currentAnchorDate);
    }

    /// <summary>
    /// Belirtilen başlangıç tarihinden itibaren birbirini takip eden n adet nakit akış dönemini üretir.
    /// </summary>
    /// <param name="asOf">Dönem dizisinin oluşturulacağı referans tarihi.</param>
    /// <param name="anchor">Dönem çapası.</param>
    /// <param name="count">Üretilecek dönem sayısı (1-60 arası).</param>
    /// <returns>Sıralı nakit akış dönemleri listesi.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Dönem sayısı 1 ile 60 arasında değilse fırlatılır.</exception>
    public IReadOnlyList<CashFlowPeriod> GetPeriods(DateOnly asOf, PeriodAnchor anchor, int count)
    {
        ArgumentNullException.ThrowIfNull(anchor);

        if (count is < 1 or > 60)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Dönem sayısı 1 ile 60 arasında olmalıdır.");
        }

        var first = GetPeriod(asOf, anchor);
        return Enumerable.Range(0, count)
            .Select(index =>
            {
                var start = CalendarRules.AddMonthsKeepingDay(first.Start, index, anchor.DayOfMonth);
                var end = CalendarRules.AddMonthsKeepingDay(first.Start, index + 1, anchor.DayOfMonth);
                return new CashFlowPeriod(start, end);
            })
            .ToArray();
    }

    /// <summary>
    /// Belirtilen tarihe eşit veya o tarihten sonraki ilk dönem başlangıç tarihini bulur.
    /// </summary>
    /// <param name="date">Sorgu referans tarihi.</param>
    /// <param name="anchor">Dönem çapası.</param>
    /// <returns>Dönem başlangıç tarihi.</returns>
    public DateOnly GetFirstPeriodStartOnOrAfter(DateOnly date, PeriodAnchor anchor)
    {
        ArgumentNullException.ThrowIfNull(anchor);

        var containingPeriod = GetPeriod(date, anchor);
        return date == containingPeriod.Start
            ? containingPeriod.Start
            : containingPeriod.End;
    }

    /// <summary>
    /// Belirtilen tarihten kesinlikle sonraki ilk dönem başlangıç tarihini bulur.
    /// </summary>
    /// <param name="date">Sorgu referans tarihi.</param>
    /// <param name="anchor">Dönem çapası.</param>
    /// <returns>Kesinlikle sonraki ilk dönem başlangıç tarihi.</returns>
    public DateOnly GetFirstPeriodStartStrictlyAfter(DateOnly date, PeriodAnchor anchor)
    {
        ArgumentNullException.ThrowIfNull(anchor);

        var first = GetFirstPeriodStartOnOrAfter(date, anchor);
        return first > date
            ? first
            : CalendarRules.AddMonthsKeepingDay(first, 1, anchor.DayOfMonth);
    }

    /// <summary>
    /// Verilen anlık durum (snapshot) tarihinden sonraki ilk dönem kapanış (mutabakat) tarihini bulur.
    /// </summary>
    /// <param name="snapshotDate">Değerleme / anlık durum tarihi.</param>
    /// <param name="anchor">Dönem çapası.</param>
    /// <returns>Sonraki dönem kapanış tarihi.</returns>
    public DateOnly GetNextSettlementDate(DateOnly snapshotDate, PeriodAnchor anchor)
    {
        ArgumentNullException.ThrowIfNull(anchor);

        return GetPeriod(snapshotDate, anchor).End;
    }
}
