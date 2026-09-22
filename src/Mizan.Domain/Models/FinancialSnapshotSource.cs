namespace Mizan.Domain.Models;

/// <summary>
/// Finansal durum anlık görüntüsünün (snapshot) hangi işlem veya yaşam döngüsü
/// adımı sonucunda oluşturulduğunu belirten kaynak türü. İlk kurulum, periyodik
/// dönem kapanışı veya sistem toparlama durumlarını ayırt etmek için vardır.
/// </summary>
public enum FinancialSnapshotSource
{
    /// <summary>Uygulamanın ilk kurulumunda (onboarding) kaydedilen başlangıç durumu.</summary>
    Initial = 0,

    /// <summary>Düzenli dönem kapanışı ve mutabakat sonucunda kaydedilen güncel durum.</summary>
    MonthlyUpdate = 1,

    /// <summary>Dönem takviminde veya veritabanında meydana gelen sapmaların onarımı/kurtarılması.</summary>
    Recovery = 2
}
