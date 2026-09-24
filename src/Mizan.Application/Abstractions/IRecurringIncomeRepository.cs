using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Maaş, kira veya serbest meslek gibi dönemsel tekrarlayan düzenli gelir akışlarının
/// kalıcı veri deposuna erişimini ve yönetimini sağlayan dar port arayüzü.
/// </summary>
public interface IRecurringIncomeRepository
{
    /// <summary>
    /// Kayıtlı tüm düzenli gelir akışlarını etkin tutar geçmişleriyle birlikte listeler.
    /// </summary>
    Task<IReadOnlyList<RecurringIncome>> GetRecurringIncomesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir düzenli gelir akışını ve tutar geçmişini kaydeder veya günceller.
    /// </summary>
    Task UpsertRecurringIncomeAsync(RecurringIncome income, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen düzenli gelir akışını ve geçmişini kalıcı olarak siler.
    /// </summary>
    Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default);
}
