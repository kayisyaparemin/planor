using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kredi ve kredi kartı dışındaki belirli vadeli veya taksitli geçici borç ödeme planlarının
/// kalıcı veri deposuna erişimini ve yönetimini sağlayan dar port arayüzü.
/// </summary>
public interface ITemporaryPaymentPlanRepository
{
    /// <summary>
    /// Kayıtlı tüm geçici ödeme planlarını bağlı taksitleriyle birlikte listeler.
    /// </summary>
    Task<IReadOnlyList<TemporaryPaymentPlan>> GetPaymentPlansAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir geçici ödeme planını ve taksitlerini kaydeder veya günceller.
    /// </summary>
    Task UpsertPaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir geçici ödeme planını ve bağlı tüm taksitlerini kalıcı olarak siler.
    /// </summary>
    Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default);
}
