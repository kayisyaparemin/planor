namespace Mizan.Application.Models;

/// <summary>
/// Kapanan bir nakit akış döneminin dondurulan/revize planı ile fiilî gerçekleşmesi
/// arasındaki genel bakiye farkını, kategorik karşılaştırma satırlarını ve
/// kullanıcıya sunulan Türkçe özet metnini içeren sonuç sözleşmesi.
/// Dönem karnesinin kullanıcıya ve tarihçeye tek parça sunulmasını sağlamak için vardır.
/// </summary>
public sealed record PlanActualComparison(
    decimal PlannedEndingBalance,
    decimal ActualEndingBalance,
    decimal Difference,
    string Summary,
    IReadOnlyList<PlanActualComparisonLine> Lines);
