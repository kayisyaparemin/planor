namespace Mizan.Domain.Models;

/// <summary>
/// Kapanan bir nakit akış dönemine ait kesinleşmiş fiilî durum sözleşmesi.
/// Dönem başında dondurulan plan taahhüdüyle (<see cref="PeriodPlanSnapshot"/>) karşılaştırılarak
/// tüm gerçekleşen gelirleri, borç ödemelerini, yaşam harcamalarını, plansız akışları ve
/// kasa mutabakat farkını dondurmak için vardır.
/// </summary>
public sealed record PeriodActual
{
    /// <summary>Dönem gerçekleşmesi kaydının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Bu gerçekleşmenin kıyaslandığı dondurulmuş dönem planının kimliği.</summary>
    public Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Dönem başında geçerli olan kaynak finansal durum kimliği.</summary>
    public Guid SourceFinancialSnapshotId { get; init; }

    /// <summary>Dönem kapanışıyla üretilen yeni dönem finansal durum kimliği.</summary>
    public Guid ResultFinancialSnapshotId { get; init; }

    /// <summary>Kapanan dönemin başlangıç tarihi (dahil).</summary>
    public DateOnly PeriodStart { get; init; }

    /// <summary>Kapanan dönemin bitiş tarihi (hariç).</summary>
    public DateOnly PeriodEnd { get; init; }

    /// <summary>Dönemin kesinleştirilerek kapatıldığı UTC zaman damgası.</summary>
    public DateTimeOffset FinalizedAtUtc { get; init; }

    /// <summary>Dönem içinde fiilen gerçekleşen toplam gelir (planlı + plansız).</summary>
    public decimal ActualIncome { get; init; }

    /// <summary>Fiilen ödenen banka kredisi taksitleri toplamı.</summary>
    public decimal ActualLoanPayments { get; init; }

    /// <summary>Fiilen ödenen kredi kartı borçları toplamı.</summary>
    public decimal ActualCardPayments { get; init; }

    /// <summary>Fiilen ödenen geçici borçlar toplamı.</summary>
    public decimal ActualTemporaryPayments { get; init; }

    /// <summary>Fiilen ödenen taksitli borçlar toplamı.</summary>
    public decimal ActualInstallmentPayments { get; init; }

    /// <summary>Fiilen ödenen diğer takvimli ödemeler toplamı.</summary>
    public decimal ActualOtherScheduledPayments { get; init; }

    /// <summary>Fiilen yapılan planlı büyük harcamalar toplamı.</summary>
    public decimal ActualLargeExpenses { get; init; }

    /// <summary>Fiilen yapılan tüm zorunlu borç ödemelerinin toplamı.</summary>
    public decimal ActualMandatoryPayments { get; init; }

    /// <summary>Fiilen harcanan serbest yaşam gideri tutarı.</summary>
    public decimal ActualLivingSpend { get; init; }

    /// <summary>Fiilen yansıyan faiz maliyetleri toplamı.</summary>
    public decimal ActualInterest { get; init; }

    /// <summary>Dönem içinde gerçekleşen plansız gelirlerin toplamı.</summary>
    public decimal UnplannedIncome { get; init; }

    /// <summary>Dönem içinde gerçekleşen plansız ödemelerin toplamı.</summary>
    public decimal UnplannedPayments { get; init; }

    /// <summary>Hesap hareketlerinden türetilen matematiksel dönem sonu kapanış bakiyesi.</summary>
    public decimal DerivedEndingBalance { get; init; }

    /// <summary>Kullanıcının cüzdanında/hesabında teyit ettiği fiilî kapanış bakiyesi.</summary>
    public decimal ConfirmedEndingBalance { get; init; }

    /// <summary>Teyit edilen bakiye ile hesaplanan bakiye arasındaki mutabakat düzeltmesi farkı (teyit - türetilen).</summary>
    public decimal ReconciliationAdjustment { get; init; }

    /// <summary>Plan ile fiili durum arasındaki kıyaslamayı özetleyen açıklama metni.</summary>
    public string ComparisonSummary { get; init; } = string.Empty;

    /// <summary>Dönem kapanışına dair kullanıcının girdiği isteğe bağlı not.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Dönem içindeki tekil plan ödemelerinin gerçekleşme satırları.</summary>
    public IReadOnlyList<ActualPayment> Payments { get; init; } = [];

    /// <summary>Dönem içindeki plansız gelir ve ödeme akışları.</summary>
    public IReadOnlyList<ActualFlow> Flows { get; init; } = [];

    /// <summary>Fiilî yaşam giderinin kategori bazlı dökümü.</summary>
    public IReadOnlyList<ActualLivingBreakdown> LivingBreakdown { get; init; } = [];

    /// <summary>Dönem içindeki tüm fiilî nakit çıkışlarının toplamı.</summary>
    public decimal TotalActualOutflows => ActualMandatoryPayments +
                                          ActualLivingSpend +
                                          ActualLargeExpenses +
                                          ActualInterest +
                                          UnplannedPayments;

    /// <summary>Dönem sonunun negatif bir nakit bakiyesiyle (finansman açığıyla) teyit edilip edilmediğini belirtir.</summary>
    public bool HasDeficit => ConfirmedEndingBalance < 0m;

    /// <summary>Belirtilen tarihin kapanan dönemin yarı açık aralığına [PeriodStart, PeriodEnd) düşüp düşmediğini denetler.</summary>
    public bool ContainsDate(DateOnly date) => date >= PeriodStart && date < PeriodEnd;
}
