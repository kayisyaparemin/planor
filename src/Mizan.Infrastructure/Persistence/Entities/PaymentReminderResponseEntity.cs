using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kullanıcının ödeme hatırlatıcı bildirimlerine verdiği yanıtın (ödendi / ertelendi) SQLite tablo varlığı.
/// </summary>
[Table("payment_reminder_responses")]
internal sealed class PaymentReminderResponseEntity
{
    [PrimaryKey]
    public string DueKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DueDate { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public int Kind { get; set; }

    public string AnsweredAt { get; set; } = string.Empty;

    public string? SnoozedUntil { get; set; }
}
