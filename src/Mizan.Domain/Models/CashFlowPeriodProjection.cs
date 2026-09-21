namespace Mizan.Domain.Models;

/// <summary>
/// Tek bir nakit akış dönemine ait gelir, zorunlu gider, serbest harcama havuzu,
/// bakiye akışı ve faiz maliyetlerini içeren bütüncül projeksiyon sözleşmesi.
/// Kullanıcının o dönemdeki likidite ve nakit dengesini temsil eder.
/// </summary>
public sealed record CashFlowPeriodProjection
{
    /// <summary>Nakit akış döneminin yarı açık zaman aralığı [Start, End).</summary>
    public CashFlowPeriod Period { get; init; } = default!;

    /// <summary>Dönem başlangıç tarihi (döneme dahil).</summary>
    public DateOnly PeriodStart => Period.Start;

    /// <summary>Dönem bitiş tarihi (sonraki dönemin başlangıcı, döneme dahil değil).</summary>
    public DateOnly PeriodEnd => Period.End;

    /// <summary>Dönemdeki düzenli gelir akışlarının toplamı.</summary>
    public decimal RecurringIncomeTotal { get; init; }

    /// <summary>Dönemdeki tek seferlik arızi gelirlerin toplamı.</summary>
    public decimal AdHocIncomeTotal { get; init; }

    /// <summary>Dönemdeki tüm gelirlerin genel toplamı.</summary>
    public decimal TotalIncome { get; init; }

    /// <summary>Dönemdeki toplam kredi taksiti ve erken ödeme tutarı.</summary>
    public decimal LoanPayments { get; init; }

    /// <summary>Dönemdeki toplam kredi kartı ekstre ödemeleri tutarı.</summary>
    public decimal CreditCardPayments { get; init; }

    /// <summary>Dönemdeki geçici borç ödemeleri toplamı.</summary>
    public decimal TemporaryPayments { get; init; }

    /// <summary>Dönemdeki taksitli harcama/borç ödemeleri toplamı.</summary>
    public decimal InstallmentPayments { get; init; }

    /// <summary>Dönemdeki diğer periyodik planlı ödemeler toplamı.</summary>
    public decimal OtherScheduledPayments { get; init; }

    /// <summary>Dönemdeki tüm zorunlu borç yükümlülüklerinin genel toplamı.</summary>
    public decimal MandatoryOutflow { get; init; }

    /// <summary>Zorunlu ödemeler düşüldükten sonra kalan serbest nakit (TotalIncome - MandatoryOutflow).</summary>
    public decimal AvailableAfterMandatory { get; init; }

    /// <summary>Dönem için ayrılan değişken yaşam gideri havuzu.</summary>
    public decimal VariableExpenseAllowance { get; init; }

    /// <summary>Yaşam gideri ve büyük harcamalar düşüldükten sonra kalan tahmini dönem fazlası/açığı.</summary>
    public decimal EstimatedSurplus { get; init; }

    /// <summary>Bu dönemde planlanan tek seferlik büyük nakit harcamaların toplamı.</summary>
    public decimal PlannedLargeCashExpenses { get; init; }

    /// <summary>Dönemin ilk günündeki nakit açılış bakiyesi.</summary>
    public decimal OpeningBalance { get; init; }

    /// <summary>Finansman açığı (KMH) faizi işletilmeden önceki ara kapanış bakiyesi.</summary>
    public decimal EndingBalanceBeforeDeficitInterest { get; init; }

    /// <summary>Kapanış bakiyesi negatif olduğunda işletilen finansman açığı faiz tutarı.</summary>
    public decimal DeficitFinancingInterest { get; init; }

    /// <summary>Dönemin son anındaki nihai kapanış bakiyesi; sonraki dönemin açılışına eşittir.</summary>
    public decimal EndingBalance { get; init; }

    /// <summary>Kredi kartı ekstrelerinde devreden borç faizi (carry faizi) toplamı.</summary>
    public decimal CardInterestGenerated { get; init; }

