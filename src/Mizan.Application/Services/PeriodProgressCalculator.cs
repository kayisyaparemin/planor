using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık dönemin gidişatını hesaplar: kullanıcının girdiği tek bakiyeden yaşam havuzundan ne harcandığını,
/// ne kaldığını ve bu gidişatla dönemin nerede kapanacağını çıkarır. Eskide bu hesap 214 satırlık tek bir
/// metottu ve KMH kuralını projeksiyon motorundan elle kopyalamıştı (T6); artık ödeme satırının durumu
/// <see cref="PeriodPaymentLineClassifier"/>'da, KMH kuralı <c>DeficitFinancingRules</c>'ta durur.
/// Gelir dondurulan plandaki tarihiyle ayrılır: gözlem günü ve öncesinde yatan bakiyenin içindedir,
/// sonra yatacak olan dönem sonuna eklenir (S31).
/// </summary>
public static class PeriodProgressCalculator
{
    /// <summary>
    /// Açık dönemin defterinden ana sayfanın mevcut dönem verisini üretir. Hiçbir şey yazmaz.
    /// </summary>
    /// <param name="ledger">Açık dönemin planı, revizyonları, gözlemi ve hatırlatıcı cevapları.</param>
    /// <param name="currentCardPayments">Kart kimliğine göre, kartın bugünkü hâliyle bu dönemde vadesi gelen ödeme.</param>
    /// <param name="deficitFinancingInterestRate">Kullanıcının dönemlik KMH faiz oranı (0,05 = %5).</param>
    /// <param name="today">Gidişatın hesaplandığı gün.</param>
    public static PeriodProgress Calculate(
        OpenPeriodLedger ledger,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments,
        decimal deficitFinancingInterestRate,
        DateOnly today)
    {
        var plan = ledger.Plan;
        var latest = ledger.LatestRevision;
        var lines = PeriodPaymentLineClassifier.Classify(ledger, today);
        var remaining = RemainingPayments(lines.RemainingLines, currentCardPayments);
        var allowance = latest?.PlannedVariableExpenseAllowance ?? plan.PlannedVariableExpenseAllowance;
        var trajectory = ProjectTrajectory(ledger, lines, remaining, allowance, deficitFinancingInterestRate);
        var totalDays = Math.Max(0, plan.PeriodEnd.DayNumber - plan.PeriodStart.DayNumber);

        return new PeriodProgress
        {
            PeriodPlanSnapshotId = plan.Id,
            PeriodStart = plan.PeriodStart,
            PeriodEnd = plan.PeriodEnd,
            Today = today,
            ElapsedDays = Math.Clamp(today.DayNumber - plan.PeriodStart.DayNumber, 0, totalDays),
            TotalDays = totalDays,
            RevisionCount = ledger.Revisions.Count,
            PlannedIncome = latest?.PlannedIncome ?? plan.PlannedIncome,
            PlannedMandatoryPayments = latest?.PlannedMandatoryPayments ?? plan.PlannedMandatoryPayments,
            PlannedEndingBalance = latest?.PlannedEndingBalance ?? plan.PlannedEndingBalance,
            PlannedVariableExpenseAllowance = allowance,
            PlannedDeficitInterest = latest?.PlannedDeficitInterest ?? plan.PlannedDeficitInterest,
            ObservedLivingSpend = trajectory?.LivingSpend,
            RemainingVariableExpenseAllowance = trajectory?.RemainingAllowance,
            ProjectedDeficitInterest = trajectory?.DeficitInterest,
            ProjectedEndingBalance = trajectory?.EndingBalance,
            Pace = CalculatePace(ledger.Plan, ledger.LatestObservation, trajectory?.LivingSpend, allowance),
            Cards = CompareCards(ledger.CurrentPaymentLines, currentCardPayments),
            Observation = ledger.LatestObservation,
            Observations = ledger.Observations.OrderBy(x => x.ObservedOn).ToArray(),
            Path = trajectory is null
                ? PeriodBalancePathCalculator.FromPlan(ledger)
                : PeriodBalancePathCalculator.FromObservations(
                    ledger, lines, currentCardPayments, trajectory.RemainingAllowance, trajectory.DeficitInterest),
            RemainingPayments = remaining,
            IsClosable = today >= plan.SettlementAvailableFrom,
            SnoozedLineIds = lines.SnoozedLineIds
        };
    }

