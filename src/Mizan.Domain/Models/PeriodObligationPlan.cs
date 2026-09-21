namespace Mizan.Domain.Models;

/// <summary>
/// Tüm projeksiyon dönemi boyunca doğal dönemsellikle gruplanmış yükümlülük planını
/// ve projeksiyon ufku dışında kalan kalemleri bir arada sunan sonuç sözleşmesi.
/// </summary>
public sealed record PeriodObligationPlan(
    IReadOnlyList<PeriodObligationGroup> Groups,
    IReadOnlyList<ObligationItem> PreFirstPeriodItems,
    IReadOnlyList<ObligationItem> PostHorizonItems)
{
    /// <summary>
    /// Belirtilen nakit akış dönemine ait yükümlülük grubunu getirir.
    /// </summary>
    public PeriodObligationGroup this[CashFlowPeriod period] =>
        Groups.FirstOrDefault(x => x.Period == period)
        ?? throw new KeyNotFoundException($"Dönem plana dahil değil: {period.Start:yyyy-MM-dd}");

    /// <summary>
    /// Verilen vade tarihinin düştüğü nakit akış döneminin yükümlülük grubunu bulur.
    /// </summary>
    public PeriodObligationGroup? FindGroup(DateOnly paymentDate) =>
        Groups.FirstOrDefault(x => x.Period.Contains(paymentDate));
}
