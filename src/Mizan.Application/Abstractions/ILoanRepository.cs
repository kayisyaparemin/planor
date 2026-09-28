using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcının tüketici, konut ve taşıt kredileri ile bunlara bağlı erken ve ara ödemelerin
/// kalıcı veri deposuna erişimini ve yönetimini sağlayan dar port arayüzü.
/// </summary>
public interface ILoanRepository
{
    /// <summary>
    /// Kayıtlı tüm kredileri listeler.
    /// </summary>
    Task<IReadOnlyList<Loan>> GetLoansAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir krediyi kaydeder veya günceller.
    /// </summary>
    Task UpsertLoanAsync(Loan loan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Krediyi ve erken ödemelerinin tamamını tek işlemde yazar: listede olmayan erken ödemeleri
    /// siler, başka kredininkine dokunmaz. Kredi formunun tek kaydıdır; ikisi ayrı yazılırsa kredi
    /// yazıldığı anda erken ödemeler kaybolabilir (S64-12).
    /// </summary>
    Task UpsertLoanWithPrepaymentsAsync(
        Loan loan, IReadOnlyList<LoanPrepayment> prepayments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir krediyi ve ona bağlı erken ödeme kayıtlarını kalıcı olarak siler.
    /// </summary>
    Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kayıtlı tüm kredi erken/ara ödemelerini listeler.
    /// </summary>
    Task<IReadOnlyList<LoanPrepayment>> GetLoanPrepaymentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir kredi erken/ara ödemesini kaydeder veya günceller.
    /// </summary>
    Task UpsertLoanPrepaymentAsync(LoanPrepayment prepayment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kredi erken/ara ödemesini kalıcı olarak siler.
    /// </summary>
    Task DeleteLoanPrepaymentAsync(Guid id, CancellationToken cancellationToken = default);
}
