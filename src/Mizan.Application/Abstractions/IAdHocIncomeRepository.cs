using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Prim, ikramiye, varlık satışı veya hediye gibi münferit ve tek seferlik gelirlerin
/// kalıcı veri deposuna erişimini ve yönetimini sağlayan dar port arayüzü.
/// </summary>
public interface IAdHocIncomeRepository
{
    /// <summary>
    /// Kayıtlı tüm münferit tek seferlik gelirleri listeler.
    /// </summary>
    Task<IReadOnlyList<AdHocIncome>> GetAdHocIncomesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir münferit tek seferlik geliri kaydeder veya günceller.
    /// </summary>
    Task UpsertAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen münferit tek seferlik geliri kalıcı olarak siler.
    /// </summary>
    Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default);
}
