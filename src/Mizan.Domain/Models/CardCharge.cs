namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartına yansıyacak bekleyen harcama veya geleceğe sarkan taksitli işlem kalemini temsil eder.
/// Henüz ekstreye girmemiş harcamaların ve taksitlerin nakit akış dönemlerine borç olarak yansıtılmasını sağlar.
/// </summary>
public sealed record CardCharge
{
    /// <summary>Harcama veya taksit kaleminin benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>İşlemin ait olduğu kredi kartının kimliği.</summary>
    public Guid CreditCardId { get; init; }

    /// <summary>Harcamanın açıklaması (örn. Market, Beyaz Eşya 2/6).</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>İşlemin ekstreye veya karta yansıyacağı hesap tarihi.</summary>
    public DateOnly PostingDate { get; init; }

    /// <summary>Harcama veya taksit tutarı.</summary>
    public decimal Amount { get; init; }
}
