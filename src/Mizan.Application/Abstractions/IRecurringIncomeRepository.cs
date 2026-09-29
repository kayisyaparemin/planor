using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kira, emekli aylığı veya serbest meslek gibi dönemsel tekrarlayan düzenli gelir akışlarının
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
    /// Bir düzenli gelir akışını ve ona eklenecek tutar kayıtlarını tek işlemde yazar: biri yazılamazsa
    /// hiçbiri yazılmaz. Akışın var olan tutar kayıtlarına dokunulmaz (S67-3).
    /// </summary>
    Task UpsertRecurringIncomeWithAmountsAsync(
        RecurringIncome income, IReadOnlyList<IncomeAmountHistory> newAmounts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen düzenli gelir akışını ve geçmişini kalıcı olarak siler.
    /// </summary>
    Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kayıtlı tüm düzenli gelir tutar ve zam revizyon kayıtlarını listeler.
    /// </summary>
    Task<IReadOnlyList<IncomeAmountHistory>> GetIncomeAmountHistoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Düzenli bir gelir akışına ait tutar veya zam geçmişi kaydeder veya günceller.
    /// </summary>
    Task UpsertIncomeAmountHistoryAsync(IncomeAmountHistory history, CancellationToken cancellationToken = default);
}
