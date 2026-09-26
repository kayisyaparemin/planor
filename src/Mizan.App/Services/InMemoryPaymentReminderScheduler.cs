using Mizan.Application.Models;
using Mizan.Presentation.Services;

namespace Mizan.App.Services;

/// <summary>
/// İşletim sistemi bildirim zamanlamalarını ve bekleyen kullanıcı yanıtlarını
/// bellek içinde yöneten yerel zamanlayıcı sağlayıcısıdır.
/// </summary>
public sealed class InMemoryPaymentReminderScheduler : IPaymentReminderScheduler
{
    private readonly Dictionary<Guid, IReadOnlyList<PaymentReminder>> _schedules = [];
    private readonly Dictionary<Guid, List<PaymentReminderAnswer>> _pendingAnswers = [];
    private readonly object _lock = new();

    /// <inheritdoc />
    public void Schedule(Guid profileId, IReadOnlyList<PaymentReminder> reminders)
    {
        lock (_lock)
        {
            _schedules[profileId] = reminders;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<PaymentReminderAnswer> ReadPendingAnswers(Guid profileId)
    {
        lock (_lock)
        {
            return _pendingAnswers.TryGetValue(profileId, out var list)
                ? list.ToArray()
                : [];
        }
    }

    /// <inheritdoc />
    public void AcknowledgeAnswers(Guid profileId, int count)
    {
        lock (_lock)
        {
            if (_pendingAnswers.TryGetValue(profileId, out var list))
            {
                var removeCount = Math.Min(count, list.Count);
                list.RemoveRange(0, removeCount);
            }
        }
    }

    /// <inheritdoc />
    public void CancelAll(Guid profileId)
    {
        lock (_lock)
        {
            _schedules.Remove(profileId);
            _pendingAnswers.Remove(profileId);
        }
    }

    /// <summary>Test veya simülasyon amaçlı bildirim yanıtı kuyruğuna yanıt ekler.</summary>
    public void EnqueueAnswer(Guid profileId, PaymentReminderAnswer answer)
    {
        lock (_lock)
        {
            if (!_pendingAnswers.TryGetValue(profileId, out var list))
            {
                list = [];
                _pendingAnswers[profileId] = list;
            }

            list.Add(answer);
        }
    }
}
