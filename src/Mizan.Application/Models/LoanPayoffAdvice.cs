namespace Mizan.Application.Models;

/// <summary>
/// "Bu krediyi şu gün kapatabilirsin" önerisi. Kapatma kararı bakiyeye bakılarak verilemez:
/// açık faizi kredi faizinden yüksekse krediyi açık parasıyla kapatmak zarardır. Bu model,
/// projeksiyonun kapamalı ve kapamasız karşılaştırmasından çıkan sonucu taşır.
/// </summary>
public sealed record LoanPayoffAdvice
{
    /// <summary>Önerinin ait olduğu kredinin kimliği.</summary>
    public Guid LoanId { get; init; }

    /// <summary>Kredinin banka ve adıyla gösterilen ismi.</summary>
    public string LoanName { get; init; } = string.Empty;

    /// <summary>Önerinin sonucu.</summary>
    public LoanPayoffAdviceStatus Status { get; init; }

    /// <summary>
    /// Önerilen (koşulları sağlayan en erken) kapatma günü; <see cref="LoanPayoffAdviceStatus.AlreadyClosing"/>
    /// durumunda planlanmış kapamanın günü.
    /// </summary>
    public DateOnly? Date { get; init; }

    /// <summary>Önerilen gün kapatmak için ödenecek tutar.</summary>
    public decimal? PayoffAmount { get; init; }

    /// <summary>Kredinin ömrü boyunca ödenmeyecek faiz: kapamasız toplam ödeme − kapamalı toplam ödeme.</summary>
    public decimal? InterestSaving { get; init; }

    /// <summary>
    /// Net kazanç: 12. dönem sonu bakiye farkı + ufuk sonrasında ödenmeyecek taksitler.
    /// Kapatma için kullanılan paranın açık faizi maliyeti bu farkın içindedir.
    /// </summary>
    public decimal? NetGain { get; init; }

    /// <summary>Koşulları sağlayan en kârlı gün; en erken günden farklıysa dolu.</summary>
    public DateOnly? BestDate { get; init; }

    /// <summary><see cref="BestDate"/> gününde kapatmanın net kazancı.</summary>
    public decimal? BestNetGain { get; init; }
}
