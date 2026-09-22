namespace Mizan.Domain.Models;

/// <summary>
/// Dönem planındaki tekil bir ödeme satırının (<see cref="PeriodPlanPaymentLine"/>) dönem sonundaki fiilî sonucu.
/// Planlanan ödeme ile fiilen yapılan ödeme arasındaki tutar ve tarih farkını belgelemek için vardır.
/// </summary>
public sealed record ActualPayment
{
    /// <summary>Fiilî ödeme kaydının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Bağlı olduğu dönem gerçekleşmesi kaydının kimliği.</summary>
    public Guid PeriodActualId { get; init; }

    /// <summary>Karşılık geldiği dondurulmuş plan ödeme satırının kimliği.</summary>
    public Guid PeriodPlanPaymentLineId { get; init; }

    /// <summary>Ödemenin kaynaklandığı asıl finansal varlık/borç kimliği (kredi, kart vb.).</summary>
    public Guid SourceEntityId { get; init; }

    /// <summary>Ödeme kaynağının türü.</summary>
    public PlanPaymentSourceType SourceType { get; init; }

    /// <summary>Ödeme kaleminin adı.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Planda öngörülen ödeme tarihi.</summary>
    public DateOnly PlannedDate { get; init; }

    /// <summary>Planda öngörülen ödeme tutarı (tahmini veya kesin).</summary>
    public decimal? PlannedAmount { get; init; }

    /// <summary>Ödemenin fiilen yapıldığı tarih (ödenmediyse null).</summary>
    public DateOnly? ActualPaymentDate { get; init; }

    /// <summary>Fiilen ödenen tutar (ödenmediyse 0).</summary>
    public decimal ActualAmount { get; init; }

    /// <summary>Ödemenin fiilî kapanış durumu.</summary>
    public ActualPaymentStatus Status { get; init; }

    /// <summary>Ödemeye dair kullanıcının girdiği isteğe bağlı not.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Ödemenin yapılıp yapılmadığını (tam veya farklı tutarda) belirtir.</summary>
    public bool IsSettled => Status != ActualPaymentStatus.Unpaid;

    /// <summary>Fiilen ödenen tutar ile planlanan tutar arasındaki fark (fiili - planlanan).</summary>
    public decimal Variance => ActualAmount - (PlannedAmount ?? 0m);
}
