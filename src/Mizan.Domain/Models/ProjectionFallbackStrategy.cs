namespace Mizan.Domain.Models;

/// <summary>
/// Gelecek dönem projeksiyonunda ekstre tutarı henüz bilinmiyorken kullanılacak yedek ödeme stratejisi.
/// </summary>
public enum ProjectionFallbackStrategy
{
    /// <summary>Gelecek dönem için herhangi bir ödeme varsayılmaz.</summary>
    None = 0,
    /// <summary>Tahmini asgari tutar kadar ödeme yapılacağı varsayılır.</summary>
    Minimum = 1,
    /// <summary>Tahmini ekstre borcunun tamamının ödeneceği varsayılır.</summary>
    FullStatement = 2,
    /// <summary>Kullanıcının belirlediği sabit tutar kadar ödeme yapılacağı varsayılır.</summary>
    FixedAmount = 3
}
