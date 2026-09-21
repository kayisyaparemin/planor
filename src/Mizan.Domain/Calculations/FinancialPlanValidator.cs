using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Bütüncül finansal planın ve alt bileşenlerinin (ayarlar, gelirler, borçlar, kartlar)
/// nakit akış projeksiyonu ve simülasyon hesaplamaları öncesinde matematiksel ve mantıksal
/// iş kurallarına uygunluğunu denetleyen saf doğrulayıcı.
/// </summary>
public static class FinancialPlanValidator
{
    /// <summary>
    /// Verilen finansal planın ayarlarını, gelir tutarlarını ve harcama sınırlarını doğrular.
    /// </summary>
    /// <param name="plan">Doğrulanacak bütüncül finansal plan.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plan"/> null ise fırlatılır.</exception>
    /// <exception cref="InvalidOperationException">Herhangi bir iş kuralı ihlali durumunda fırlatılır.</exception>
    public static void Validate(FinancialPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        UserSettingsValidator.Validate(plan.Settings);
        ValidateIncomes(plan);
        ValidateExpenses(plan);
        ValidateObligations(plan);
    }

    private static void ValidateIncomes(FinancialPlan plan)
    {
        foreach (var history in plan.IncomeHistories)
        {
            if (history.Amount < 0m)
            {
                throw new InvalidOperationException("Gelir tutarı negatif olamaz.");
            }
        }

        foreach (var adHoc in plan.AdHocIncomes)
        {
            if (adHoc.Amount < 0m)
            {
                throw new InvalidOperationException("Tek seferlik arızi gelir tutarı negatif olamaz.");
            }
        }
    }

    private static void ValidateExpenses(FinancialPlan plan)
    {
        foreach (var expense in plan.PlannedLargeExpenses)
        {
            PlannedLargeExpenseValidator.Validate(expense);
        }
    }

    private static void ValidateObligations(FinancialPlan plan)
    {
        foreach (var loan in plan.Loans)
        {
            if (loan.MonthlyPayment < 0m)
            {
                throw new InvalidOperationException("Kredi aylık taksit tutarı negatif olamaz.");
            }

            if (loan.RemainingInstallmentCount < 0)
            {
                throw new InvalidOperationException("Kredi kalan taksit sayısı negatif olamaz.");
            }
        }

        foreach (var prepayment in plan.LoanPrepayments)
        {
            if (prepayment.PrincipalAmount < 0m)
            {
                throw new InvalidOperationException("Erken ödeme anapara tutarı negatif olamaz.");
            }
        }

        foreach (var paymentPlan in plan.PaymentPlans)
        {
            TemporaryPaymentPlanValidator.Validate(paymentPlan);
        }

        foreach (var card in plan.CreditCards)
        {
            CreditCardValidator.Validate(card);
        }
    }
}
