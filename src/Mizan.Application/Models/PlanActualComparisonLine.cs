namespace Mizan.Application.Models;

/// <summary>
/// Dondurulan veya revize edilen dönem planındaki tek bir kategori taahhüdü ile
/// dönem sonunda gerçekleşen fiilî tutarın karşılaştırma satırını temsil eden model.
/// Kapanan dönemin bütçe sapmalarını kategori bazında ayrıştırmak için vardır.
/// </summary>
public sealed record PlanActualComparisonLine(
    string Category,
    decimal Planned,
    decimal Actual,
    decimal Difference);
