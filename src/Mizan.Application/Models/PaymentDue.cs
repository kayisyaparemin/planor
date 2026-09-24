namespace Mizan.Application.Models;

/// <summary>
/// Hatırlatılacak tekil bir ödeme kalemini kaynak, isim, vade ve tutarıyla temsil eden model.
/// </summary>
public sealed record PaymentDue(
    string Key,
    string Name,
    DateOnly DueDate,
    decimal? Amount);
