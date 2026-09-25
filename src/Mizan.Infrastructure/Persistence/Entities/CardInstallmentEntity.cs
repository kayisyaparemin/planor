using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kredi kartına ait bekleyen veya taksitli harcamaları saklayan
/// <c>card_installments</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableCardInstallments)]
internal sealed class CardInstallmentEntity
{
    /// <summary>Harcama veya taksit kaleminin metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Bağlı olduğu kredi kartının metin formatındaki GUID kimliği.</summary>
    [Indexed]
    public string CreditCardId { get; set; } = string.Empty;

    /// <summary>Harcamanın açıklaması.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Harcamanın karta yansıyacağı hesap tarihi (yyyy-MM-dd).</summary>
    [Indexed]
    public string PostingDate { get; set; } = string.Empty;

    /// <summary>Harcama tutarı.</summary>
    public decimal Amount { get; set; }
}
