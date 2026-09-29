using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Düzenli ve tek seferlik gelir akışlarının doğrulanmasını, kaydedilmesini,
/// silinmesini ve açık dönem plan revizyonlarının tetiklenmesini yöneten dar kullanım senaryosu portu.
/// </summary>
public interface IIncomePlanService
{
    /// <summary>
    /// Düzenli gelir akışını ve ona eklenecek tutar kayıtlarını doğrular, tek işlemde yazar ve tek plan
    /// revizyonu tetikler. Yeni gelir ilk tutarıyla birlikte gelir; ayrı yazılsaydı arada tutarı olmayan
    /// bir gelir kalır ve iki revizyon doğardı (S67-3). Tutar kayıtları eklenir, var olanlar değişmez.
    /// </summary>
    Task SaveRecurringIncomeAsync(
        RecurringIncome income, IReadOnlyList<IncomeAmountHistory> newAmounts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen düzenli gelir akışını ve geçmişini siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tek seferlik münferit arızi geliri doğrular, depoya kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SaveAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen tek seferlik münferit geliri siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default);
}
