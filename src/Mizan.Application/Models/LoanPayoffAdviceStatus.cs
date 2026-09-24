namespace Mizan.Application.Models;

/// <summary>
/// Bir kredi için erken kapama önerisinin sonucunu sınıflandırır. Kullanıcıya yalnız "kapat"
/// ya da "kapatma" demek yetmez; önerinin neden çıkmadığı (açık, kazançsızlık, eksik anapara)
/// ekranın vereceği tavsiyeyi değiştirdiği için ayrı durumlar vardır.
/// </summary>
public enum LoanPayoffAdviceStatus
{
    /// <summary>Hiçbir dönemde açığı büyütmeden ve kazançla kapatılabilecek bir taksit günü var.</summary>
    Recommended,

    /// <summary>Kapatmak kazançlı olurdu ama her aday günde bir dönemde açık oluşuyor ya da büyüyor.</summary>
    NoSafeMonth,

    /// <summary>Hiçbir aday günde kapatmak net kazanç sağlamıyor.</summary>
    NotWorthIt,

    /// <summary>Faiz türetilemiyor; kalan anapara ya da bankanın kapatma tutarı gerekli.</summary>
    NeedsPrincipal,

    /// <summary>Bu kredi için zaten bir erken kapama planlanmış.</summary>
    AlreadyClosing
}
