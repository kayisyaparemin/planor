namespace Mizan.Presentation.Models;

/// <summary>
/// Dönem ayrıntısındaki ödeme listesinin bir satırı (EK-V9 S3): "bu dönem neyi, hangi gün, ne kadar ödeyeceğim?"
/// Eskide her ödeme ayrı bir kart, kategori adı, tahsis metni ve üç çiple ~14 etiket tutuyordu; satır artık yalnız
/// ad, gün ve tutarı taşır (S75-4). Kart ekstresi satırı o kartın Kart Kontrol'ünü açar; ekstrenin nasıl ödeneceği
/// orada seçilir (S75-6).
/// </summary>
public sealed record PeriodPaymentRow
{
    /// <summary>Ödemenin vadesi.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Ödemenin adı (kredi, kart, taksit planı ya da büyük harcama).</summary>
    public required string Name { get; init; }

    /// <summary>Ödenecek tutar; kart ekstresinin tutarı belirlenemediyse <c>null</c>.</summary>
    public decimal? Amount { get; init; }

    /// <summary>Kartın ödeme şekli hiç seçilmediği için tutar tahmin mi (S75-4).</summary>
    public bool IsEstimate { get; init; }

    /// <summary>Satır bir kart ekstresiyse kartın kimliği; satır o kartın Kart Kontrol'ünü açar.</summary>
    public Guid? CardId { get; init; }

    /// <summary>Satır bir kart ekstresi mi; yalnız kart satırı dokunulabilir.</summary>
    public bool IsCard => CardId is not null;

    /// <summary>Tutar belirlenemedi mi; tutarın yerinde "Belirlenmedi" yazar.</summary>
    public bool IsUndetermined => Amount is null;
}
