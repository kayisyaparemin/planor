using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// 12 dönemlik projeksiyon motoruna girdi olarak sunulmaya hazır finansal planı
/// ve çözümlenmiş projeksiyon başlangıç sınırlarını taşıyan sorgu veri transfer modeli.
/// </summary>
public sealed record ProjectionQueryPlan(
    FinancialPlan Plan,
    ProjectionBoundary? Boundary);
