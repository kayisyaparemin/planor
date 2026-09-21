using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi ve kart haricindeki vadeli ödeme planlarından (senet, elden taksit, geçici borç)
/// henüz ödenmemiş taksitleri ayıklayarak standart yükümlülük kalemlerine dönüştüren saf hesaplayıcı.
/// </summary>
public sealed class ScheduledPaymentCalculator
{
    /// <summary>
    /// Verilen vadeli ödeme planlarındaki ödenmemiş taksitleri takvim sırasına göre yükümlülük kalemlerine dönüştürür.
    /// </summary>
    public IReadOnlyList<ObligationItem> GetItems(IEnumerable<TemporaryPaymentPlan> plans)
    {
        ArgumentNullException.ThrowIfNull(plans);

        var items = new List<ObligationItem>();
        foreach (var plan in plans)
        {
            var unpaid = plan.Installments
                .Where(x => !x.IsPaid)
                .OrderBy(x => x.DueDate)
                .ToArray();

            if (unpaid.Length == 0)
            {
                continue;
            }

            var finalDate = unpaid[^1].DueDate;
            var type = plan.Kind switch
            {
                PaymentPlanKind.Temporary => ObligationType.TemporaryPayment,
                PaymentPlanKind.Installment => ObligationType.InstallmentPayment,
                PaymentPlanKind.Recurring or PaymentPlanKind.OtherScheduled => ObligationType.OtherScheduledPayment,
                _ => throw new ArgumentOutOfRangeException(nameof(plans), $"Desteklenmeyen plan türü: {plan.Kind}")
            };

            items.AddRange(unpaid.Select(x => new ObligationItem(
                plan.Name,
                type,
                x.DueDate,
                x.Amount)
            {
                IsFinalPayment = x.DueDate == finalDate,
                PaymentId = x.Id
            }));
        }

        return items;
    }
}
