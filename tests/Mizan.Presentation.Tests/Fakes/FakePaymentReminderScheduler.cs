using Mizan.Application.Models;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// İşletim sistemi bildirim zamanlayıcı portunun sunum birim testleri için bellek içi sahtesidir.
/// </summary>
internal sealed class FakePaymentReminderScheduler : IPaymentReminderScheduler
{
    public Dictionary<Guid, IReadOnlyList<PaymentReminder>> ScheduledReminders { get; } = [];
    public List<PaymentReminderAnswer> PendingAnswers { get; set; } = [];
    public int AcknowledgedCount { get; private set; }
    public List<Guid> CancelledProfiles { get; } = [];

    public void Schedule(Guid profileId, IReadOnlyList<PaymentReminder> reminders)
    {
        ScheduledReminders[profileId] = reminders;
    }

    public IReadOnlyList<PaymentReminderAnswer> ReadPendingAnswers(Guid profileId) =>
        PendingAnswers;

    public void AcknowledgeAnswers(Guid profileId, int count)
    {
        AcknowledgedCount += count;
        if (PendingAnswers.Count >= count)
        {
            PendingAnswers.RemoveRange(0, count);
        }
    }

    public void CancelAll(Guid profileId)
    {
        CancelledProfiles.Add(profileId);
        ScheduledReminders.Remove(profileId);
    }
}
