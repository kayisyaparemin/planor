using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Ödeme günü hatırlatıcılarının zamanlamasını, panosunu, yaklaşan vadeleri ve kullanıcı yanıtlarını
/// koordine eden kullanım senaryosu servisidir.
/// </summary>
public sealed class PaymentReminderService : IPaymentReminderService
{
    private readonly IPaymentReminderRepository _reminderRepository;
    private readonly IPeriodHistoryRepository _periodHistoryRepository;
    private readonly PaymentDueCollector _dueCollector;

    /// <summary>Çekirdek bağımlılıklarla servisi başlatır.</summary>
    public PaymentReminderService(
        IPaymentReminderRepository reminderRepository,
        IPeriodHistoryRepository periodHistoryRepository,
        PaymentDueCollector dueCollector)
    {
        _reminderRepository = reminderRepository ?? throw new ArgumentNullException(nameof(reminderRepository));
        _periodHistoryRepository = periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
        _dueCollector = dueCollector ?? throw new ArgumentNullException(nameof(dueCollector));
    }

    /// <summary>Tüm odaklı portlarla servisi başlatan kolaylık yapıcısı.</summary>
    public PaymentReminderService(
        IPaymentReminderRepository reminderRepository,
        IPeriodHistoryRepository periodHistoryRepository,
        IPeriodObservationRepository periodObservationRepository,
        IPlanReader planReader,
        FinancialProjectionService projectionService)
        : this(
            reminderRepository,
            periodHistoryRepository,
            new PaymentDueCollector(periodObservationRepository, planReader, projectionService))
    {
    }

    /// <inheritdoc />
    public Task<PaymentReminderMode> GetModeAsync(CancellationToken cancellationToken = default) =>
        _reminderRepository.GetModeAsync(cancellationToken);

    /// <inheritdoc />
    public Task SaveModeAsync(PaymentReminderMode mode, CancellationToken cancellationToken = default) =>
        _reminderRepository.SaveModeAsync(mode, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentReminder>> GetRemindersAsync(
        DateTime now,
        CancellationToken cancellationToken = default) =>
        (await GetBoardAsync(now, cancellationToken)).Reminders;

    /// <inheritdoc />
    public async Task<PaymentReminderBoard> GetBoardAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var mode = await _reminderRepository.GetModeAsync(cancellationToken);
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var openPlan = history.FindOpenPlan();
        var responses = (await _reminderRepository.GetResponsesAsync(cancellationToken))
            .Where(x => openPlan is null || x.DueDate > openPlan.PeriodStart)
            .ToArray();
        var snoozed = responses.Where(x => x.Kind == PaymentReminderAnswerKind.Snoozed).ToArray();
        var paid = responses.Where(x => x.Kind == PaymentReminderAnswerKind.Paid).ToArray();

        if (mode == PaymentReminderMode.Off)
        {
            return new PaymentReminderBoard { Mode = mode, Snoozed = snoozed, Paid = paid };
        }

        var dues = await GetUpcomingPaymentDuesAsync(now, cancellationToken);
        var reminders = PaymentReminderPlanner.Plan(mode, dues, now)
            .Concat(PaymentReminderPlanner.FollowUps(snoozed, now))
            .OrderBy(x => x.NotifyAt)
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .ToArray();
        var snoozedKeys = snoozed.Select(x => x.DueKey).ToHashSet(StringComparer.Ordinal);
        var activeDues = dues.Where(x => !snoozedKeys.Contains(x.Key));
        var upcoming = PaymentReminderFormatter.Preview(PaymentReminderPlanner.Plan(mode, activeDues, now), now);

        return new PaymentReminderBoard
        {
            Mode = mode,
            Reminders = reminders,
            Upcoming = upcoming,
            Snoozed = snoozed,
            Paid = paid,
            Sample = PaymentReminderPlanner.Sample(dues, now)
        };
    }

    /// <inheritdoc />
    public async Task RecordAnswerAsync(
        PaymentReminderAnswer answer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(answer);
        if (answer.Payments.Count == 0)
        {
            return;
        }

        var existing = (await _reminderRepository.GetResponsesAsync(cancellationToken))
            .ToDictionary(x => x.DueKey, StringComparer.Ordinal);

        var responses = answer.Payments
            .GroupBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => x.First())
            .Where(x => answer.Kind == PaymentReminderAnswerKind.Paid ||
                        !existing.TryGetValue(x.Key, out var previous) ||
                        previous.Kind != PaymentReminderAnswerKind.Paid)
            .Select(x => new PaymentReminderResponse
            {
                DueKey = x.Key,
                Name = x.Name,
                DueDate = x.DueDate,
                Amount = x.Amount,
                Kind = answer.Kind,
                AnsweredAt = answer.AnsweredAt,
                SnoozedUntil = answer.Kind == PaymentReminderAnswerKind.Snoozed ? answer.SnoozedUntil : null
            })
            .ToArray();

        if (responses.Length > 0)
        {
            await _reminderRepository.UpsertResponsesAsync(responses, cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task UndoAnswerAsync(string dueKey, CancellationToken cancellationToken = default) =>
        _reminderRepository.DeleteResponseAsync(dueKey, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<PaymentReminderResponse>> GetResponsesAsync(
        CancellationToken cancellationToken = default) =>
        _reminderRepository.GetResponsesAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentDue>> GetUpcomingPaymentDuesAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var openPlan = history.FindOpenPlan();
        if (openPlan is null)
        {
            return [];
        }

        var dues = await _dueCollector.CollectAsync(openPlan, history, now, cancellationToken);

        var answeredPaid = (await _reminderRepository.GetResponsesAsync(cancellationToken))
            .Where(x => x.Kind == PaymentReminderAnswerKind.Paid)
            .Select(x => x.DueKey)
            .ToHashSet(StringComparer.Ordinal);

        return dues
            .Where(x => !answeredPaid.Contains(x.Key))
            .GroupBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => x.First())
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .ToArray();
    }
}
