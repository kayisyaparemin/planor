namespace Mizan.Domain.Models;

/// <summary>
/// Nakit akışı planlamasının temel zaman dilimi olan ve iki dönem çapası arasında kalan
/// yarı açık tarih aralığını [Start, End) temsil eder. Dönem sonu (End) sonraki dönemin başlangıcıdır
/// ve bu döneme dahil değildir; böylece günler arasında boşluk veya çakışma oluşmaz.
/// </summary>
public sealed record CashFlowPeriod
{
    /// <summary>Dönemin ilk günü (aralığa dahil).</summary>
    public DateOnly Start { get; }

    /// <summary>Dönemin bitiş anı / sonraki dönemin ilk günü (aralığa dahil değil).</summary>
    public DateOnly End { get; }

    /// <summary>Dönem içerisindeki net gün sayısı.</summary>
    public int DayCount => End.DayNumber - Start.DayNumber;

    /// <summary>
    /// Başlangıç ve bitiş sınırlarıyla yeni bir nakit akış dönemi oluşturur.
    /// </summary>
    /// <param name="start">Dönem başlangıç tarihi (dahil).</param>
    /// <param name="end">Dönem bitiş tarihi (dahil değil).</param>
    /// <exception cref="ArgumentException">Bitiş tarihi başlangıç tarihinden sonra değilse fırlatılır.</exception>
    public CashFlowPeriod(DateOnly start, DateOnly end)
    {
        if (end <= start)
        {
            throw new ArgumentException("Dönem bitiş tarihi başlangıç tarihinden sonra olmalıdır.", nameof(end));
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Belirtilen tarihin bu dönemin yarı açık aralığına [Start, End) dahil olup olmadığını belirler.
    /// </summary>
    /// <param name="date">Kontrol edilecek tarih.</param>
    /// <returns>Tarih döneme dahilse true, aksi takdirde false.</returns>
    public bool Contains(DateOnly date) => date >= Start && date < End;
}
