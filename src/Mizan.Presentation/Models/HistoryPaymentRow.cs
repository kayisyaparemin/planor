namespace Mizan.Presentation.Models;

/// <summary>
/// Geçmiş dönem ayrıntısındaki bir gerçekleşen ödemenin satır verisi (EK-V12 S5).
/// </summary>
public sealed record HistoryPaymentRow
{
    /// <summary>Ödemenin adı.</summary>
    public required string Name { get; init; }

    /// <summary>Planlanan ödeme tarihi.</summary>
    public required DateOnly PlannedDate { get; init; }

    /// <summary>Planlanan tutar; belirsizse null.</summary>
    public decimal? PlannedAmount { get; init; }

    /// <summary>Fiilen gerçekleşen / ödenen tutar.</summary>
    public required decimal ActualAmount { get; init; }

    /// <summary>Ödeme durum metni (Ödendi, Farklı tutar, Ödenmedi).</summary>
    public required string StatusText { get; init; }

    /// <summary>Ödeme durumunun semantik renk adı (Positive, Warning, Negative).</summary>
    public required string StatusSemantic { get; init; }
}
