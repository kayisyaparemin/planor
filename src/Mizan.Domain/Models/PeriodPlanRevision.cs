namespace Mizan.Domain.Models;

/// <summary>
/// Dönem devam ederken kullanıcının planlama kararlarında (kart ödeme tercihi,
/// yeni borç planı vb.) yaptığı kasıtlı revizyonları kaydeden tarihçe sözleşmesi.
/// Orijinal dondurulmuş planın üzerine yazılmasını engelleyip planlama değişikliklerinin
/// sırasını ve gerekçesini append-only olarak takip etmek için vardır.
/// </summary>
public sealed record PeriodPlanRevision
{
    /// <summary>Plan revizyonunun tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Revize edilen dondurulmuş dönem planının kimliği.</summary>
    public Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Revizyon sıra numarası (1, 2, 3...).</summary>
    public int RevisionNumber { get; init; }

    /// <summary>Revizyonun oluşturulduğu UTC zaman damgası.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Revizyonu tetikleyen planlama değişikliğinin nedeni veya açıklaması.</summary>
    public string Trigger { get; init; } = string.Empty;

    /// <summary>Revize edilmiş toplam beklenen gelir tutarı.</summary>
    public decimal PlannedIncome { get; init; }

    /// <summary>Revize edilmiş toplam kredi ödemeleri.</summary>
    public decimal PlannedLoanPayments { get; init; }

    /// <summary>Revize edilmiş toplam kredi kartı ödemeleri.</summary>
    public decimal PlannedCardPayments { get; init; }

    /// <summary>Revize edilmiş toplam geçici borç ödemeleri.</summary>
    public decimal PlannedTemporaryPayments { get; init; }

    /// <summary>Revize edilmiş toplam taksitli borç ödemeleri.</summary>
    public decimal PlannedInstallmentPayments { get; init; }

    /// <summary>Revize edilmiş diğer planlı takvimli ödemeler.</summary>
    public decimal PlannedOtherScheduledPayments { get; init; }

    /// <summary>Revize edilmiş toplam zorunlu borç ödemeleri.</summary>
    public decimal PlannedMandatoryPayments { get; init; }

    /// <summary>Revize edilmiş dönemlik yaşam gideri serbest havuzu.</summary>
    public decimal PlannedVariableExpenseAllowance { get; init; }

    /// <summary>Revize edilmiş planlı büyük harcamalar toplamı.</summary>
    public decimal PlannedLargeExpenses { get; init; }

    /// <summary>Revize edilmiş kredi kartı devreden borç faizi beklentisi.</summary>
    public decimal PlannedCardInterest { get; init; }

    /// <summary>Revize edilmiş negatif bakiye finansman faizi beklentisi.</summary>
    public decimal PlannedDeficitInterest { get; init; }

    /// <summary>Revize edilmiş toplam faiz yükü (kart carry faizi + finansman açığı faizi).</summary>
    public decimal PlannedInterest => PlannedCardInterest + PlannedDeficitInterest;

    /// <summary>Revize edilmiş hedeflenen dönem kapanış bakiyesi.</summary>
    public decimal PlannedEndingBalance { get; init; }

    /// <summary>Revizyonla ilgili ek açıklama veya not.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Revize edilmiş tekil ödeme taahhüdü satırları.</summary>
    public IReadOnlyList<PeriodPlanPaymentLine> PaymentLines { get; init; } = [];

    /// <summary>Revize edilen planın negatif kapanış bakiyesiyle (finansman açığıyla) sonuçlanıp sonuçlanmadığını belirtir.</summary>
    public bool HasDeficit => PlannedEndingBalance < 0m;
}
