using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Bir güne bakiye gözlemi yazılıp yazılamayacağını kullanıcıya anlatılabilir bir hatayla denetler (S68-4).
/// Kural <see cref="PeriodObservationRules.CanObserveOn"/>'dadır; kaydetme ve kaydetmeden önizleme aynı
/// sebep metniyle reddetmezse "Bakiye gir" sayfası önizlemeyi gösterip kaydetmede başka şey söylerdi.
/// </summary>
public static class ObservationDayGuard
{
    /// <summary>
    /// Gün gözlem almıyorsa nedenini söyleyen <see cref="InvalidOperationException"/> fırlatır.
    /// </summary>
    /// <param name="plan">Açık dönemin dondurulan planı.</param>
    /// <param name="observedOn">Gözlemin yazılmak istendiği gün.</param>
    /// <param name="today">Bugün.</param>
    public static void EnsureCanObserve(PeriodPlanSnapshot plan, DateOnly observedOn, DateOnly today)
    {
        var period = new CashFlowPeriod(plan.PeriodStart, plan.PeriodEnd);
        if (PeriodObservationRules.CanObserveOn(period, observedOn, today))
        {
            return;
        }

        throw new InvalidOperationException(today >= period.End
            ? "Dönem sona erdi ama kapanış yapılmadı; bakiye girmeden önce kapanışı tamamlayın."
            : "Gözlem günü dönemin dışında olamaz ve bugünden sonra olamaz.");
    }
}
