namespace Mizan.Application.Models;

/// <summary>
/// Bildirimden ya da arayüz kartından gelen kullanıcı yanıtını taşıyan model.
/// Aynı güne düşen tüm ödemeler tek bir cevapla topluca işaretlenir.
/// </summary>
public sealed record PaymentReminderAnswer(
    PaymentReminderAnswerKind Kind,
    DateTime AnsweredAt,
    DateTime? SnoozedUntil,
    IReadOnlyList<PaymentDue> Payments);
