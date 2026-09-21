namespace Mizan.Domain.Models;

/// <summary>
/// Kredinin takvimindeki tekil bir ödeme kalemini temsil eden değer nesnesi.
/// Taksit ödemelerinde kredinin kimliğini, erken/ara ödemelerde ise ilgili erken ödeme kaydının kimliğini taşır.
/// </summary>
/// <param name="Date">Ödemenin yapılacağı kesin vade veya erken ödeme tarihi.</param>
/// <param name="Amount">Ödenecek toplam nakit tutarı (anapara payı, kıst faiz ve yasal komisyonlar dahil).</param>
/// <param name="Kind">Ödemenin niteliği (normal taksit, tam erken kapama veya kısmi ara ödeme).</param>
/// <param name="SourceId">Ödemenin kaynak kimliği (taksit için kredi Id'si, erken ödeme için LoanPrepayment Id'si).</param>
/// <param name="IsFinal">Bu ödemenin krediyi tamamen sonlandıran nihai ödeme olup olmadığı.</param>
public sealed record LoanScheduledPayment(
    DateOnly Date,
    decimal Amount,
    LoanPaymentKind Kind,
    Guid SourceId,
    bool IsFinal);
