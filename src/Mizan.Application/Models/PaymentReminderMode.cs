namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının borç ve ödeme günleri için bildirim alma sıklığını ve davranışını belirleyen mod.
/// </summary>
public enum PaymentReminderMode
{
    /// <summary>Bildirim gönderilmez.</summary>
    Off = 0,

    /// <summary>Yalnızca ödeme günü sabahı (09:00) tek bildirim gönderilir.</summary>
    Relaxed = 1,

    /// <summary>Ödeme gününden 3 gün önce, 1 gün önce akşam, ödeme günü sabah ve akşam olmak üzere 4 bildirim gönderilir.</summary>
    Aggressive = 2
}
