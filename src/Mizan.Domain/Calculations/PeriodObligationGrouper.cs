using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kullanıcının borç ve harcama yükümlülüklerini doğal dönemsellik ilkesine göre
/// ilgili nakit akış dönemlerine [Start, End) gruplayan ve ufuk dışı kalemleri ayrıştıran saf hesaplayıcı.
/// </summary>
public sealed class PeriodObligationGrouper
{
    /// <summary>
    /// Verilen yükümlülükleri nakit akış dönemlerinin yarı açık aralıklarına göre gruplar.
    /// </summary>
    /// <param name="periods">Kronolojik sırayla birbirini takip eden nakit akış dönemleri.</param>
    /// <param name="obligations">Gruplanacak tüm borç ve harcama yükümlülükleri.</param>
    /// <returns>Dönem gruplarını ve dönem dışı kalemleri içeren bütüncül plan.</returns>
    /// <exception cref="ArgumentNullException">Parametrelerden biri null ise fırlatılır.</exception>
    /// <exception cref="ArgumentException">Dönem listesi boş ise fırlatılır.</exception>
    public PeriodObligationPlan Group(
        IReadOnlyList<CashFlowPeriod> periods,
        IEnumerable<ObligationItem> obligations)
    {
        ArgumentNullException.ThrowIfNull(periods);
        ArgumentNullException.ThrowIfNull(obligations);

        if (periods.Count == 0)
        {
            throw new ArgumentException("En az bir dönem gereklidir.", nameof(periods));
        }

        var ordered = obligations
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.Name)
            .ToArray();

        var firstStart = periods[0].Start;
        var lastEnd = periods[^1].End;

        var groups = new List<PeriodObligationGroup>(periods.Count);
        foreach (var period in periods)
        {
            var itemsInPeriod = ordered.Where(x => period.Contains(x.DueDate)).ToArray();
            groups.Add(new PeriodObligationGroup(period, itemsInPeriod));
        }

        var preFirst = ordered.Where(x => x.DueDate < firstStart).ToArray();
        var postHorizon = ordered.Where(x => x.DueDate >= lastEnd).ToArray();

        return new PeriodObligationPlan(groups, preFirst, postHorizon);
    }
}
