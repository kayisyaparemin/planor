using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kullanıcının kredi kartları, kart ekstreleri, taksitli harcamaları ve ödeme tercihleri
/// kümesini bütünsel bir kök varlık olarak kalıcı veri deposunda yöneten dar port arayüzü.
/// </summary>
public interface ICreditCardRepository
{
    /// <summary>
    /// Kayıtlı tüm kredi kartlarını bağlı ekstre, harcama ve ödeme tercihleriyle birlikte listeler.
    /// </summary>
    Task<IReadOnlyList<CreditCard>> GetCreditCardsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir kredi kartını ve alt bileşenlerini kaydeder veya günceller.
    /// </summary>
    Task UpsertCreditCardAsync(CreditCard card, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir kredi kartını ve alt bileşenlerini kalıcı olarak siler.
    /// </summary>
    Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default);
}
