using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kurulum sihirbazında toplanan başlangıç verilerini dondurarak ilk finansal planı
/// oluşturan uygulama servisidir. İlk gözlem yazılmaz: dönem planın açılış bakiyesiyle başlar, ilk noktayı
/// kullanıcı girmez (S68-1); eskiden yazılan gözlem çapa günü ilerideyse dönemden önceki güne düşüyordu.
/// </summary>
public sealed class OnboardingService(
    OnboardingPlanWriter planWriter,
    FinancialSnapshotService snapshotService,
    IClock clock) : IOnboardingService
{
    private readonly OnboardingPlanWriter _planWriter =
        planWriter ?? throw new ArgumentNullException(nameof(planWriter));
    private readonly FinancialSnapshotService _snapshotService =
        snapshotService ?? throw new ArgumentNullException(nameof(snapshotService));
    private readonly IClock _clock =
        clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public async Task InitializeFromOnboardingAsync(
        OnboardingDraft draft,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var today = _clock.Today;
        var normalizedSettings = NormalizeSettings(draft.Settings);
        var validatedCards = NormalizeCards(draft.CreditCards, normalizedSettings.ProjectionAnchorDate, normalizedSettings.PeriodAnchor.DayOfMonth, today);
        await _planWriter.WritePlanDataAsync(draft, normalizedSettings, validatedCards, cancellationToken);

        var plan = new FinancialPlan
        {
            Settings = normalizedSettings,
            RecurringIncomes = draft.RecurringIncomes,
            IncomeHistories = draft.IncomeAmountHistories,
            CreditCards = validatedCards,
            Loans = draft.Loans,
            PaymentPlans = draft.PaymentPlans,
            PlannedLargeExpenses = draft.PlannedLargeExpenses
        };

        await _snapshotService.EnsureInitialSnapshotAsync(plan, cancellationToken);
    }

    private UserSettings NormalizeSettings(UserSettings settings)
    {
        var anchor = settings.PeriodAnchor;
        var today = _clock.Today;
        var candidateAnchor = CalendarRules.ResolveDay(today.Year, today.Month, anchor.DayOfMonth);
        var anchorDate = today <= candidateAnchor
            ? candidateAnchor
            : CalendarRules.AddMonthsKeepingDay(candidateAnchor, 1, anchor.DayOfMonth);

        return settings with { ProjectionAnchorDate = anchorDate };
    }

    private CreditCard[] NormalizeCards(
        IReadOnlyList<CreditCard> cards, DateOnly anchorDate, int anchorDay, DateOnly today)
    {
        var periodEnd = CalendarRules.AddMonthsKeepingDay(anchorDate, 1, anchorDay);
        var period = new CashFlowPeriod(anchorDate, periodEnd);

        return cards.Select(card =>
        {
            var baseCard = card.BalanceAsOfDate == default ? card with { BalanceAsOfDate = today } : card;
            if (baseCard.CarriedBalance <= 0m || baseCard.CurrentStatement is not null)
            {
                return baseCard;
            }

            var candidateDue = CalendarRules.ResolveDay(anchorDate.Year, anchorDate.Month, baseCard.PaymentDueDay);
            var dueDate = period.Contains(candidateDue)
                ? candidateDue
                : CalendarRules.AddMonthsKeepingDay(candidateDue, 1, baseCard.PaymentDueDay);

            var closeMonth = baseCard.StatementClosingDay < baseCard.PaymentDueDay ? dueDate : CalendarRules.AddMonthsKeepingDay(dueDate, -1, baseCard.StatementClosingDay);
            var statementDate = CalendarRules.ResolveDay(closeMonth.Year, closeMonth.Month, baseCard.StatementClosingDay);

            var statement = new CreditCardStatement
            {
                Id = Guid.NewGuid(),
                CreditCardId = baseCard.Id,
                StatementDate = statementDate,
                DueDate = dueDate,
                StatementAmount = baseCard.CarriedBalance,
                MinimumPaymentAmount = MoneyRules.Round(baseCard.CarriedBalance * (baseCard.Limit > 50000m ? 0.40m : 0.20m)),
                CreatedAt = _clock.UtcNow,
                UpdatedAt = _clock.UtcNow
            };

            return baseCard with { CurrentStatement = statement };
        }).ToArray();
    }
}
