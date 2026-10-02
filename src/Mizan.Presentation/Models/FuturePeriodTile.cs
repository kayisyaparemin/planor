namespace Mizan.Presentation.Models;

/// <summary>
/// "12 Dönem" ızgarasının bir karosu: bir dönemin sonunda ne kalacağı (S3). Eski ekranda her dönem ~18 etiketlik
/// bir karttı; dönemin kırılımı artık dönem ayrıntısında (V9), karo yalnız dönemi ve sonunu taşır (GS26-1).
/// </summary>
/// <param name="Index">Dönemin zincirdeki sırası (0–11); ızgaradaki yerini sayfa bundan çıkarır.</param>
/// <param name="PeriodStart">Dönemin ilk günü; karo dönemi ayıyla adlandırır.</param>
/// <param name="EndingBalance">Dönem sonu bakiyesi.</param>
/// <param name="IsLowest">12 dönemin en düşük sonu mu; hero'daki dönemle aynı karo kalın görünür.</param>
/// <param name="ShowsYear">Ay adının yanında yıl yazılır mı: ilk karoda ve yılın ilk döneminde.</param>
public sealed record FuturePeriodTile(
    int Index,
    DateOnly PeriodStart,
    decimal EndingBalance,
    bool IsLowest,
    bool ShowsYear)
{
    /// <summary>Dönem eksiyle bitiyor mu; karonun tutarı olumsuz renge döner.</summary>
    public bool IsNegative => EndingBalance < 0m;
}
