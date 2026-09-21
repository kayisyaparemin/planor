namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir vadeye bağlanmış takvimli bir nakit çıkış veya taksit tutarını temsil eden değer nesnesi.
/// Kredi kartı taksitleri, geçici ödeme planları ve simülasyon harcamalarında vade-tutar eşleşmesini taşır.
/// </summary>
/// <param name="Date">Taksit veya harcamanın vadesi (ödeme tarihi).</param>
/// <param name="Amount">İlgili vadede ödenmesi gereken kuruş hassasiyetinde tutar.</param>
public sealed record ScheduledAmount(DateOnly Date, decimal Amount);
