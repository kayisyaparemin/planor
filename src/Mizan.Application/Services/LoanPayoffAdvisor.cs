using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// "Bu krediyi hangi gün kapatabilirim?" sorusunu cevaplar. Soru bakiyeye bakarak cevaplanamaz:
/// açık faizi kredi faizinden yüksekse krediyi açık parasıyla kapatmak zarardır. Bu yüzden her
/// kredi için ufuktaki her taksit günü denenir (o günün taksiti ödenir, işleyen faiz sıfırdır)
/// ve projeksiyon kapamalı ve kapamasız koşturulur. Bir gün yalnız iki koşul birlikte sağlanırsa
/// önerilir: hiçbir dönemde açık baz çizgiden büyümez ve net kazanç pozitiftir (I26).
/// </summary>
/// <remarks>
/// Net kazanç = 12. dönem sonu bakiye farkı + ufuk sonrasında ödenmeyecek taksitler. İkinci terim
/// şart: uzun bir kredinin kazancının çoğu 12 dönemin dışındadır. Ufuk sonrasındaki açık faizi
/// sayılmaz (bilinen sadeleştirme, bkz. INVARYANTLAR.md → Kasıtlı sadeleştirmeler).
/// </remarks>
public sealed class LoanPayoffAdvisor(
    FinancialProjectionCalculator projectionCalculator,
    ScenarioPlanBuilder scenarioPlanBuilder,
    LoanAmortizationCalculator amortizationCalculator,
    LoanPaymentScheduleBuilder scheduleBuilder)
{
    // Bir kuruşluk yuvarlama farkı açığı büyütmüş sayılmaz.
    private const decimal DeficitTolerance = 0.01m;

    private readonly FinancialProjectionCalculator _projectionCalculator =
        projectionCalculator ?? throw new ArgumentNullException(nameof(projectionCalculator));
    private readonly ScenarioPlanBuilder _scenarioPlanBuilder =
        scenarioPlanBuilder ?? throw new ArgumentNullException(nameof(scenarioPlanBuilder));
    private readonly LoanAmortizationCalculator _amortizationCalculator =
        amortizationCalculator ?? throw new ArgumentNullException(nameof(amortizationCalculator));
    private readonly LoanPaymentScheduleBuilder _scheduleBuilder =
        scheduleBuilder ?? throw new ArgumentNullException(nameof(scheduleBuilder));

    /// <summary>
    /// Plandaki her aktif kredi için erken kapama önerisini üretir. Ufukta denenecek taksit günü
    /// olmayan kredi listeye girmez.
    /// </summary>
    public IReadOnlyList<LoanPayoffAdvice> Advise(
        FinancialPlan plan,
        DateOnly asOf,
        DateOnly? firstPeriodStartDate = null,
        int periodCount = 12,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var baseline = _projectionCalculator.Calculate(plan, asOf, periodCount, firstPeriodStartDate);
        var horizon = new Horizon(plan, baseline, asOf, firstPeriodStartDate, periodCount);

        var advice = new List<LoanPayoffAdvice>();
        foreach (var loan in plan.Loans.Where(x => x.IsActive && x.RemainingInstallmentCount > 0))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (AdviseLoan(horizon, loan, cancellationToken) is { } result)
            {
                advice.Add(result);
            }
        }
        return advice;
    }

    private LoanPayoffAdvice? AdviseLoan(Horizon horizon, Loan loan, CancellationToken cancellationToken)
    {
        var name = $"{loan.Bank} {loan.Name}".Trim();
        var closing = horizon.Plan.LoanPrepayments
            .FirstOrDefault(x => x.LoanId == loan.Id && x.Mode == LoanPrepaymentMode.FullClosure);
        if (closing is not null)
        {
            return new LoanPayoffAdvice { LoanId = loan.Id, LoanName = name, Status = LoanPayoffAdviceStatus.AlreadyClosing, Date = closing.Date };
        }
        if (_amortizationCalculator.Analyze(loan).Amortization is null)
        {
            return new LoanPayoffAdvice { LoanId = loan.Id, LoanName = name, Status = LoanPayoffAdviceStatus.NeedsPrincipal };
        }

        var baselineReplay = _scheduleBuilder.Replay(loan, horizon.Plan.LoanPrepayments);
        var candidates = FindCandidateDates(baselineReplay, horizon);
        if (candidates.Length == 0)
        {
            return null;
        }

        var evaluations = new List<CandidateEvaluation>();
        foreach (var date in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Evaluate(horizon, loan, baselineReplay, date) is { } evaluation)
            {
                evaluations.Add(evaluation);
            }
        }
        return Summarize(loan.Id, name, evaluations);
    }

    private static DateOnly[] FindCandidateDates(LoanReplay replay, Horizon horizon)
    {
        var horizonStart = horizon.Baseline[0].PeriodStart;
        var horizonEnd = horizon.Baseline[^1].PeriodEnd;
        return replay.Payments
            .Where(x => x.Kind == LoanPaymentKind.Installment && !x.IsFinal)
            .Select(x => x.Date)
            .Where(date => date >= horizon.AsOf && date >= horizonStart && date < horizonEnd)
            .Distinct()
            .ToArray();
    }

    private CandidateEvaluation? Evaluate(Horizon horizon, Loan loan, LoanReplay baselineReplay, DateOnly date)
    {
        var request = new SimulationRequest(SimulationScenarioType.LoanEarlyClosure, "Erken kapama önerisi", 0m, date)
        {
            LoanId = loan.Id
        };
        FinancialPlan scenarioPlan;
        try
        {
            scenarioPlan = _scenarioPlanBuilder.Build(horizon.Plan, request);
        }
        catch (InvalidOperationException)
        {
            // Erken ödeme doğrulayıcısının reddettiği gün önerilemez; tek bir günün reddi
            // diğer günleri ve diğer kredilerin önerisini düşürmesin diye atlanır.
            return null;
        }

        var scenario = _projectionCalculator.Calculate(scenarioPlan, horizon.AsOf, horizon.PeriodCount, horizon.FirstPeriodStartDate);
        var scenarioReplay = _scheduleBuilder.Replay(loan, scenarioPlan.LoanPrepayments);
        var loanPaymentIds = scenarioPlan.LoanPrepayments.Where(x => x.LoanId == loan.Id).Select(x => x.Id).Append(loan.Id).ToHashSet();
        var beyondHorizonSaving = (baselineReplay.Total - LoanPaymentsIn(horizon.Baseline, loanPaymentIds)) -
                                  (scenarioReplay.Total - LoanPaymentsIn(scenario, loanPaymentIds));

        return new CandidateEvaluation(
            date,
            scenarioReplay.Payments.Where(x => x.SourceId == request.ScenarioId).Sum(x => x.Amount),
            baselineReplay.Total - scenarioReplay.Total,
            scenario[^1].EndingBalance - horizon.Baseline[^1].EndingBalance + beyondHorizonSaving,
            horizon.Baseline.Zip(scenario).Any(pair => Deficit(pair.Second) > Deficit(pair.First) + DeficitTolerance));
    }

    private static LoanPayoffAdvice Summarize(Guid loanId, string name, IReadOnlyList<CandidateEvaluation> evaluations)
    {
        var gainful = evaluations.Where(x => x.NetGain > 0m).ToArray();
        var safe = gainful.Where(x => !x.AddsDeficit).ToArray();
        if (safe.Length == 0)
        {
            var status = gainful.Length > 0 ? LoanPayoffAdviceStatus.NoSafeMonth : LoanPayoffAdviceStatus.NotWorthIt;
            return new LoanPayoffAdvice { LoanId = loanId, LoanName = name, Status = status };
        }

        var earliest = safe[0];
        var best = safe.MaxBy(x => x.NetGain)!;
        var bestDiffers = best.Date != earliest.Date;
        return new LoanPayoffAdvice
        {
            LoanId = loanId, LoanName = name, Status = LoanPayoffAdviceStatus.Recommended,
            Date = earliest.Date, PayoffAmount = earliest.PayoffAmount,
            InterestSaving = earliest.InterestSaving, NetGain = earliest.NetGain,
            BestDate = bestDiffers ? best.Date : null, BestNetGain = bestDiffers ? best.NetGain : null
        };
    }

    private static decimal Deficit(CashFlowPeriodProjection period) => Math.Max(0m, -period.EndingBalance);

    private static decimal LoanPaymentsIn(IReadOnlyList<CashFlowPeriodProjection> periods, HashSet<Guid> paymentIds) =>
        periods.SelectMany(x => x.MandatoryItems)
            .Where(x => x.Type == ObligationType.Loan && paymentIds.Contains(x.PaymentId))
            .Sum(x => x.Amount);

    private sealed record Horizon(
        FinancialPlan Plan,
        IReadOnlyList<CashFlowPeriodProjection> Baseline,
        DateOnly AsOf,
        DateOnly? FirstPeriodStartDate,
        int PeriodCount);

    private sealed record CandidateEvaluation(
        DateOnly Date,
        decimal PayoffAmount,
        decimal InterestSaving,
        decimal NetGain,
        bool AddsDeficit);
}