    /// <summary>Bu dönemde negatif bakiye oluşması durumunda uygulanan finansman açığı faiz oranı.</summary>
    public decimal AppliedDeficitInterestRate { get; init; }

    /// <summary>Projeksiyon başlangıç çapa referans tarihi.</summary>
    public DateOnly ProjectionAnchorDate { get; init; }

    /// <summary>Kart ödemelerinden en az birinin yedek kural (fallback) ile tahmin edilip edilmediği.</summary>
    public bool IsEstimatedCardPayment { get; init; }

    /// <summary>Kart ödemelerinden en az birinin belirsiz (undetermined) durumda olup olmadığı.</summary>
    public bool HasUndeterminedCardPayment { get; init; }

    /// <summary>Dönemin herhangi bir aşamasında nakit açığı (negatif bakiye) oluşup oluşmadığı.</summary>
    public bool HasDeficit { get; init; }

    /// <summary>Döneme ait tekil gelir kalemleri dökümü.</summary>
    public IReadOnlyList<IncomeProjectionItem> IncomeItems { get; init; } = [];

    /// <summary>Döneme ait tekil zorunlu yükümlülük kalemleri dökümü.</summary>
    public IReadOnlyList<ObligationItem> MandatoryItems { get; init; } = [];

    /// <summary>Döneme ait planlanan büyük harcama kalemleri dökümü.</summary>
    public IReadOnlyList<PlannedLargeExpense> LargeExpenseItems { get; init; } = [];

    /// <summary>Döneme ait kredi kartı ekstre projeksiyon durumları dökümü.</summary>
    public IReadOnlyList<CreditCardPaymentProjectionStatus> CardPaymentStatuses { get; init; } = [];

    /// <summary>Önceki dönemlerden devreden nakit açığı tutarı.</summary>
    public decimal CarryOverDeficit => OpeningBalance < 0m ? Math.Abs(OpeningBalance) : 0m;

    /// <summary>Zorunlu ödemeler ve önceki dönem açığı düşüldükten sonra kalan net tutar.</summary>
    public decimal AvailableAfterCarryOverDeficit => AvailableAfterMandatory - CarryOverDeficit;

    /// <summary>Dönemin kendi içindeki net nakit katkısı (tahmini fazla/açık).</summary>
    public decimal CurrentPeriodNetContribution => EstimatedSurplus;

    /// <summary>Finansman açığı faizine tabi olan anapara açık tutarı.</summary>
    public decimal DeficitPrincipal => EndingBalanceBeforeDeficitInterest < 0m
        ? Math.Abs(EndingBalanceBeforeDeficitInterest)
        : 0m;

    /// <summary>Bu dönemde üretilen toplam faiz maliyeti (Kart faizi + Açık faizi).</summary>
    public decimal TotalInterestGenerated => CardInterestGenerated + DeficitFinancingInterest;

    /// <summary>Bu dönemin net fazlasıyla kapatılan devreden açık tutarı.</summary>
    public decimal DeficitCoveredThisPeriod => CarryOverDeficit == 0m
        ? 0m
        : Math.Min(CarryOverDeficit, Math.Max(0m, CurrentPeriodNetContribution));

    /// <summary>Dönem sonunda henüz kapatılamamış ve sonraki döneme devredecek olan açık tutarı.</summary>
    public decimal RemainingCarryOverDeficit => EndingBalance < 0m ? Math.Abs(EndingBalance) : 0m;

    /// <summary>Döneme devreden bir nakit açığıyla başlanıp başlanmadığı.</summary>
    public bool HasCarryOverDeficit => CarryOverDeficit > 0m;

    /// <summary>Döneme devreden açıkla başlanıp dönem sonunda pozitife geçilerek açığın tamamen kapatılıp kapatılmadığı.</summary>
    public bool RecoveredCarryOverDeficit => HasCarryOverDeficit && EndingBalance >= 0m;
}
