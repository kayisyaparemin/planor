namespace Mizan.Domain.Models;

/// <summary>
/// Aktif ekstre için kullanıcının o aya özel seçtiği ödeme modu.
/// </summary>
public enum CurrentStatementPaymentMode
{
    /// <summary>Asgari ödeme tutarı seçildi.</summary>
    Minimum = 0,
    /// <summary>Ekstre borcunun tamamı seçildi.</summary>
    Full = 1,
    /// <summary>Kullanıcının belirlediği özel bir tutar seçildi.</summary>
    Custom = 2
}
