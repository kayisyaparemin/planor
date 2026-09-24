namespace Mizan.Application.Models;

/// <summary>
/// Hatırlatıcı kartı ve ödeme durum panosu için gerekli tüm görünüm verilerini toplayan model.
/// </summary>
public sealed record PaymentReminderBoard
{
    /// <summary>Kullanıcının etkin hatırlatıcı modu.</summary>
    public required PaymentReminderMode Mode { get; init; }

    /// <summary>Telefonda kurulacak bildirimler.</summary>
    public IReadOnlyList<PaymentReminder> Reminders { get; init; } = [];

    /// <summary>Kartın "Sıradaki ödemeler" satırları.</summary>
    public IReadOnlyList<PaymentReminderDay> Upcoming { get; init; } = [];

    /// <summary>Ertelenmiş ödemelerin yanıt satırları.</summary>
    public IReadOnlyList<PaymentReminderResponse> Snoozed { get; init; } = [];

    /// <summary>Ödenmiş olarak işaretlenen ödemelerin yanıt satırları.</summary>
    public IReadOnlyList<PaymentReminderResponse> Paid { get; init; } = [];

    /// <summary>Deneme bildirimi için sıradaki ilk ödeme günü bildirimi.</summary>
    public PaymentReminder? Sample { get; init; }
}
