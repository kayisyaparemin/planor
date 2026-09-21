namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının belirlediği finansal hedef tutara nakit akış projeksiyonu
/// boyunca ulaşılabilirlik durumunu ve ilk ulaşılan dönemi temsil eden sözleşme.
/// Simülasyon ve hedef analizi ekranlarında kullanıcının hedefe ne zaman
/// varacağını göstermek için vardır.
/// </summary>
public sealed record TargetReachabilityResult(
    bool IsAlreadyReached,
    CashFlowPeriodProjection? FirstReachedPeriod)
{
    /// <summary>
    /// Hedefe başlangıçta veya projeksiyon dönemlerinden birinde ulaşılıp ulaşılmadığı.
    /// </summary>
    public bool IsReached => IsAlreadyReached || FirstReachedPeriod is not null;
}
