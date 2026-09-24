namespace Mizan.Application.Models;

/// <summary>
/// Hatırlatıcı arayüz kartında gün bazında gruplanmış özet satırını temsil eden model.
/// Bildirimin anlık başlığından farklı olarak bugüne göre göreceli zamanı ("3 gün sonra") gösterir.
/// </summary>
public sealed record PaymentReminderDay(
    DateOnly DueDate,
    string When,
    string What,
    string Schedule,
    IReadOnlyList<PaymentDue> Payments);
