using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kredilere ait kısmi ara ödeme veya erken kapama taahhütlerini saklayan
/// <c>loan_prepayments</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableLoanPrepayments)]
internal sealed class LoanPrepaymentEntity
{
    /// <summary>Erken ödeme kaydının metin formatındaki benzersiz GUID kimliği.</summary>
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>Hedef kredinin metin formatındaki GUID kimliği.</summary>
    [Indexed]
    public string LoanId { get; set; } = string.Empty;

    /// <summary>Erken ödeme tarihi (yyyy-MM-dd).</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>Erken ödeme modu (0: FullClosure, 1: ReduceInstallment, 2: ReduceTerm).</summary>
    public int Mode { get; set; }

    /// <summary>Kısmi ödemede anaparadan düşülecek tutar.</summary>
    public decimal? PrincipalAmount { get; set; }
}
