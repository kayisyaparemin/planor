namespace Mizan.Domain.Models;

/// <summary>
/// Kesilmiş mevcut ekstre (<see cref="CreditCard.CurrentStatement"/>) için seçilen anlık ödeme modunu ve tutarını temsil eder.
/// Dönem kapanışı öncesinde kullanıcının o ekstre için yapmayı taahhüt ettiği ödeme kararını saklar.
/// </summary>
public sealed record CurrentStatementPaymentPlan
{
    /// <summary>Mevcut ekstre için geçerli ödeme modu (Asgari, Tamamı, Başka Tutar).</summary>
    public CurrentStatementPaymentMode Mode { get; init; } = CurrentStatementPaymentMode.Minimum;

    /// <summary>Özel tutar modu seçildiğinde ödenecek miktar.</summary>
    public decimal? CustomAmount { get; init; }
}
