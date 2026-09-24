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
    IPlanChangeRecorder planChangeRecorder) : IIncomePlanService
{
    private const string IncomeChangeTrigger = "Gelir planı değişti";

    private readonly IRecurringIncomeRepository _recurringIncomeRepository =
        recurringIncomeRepository ?? throw new ArgumentNullException(nameof(recurringIncomeRepository));
    private readonly IAdHocIncomeRepository _adHocIncomeRepository =
        adHocIncomeRepository ?? throw new ArgumentNullException(nameof(adHocIncomeRepository));
    private readonly IPlanChangeRecorder _planChangeRecorder =
        planChangeRecorder ?? throw new ArgumentNullException(nameof(planChangeRecorder));

    /// <inheritdoc />
    public async Task SaveRecurringIncomeAsync(RecurringIncome income, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(income);
        cancellationToken.ThrowIfCancellationRequested();

        CalendarRules.ValidateDay(income.PaymentDay);
        await _recurringIncomeRepository.UpsertRecurringIncomeAsync(income, cancellationToken);
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
    public async Task SaveIncomeAmountHistoryAsync(IncomeAmountHistory history, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(history);
        cancellationToken.ThrowIfCancellationRequested();

        if (history.Amount <= 0m)
        {
            throw new InvalidOperationException("Gelir tutarı sıfırdan büyük olmalıdır.");
        }

        await _recurringIncomeRepository.UpsertIncomeAmountHistoryAsync(history, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(IncomeChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteIncomeAmountHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _recurringIncomeRepository.DeleteIncomeAmountHistoryAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(IncomeChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveAdHocIncomeAsync(AdHocIncome income, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(income);
        cancellationToken.ThrowIfCancellationRequested();

        if (income.Amount <= 0m)
        {
            throw new InvalidOperationException("Gelir tutarı sıfırdan büyük olmalıdır.");
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
