namespace Mizan.Application.Models;

/// <summary>
/// Harcamanın süreye göre hızlı mı yavaş mı gittiği: yaşam havuzundan harcanan oran ile dönemden geçen süre
/// oranı ve aradaki fark. İki oran da <see cref="ObservedOn"/>'a, yani son gözlemin gününe göre alınır.
/// Süre bugüne göre alınsaydı kullanıcı bakiye girmedikçe harcama donar, süre ilerler ve ekran "harcama geride"
/// diyerek yanlış güven verirdi (S70).
/// </summary>
/// <param name="ObservedOn">İki oranın da dayandığı gün: son gözlemin günü.</param>
/// <param name="SpentRatio">Havuzdan harcanan oran (0,4 = %40). Havuz aşılınca 1'i geçer; kırpılmaz, kırpmak sunumun işidir.</param>
/// <param name="ElapsedRatio">Dönemden geçen süre oranı: (gözlem günü − dönem başı) / dönem gün sayısı.</param>
public sealed record SpendingPace(DateOnly ObservedOn, decimal SpentRatio, decimal ElapsedRatio)
{
    /// <summary>Harcanan oran ile geçen süre oranı arasındaki fark, puan olarak; pozitif harcama önde, negatif geride.</summary>
    public decimal GapPoints => (SpentRatio - ElapsedRatio) * 100m;
}
