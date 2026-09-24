using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Hatırlatıcı tercih modunun ve kullanıcı yanıtlarının kalıcı veri erişimini sağlayan dar port arayüzü.
/// </summary>
public interface IPaymentReminderRepository
{
    /// <summary>
    /// Kullanıcının belirlediği mevcut hatırlatıcı bildirim modunu getirir.
    /// </summary>
    Task<PaymentReminderMode> GetModeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının hatırlatıcı bildirim modunu kalıcı olarak kaydeder.
    /// </summary>
    Task SaveModeAsync(PaymentReminderMode mode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kayıtlı tüm hatırlatıcı yanıtlarını (ödendi ve ertelendi kayıtları) getirir.
    /// </summary>
    Task<IReadOnlyList<PaymentReminderResponse>> GetResponsesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Hatırlatıcı yanıtlarını kalıcı veri deposuna ekler veya günceller.
    /// </summary>
    Task UpsertResponsesAsync(IReadOnlyList<PaymentReminderResponse> responses, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen ödeme anahtarına (<c>dueKey</c>) ait yanıtı kalıcı veri deposundan siler.
    /// </summary>
    Task DeleteResponseAsync(string dueKey, CancellationToken cancellationToken = default);
}
