namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartı ödeme planındaki münferit bir ödemenin niteliği.
/// </summary>
public enum CreditCardPaymentType
{
    /// <summary>Belirlenen sabit tutarlı ödeme.</summary>
    FixedAmount = 0,
    /// <summary>Asgari tutar ödemesi.</summary>
    Minimum = 1,
    /// <summary>Ekstre borcunun tamamının ödenmesi.</summary>
    FullStatement = 2
}
