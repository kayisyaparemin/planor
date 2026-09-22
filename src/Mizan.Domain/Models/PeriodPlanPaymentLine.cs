namespace Mizan.Domain.Models;

/// <summary>
/// Dondurulmuş bir dönem planına (PeriodPlanSnapshot) ait tekil bir ödeme taahhüdü satırı.
/// Dönem başında vadesi ve beklenen tutarı dondurulan her borç/harcama bu modelle temsil edilir;
/// dönem kapanışında fiili gerçekleşme (ActualPayment) bu satırlara bağlanarak mutabakat sağlanır.
/// </summary>
public sealed record PeriodPlanPaymentLine
{
    /// <summary>Ödeme satırının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Satırın ait olduğu dondurulmuş dönem planının kimliği.</summary>
    public Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Ödemenin kaynaklandığı asıl varlığın (kredi, kredi kartı, borç planı vb.) kimliği.</summary>
    public Guid SourceEntityId { get; init; }

    /// <summary>Ödemenin kaynaklandığı finansal enstrüman türü.</summary>
    public PlanPaymentSourceType SourceType { get; init; }

    /// <summary>Ödemenin adı veya kurum/sözleşme açıklaması (örn. "Konut Kredisi", "Garanti Bonus").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Ödemenin yapılması planlanan kesin vade tarihi.</summary>
    public DateOnly PlannedDate { get; init; }

    /// <summary>Dönem başında taahhüt edilen veya beklenen ödeme tutarı.</summary>
    public decimal? PlannedAmount { get; init; }

    /// <summary>Tutarın projeksiyondan gelen bir tahmin mi yoksa kesin sözleşme tutarı mı olduğu.</summary>
    public bool IsEstimate { get; init; }

    /// <summary>Ödeme kalemine ilişkin ek detay veya not.</summary>
    public string Detail { get; init; } = string.Empty;
}
