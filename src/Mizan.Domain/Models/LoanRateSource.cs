namespace Mizan.Domain.Models;

/// <summary>
/// Kredi itfa analizinde aylık efektif faiz oranının ve anaparanın hangi veri kaynağından
/// türetildiğini belirtir. Kullanıcı doğrudan kalan anaparayı girmiş olabileceği gibi,
/// bankadan alınan tarihli resmi kapatma tutarı da kullanılmış olabilir.
/// </summary>
public enum LoanRateSource
{
    /// <summary>Faiz, kullanıcının girdiği kalan anapara borcundan bisection ile türetildi.</summary>
    RemainingPrincipal,

    /// <summary>Faiz ve anapara, bankanın tarihli resmi erken kapatma teklifinden çözüldü.</summary>
    BankQuote
}