    // Kullanıcı fiş girmez (S20): yaşam harcaması, bakiyedeki düşüşten bakiyeye yansımış ödemeler çıkarılarak
    // geri çözülür. Havuz günlere bölünmez; havuz içinde harcamak dönem sonunu değiştirmez, yalnız aşım değiştirir.
    // Bakiye girilmediyse hiçbir gidişat rakamı üretilmez.
    private static Trajectory? ProjectTrajectory(
        OpenPeriodLedger ledger,
        PeriodPaymentLineClassification lines,
        IReadOnlyList<PeriodRemainingPayment> remainingPayments,
        decimal plannedAllowance,
        decimal deficitFinancingInterestRate)
    {
        if (ledger.LatestObservation is not { } observation)
        {
            return null;
        }

        var balance = observation.ObservedBalance;

        var incomeInBalance = IncomeReceivedBy(ledger.CurrentIncomeLines, observation.ObservedOn);
        var incomeStillToCome = ledger.CurrentIncomeLines.Sum(x => x.PlannedAmount) - incomeInBalance;
        var livingSpend = Math.Max(0m, ledger.Plan.OpeningBalance + incomeInBalance - lines.SettledBeforeObservation - balance);
        var remainingAllowance = Math.Max(0m, plannedAllowance - livingSpend);
        var endingBeforeInterest = balance
                                   + incomeStillToCome
                                   - lines.SettledAfterObservation
                                   - remainingPayments.Sum(x => x.Amount)
                                   - remainingAllowance;

        // Kart faizi burada yoktur: karta biner, sonraki ekstreye yansır; nakit dönem sonunu değiştirmez (I11).
        var deficitInterest = DeficitFinancingRules.CalculateInterest(endingBeforeInterest, deficitFinancingInterestRate);
        return new Trajectory(livingSpend, remainingAllowance, deficitInterest, endingBeforeInterest - deficitInterest);
    }

    // Tempo iki oranı da gözlem gününe göre alır (S70): bugüne göre alınırsa bakiye girilmedikçe süre ilerler,
    // harcama donar ve "geride" görünür. Havuz 0 ise harcanan oran tanımsızdır; aşım kırpılmaz.
    private static SpendingPace? CalculatePace(
        PeriodPlanSnapshot plan,
        PeriodObservation? observation,
        decimal? livingSpend,
        decimal allowance)
    {
        var totalDays = plan.PeriodEnd.DayNumber - plan.PeriodStart.DayNumber;
        if (observation is null || livingSpend is null || allowance <= 0m || totalDays <= 0)
        {
            return null;
        }

        var elapsedDays = observation.ObservedOn.DayNumber - plan.PeriodStart.DayNumber;
        return new SpendingPace(observation.ObservedOn, livingSpend.Value / allowance, (decimal)elapsedDays / totalDays);
    }

    // Gözlem günü yatan gelir bakiyenin içinde sayılır: kullanıcı bakiyesine en çok gelir günü bakar ve gelir
    // genelde gece ya da sabah yatar. Yanılırsak hata kötümser tarafta kalır (S31).
    private static decimal IncomeReceivedBy(IReadOnlyList<PeriodPlanIncomeLine> incomeLines, DateOnly observedOn) =>
        incomeLines.Where(x => x.PlannedDate <= observedOn).Sum(x => x.PlannedAmount);

    // Kalan kart satırı dondurulan tahminle değil kartın bugünkü hâliyle gösterilir ve sayılır: dönem içi plansız
    // harcama planı değil, gidişatı değiştirir (I23). Liste ve dönem sonu tahmini aynı tutarları kullanır (I165).
    private static PeriodRemainingPayment[] RemainingPayments(
        IReadOnlyList<PeriodPlanPaymentLine> remainingLines,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments) =>
        remainingLines
            .Select(line => new PeriodRemainingPayment(
                line.Id, line.Name, line.PlannedDate, ProjectedPaymentAmount.Of(line, currentCardPayments)))
            .ToArray();

    private static PeriodCardComparison[] CompareCards(
        IReadOnlyList<PeriodPlanPaymentLine> lines,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments) =>
        lines
            .Where(x => x.SourceType == PlanPaymentSourceType.CreditCard)
            .OrderBy(x => x.PlannedDate)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Select(line => new PeriodCardComparison(
                line.SourceEntityId,
                line.Name,
                line.PlannedDate,
                line.PlannedAmount ?? 0m,
                currentCardPayments.TryGetValue(line.SourceEntityId, out var current) ? current : null))
            .ToArray();

    private sealed record Trajectory(
        decimal LivingSpend,
        decimal RemainingAllowance,
        decimal DeficitInterest,
        decimal EndingBalance);
}
