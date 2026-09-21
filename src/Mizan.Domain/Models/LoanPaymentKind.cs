namespace Mizan.Domain.Models;

/// <summary>
/// Bir kredinin takvimindeki nakit çıkışının niteliğini (normal taksit, tam erken kapama veya kısmi ara ödeme) belirtir.
/// </summary>
public enum LoanPaymentKind
{
    /// <summary>Kredinin standart aylık annüite taksit ödemesi.</summary>
    Installment = 0,

    /// <summary>Kalan tüm borcu kapatan erken kapama ödemesi.</summary>
    EarlyClosure = 1,

    /// <summary>Kalan anaparadan düşülen kısmi ara ödeme.</summary>
    PartialPrepayment = 2
}
