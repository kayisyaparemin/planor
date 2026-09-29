namespace Mizan.Application.Models;

/// <summary>
/// Ana sayfa grafiğinin çizdiği yol: açık dönemin her günü için günün başındaki bakiye (S71). Kullanıcı dönem
/// içinde birkaç kez bakiye girer; noktaları düz çizgiyle birleştirmek, kira günündeki tek bir düşüşü günlere
/// yayarak yanlış bir hikâye anlatır. Rota aralarda ve son gözlemden sonra planın gelir ve ödeme günlerini izler.
/// İki parçası vardır çünkü ekran ikisini ayrı çizer: katedilen yol düz, önümüzdeki yol kesikli.
/// </summary>
/// <param name="Travelled">
/// Dönem başından son gözleme kadar olan günler; son noktası son gözlemdir. Gözlem yoksa yalnız açılış noktası.
/// </param>
/// <param name="Ahead">
/// Son gözlemden (gözlem yoksa dönem başından) dönemin bitiş gününe kadar olan günler. İlk noktası
/// <paramref name="Travelled"/>'ın son noktasıdır, çizgi orada kesintisiz devam etsin diye; son noktası dönem sonudur.
/// </param>
public sealed record PeriodBalancePath(
    IReadOnlyList<BalancePathPoint> Travelled,
    IReadOnlyList<BalancePathPoint> Ahead);
