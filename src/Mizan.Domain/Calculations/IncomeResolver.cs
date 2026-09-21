using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Düzenli gelir akışlarının belirli bir referans tarihte yürürlükte olan güncel tutarlarını çözen saf hesaplayıcı.
/// Çoklu akışları destekler; her akış için etkin tarihi referans tarihten küçük veya eşit olan en güncel kaydı seçer.
/// </summary>
public sealed class IncomeResolver
{
    /// <summary>
    /// Verilen referans tarihi itibarıyla aktif olan tüm düzenli gelir akışlarının yürürlükteki tutarlarını çözümler.
    /// </summary>
    /// <param name="asOfDate">Geçerlilik tarihi (genellikle dönem başlangıcı).</param>
    /// <param name="streams">Kullanıcının tanımladığı düzenli gelir akışları kümesi.</param>
    /// <param name="history">Düzenli gelirlere ait etkin tarihli tutar/zam kayıtları.</param>
    /// <returns>Referans tarihte geçerli olan aktif gelir akışlarının çözümlenmiş listesi.</returns>
    public IReadOnlyList<ActiveRecurringIncome> Resolve(
        DateOnly asOfDate,
        IEnumerable<RecurringIncome> streams,
        IEnumerable<IncomeAmountHistory> history)
    {
        ArgumentNullException.ThrowIfNull(streams);
        ArgumentNullException.ThrowIfNull(history);

        var activeStreams = streams.Where(s => s.IsActive).ToList();
        if (activeStreams.Count == 0)
        {
            return [];
        }

        var historyList = history.ToList();
        var resolvedList = new List<ActiveRecurringIncome>();

        foreach (var stream in activeStreams)
        {
            var latest = historyList
                .Where(h => h.RecurringIncomeId == stream.Id && h.EffectiveDate <= asOfDate)
                .OrderByDescending(h => h.EffectiveDate)
                .ThenByDescending(h => h.Id)
                .FirstOrDefault();

            if (latest is not null)
            {
                resolvedList.Add(new ActiveRecurringIncome
                {
                    RecurringIncomeId = stream.Id,
                    Name = stream.Name,
                    PaymentDay = stream.PaymentDay,
                    Amount = latest.Amount,
                    EffectiveDate = latest.EffectiveDate
                });
            }
        }

        return resolvedList;
    }
}
