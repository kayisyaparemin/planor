using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kredi kartı ekstre ödeme tercihlerinin etkin tarihli (effective-dated) tarihçe kayıtlarını saklayan
/// <c>credit_card_payment_preferences</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableCreditCardPaymentPreferences)]
internal sealed class CreditCardPaymentPreferenceEntity
{
    /// <summary>Tercih kaydının metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Bağlı olduğu kredi kartının metin formatındaki GUID kimliği.</summary>
    [Indexed]
    public string CreditCardId { get; set; } = string.Empty;

    /// <summary>Ödeme modu (0: Minimum, 1: Full, 2: Custom).</summary>
    public int Mode { get; set; }

    /// <summary>Özel ödeme tutarı.</summary>
    public decimal? CustomAmount { get; set; }

    /// <summary>Tercihin yürürlüğe girdiği ekstre tarihi (yyyy-MM-dd).</summary>
    [Indexed]
    public string EffectiveFromStatementDate { get; set; } = string.Empty;

    /// <summary>Tercihin oluşturulduğu an (ISO 8601 UTC).</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>Kullanıcı açıklaması veya notu.</summary>
    public string Note { get; set; } = string.Empty;
}
