using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Belirli bir nakit akış dönemi için aktif düzenli gelir akışlarını ve tek seferlik arızi gelirleri
/// eşleştirerek dönemsel gelir özetini ve kalemlerini deterministik olarak hesaplayan saf Domain hesaplayıcısı.
/// Çoklu gelir akışlarını (S2), gelirin kendi ödeme gününü (S3) ve çapa öncesi gelir penceresini (BR-INCOME-01) destekler.
/// </summary>
public sealed class IncomeProjectionCalculator
{
    private readonly IncomeResolver _incomeResolver;

    /// <summary>
    /// Gerekli çözümleyici bağımlılığıyla yeni bir gelir projeksiyon hesaplayıcısı başlatır (Kural M1).
    /// </summary>
    /// <param name="incomeResolver">Düzenli gelir akışlarını çözümleyen saf hesaplayıcı.</param>
    public IncomeProjectionCalculator(IncomeResolver incomeResolver)
    {
        ArgumentNullException.ThrowIfNull(incomeResolver);
        _incomeResolver = incomeResolver;
    }

    /// <summary>
    /// Dönem için geçerli düzenli gelir akışlarını geçmişleriyle birlikte çözümler ve tek seferlik gelirlerle birleştirerek dönem gelir özetini hesaplar.
    /// </summary>
    /// <param name="period">Gelirlerin hesaplanacağı nakit akış dönemi.</param>
    /// <param name="streams">Tanımlı düzenli gelir akışları kümesi.</param>
    /// <param name="history">Düzenli gelirlere ait tutar geçmişi kayıtları.</param>
    /// <param name="adHocIncomes">Tek seferlik arızi gelirler kümesi.</param>
    /// <param name="prePeriodIncomeStart">Verildiğinde, bu tarih ile dönem başlangıcı arasındaki arızi gelirleri de ilk döneme dahil eden pencere başlangıcı.</param>
    /// <returns>Döneme ait sıralı gelir kalemleri ve toplamları.</returns>
    public IncomeProjectionSummary Calculate(
        CashFlowPeriod period,
        IEnumerable<RecurringIncome> streams,
        IEnumerable<IncomeAmountHistory> history,
        IEnumerable<AdHocIncome> adHocIncomes,
        DateOnly? prePeriodIncomeStart = null)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(streams);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(adHocIncomes);

        var resolved = _incomeResolver.Resolve(period.Start, streams, history);
        return Calculate(period, resolved, adHocIncomes, prePeriodIncomeStart);
    }

    /// <summary>
    /// Önceden çözümlenmiş aktif düzenli gelirleri tek seferlik gelirlerle birleştirerek dönem gelir özetini hesaplar.
    /// </summary>
    /// <param name="period">Gelirlerin hesaplanacağı nakit akış dönemi.</param>
    /// <param name="resolvedRecurringIncomes">Dönem başlangıcı itibarıyla çözümlenmiş aktif düzenli gelirler.</param>
    /// <param name="adHocIncomes">Tek seferlik arızi gelirler kümesi.</param>
    /// <param name="prePeriodIncomeStart">Verildiğinde, bu tarih ile dönem başlangıcı arasındaki arızi gelirleri de ilk döneme dahil eden pencere başlangıcı.</param>
    /// <returns>Döneme ait sıralı gelir kalemleri ve toplamları.</returns>
    public IncomeProjectionSummary Calculate(
        CashFlowPeriod period,
        IEnumerable<ActiveRecurringIncome> resolvedRecurringIncomes,
        IEnumerable<AdHocIncome> adHocIncomes,
        DateOnly? prePeriodIncomeStart = null)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(resolvedRecurringIncomes);
        ArgumentNullException.ThrowIfNull(adHocIncomes);

        var items = new List<IncomeProjectionItem>();
        items.AddRange(BuildRecurringItems(period, resolvedRecurringIncomes));
        items.AddRange(BuildAdHocItems(period, adHocIncomes, prePeriodIncomeStart));

        var ordered = items.OrderBy(x => x.SourceDate).ThenBy(x => x.Name).ToArray();
        var recurringTotal = MoneyRules.Round(ordered.Where(x => x.Type == IncomeSourceType.Recurring).Sum(x => x.Amount));
        var adHocTotal = MoneyRules.Round(ordered.Where(x => x.Type == IncomeSourceType.AdHoc).Sum(x => x.Amount));
        var totalIncome = recurringTotal + adHocTotal;

        return new IncomeProjectionSummary(ordered, recurringTotal, adHocTotal, totalIncome);
    }

    private static IEnumerable<IncomeProjectionItem> BuildRecurringItems(
        CashFlowPeriod period,
        IEnumerable<ActiveRecurringIncome> recurringIncomes)
    {
        foreach (var recurring in recurringIncomes)
        {
            var paymentDate = ResolvePaymentDate(period, recurring.PaymentDay);
            var name = string.IsNullOrWhiteSpace(recurring.Name) ? "Düzenli Gelir" : recurring.Name;

            yield return new IncomeProjectionItem(
                name,
                IncomeSourceType.Recurring,
                paymentDate,
                recurring.Amount)
            {
                RecurringIncomeId = recurring.RecurringIncomeId
            };
        }
    }

    private static IEnumerable<IncomeProjectionItem> BuildAdHocItems(
        CashFlowPeriod period,
        IEnumerable<AdHocIncome> adHocIncomes,
        DateOnly? prePeriodIncomeStart)
    {
        foreach (var adHoc in adHocIncomes)
        {
            if (period.Contains(adHoc.ExactDate) ||
                IsWithinPrePeriodWindow(adHoc.ExactDate, prePeriodIncomeStart, period.Start))
            {
                var name = string.IsNullOrWhiteSpace(adHoc.Description) ? "Tek Seferlik Gelir" : adHoc.Description;

                yield return new IncomeProjectionItem(
                    name,
                    IncomeSourceType.AdHoc,
                    adHoc.ExactDate,
                    adHoc.Amount)
                {
                    AdHocIncomeId = adHoc.Id
                };
            }
        }
    }

    private static bool IsWithinPrePeriodWindow(
        DateOnly incomeDate,
        DateOnly? windowStart,
        DateOnly periodStart) =>
        windowStart is { } start &&
        incomeDate >= start &&
        incomeDate < periodStart;

    private static DateOnly ResolvePaymentDate(CashFlowPeriod period, int paymentDay)
    {
        var candidate1 = CalendarRules.ResolveDay(period.Start.Year, period.Start.Month, paymentDay);
        if (period.Contains(candidate1))
        {
            return candidate1;
        }

        var candidate2 = CalendarRules.ResolveDay(period.End.Year, period.End.Month, paymentDay);
        if (period.Contains(candidate2))
        {
            return candidate2;
        }

        return period.Start;
    }
}
