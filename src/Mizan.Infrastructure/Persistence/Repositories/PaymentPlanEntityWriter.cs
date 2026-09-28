using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Bir ödeme planını taksitleriyle birlikte yazan dahili yardımcı sınıf. Plan hem kendi deposundan
/// hem dönem kapanışından (ödenen taksit, devreden vade) yazılıyor; kapanış yalnız plan satırını
/// yazıp taksitleri unuttuğu için ikisi aynı yazıcıyı paylaşır. Çağıran işlemi açar.
/// </summary>
internal static class PaymentPlanEntityWriter
{
    public static void Save(SQLiteConnection conn, TemporaryPaymentPlan plan)
    {
        var planId = plan.Id.ToString();
        conn.Upsert(new PaymentPlanEntity
        {
            Id = planId,
            Name = plan.Name,
            Kind = (int)plan.Kind,
            OriginalAmount = plan.OriginalAmount,
            TotalRepaymentAmount = plan.TotalRepaymentAmount
        });

        conn.Execute("DELETE FROM payment_installments WHERE PlanId = ?", planId);
        foreach (var installment in plan.Installments)
        {
            conn.Insert(new PaymentInstallmentEntity
            {
                Id = installment.Id.ToString(),
                PlanId = planId,
                DueDate = installment.DueDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                Amount = installment.Amount,
                IsPaid = installment.IsPaid
            });
        }
    }
}
