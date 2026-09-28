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
    /// Kredi sözleşmesini iş kurallarına ve banka kapatma tutarı otoritesine göre doğrular, erken
    /// ödemelerini kredinin bu hâline göre yeniden doğrular, ikisini tek işlemde kaydeder ve açık
    /// dönem varsa tek plan revizyonu tetikler. Listede olmayan erken ödemeler silinir (S64-12).
    /// </summary>
    Task SaveLoanAsync(Loan loan, IReadOnlyList<LoanPrepayment> prepayments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Krediyi kaydetmeden, <see cref="SaveLoanAsync"/> onu hangi faiz ve bugünkü kapatma bedeliyle
    /// yazacaksa onu çözer; kayıt reddedilecekse null döner. Kredi formunun canlı faiz kartı içindir (S64-9).
    /// </summary>
    LoanPayoffOverview? PreviewLoan(Loan loan);

    /// <summary>
    /// Erken ödemeleri, kredinin kaydedileceği hâlden hesaplanan o günkü tutarlarıyla tarih sırasında
    /// listeler; her erken ödeme için bir satır. Kredi kayıtta reddedilecekse ya da faizi çözülemiyorsa
    /// tutarlar null'dır. Kredi formunun canlı erken ödeme listesi içindir (S64-14).
    /// </summary>
    IReadOnlyList<PlannedLoanPrepayment> PreviewLoanPrepayments(Loan loan, IReadOnlyList<LoanPrepayment> prepayments);

    /// <summary>
    /// Yeni bir erken ödemenin, kredinin kaydedileceği hâline ve diğer erken ödemelere göre kabul
    /// edilip edilmeyeceğini söyler: kabul edilecekse null, edilmeyecekse kullanıcıya gösterilecek
    /// mesaj. Kayıt aynı kuralı uygular (S64-14).
    /// </summary>
    string? ValidateLoanPrepayment(Loan loan, IReadOnlyList<LoanPrepayment> prepayments, LoanPrepayment candidate);

    /// <summary>
    /// Belirtilen krediyi ve bağlı erken ödeme kayıtlarını siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default);

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
