using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Dönem kapanış mutabakatında fiili ödemeler uygulandıktan sonra
/// bir sonraki döneme devreden güncellenmiş finansal enstrümanların (krediler, kartlar,
/// vadeli planlar, büyük harcamalar) ve tüketilen erken ödemelerin nihai durumunu taşır.
/// </summary>
public sealed record ReconciledFinancialInstruments
{
    /// <summary>Kapanış sonrası güncellenmiş ve yeni taksit tarihleri belirlenmiş krediler.</summary>
    public required IReadOnlyList<Loan> Loans { get; init; }

    /// <summary>Kapanış sonrası taksit durumları veya vadeleri güncellenmiş vadeli ödeme planları.</summary>
    public required IReadOnlyList<TemporaryPaymentPlan> PaymentPlans { get; init; }

    /// <summary>Kapanış sonrası fiili ödemesi düşülmüş ve devreden anaparası hesaplanmış kredi kartları.</summary>
    public required IReadOnlyList<CreditCard> CreditCards { get; init; }

    /// <summary>Kapanış sonrası tamamlanan veya yeni döneme devredilen planlı büyük harcamalar.</summary>
    public required IReadOnlyList<PlannedLargeExpense> LargeExpenses { get; init; }

    /// <summary>Kapanışta tüketilen (ödenen veya süresi geçtiği için iptal edilen) erken ödeme kimlikleri.</summary>
    public required IReadOnlyList<Guid> RemovedLoanPrepaymentIds { get; init; }
}
