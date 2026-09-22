namespace Mizan.Domain.Models;

/// <summary>
/// Dondurulmuş dönem planında yer alan bir ödeme satırının (<see cref="PeriodPlanPaymentLine"/>)
/// dönem içindeki anlık gözlem sonucu.
/// Vadesi gelmeden önce veya vadesinde kullanıcının yaptığı fiilî ödemeyi dönem kapanışına kadar
/// hafızada tutmak ve dönem sonu mutabakatını önceden doldurmak için vardır.
/// </summary>
public sealed record PeriodObservationPayment
{
    /// <summary>Gözlenen ödeme satırının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Bağlı olduğu dönem gözlem defterinin kimliği.</summary>
    public Guid PeriodObservationId { get; init; }

    /// <summary>Karşılık geldiği dondurulmuş plan ödeme satırının kimliği.</summary>
    public Guid PeriodPlanPaymentLineId { get; init; }

    /// <summary>Ödemenin gözlenen fiilî durumu (ödendi, farklı tutar, ödenmedi).</summary>
    public ActualPaymentStatus Status { get; init; }

    /// <summary>Fiilen ödendiği gözlenen tutar (ödenmediyse 0).</summary>
    public decimal ActualAmount { get; init; }

    /// <summary>Ödemenin fiilen yapıldığı tarih (ödenmediyse null).</summary>
    public DateOnly? ActualPaymentDate { get; init; }

    /// <summary>Ödemeye dair kullanıcının girdiği isteğe bağlı not.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Ödemenin yapılmış sayılıp sayılmadığını belirtir (tam veya farklı tutarda).</summary>
    public bool IsSettled => Status != ActualPaymentStatus.Unpaid;

    /// <summary>Fiilî tutar ile planlanan tutar arasındaki farkı hesaplar (fiili - planlanan).</summary>
    public decimal CalculateVariance(decimal? plannedAmount) => ActualAmount - (plannedAmount ?? 0m);
}
