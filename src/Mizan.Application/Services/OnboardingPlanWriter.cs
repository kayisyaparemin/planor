using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kurulum sihirbazında toplanan temel kullanıcı ayarlarını, gelir akışlarını
/// ve finansal borç enstrümanlarını ilgili dar depolara topluca kaydeden odaklı yazıcıdır.
/// </summary>
public sealed class OnboardingPlanWriter(
    IUserSettingsRepository settingsRepository,
    IRecurringIncomeRepository recurringIncomeRepository,
    FinancialInstrumentWriter instrumentWriter)
{
    private readonly IUserSettingsRepository _settingsRepository =
        settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    private readonly IRecurringIncomeRepository _recurringIncomeRepository =
        recurringIncomeRepository ?? throw new ArgumentNullException(nameof(recurringIncomeRepository));
    private readonly FinancialInstrumentWriter _instrumentWriter =
        instrumentWriter ?? throw new ArgumentNullException(nameof(instrumentWriter));

    /// <summary>
    /// Kurulum taslağında yer alan tüm başlangıç verilerini depolarına yazar.
    /// </summary>
    public async Task WritePlanDataAsync(
        OnboardingDraft draft,
        UserSettings normalizedSettings,
        IReadOnlyList<CreditCard> validatedCards,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(normalizedSettings);
        ArgumentNullException.ThrowIfNull(validatedCards);

        await _settingsRepository.SaveSettingsAsync(normalizedSettings, cancellationToken);

        foreach (var income in draft.RecurringIncomes)
        {
            await _recurringIncomeRepository.UpsertRecurringIncomeAsync(income, cancellationToken);
        }

        foreach (var history in draft.IncomeAmountHistories)
        {
            await _recurringIncomeRepository.UpsertIncomeAmountHistoryAsync(history, cancellationToken);
        }

        await _instrumentWriter.WriteCreditCardsAsync(validatedCards, cancellationToken);
        await _instrumentWriter.WriteLoansAsync(draft.Loans, cancellationToken);
        await _instrumentWriter.WritePaymentPlansAsync(draft.PaymentPlans, cancellationToken);
        await _instrumentWriter.WriteLargeExpensesAsync(draft.PlannedLargeExpenses, cancellationToken);
    }
}
