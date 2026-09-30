using Mizan.Application.Models;

namespace Mizan.Presentation.Charts;

/// <summary>
/// Açık dönemin bakiye rotasını (S71) trend grafiğinin ham verisine çevirir. Ana sayfa ile "Bakiye gir"
/// önizlemesi aynı rotayı çizer; ikisi grafiği ayrı ayrı kurarsa rol anahtarları ya da nokta işaretleri bir
/// gün ayrışır ve kullanıcı önizlemede gördüğünü kaydettikten sonra ana sayfada başka biçimde görür (M8).
/// </summary>
public static class BalancePathTrend
{
    // Rol anahtarları (TASARIM-SISTEMI § Rol → token): katedilen yol düz, önümüzdeki yol kesikli.
    private const string TravelledKey = "actual";
    private const string AheadKey = "projection";
    // Bakiyenin girildiği günler grafikte nokta olur (S68-1).
    private const string MarkerKey = "observation";

    /// <summary>Gidişatın rotasından, bugününden, planın dönem sonundan ve bakiye girişlerinden grafiği kurar.</summary>
    /// <param name="progress">Açık dönemin gidişatı ya da kaydetmeden önizlemesi.</param>
    public static ChartTrend From(PeriodProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        return new ChartTrend(
            ToSeries(TravelledKey, progress.Path.Travelled),
            ToSeries(AheadKey, progress.Path.Ahead),
            progress.Today,
            new ChartThreshold(progress.PlannedEndingBalance),
            new ChartSeries(
                MarkerKey,
                progress.Observations.Select(observation => new ChartPoint(observation.ObservedOn, observation.ObservedBalance)).ToList()));
    }

    private static ChartSeries ToSeries(string key, IReadOnlyList<BalancePathPoint> points) =>
        new(key, points.Select(point => new ChartPoint(point.Date, point.Balance)).ToList());
}
