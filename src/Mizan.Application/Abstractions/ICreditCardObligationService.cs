using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kredi kartı hesaplarının, hesap kesim ekstrelerinin, ödeme tercihi geçmişinin
/// ve vadeli özel ödeme planlarının yönetimini sağlayan kullanım senaryosu portu.
/// </summary>
public interface ICreditCardObligationService
{
    /// <summary>
    /// Kredi kartını doğrular, ödeme tercihi geçmişini kontrol edip günceller,
    /// depoya kaydeder ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task SaveCreditCardAsync(CreditCard card, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kredi kartına banka tarafından kesilen güncel ekstreyi ve bu ekstre için belirlenen
    /// ödeme planını bağlar, normalleştirir ve karta kaydeder.
    /// </summary>
    Task SaveCreditCardStatementAsync(
        Guid creditCardId,
        CreditCardStatement statement,
        CurrentStatementPaymentPlan paymentPlan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen kredi kartını kalıcı olarak siler ve açık dönem varsa plan revizyonu tetikler.
    /// </summary>
    Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kredi kartının belirli bir son ödeme tarihi için özel bir ödeme planı (Asgari, Tamamı veya Sabit Tutar)
    /// tanımlar ya da mevcut planı günceller.
    /// </summary>
    Task SaveCreditCardPaymentPlanAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CreditCardPaymentType paymentType,
        decimal? amount = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ekstre veya vade bazlı ödeme modunu ayarlar. Vade mevcut ekstrenin vadesiyse ekstre planı güncellenir;
    /// aksi hâlde özel ödeme planı kaydedilir. (Sabit tutar bu ekrandan seçilemez).
    /// </summary>
    Task SetStatementPaymentModeAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CreditCardPaymentType paymentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kredi kartının belirtilen son ödeme tarihindeki özel ödeme planını kaldırır.
    /// </summary>
    Task RemoveCreditCardPaymentPlanAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CancellationToken cancellationToken = default);
}
