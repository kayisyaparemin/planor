using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının kredi, kredi kartı, vadeli borç planı ve büyük harcama sözleşmelerini
/// tek bir transfer paketi olarak taşıyan ara veri modelidir.
/// Plan okuyucu servislerin bağımlılık sayısını kontrol altında tutmak için vardır (Kural M3).
/// </summary>
public sealed record FinancialInstrumentBundle(
    IReadOnlyList<Loan> Loans,
    IReadOnlyList<LoanPrepayment> LoanPrepayments,
    IReadOnlyList<TemporaryPaymentPlan> PaymentPlans,
    IReadOnlyList<CreditCard> CreditCards,
    IReadOnlyList<PlannedLargeExpense> PlannedLargeExpenses);
