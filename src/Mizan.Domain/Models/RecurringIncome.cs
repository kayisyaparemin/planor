namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının düzenli olarak tekrar eden bir gelir kaynağını (aylık gelir, kira geliri, düzenli hakediş vb.) temsil eder.
/// Nakit akış dönemlerinde periyodik gelir girişlerinin üretilmesi ve bütçe planlaması için temel tanımdır.
/// </summary>
public sealed record RecurringIncome
{
    /// <summary>Gelir akışının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gelir akışının adı (örn. Aylık Gelir, Kira Geliri, Emekli Aylığı).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gelirin her ay hesaba geçtiği gün (1-31).</summary>
    public int PaymentDay { get; init; }

    /// <summary>Gelir akışının aktif olarak devam edip etmediği.</summary>
    public bool IsActive { get; init; } = true;
}
