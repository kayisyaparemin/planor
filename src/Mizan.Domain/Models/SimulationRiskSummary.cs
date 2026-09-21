namespace Mizan.Domain.Models;

/// <summary>
/// 12 dönemlik simülasyon projeksiyonunda ortaya çıkan en kritik likidite risklerini,
/// dip bakiye çukurlarını, finansman açığı sürelerini ve senaryo maliyetlerini özetleyen sözleşme.
/// </summary>
public sealed record SimulationRiskSummary
{
    /// <summary>12 dönem boyunca zorunlu ödemeler sonrası görülen en düşük serbest nakit tutarı.</summary>
    public decimal LowestAvailableAfterMandatory { get; init; }

    /// <summary>12 dönem boyunca görülen en düşük dönem net nakit fazlası/açığı tutarı.</summary>
    public decimal LowestSurplus { get; init; }

    /// <summary>12 dönem boyunca görülen en dip dönem kapanış bakiyesi.</summary>
    public decimal LowestProjectedBalance { get; init; }

    /// <summary>En düşük kapanış bakiyesinin (en derin nakit çukurunun) görüldüğü dönem.</summary>
    public CashFlowPeriod LowestPeriod { get; init; } = default!;

    /// <summary>Dönem net katkısının (surplus) ilk kez negatife düştüğü dönem (varsa).</summary>
    public CashFlowPeriod? FirstNegativeSurplusPeriod { get; init; }

    /// <summary>Kapanış bakiyesinin ilk kez negatife düşüp finansman açığı (KMH) ürettiği dönem (varsa).</summary>
    public CashFlowPeriod? FirstNegativeProjectedBalancePeriod { get; init; }

    /// <summary>Senaryo boyunca oluşan en yüksek devreden nakit açığı tutarı.</summary>
    public decimal MaximumCarryOverDeficit { get; init; }

    /// <summary>Oluşan finansman açığının dönem net fazlalarıyla tamamen telafi edilip pozitife geçildiği dönem.</summary>
    public CashFlowPeriod? RecoveryPeriod { get; init; }

    /// <summary>12. dönemin nihai kapanış bakiyesi.</summary>
    public decimal EndingProjectedBalance { get; init; }

    /// <summary>Senaryonun gerektirdiği toplam nakit ve taksitli harcama/ödeme maliyeti.</summary>
    public decimal TotalScenarioCost { get; init; }

    /// <summary>Finansman kredisinde anapara haricinde ödenen toplam faiz ve finansman maliyeti.</summary>
    public decimal? FinancingCost { get; init; }

    /// <summary>İlk finansman açığının (negatif bakiyenin) oluştuğu dönem.</summary>
    public CashFlowPeriod? FirstDeficitPeriod => FirstNegativeProjectedBalancePeriod;
}
