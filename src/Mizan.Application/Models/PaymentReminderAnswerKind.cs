namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının bir hatırlatıcı bildirimine veya arayüz kartına verdiği aksiyon türü.
/// </summary>
public enum PaymentReminderAnswerKind
{
    /// <summary>Ödeme ertelendi; belirli bir süre sonra tekrar hatırlatılacak.</summary>
    Snoozed = 1,

    /// <summary>Ödeme yapıldı; bu vade için hatırlatıcılar durdurulacak.</summary>
    Paid = 2
}
