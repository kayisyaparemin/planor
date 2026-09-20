namespace Mizan.Domain.Models;

/// <summary>
/// Krediye yapılan erken ödemenin borç ve ödeme planına yansıma biçimi.
/// </summary>
public enum LoanPrepaymentMode
{
    /// <summary>Kalan anaparanın tamamı ödenir, kredi tamamen kapatılır.</summary>
    FullClosure = 0,
    /// <summary>Taksit tutarı aynı kalır, vade kısalır (son taksit küçülebilir).</summary>
    ReduceTerm = 1,
    /// <summary>Vade aynı kalır, aylık taksit tutarı küçülür.</summary>
    ReduceInstallment = 2
}
