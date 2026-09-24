using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Simülasyon senaryolarından veya kullanım senaryolarından gelen gelir kayıtlarını
/// (düzenli gelir tutar geçmişi ve tek seferlik arızi gelirler) ilgili dar depolara kaydeden odaklı yazıcıdır.
/// </summary>
public sealed class IncomePlanWriter(
    IRecurringIncomeRepository recurringIncomeRepository,
    IAdHocIncomeRepository adHocIncomeRepository)
{
    private readonly IRecurringIncomeRepository _recurringIncomeRepository =
        recurringIncomeRepository ?? throw new ArgumentNullException(nameof(recurringIncomeRepository));
    private readonly IAdHocIncomeRepository _adHocIncomeRepository =
        adHocIncomeRepository ?? throw new ArgumentNullException(nameof(adHocIncomeRepository));

    /// <summary>
    /// Tek seferlik münferit arızi gelir kayıtlarını kaydeder.
    /// </summary>
    public async Task WriteAdHocIncomesAsync(
        IEnumerable<AdHocIncome> incomes,
        CancellationToken cancellationToken = default)
    {
        foreach (var income in incomes)
        {
            await _adHocIncomeRepository.UpsertAdHocIncomeAsync(income, cancellationToken);
        }
    }

    /// <summary>
    /// Düzenli gelir akışına ait tutar ve zam revizyon kayıtlarını kaydeder.
    /// </summary>
    public async Task WriteIncomeHistoriesAsync(
        IEnumerable<IncomeAmountHistory> histories,
        CancellationToken cancellationToken = default)
    {
        foreach (var history in histories)
        {
            await _recurringIncomeRepository.UpsertIncomeAmountHistoryAsync(history, cancellationToken);
        }
    }
}
