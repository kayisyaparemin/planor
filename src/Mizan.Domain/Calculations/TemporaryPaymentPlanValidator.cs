using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi ve kart dışındaki ödeme planlarının ve taksitlerinin iş kurallarına uygunluğunu denetleyen saf doğrulayıcı.
/// Hatalı veya eksik girilen borç planlarının projeksiyonda hesaplama kırılmalarına yol açmasını engellemek için vardır.
/// </summary>
public static class TemporaryPaymentPlanValidator
{
    /// <summary>
    /// Bir ödeme planının adını, taksit tutarlarını, vade tarihlerini ve ilişki bütünlüğünü doğrular.
    /// </summary>
    /// <param name="plan">Doğrulanacak ödeme planı.</param>
    /// <exception cref="ArgumentNullException"><paramref name="plan"/> null ise fırlatılır.</exception>
    /// <exception cref="InvalidOperationException">İş kuralı ihlalinde fırlatılır.</exception>
    public static void Validate(TemporaryPaymentPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (string.IsNullOrWhiteSpace(plan.Name))
        {
            throw new InvalidOperationException("Ödeme planı adı boş olamaz.");
        }

        ValidateAmounts(plan);
        ValidateInstallments(plan);
    }

    private static void ValidateAmounts(TemporaryPaymentPlan plan)
    {
        if (plan.OriginalAmount is { } originalAmount && originalAmount <= 0m)
        {
            throw new InvalidOperationException("Orijinal borç tutarı sıfırdan büyük olmalıdır.");
        }

        if (plan.TotalRepaymentAmount is { } repaymentAmount && repaymentAmount <= 0m)
        {
            throw new InvalidOperationException("Toplam geri ödeme tutarı sıfırdan büyük olmalıdır.");
        }
    }

    private static void ValidateInstallments(TemporaryPaymentPlan plan)
    {
        if (plan.Installments.Count == 0)
        {
            throw new InvalidOperationException("Ödeme planında en az bir taksit bulunmalıdır.");
        }

        foreach (var installment in plan.Installments)
        {
            if (installment.DueDate == default)
            {
                throw new InvalidOperationException("Taksit vade tarihi geçersiz.");
            }

            if (installment.Amount <= 0m)
            {
                throw new InvalidOperationException("Taksit tutarı sıfırdan büyük olmalıdır.");
            }

            if (installment.PlanId != plan.Id)
            {
                throw new InvalidOperationException("Taksit ait olduğu plan ile eşleşmiyor.");
            }
        }
    }
}
