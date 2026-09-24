using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Düzenli gelir, gelir geçmişi ve arızi gelir depolarından verileri okuyan yardımcı servistir.
/// Plan okuyucunun bağımlılık sayısını 5 altında tutmak için vardır (Kural M3).
/// </summary>
public sealed class IncomePlanReader(
    IRecurringIncomeRepository recurringIncomeRepository,
    IAdHocIncomeRepository adHocIncomeRepository)
{
    private readonly IRecurringIncomeRepository _recurringIncomeRepository =
        recurringIncomeRepository ?? throw new ArgumentNullException(nameof(recurringIncomeRepository));
    private readonly IAdHocIncomeRepository _adHocIncomeRepository =
        adHocIncomeRepository ?? throw new ArgumentNullException(nameof(adHocIncomeRepository));

    /// <summary>
    /// Tüm gelir akışlarını ve tutar geçmişlerini paralel olarak okuyarak tek bir pakette döner.
    /// </summary>
    public async Task<IncomePlanBundle> ReadIncomesAsync(CancellationToken cancellationToken = default)
    {
        var recurringTask = _recurringIncomeRepository.GetRecurringIncomesAsync(cancellationToken);
        var historiesTask = _recurringIncomeRepository.GetIncomeAmountHistoriesAsync(cancellationToken);
        var adHocTask = _adHocIncomeRepository.GetAdHocIncomesAsync(cancellationToken);

        await Task.WhenAll(recurringTask, historiesTask, adHocTask);

        return new IncomePlanBundle(
            await recurringTask,
            await historiesTask,
            await adHocTask);
    }
}
