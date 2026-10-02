using Mizan.Application.Abstractions;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Düzenli gelir akışları ve tek seferlik arızi gelirlerin doğrulanmasını, depolanmasını
/// ve açık dönem plan revizyonlarının tetiklenmesini yöneten dar kullanım senaryosu servisi.
/// </summary>
public sealed class IncomePlanService(
    IRecurringIncomeRepository recurringIncomeRepository,
    IAdHocIncomeRepository adHocIncomeRepository,
    IPlanChangeRecorder planChangeRecorder,
    IClock clock) : IIncomePlanService
{
    private const string IncomeChangeTrigger = "Gelir planı değişti";
    private const string AmountMustBePositiveMessage = "Gelir tutarı sıfırdan büyük olmalıdır.";

    private readonly IRecurringIncomeRepository _recurringIncomeRepository =
        recurringIncomeRepository ?? throw new ArgumentNullException(nameof(recurringIncomeRepository));
    private readonly IAdHocIncomeRepository _adHocIncomeRepository =
        adHocIncomeRepository ?? throw new ArgumentNullException(nameof(adHocIncomeRepository));
    private readonly IPlanChangeRecorder _planChangeRecorder =
        planChangeRecorder ?? throw new ArgumentNullException(nameof(planChangeRecorder));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public async Task SaveRecurringIncomeAsync(
        RecurringIncome income, IReadOnlyList<IncomeAmountHistory> newAmounts, IReadOnlyList<Guid> removedAmountIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(income);
        ArgumentNullException.ThrowIfNull(newAmounts);
        ArgumentNullException.ThrowIfNull(removedAmountIds);
        cancellationToken.ThrowIfCancellationRequested();

        CalendarRules.ValidateDay(income.PaymentDay);
        var bound = newAmounts.Select(x => x with { RecurringIncomeId = income.Id }).ToArray();
        var existing = (await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync(cancellationToken))
            .Where(x => x.RecurringIncomeId == income.Id)
            .ToArray();

        // Ekran aynı kuralları uygular; burada ekran dışından gelen çağrı da reddedilir (S67 V6d2 notları c).
        if (IncomeAmountRules.CheckChange(existing, bound, removedAmountIds, _clock.Today) is { } error)
        {
            throw new InvalidOperationException(error);
        }

        await _recurringIncomeRepository.UpsertRecurringIncomeWithAmountsAsync(income, bound, removedAmountIds, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(IncomeChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteRecurringIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _recurringIncomeRepository.DeleteRecurringIncomeAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(IncomeChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(income);
        cancellationToken.ThrowIfCancellationRequested();

        if (income.Amount <= 0m)
        {
            throw new InvalidOperationException(AmountMustBePositiveMessage);
        }

        await _adHocIncomeRepository.UpsertAdHocIncomeAsync(income, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(IncomeChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAdHocIncomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _adHocIncomeRepository.DeleteAdHocIncomeAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(IncomeChangeTrigger, cancellationToken);
    }
}
