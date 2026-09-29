namespace Mizan.Application.Models;

/// <summary>
/// Bakiye rotasında tek bir gün: o günün başındaki bakiye (S71). Gözlem gününde girilen tutarın kendisidir.
/// Günün başı seçildi çünkü böylece dönemin ilk noktası açılış bakiyesi, son noktası dönem sonu olur ve biten
/// dönemle başlayan dönem aynı günde aynı tutarda buluşur; günün sonu seçilseydi ilk nokta ilk günün
/// hareketlerini içerir, açılış bakiyesi grafikte hiç görünmezdi.
/// </summary>
/// <param name="Date">Gün.</param>
/// <param name="Balance">Günün başındaki bakiye, kuruşa yuvarlanmış.</param>
public sealed record BalancePathPoint(DateOnly Date, decimal Balance);
