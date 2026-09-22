namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartına ait banka tarafından kesilmiş resmi hesap kesim ekstresini temsil eder.
/// Ekstre tutarı, asgari ödeme ve vade bilgilerini içererek o dönemin kart borcunun kesinleşmiş temelini oluşturur.
/// </summary>
public sealed record CreditCardStatement
{
    /// <summary>Ekstre kaydının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Ekstrenin ait olduğu kredi kartının kimliği.</summary>
    public Guid CreditCardId { get; init; }

    /// <summary>Ekstrenin düzenlendiği hesap kesim tarihi.</summary>
    public DateOnly StatementDate { get; init; }

    /// <summary>Ekstre borcunun en son ödenmesi gereken son ödeme tarihi.</summary>
    public DateOnly DueDate { get; init; }

    /// <summary>Ekstrede yer alan toplam borç tutarı.</summary>
    public decimal StatementAmount { get; init; }

    /// <summary>Yasal gecikmeye girmemek için ödenmesi gereken asgari tutar.</summary>
    public decimal MinimumPaymentAmount { get; init; }

    /// <summary>Banka tarafından bildirilen bir sonraki hesap kesim tarihi.</summary>
    public DateOnly? NextStatementDate { get; init; }

    /// <summary>Banka tarafından bildirilen bir sonraki son ödeme tarihi.</summary>
    public DateOnly? NextDueDate { get; init; }

    /// <summary>Kaydın sisteme ilk girildiği an.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Kaydın son güncellendiği an.</summary>
    public DateTimeOffset UpdatedAt { get; init; }
}
