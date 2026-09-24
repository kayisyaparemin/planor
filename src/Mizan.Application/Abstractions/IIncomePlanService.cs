using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Düzenli ve tek seferlik gelir akışlarının doğrulanmasını, kaydedilmesini,
/// silinmesini ve açık dönem plan revizyonlarının tetiklenmesini yöneten dar kullanım senaryosu portu.
/// </summary>
public interface IIncomePlanService
{
    /// <summary>
    /// Düzenli gelir akışını doğrular, depoya kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SaveRecurringIncomeAsync(RecurringIncome income, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen düzenli gelir akışını ve geçmişini siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Düzenli bir gelir akışına ait tutar veya zam geçmişi kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SaveIncomeAmountHistoryAsync(IncomeAmountHistory history, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen gelir tutar geçmişi kaydını siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteIncomeAmountHistoryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tek seferlik münferit arızi geliri doğrular, depoya kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SaveAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen tek seferlik münferit geliri siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default);
}
