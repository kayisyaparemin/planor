namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartı ekstre projeksiyonunda ödeme tutarı ve tipinin hangi kaynaktan
/// (kesilmiş ekstre planı, son ödeme tarihi istisnası, genel kart stratejisi veya simülasyon yedeği)
/// çözümlendiğini belirten durum.
/// </summary>
public enum CreditCardPaymentResolution
{
    /// <summary>Hiçbir strateji veya plan eşleşmedi; ödeme tutarı belirlenemedi.</summary>
    Undetermined = 0,

    /// <summary>Belirli bir son ödeme tarihi için tanımlanmış özel ödeme planı uygulandı.</summary>
    DueDateOverride = 1,

    /// <summary>Kartın genel ödeme stratejisi (Asgari, Tamamı veya Sabit Tutar) uygulandı.</summary>
    GeneralStrategy = 2,

    /// <summary>Strateji belirsiz olduğunda simülasyon amaçlı yedek strateji uygulandı.</summary>
    ProjectionFallback = 3,

    /// <summary>Kesilmiş mevcut ekstre için kullanıcının belirlediği anlık plan uygulandı.</summary>
    CurrentStatementPlan = 4
}
