namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartı için kullanıcının benimsediği genel ödeme stratejisi.
/// Ekstre geldiğinde varsayılan tutarın nasıl belirleneceğini yönetir.
/// </summary>
public enum CreditCardPaymentStrategy
{
    /// <summary>Her ekstre kesildiğinde kullanıcıya ne kadar ödeyeceği sorulur.</summary>
    AskEachStatement = 0,
    /// <summary>Yasal asgari tutar ödenir.</summary>
    Minimum = 1,
    /// <summary>Ekstre borcunun tamamı ödenir, devreden bakiye bırakılmaz.</summary>
    FullStatement = 2,
    /// <summary>Kullanıcının belirlediği sabit bir tutar ödenir.</summary>
    FixedAmount = 3
}
