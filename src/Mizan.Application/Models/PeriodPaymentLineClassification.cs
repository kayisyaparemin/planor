using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Açık dönemin güncel ödeme satırlarının belirli bir güne göre durumu: hangileri yapıldı,
/// yapılanların ne kadarı kullanıcının girdiği bakiyeye zaten yansımış, hangileri hâlâ kalan.
/// Gidişat, gözlenen bakiyeden yaşam harcamasını geri çözerken yalnız bakiyeye yansımış ödemeleri
/// düşebilir; gözlemden sonra yapılanları ise bakiyeden ayrıca çıkarmak zorundadır. Bu ayrım
/// yapılmazsa aynı ödeme ya iki kez sayılır ya da hiç sayılmaz.
/// </summary>
public sealed record PeriodPaymentLineClassification
{
    /// <summary>
    /// Yapılmış ve gözlenen bakiyeye yansımış sayılan ödemelerin toplamı.
    /// Gözlem yoksa yapılmış ödemelerin tamamıdır.
    /// </summary>
    public required decimal SettledBeforeObservation { get; init; }

    /// <summary>Gözlemden sonra yapılmış, bu yüzden gözlenen bakiyede henüz görünmeyen ödemelerin toplamı.</summary>
    public required decimal SettledAfterObservation { get; init; }

    /// <summary>
    /// Henüz yapılmamış satırlar — vadesi gelmemiş, ertelenmiş ya da "ödenmedi" işaretli —
    /// önce vadeye, sonra ada göre sıralı.
    /// </summary>
    public required IReadOnlyList<PeriodPlanPaymentLine> RemainingLines { get; init; }

    /// <summary>Kalan satırlardan hatırlatıcıda "Ertele" denenlerin kimlikleri; vadesi geçse de kalan sayılmalarının sebebi.</summary>
    public required IReadOnlySet<Guid> SnoozedLineIds { get; init; }
}
