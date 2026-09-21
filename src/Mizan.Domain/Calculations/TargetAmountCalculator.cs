using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// 12 dönemlik nakit akış projeksiyonu üzerinde kullanıcının hedef birikim tutarına
/// hangi dönemde ulaştığını veya başlangıçta zaten ulaşıp ulaşmadığını hesaplar.
/// Simülasyon motorunun ve finansal plan hedef takibinin deterministik karar vermesi için vardır.
/// </summary>
public sealed class TargetAmountCalculator
{
    /// <summary>
    /// Projeksiyon dönemleri içinde dönem sonu bakiyesi hedef tutarı ilk kez yakalayan veya aşan dönemi döner.
    /// </summary>
    /// <param name="projections">Nakit akış dönem projeksiyonları koleksiyonu.</param>
    /// <param name="targetAmount">Ulaşılması hedeflenen pozitif parasal tutar.</param>
    /// <returns>Hedefe ilk ulaşılan dönem veya 12 dönem boyunca ulaşılamadıysa null.</returns>
    public CashFlowPeriodProjection? FindFirstReached(
        IEnumerable<CashFlowPeriodProjection> projections,
        decimal targetAmount)
    {
        ArgumentNullException.ThrowIfNull(projections);
        ValidateTarget(targetAmount);

        return projections
            .OrderBy(x => x.PeriodStart)
            .FirstOrDefault(x => x.EndingBalance >= targetAmount);
    }

    /// <summary>
    /// Hedef tutara başlangıç açılış bakiyesiyle mi yoksa ileriki bir dönemin kapanışında mı
    /// ulaşıldığını detaylı olarak çözümler.
    /// </summary>
    /// <param name="projections">Nakit akış dönem projeksiyonları koleksiyonu.</param>
    /// <param name="targetAmount">Ulaşılması hedeflenen pozitif parasal tutar.</param>
    /// <returns>Ulaşılabilirlik durumu ve ulaşılan ilk dönemi içeren sonuç sözleşmesi.</returns>
    public TargetReachabilityResult FindFirstReachable(
        IEnumerable<CashFlowPeriodProjection> projections,
        decimal targetAmount)
    {
        ArgumentNullException.ThrowIfNull(projections);
        ValidateTarget(targetAmount);

        var ordered = projections
            .OrderBy(x => x.PeriodStart)
            .ToArray();

        if (ordered.Length == 0)
        {
            return new TargetReachabilityResult(false, null);
        }

        if (ordered[0].OpeningBalance >= targetAmount)
        {
            return new TargetReachabilityResult(true, null);
        }

        var firstReached = ordered.FirstOrDefault(x => x.EndingBalance >= targetAmount);
        return new TargetReachabilityResult(false, firstReached);
    }

    private static void ValidateTarget(decimal targetAmount)
    {
        if (targetAmount <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetAmount),
                "Hedef tutar 0'dan büyük olmalıdır.");
        }
    }
}
