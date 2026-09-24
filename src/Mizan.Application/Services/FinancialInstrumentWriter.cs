using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Simülasyon senaryolarından veya kullanım senaryolarından gelen finansal enstrüman kayıtlarını
/// (krediler, erken ödemeler, kart harcamaları, vadeli borçlar, büyük harcamalar) dar depolara kaydeden odaklı yazıcıdır.
/// </summary>
public sealed class FinancialInstrumentWriter(
    ILoanRepository loanRepository,
    ITemporaryPaymentPlanRepository paymentPlanRepository,
    ICreditCardRepository creditCardRepository,
    IPlannedLargeExpenseRepository largeExpenseRepository)
{
    private readonly ILoanRepository _loanRepository =
        loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
    private readonly ITemporaryPaymentPlanRepository _paymentPlanRepository =
        paymentPlanRepository ?? throw new ArgumentNullException(nameof(paymentPlanRepository));
    private readonly ICreditCardRepository _creditCardRepository =
        creditCardRepository ?? throw new ArgumentNullException(nameof(creditCardRepository));
    private readonly IPlannedLargeExpenseRepository _largeExpenseRepository =
        largeExpenseRepository ?? throw new ArgumentNullException(nameof(largeExpenseRepository));

    /// <summary>
    /// Planlanan büyük harcama kayıtlarını kaydeder.
    /// </summary>
    public async Task WriteLargeExpensesAsync(
        IEnumerable<PlannedLargeExpense> expenses,
        CancellationToken cancellationToken = default)
    {
        foreach (var expense in expenses)
        {
            await _largeExpenseRepository.UpsertPlannedLargeExpenseAsync(expense, cancellationToken);
        }
    }

    /// <summary>
    /// Geçici ve taksitli borç ödeme planlarını kaydeder.
    /// </summary>
    public async Task WritePaymentPlansAsync(
        IEnumerable<TemporaryPaymentPlan> plans,
        CancellationToken cancellationToken = default)
    {
        foreach (var plan in plans)
        {
            await _paymentPlanRepository.UpsertPaymentPlanAsync(plan, cancellationToken);
        }
    }

    /// <summary>
    /// Güncellenen kredi kartı varlıklarını kaydeder.
    /// </summary>
    public async Task WriteCreditCardsAsync(
        IEnumerable<CreditCard> cards,
        CancellationToken cancellationToken = default)
    {
        foreach (var card in cards)
        {
            await _creditCardRepository.UpsertCreditCardAsync(card, cancellationToken);
        }
    }

    /// <summary>
    /// Krediye ait erken/ara ödeme kayıtlarını kaydeder.
    /// </summary>
    public async Task WriteLoanPrepaymentsAsync(
        IEnumerable<LoanPrepayment> prepayments,
        CancellationToken cancellationToken = default)
    {
        foreach (var prepayment in prepayments)
        {
            await _loanRepository.UpsertLoanPrepaymentAsync(prepayment, cancellationToken);
        }
    }
}
