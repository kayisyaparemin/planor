using Mizan.Domain.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// 12 Dönem ile simülatörün aynı zincirden aynı soruyu aynı kuralla cevaplaması için ortak özet kuralları (M8):
/// bakiye en çok hangi dönemde düşüyor. İki ekranda ayrı kopya olsaydı, aynı rakamlara bakan iki ekran eşit
/// dönemlerde farklı dönemi "en düşük" gösterebilirdi.
/// </summary>
public static class ProjectionSummary
{
    /// <summary>
    /// En düşük dönem sonunun sırası. Eşitlikte ilk dönem seçilir: kullanıcı en erken sıkışacağı anı görmeli.
    /// </summary>
    /// <param name="periods">Zincirin dönemleri, en az bir tane.</param>
    public static int LowestIndex(IReadOnlyList<CashFlowPeriodProjection> periods)
    {
        var lowest = 0;
        for (var index = 1; index < periods.Count; index++)
        {
            if (periods[index].EndingBalance < periods[lowest].EndingBalance)
            {
                lowest = index;
            }
        }

        return lowest;
    }
}
