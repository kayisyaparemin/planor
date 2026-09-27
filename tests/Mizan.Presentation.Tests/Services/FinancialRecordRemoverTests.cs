using Mizan.Presentation.Models;
using Mizan.Presentation.Services;
using Mizan.Presentation.Tests.Fakes;
using Xunit;

namespace Mizan.Presentation.Tests.Services;

/// <summary>
/// Silinen kaydın türüne göre doğru dar porta gittiğini doğrular (S62-3).
/// </summary>
public sealed class FinancialRecordRemoverTests
{
    private readonly FakeCreditCardObligationService _cards = new();
    private readonly FakeObligationManagementService _obligations = new();
    private readonly FakeIncomePlanService _incomes = new();
    private readonly FinancialRecordRemover _remover;

    public FinancialRecordRemoverTests()
    {
        _remover = new FinancialRecordRemover(_cards, _obligations, _incomes);
    }

    [Fact]
    public async Task Remove_Gelirler_GelirPortunaGider()
    {
        var recurring = Guid.NewGuid();
        var adHoc = Guid.NewGuid();

        await _remover.RemoveAsync(FinancialRecordKind.RecurringIncome, recurring);
        await _remover.RemoveAsync(FinancialRecordKind.AdHocIncome, adHoc);

        Assert.Equal([recurring], _incomes.DeletedRecurringIncomeIds);
        Assert.Equal([adHoc], _incomes.DeletedAdHocIncomeIds);
    }

    [Fact]
    public async Task Remove_Kart_KartPortunaGider()
    {
        var id = Guid.NewGuid();

        await _remover.RemoveAsync(FinancialRecordKind.CreditCard, id);

        Assert.Equal([id], _cards.DeletedCardIds);
    }

    [Fact]
    public async Task Remove_KrediPlanVeHarcama_YukumlulukPortunaGider()
    {
        var loan = Guid.NewGuid();
        var plan = Guid.NewGuid();
        var expense = Guid.NewGuid();

        await _remover.RemoveAsync(FinancialRecordKind.Loan, loan);
        await _remover.RemoveAsync(FinancialRecordKind.PaymentPlan, plan);
        await _remover.RemoveAsync(FinancialRecordKind.LargeExpense, expense);

        Assert.Equal([loan], _obligations.DeletedLoanIds);
        Assert.Equal([plan], _obligations.DeletedPaymentPlanIds);
        Assert.Equal([expense], _obligations.DeletedLargeExpenseIds);
        Assert.Empty(_cards.DeletedCardIds);
        Assert.Empty(_incomes.DeletedRecurringIncomeIds);
    }
}
