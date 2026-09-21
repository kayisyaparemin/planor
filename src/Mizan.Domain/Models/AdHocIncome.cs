namespace Mizan.Domain.Models;

/// <summary>
/// Tekrar etmeyen, belirli bir kesin tarihte tahsil edilecek tek seferlik arızi geliri (ikramiye, prim, satış, iade vb.) temsil eder.
/// Nakit akış projeksiyonlarında ve dönem bütçelerinde arızi nakit girişlerinin takibini sağlar.
/// </summary>
public sealed record AdHocIncome
{
    /// <summary>Gelirin benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Tahsil edilecek net tutar.</summary>
    public decimal Amount { get; init; }

    /// <summary>Gelirin hesaba geçeceği kesin tarih.</summary>
    public DateOnly ExactDate { get; init; }

    /// <summary>Gelirin açıklaması (örn. Yıl Sonu Primi, Vergi İadesi).</summary>
    public string Description { get; init; } = string.Empty;
}
