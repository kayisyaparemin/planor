namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir nakit akış dönemine ait dondurulmuş plan taahhüdü sözleşmesi.
/// Dönem başında beklenen gelirleri, zorunlu borç çıkışlarını, serbest yaşam gideri
/// havuzunu ve hedeflenen kapanış bakiyesini kilit altına alarak dönem içi sapmaların
/// dürüstçe ölçülebilmesini sağlamak için vardır.
/// </summary>
public sealed record PeriodPlanSnapshot
{
    /// <summary>Dönem planı taahhüdünün tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Bu planın bağlı olduğu üst finansal durumun kimliği.</summary>
    public Guid FinancialSnapshotId { get; init; }

    /// <summary>Dönemin başlangıç tarihi (dahil).</summary>
    public DateOnly PeriodStart { get; init; }

    /// <summary>Dönemin bitiş tarihi (hariç).</summary>
    public DateOnly PeriodEnd { get; init; }

    /// <summary>Dönemin kapanış mutabakatına açılabileceği ilk takvim tarihi.</summary>
    public DateOnly SettlementAvailableFrom { get; init; }

    /// <summary>Planın dondurulduğu UTC zaman damgası.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Dönem başında mevcut olan açılış nakit bakiyesi.</summary>
    public decimal OpeningBalance { get; init; }

    /// <summary>Dönem içinde beklenen toplam gelir tutarı.</summary>
    public decimal PlannedIncome { get; init; }

    /// <summary>Planlanan toplam banka kredisi taksit çıkışları.</summary>
    public decimal PlannedLoanPayments { get; init; }

    /// <summary>Planlanan toplam kredi kartı ödemeleri.</summary>
    public decimal PlannedCardPayments { get; init; }

    /// <summary>Planlanan toplam geçici borç ödemeleri.</summary>
    public decimal PlannedTemporaryPayments { get; init; }

    /// <summary>Planlanan toplam taksitli borç ödemeleri.</summary>
    public decimal PlannedInstallmentPayments { get; init; }

    /// <summary>Planlanan diğer periyodik/takvimli ödemeler.</summary>
    public decimal PlannedOtherScheduledPayments { get; init; }

    /// <summary>Planlanan tüm zorunlu borç yükümlülüklerinin toplamı.</summary>
    public decimal PlannedMandatoryPayments { get; init; }

    /// <summary>Dönem için ayrılan serbest harcama ve yaşam gideri havuzu.</summary>
    public decimal PlannedVariableExpenseAllowance { get; init; }

    /// <summary>Dönem içinde planlanan tek seferlik büyük harcamaların toplamı.</summary>
    public decimal PlannedLargeExpenses { get; init; }

    /// <summary>Kredi kartı devreden borç faizi (carry faizi) beklentisi.</summary>
    public decimal PlannedCardInterest { get; init; }

    /// <summary>Dönem içi negatif bakiye (KMH) finansman faizi beklentisi.</summary>
    public decimal PlannedDeficitInterest { get; init; }

    /// <summary>Planlanan toplam faiz yükü (kart carry faizi + finansman açığı faizi).</summary>
    public decimal PlannedInterest => PlannedCardInterest + PlannedDeficitInterest;

    /// <summary>Dönem sonunda ulaşılması hedeflenen kapanış nakit bakiyesi.</summary>
    public decimal PlannedEndingBalance { get; init; }

    /// <summary>Dönem planına dahil edilen tekil ödeme taahhüdü satırları.</summary>
    public IReadOnlyList<PeriodPlanPaymentLine> PaymentLines { get; init; } = [];

    /// <summary>Dönem boyunca hedeflenen net nakit değişimi (kapanış bakiyesi - açılış bakiyesi).</summary>
    public decimal PlannedNetChange => PlannedEndingBalance - OpeningBalance;

    /// <summary>Dönemin planlanan tüm nakit çıkışlarının (zorunlu + yaşam havuzu + büyük harcamalar + faizler) toplamı.</summary>
    public decimal TotalPlannedOutflows => PlannedMandatoryPayments +
                                           PlannedVariableExpenseAllowance +
                                           PlannedLargeExpenses +
                                           PlannedInterest;

    /// <summary>Dönemin negatif kapanış bakiyesiyle (finansman açığıyla) kapanıp kapanmadığını belirtir.</summary>
    public bool HasDeficit => PlannedEndingBalance < 0m;

    /// <summary>Belirtilen tarihin bu dönemin yarı açık aralığına [PeriodStart, PeriodEnd) düşüp düşmediğini denetler.</summary>
    public bool ContainsDate(DateOnly date) => date >= PeriodStart && date < PeriodEnd;
}
