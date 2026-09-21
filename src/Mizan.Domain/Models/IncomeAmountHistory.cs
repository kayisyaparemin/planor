namespace Mizan.Domain.Models;

/// <summary>
/// Düzenli bir gelir akışının zaman içindeki etkin tarihli tutar veya zam revizyonunu temsil eder.
/// Bir akışın geçmişteki ve gelecekteki tutar değişikliklerini saklar; 'en son kazanan' kuralı akış bazında işletilir.
/// </summary>
public sealed record IncomeAmountHistory
{
    /// <summary>Tutar revizyon kaydının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Bağlı olduğu düzenli gelir akışının kimliği.</summary>
    public Guid RecurringIncomeId { get; init; }

    /// <summary>Bu tarihten itibaren geçerli olan net gelir tutarı.</summary>
    public decimal Amount { get; init; }

    /// <summary>Tutarın yürürlüğe girdiği etkin başlangıç tarihi.</summary>
    public DateOnly EffectiveDate { get; init; }

    /// <summary>Revizyona dair açıklama (örn. Yılbaşı Zammı, Enflasyon Farkı).</summary>
    public string Description { get; init; } = string.Empty;
}
