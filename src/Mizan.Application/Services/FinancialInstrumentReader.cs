using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kredi, kredi kartı, vadeli borç ve büyük harcama depolarından sözleşmeleri paralel olarak okuyan yardımcı servistir.
/// Plan okuyucunun bağımlılık sayısını 5 altında tutmak için vardır (Kural M3).
/// </summary>
public sealed class FinancialInstrumentReader(
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
    /// Tüm borç ve yükümlülük sözleşmelerini paralel olarak depolarından okuyarak tek bir pakette döner.
    /// </summary>
    public async Task<FinancialInstrumentBundle> ReadInstrumentsAsync(CancellationToken cancellationToken = default)
    {
        var loansTask = _loanRepository.GetLoansAsync(cancellationToken);
        var prepaymentsTask = _loanRepository.GetLoanPrepaymentsAsync(cancellationToken);
        var paymentPlansTask = _paymentPlanRepository.GetPaymentPlansAsync(cancellationToken);
        var creditCardsTask = _creditCardRepository.GetCreditCardsAsync(cancellationToken);
        var largeExpensesTask = _largeExpenseRepository.GetPlannedLargeExpensesAsync(cancellationToken);

        await Task.WhenAll(
            loansTask,
            prepaymentsTask,
            paymentPlansTask,
            creditCardsTask,
            largeExpensesTask);

        return new FinancialInstrumentBundle(
            await loansTask,
            await prepaymentsTask,
            await paymentPlansTask,
            await creditCardsTask,
            await largeExpensesTask);
    }
}
