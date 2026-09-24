using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Finansal plan üzerinden 12 dönemlik projeksiyonu koşturan ve sunum katmanının
/// ihtiyaç duyduğu Dashboard özeti ile dönem takvimini derleyen uygulama servisidir.
/// Saf Domain projeksiyon motorunu uygulama kullanım senaryolarına bağlamak için vardır.
/// </summary>
public sealed class FinancialProjectionService(
    FinancialProjectionCalculator projectionCalculator)
{
    private readonly FinancialProjectionCalculator _projectionCalculator =
        projectionCalculator ?? throw new ArgumentNullException(nameof(projectionCalculator));

    /// <summary>
    /// Kullanıcının ana ekranında (Dashboard) gösterilecek aktif dönem durumunu,
    /// yaklaşan ödemeleri, 12 dönem sonu nakit dengesini ve en sıkışık dönemi üretir.
    /// </summary>
    public DashboardSnapshot BuildDashboard(
        FinancialPlan plan,
        DateOnly asOf,
        DateOnly? firstPeriodStartDate = null)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var projection = _projectionCalculator.CalculatePlan(plan, asOf, 12, firstPeriodStartDate);
        var periods = projection.Periods;
        var preFirst = ExtractPreFirstObligations(projection.ObligationPlan.PreFirstPeriodItems, asOf);
        var upcoming = ExtractUpcomingPayments(preFirst, periods, asOf);
        var tightest = periods.OrderBy(x => x.EndingBalance).ThenBy(x => x.Period.Start).First();

        return new DashboardSnapshot
        {
            CurrentPeriod = periods[0],
            PreFirstPeriodObligations = preFirst,
            UpcomingPayments = upcoming,
            TwelvePeriodEndingBalance = periods[^1].EndingBalance,
            TightestPeriod = tightest,
            HasUndeterminedCardPayments = periods.Any(x => x.HasUndeterminedCardPayment),
            ProjectionAnchorDate = plan.Settings.ProjectionAnchorDate,
            ProjectionOpeningBalance = plan.Settings.ProjectionOpeningBalance,
            TwelvePeriodCreditCardInterest = projection.TotalCreditCardInterest,
            TwelvePeriodDeficitFinancingInterest = projection.TotalDeficitFinancingInterest,
            TwelvePeriodTotalInterest = projection.TotalInterestCost
        };
    }

    /// <summary>
    /// Finansal plan için belirtilen ufukta (varsayılan 12 dönem) nakit akış dönem projeksiyonlarını üretir.
    /// </summary>
    public IReadOnlyList<CashFlowPeriodProjection> BuildFuturePeriods(
        FinancialPlan plan,
        DateOnly asOf,
        int periodCount = 12,
        DateOnly? firstPeriodStartDate = null)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return _projectionCalculator.Calculate(
            plan,
            asOf,
            periodCount,
            firstPeriodStartDate);
    }

    private static ObligationItem[] ExtractPreFirstObligations(
        IEnumerable<ObligationItem> items,
        DateOnly asOf) =>
        items.Where(x => x.DueDate >= asOf).OrderBy(x => x.DueDate).ThenByDescending(x => x.Amount).ToArray();

    private static ObligationItem[] ExtractUpcomingPayments(
        IReadOnlyList<ObligationItem> preFirst,
        IEnumerable<CashFlowPeriodProjection> periods,
        DateOnly asOf) =>
        preFirst.Concat(periods
            .SelectMany(x => x.MandatoryItems)
            .Where(x => x.DueDate >= asOf)
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => x.Amount))
            .Take(5)
            .ToArray();
}
