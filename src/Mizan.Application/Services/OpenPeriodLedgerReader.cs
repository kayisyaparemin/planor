using Mizan.Domain.Calculations;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık dönemin defterini (<see cref="OpenPeriodLedger"/>) üç dar porttan okuyup bir araya getirir.
/// Açık dönemi tarihçenin kendisine sorar (<see cref="FinancialHistoryData.FindOpenPlan"/>), sonra
/// yalnız o döneme ait gözlemi ve hatırlatıcı cevaplarını ekler. Hiçbir şey yazmaz.
/// </summary>
public sealed class OpenPeriodLedgerReader(
    IPeriodHistoryRepository periodHistoryRepository,
    IPeriodObservationRepository periodObservationRepository,
    IPaymentReminderRepository paymentReminderRepository)
{
    private readonly IPeriodHistoryRepository _periodHistoryRepository =
        periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
    private readonly IPeriodObservationRepository _periodObservationRepository =
        periodObservationRepository ?? throw new ArgumentNullException(nameof(periodObservationRepository));
    private readonly IPaymentReminderRepository _paymentReminderRepository =
        paymentReminderRepository ?? throw new ArgumentNullException(nameof(paymentReminderRepository));

    /// <summary>
    /// Açık dönemin defterini okur; açık dönem yoksa (henüz plan dondurulmamış ya da son dönem
    /// kapatılmış ve yenisi başlamamışsa) <c>null</c> döner.
    /// </summary>
    public async Task<OpenPeriodLedger?> ReadAsync(CancellationToken cancellationToken = default)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var openPlan = history.FindOpenPlan();
        if (openPlan is null)
        {
            return null;
        }

        var period = new CashFlowPeriod(openPlan.PeriodStart, openPlan.PeriodEnd);
        var observation = PeriodObservationRules.Latest(
            await _periodObservationRepository.GetPeriodObservationsAsync(openPlan.Id, cancellationToken));
        var marks = await _periodObservationRepository.GetPaymentMarksAsync(openPlan.Id, cancellationToken);
        var answers = await _paymentReminderRepository.GetResponsesAsync(cancellationToken);

        return new OpenPeriodLedger(
            openPlan,
            history.FindRevisions(openPlan.Id),
            observation,
            marks,
            answers.Where(x => period.Contains(x.DueDate)).ToArray());
    }
}
