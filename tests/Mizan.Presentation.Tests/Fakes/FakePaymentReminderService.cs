using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Hatırlatıcı servis portunun sunum birim testleri için hafif bellek içi sahtesidir.
/// </summary>
internal sealed class FakePaymentReminderService : IPaymentReminderService
{
    public PaymentReminderMode Mode { get; set; } = PaymentReminderMode.Relaxed;
    public PaymentReminderBoard CurrentBoard { get; set; } = new() { Mode = PaymentReminderMode.Relaxed };
    public List<PaymentReminderAnswer> RecordedAnswers { get; } = [];
    public List<string> UndoneDueKeys { get; } = [];
    public List<PaymentDue> UpcomingDues { get; } = [];

    public Task<PaymentReminderMode> GetModeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Mode);

    public Task SaveModeAsync(PaymentReminderMode mode, CancellationToken cancellationToken = default)
    {
        Mode = mode;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PaymentReminder>> GetRemindersAsync(DateTime now, CancellationToken cancellationToken = default) =>
        Task.FromResult(CurrentBoard.Reminders);

    public Task<PaymentReminderBoard> GetBoardAsync(DateTime now, CancellationToken cancellationToken = default) =>
        Task.FromResult(CurrentBoard);

    public Task RecordAnswerAsync(PaymentReminderAnswer answer, CancellationToken cancellationToken = default)
    {
        RecordedAnswers.Add(answer);
        return Task.CompletedTask;
    }

    public Task UndoAnswerAsync(string dueKey, CancellationToken cancellationToken = default)
    {
        UndoneDueKeys.Add(dueKey);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PaymentReminderResponse>> GetResponsesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PaymentReminderResponse>>(CurrentBoard.Paid.Concat(CurrentBoard.Snoozed).ToArray());

    public Task<IReadOnlyList<PaymentDue>> GetUpcomingPaymentDuesAsync(DateTime now, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PaymentDue>>(UpcomingDues);
}
