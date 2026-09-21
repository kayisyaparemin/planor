namespace Mizan.Domain.Models;

/// <summary>
/// Kredi itfa analizi sırasında tespit edilen engelleri veya anapara uyumsuzluklarını tanımlar.
/// Hatalı veya bayat anapara girişlerini erken safhada yakalayarak simülasyon ve erken kapama
/// tavsiye motorlarının yanlış hesap yapmasını önlemek için var edilmiştir.
/// </summary>
public enum LoanAnalysisIssue
{
    /// <summary>Herhangi bir sorun yok; itfa ve faiz başarıyla çözümlendi.</summary>
    None,

    /// <summary>Kalan taksit sayısı 1'den küçük veya kredi pasif durumda.</summary>
    Finished,

    /// <summary>Kredinin ne kalan anapara borcu ne de geçerli banka kapama teklifi mevcut.</summary>
    MissingPrincipal,

    /// <summary>
    /// Kalan anapara borcu, kalan taksitlerin toplamına eşit veya büyük girilmiş (faiz ≤ 0).
    /// Kullanıcının anapara yerine kalan toplam taksit borcunu girdiği durumlarda oluşur.
    /// </summary>
    PrincipalNotBelowInstallments,

    /// <summary>
    /// Türetilen aylık faiz makul sınırın (%8) üstünde; girilen anapara güncel değil veya hatalı.
    /// </summary>
    ImplausibleRate
}
