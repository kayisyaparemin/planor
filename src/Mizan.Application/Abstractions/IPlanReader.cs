using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcının finansal planını ve projeksiyona hazır durumunu veri depolarından okuyan salt okuma portudur.
/// Kesinlikle veri yazmaz, snapshot almaz ve revizyon üretmez (Kural M4, Düğüm T5).
/// </summary>
public interface IPlanReader
{
    /// <summary>
    /// Tüm aktif finansal sözleşmeleri, gelir akışlarını ve kullanıcı ayarlarını ilgili veri depolarından
    /// okuyarak bütüncül finansal plan modelini döndürür.
    /// </summary>
    Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen tarih itibarıyla projeksiyon başlangıç sınırlarını çözer, açık nakit akış döneminin
    /// dönem içi kart harcamalarını filtreler (I16) ve projeksiyon motoruna hazır sorgu sonucunu döndürür.
    /// </summary>
    Task<ProjectionQueryPlan> GetProjectionPlanAsync(
        DateOnly asOf,
        CancellationToken cancellationToken = default);
}
