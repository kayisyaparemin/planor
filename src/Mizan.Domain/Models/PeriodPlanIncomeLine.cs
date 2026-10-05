namespace Mizan.Domain.Models;

/// <summary>
/// Dondurulmuş bir dönem planına (PeriodPlanSnapshot) ait tekil bir gelir satırı.
/// Dönem başında beklenen her gelir, ne zaman yatacağıyla birlikte bu modelle dondurulur;
/// gidişat, gözlem gününden önce yatan geliri bakiyenin içinde, sonra yatacak olanı
/// dönem sonuna eklenecek tutar olarak ayırabilsin diye vardır (S31).
/// </summary>
public sealed record PeriodPlanIncomeLine
{
    /// <summary>Gelir satırının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Satırın ait olduğu dondurulmuş dönem planının kimliği.</summary>
    public Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Gelirin türü (düzenli veya tek seferlik).</summary>
    public IncomeSourceType SourceType { get; init; }

    /// <summary>Kaynak düzenli gelir akışının kimliği (düzenli gelirse).</summary>
    public Guid? RecurringIncomeId { get; init; }

    /// <summary>Kaynak tek seferlik gelirin kimliği (tek seferlik gelirse).</summary>
    public Guid? AdHocIncomeId { get; init; }

    /// <summary>Gelirin adı veya açıklaması (örn. "Aylık Gelir", "İkramiye").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gelirin hesaba geçmesi beklenen tarih.</summary>
    public DateOnly PlannedDate { get; init; }

    /// <summary>Dönem başında beklenen gelir tutarı.</summary>
    public decimal PlannedAmount { get; init; }
}
