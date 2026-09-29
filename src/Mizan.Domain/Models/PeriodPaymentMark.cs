namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının açık dönemde bir plan ödeme satırına koyduğu "ödedim / şu tutarda ödedim / ödemedim"
/// işareti. Vadesi gelmeden yapılan ya da vadesinden farklı ödenen bir ödemeyi dönem kapanışına kadar
/// hatırlamak için vardır. Bakiye gözleminden bağımsızdır ve dönemin dondurulan planına bağlanır:
/// işaret koymak hiçbir gözlemin gününü, bakiyesini ya da kayıt zamanını değiştirmez (S68-8).
/// Eskiden gözlemin çocuğuydu; bu yüzden 14 Eylül'de girilen bakiyeye 12'sindeki ödemeyi işaretlemek
/// gözlemi 12'sine çekiyordu.
/// </summary>
public sealed record PeriodPaymentMark
{
    /// <summary>İşaretin tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>İşaretin ait olduğu dondurulmuş açık dönem planının kimliği.</summary>
    public Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>İşaretin karşılık geldiği plan ödeme satırının kimliği; satır başına en fazla bir işaret vardır.</summary>
    public Guid PeriodPlanPaymentLineId { get; init; }

    /// <summary>Ödemenin işaretlenen durumu (ödendi, farklı tutar, ödenmedi).</summary>
    public ActualPaymentStatus Status { get; init; }

    /// <summary>Fiilen ödendiği belirtilen tutar (ödenmediyse 0).</summary>
    public decimal ActualAmount { get; init; }

    /// <summary>Ödemenin fiilen yapıldığı tarih (ödenmediyse null).</summary>
    public DateOnly? ActualPaymentDate { get; init; }

    /// <summary>İşarete dair kullanıcının girdiği isteğe bağlı not.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Ödemenin yapılmış sayılıp sayılmadığını belirtir (tam veya farklı tutarda).</summary>
    public bool IsSettled => Status != ActualPaymentStatus.Unpaid;

    /// <summary>Fiilî tutar ile planlanan tutar arasındaki farkı hesaplar (fiili - planlanan).</summary>
    public decimal CalculateVariance(decimal? plannedAmount) => ActualAmount - (plannedAmount ?? 0m);
}
