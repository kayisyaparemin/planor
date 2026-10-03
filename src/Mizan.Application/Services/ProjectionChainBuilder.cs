using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// 12 dönemlik zinciri ana sayfanın dönem sonuna bağlayan tek yer (S74-1, S76-1). Eskide zincir planın çapasından
/// kuruluyordu: kullanıcı bakiye girince ana sayfa tahmini, 12 dönem planı gösteriyordu. 12 Dönem ve simülatör
/// aynı kurucuyu kullanır; biri değişirse ikisi birlikte değişir, iki ekran aynı dönem için iki ayrı rakam söylemez.
/// Hiçbir şey okumaz, yazmaz; okuma işi çağıran serviste kalır (M8: bağımlılıksız ortak yardımcı).
/// </summary>
public static class ProjectionChainBuilder
{
    /// <summary>
    /// Zinciri kurar. Açılış bakiyesi ana sayfadaki rakamdır (bakiye girildiyse tahmin, girilmediyse plan; S72-1) ve
    /// ilk dönem açık dönemin bittiği gündür. Plan projeksiyon kuramıyorsa (gelir ve bakiye yok, I12) zincir yoktur;
    /// kural zincirin kendisine sorulur: gelir silinmiş ama ana sayfada bakiye varsa zincir erimeyi gösterir.
    /// </summary>
    /// <param name="plan">Açık dönemi içeren plan.</param>
    /// <param name="progress">Ana sayfanın açık dönem verisi.</param>
    /// <param name="today">Hesabın günü.</param>
    /// <returns>Zincir; kurulamıyorsa <c>null</c>.</returns>
    public static ProjectionChain? Build(FinancialPlan plan, PeriodProgress progress, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(progress);

        var chainPlan = plan with
        {
            Settings = plan.Settings with
            {
                ProjectionAnchorDate = progress.PeriodEnd,
                ProjectionOpeningBalance = progress.ProjectedEndingBalance ?? progress.PlannedEndingBalance
            }
        };

        return chainPlan.CanBuildProjection
            ? new ProjectionChain
            {
                Plan = chainPlan,
                OpenPeriodPlan = plan,
                Today = today,
                OpenPeriodStart = progress.PeriodStart,
                FirstPeriodStart = progress.PeriodEnd,
                OpenPeriodEndingBeforeDeficitInterest = EndingBeforeInterest(progress)
            }
            : null;
    }

    // Faiz öncesi hâl, açık dönemin sonu yeniden hesaplanırken faizin gerçek rakamdan işlemesi için gerekir (V10b notları b).
    private static decimal EndingBeforeInterest(PeriodProgress progress) =>
        progress.ProjectedEndingBalance is { } projected
            ? projected + (progress.ProjectedDeficitInterest ?? 0m)
            : progress.PlannedEndingBalance + progress.PlannedDeficitInterest;
}
