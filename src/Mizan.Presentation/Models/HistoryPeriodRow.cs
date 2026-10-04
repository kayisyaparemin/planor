namespace Mizan.Presentation.Models;

/// <summary>
/// Geçmiş listesindeki bir dönemin satır verisi (EK-V12 S2).
/// </summary>
public sealed record HistoryPeriodRow
{
    /// <summary>Gerçekleşme kaydının kimliği.</summary>
    public required Guid ActualId { get; init; }

    /// <summary>Dönem başlangıç tarihi.</summary>
    public required DateOnly PeriodStart { get; init; }

    /// <summary>Dönemin son takvim günü (PeriodEnd − 1 gün).</summary>
    public required DateOnly PeriodLastDay { get; init; }

    /// <summary>Dönem sonu gerçekleşen teyitli bakiye.</summary>
    public required decimal ActualEndingBalance { get; init; }

    /// <summary>Dönem sonu planlanan bakiye.</summary>
    public required decimal PlannedEndingBalance { get; init; }

    /// <summary>Gerçekleşen ile planlanan arasındaki fark (Gerçekleşen − Planlanan).</summary>
    public required decimal Difference { get; init; }

    /// <summary>Farkın pozitif olup olmadığı.</summary>
    public bool IsDifferencePositive => Difference > 0m;

    /// <summary>Farkın negatif olup olmadığı.</summary>
    public bool IsDifferenceNegative => Difference < 0m;

    /// <summary>Dönem başarı durum rozeti metni (Planın üzerinde / altında / Planlandığı gibi).</summary>
    public required string StatusText { get; init; }

    /// <summary>Durumun semantik renk adı (Positive, Negative, Default).</summary>
    public required string StatusSemantic { get; init; }
}
