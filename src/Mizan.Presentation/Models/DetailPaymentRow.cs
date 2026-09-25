using System.Globalization;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// Dönem içinde vadesi gelen veya planlanan münferit bir borç/harcama satırını ve kart ödeme eylemlerini temsil eder.
/// </summary>
public sealed record DetailPaymentRow(
    DateOnly Date,
    string Name,
    string Category,
    decimal? Amount,
    DetailSemanticType Semantic)
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>Ödemenin ait olduğu dönemin başlangıç tarihi.</summary>
    public DateOnly PeriodDate { get; init; }

    /// <summary>Ödeme tutarının kesinleşmemiş bir tahmin olup olmadığı.</summary>
    public bool IsEstimated { get; init; }

    /// <summary>Vadesinin dönem başlangıç tarihinden önce olup olmadığı.</summary>
    public bool IsBeforePeriodStart { get; init; }

    /// <summary>Ödeme tutarının henüz belirlenmemiş olup olmadığı.</summary>
    public bool IsUndetermined { get; init; }

    /// <summary>Ödemeye dair açıklama veya not.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>İlgili ödeme bir kredi kartına aitse kartın benzersiz kimliği.</summary>
    public Guid? CreditCardId { get; init; }

    /// <summary>Kart için seçilen ödeme tercihi (asgari veya ekstre borcu).</summary>
    public CreditCardPaymentType? CardPaymentType { get; init; }

    /// <summary>Kullanıcının bu ekrandan kart ödeme tercihini değiştirip değiştiremeyeceği.</summary>
    public bool CanChangeCardPaymentMode { get; init; }

    /// <summary>Asgari ödemenin seçili olup olmadığını gösterir.</summary>
    public bool IsMinimumSelected => CardPaymentType == CreditCardPaymentType.Minimum;

    /// <summary>Dönem borcunun (tam ekstre) seçili olup olmadığını gösterir.</summary>
    public bool IsFullStatementSelected => CardPaymentType == CreditCardPaymentType.FullStatement;

    /// <summary>Kullanıcı arayüzünde gösterilecek biçimlendirilmiş Türkçe para metni.</summary>
    public string AmountText => Amount.HasValue
        ? $"{Amount.Value.ToString("N2", TurkishCulture)} TL"
        : "Belirlenmedi";
}
