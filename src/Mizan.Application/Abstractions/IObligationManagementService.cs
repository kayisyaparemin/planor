using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kredi, vadeli borç planı ve planlı büyük harcama yükümlülüklerinin doğrulanmasını,
/// kaydedilmesini, silinmesini ve açık dönem plan revizyonlarının tetiklenmesini yöneten kullanım senaryosu portu.
/// </summary>
public interface IObligationManagementService
{
    /// <summary>
    /// Kredi sözleşmesini iş kurallarına ve banka kapatma tutarı otoritesine göre doğrular,
    /// depoya kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SaveLoanAsync(Loan loan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Krediyi kaydetmeden, <see cref="SaveLoanAsync"/> onu hangi faiz ve bugünkü kapatma bedeliyle
    /// yazacaksa onu çözer; kayıt reddedilecekse null döner. Kredi formunun canlı faiz kartı içindir (S64-9).
    /// </summary>
    LoanPayoffOverview? PreviewLoan(Loan loan);

    /// <summary>
    /// Belirtilen krediyi ve bağlı erken ödeme kayıtlarını siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Krediye ait belirli bir erken/ara ödeme taahhüdünü siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteLoanPrepaymentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vadeli/senetli geçici borç ödeme planını doğrular, taksitlerini normalleştirir,
    /// depoya kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SavePaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen geçici borç ödeme planını ve tüm taksitlerini siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Planlanan büyük harcama kaydını doğrular, depoya kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SavePlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen planlanan büyük harcama kaydını siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default);
}
