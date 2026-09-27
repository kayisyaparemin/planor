using Mizan.Application.Abstractions;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.Services;

/// <summary>
/// Finansal Yapı'dan silinen kaydı türüne göre doğru dar porta yönlendirir. Üç silme portunu tek
/// yerde toplar ki ekranın görünüm modeli beş bağımlılık sınırında kalsın (Kural M3, S62-3).
/// </summary>
public sealed class FinancialRecordRemover
{
    private readonly ICreditCardObligationService _cardService;
    private readonly IObligationManagementService _obligationService;
    private readonly IIncomePlanService _incomeService;

    /// <summary>Siliciyi üç dar yazma portuyla başlatır.</summary>
    public FinancialRecordRemover(
        ICreditCardObligationService cardService,
        IObligationManagementService obligationService,
        IIncomePlanService incomeService)
    {
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
        _obligationService = obligationService ?? throw new ArgumentNullException(nameof(obligationService));
        _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
    }

    /// <summary>Kaydı ve ona bağlı kayıtları kalıcı olarak siler; açık dönem varsa plan revizyonu portta tetiklenir.</summary>
    public Task RemoveAsync(FinancialRecordKind kind, Guid id, CancellationToken cancellationToken = default) => kind switch
    {
        FinancialRecordKind.RecurringIncome => _incomeService.DeleteRecurringIncomeAsync(id, cancellationToken),
        FinancialRecordKind.AdHocIncome => _incomeService.DeleteAdHocIncomeAsync(id, cancellationToken),
        FinancialRecordKind.CreditCard => _cardService.DeleteCreditCardAsync(id, cancellationToken),
        FinancialRecordKind.Loan => _obligationService.DeleteLoanAsync(id, cancellationToken),
        FinancialRecordKind.PaymentPlan => _obligationService.DeletePaymentPlanAsync(id, cancellationToken),
        FinancialRecordKind.LargeExpense => _obligationService.DeletePlannedLargeExpenseAsync(id, cancellationToken),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}
