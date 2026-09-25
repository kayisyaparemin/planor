using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Ödeme günü hatırlatıcılarının zamanlamasını, panosunu, yaklaşan vadeleri ve kullanıcı yanıtlarını
/// yöneten kullanım senaryosu portu.
/// </summary>
public interface IPaymentReminderService
{
    /// <summary>Kullanıcının mevcut hatırlatıcı bildirim modunu getirir.</summary>
    Task<PaymentReminderMode> GetModeAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Kullanıcının hatırlatıcı bildirim modunu kaydeder.</summary>
    Task SaveModeAsync(
        PaymentReminderMode mode,
        CancellationToken cancellationToken = default);

    /// <summary>Belirtilen an itibarıyla zamanlanmış tüm hatırlatıcı bildirimlerini getirir.</summary>
    Task<IReadOnlyList<PaymentReminder>> GetRemindersAsync(
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <summary>Hatırlatıcı kartı ve panosu için tüm bildirim ve durum özetini üretir.</summary>
    Task<PaymentReminderBoard> GetBoardAsync(
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <summary>Kullanıcının bildirim üzerinden veya karttan verdiği ödeme yanıtını kaydeder.</summary>
    Task RecordAnswerAsync(
        PaymentReminderAnswer answer,
        CancellationToken cancellationToken = default);

    /// <summary>Belirtilen ödeme anahtarına (<c>dueKey</c>) ait yanıtı geri alır/siler.</summary>
    Task UndoAnswerAsync(
        string dueKey,
        CancellationToken cancellationToken = default);

    /// <summary>Kayıtlı tüm hatırlatıcı yanıtlarını getirir.</summary>
    Task<IReadOnlyList<PaymentReminderResponse>> GetResponsesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Ufuktaki (35 gün) yaklaşan ödeme vadelerini açık plandan ve projeksiyondan derler.</summary>
    Task<IReadOnlyList<PaymentDue>> GetUpcomingPaymentDuesAsync(
        DateTime now,
        CancellationToken cancellationToken = default);
}
