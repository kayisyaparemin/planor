using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Nakit akış döneminin kapanış mutabakatını (settlement) orkestre eden uygulama servisidir.
/// </summary>
public sealed class PeriodSettlementService(
    IPeriodHistoryRepository periodHistoryRepository,
    IClock clock,
    FinancialSnapshotService snapshotService,
    FinancialInstrumentReconciliationService instrumentService,
    PlanActualComparisonCalculator comparisonCalculator)
{
    private readonly IPeriodHistoryRepository _periodHistoryRepository = periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly FinancialSnapshotService _snapshotService = snapshotService ?? throw new ArgumentNullException(nameof(snapshotService));
    private readonly FinancialInstrumentReconciliationService _instrumentService = instrumentService ?? throw new ArgumentNullException(nameof(instrumentService));
    private readonly PlanActualComparisonCalculator _comparisonCalculator = comparisonCalculator ?? throw new ArgumentNullException(nameof(comparisonCalculator));

    /// <summary>Yürürlükteki nakit akış döneminin kapatılmaya hazır olup olmadığını sorgular.</summary>
    public async Task<PeriodSettlementAvailability> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var current = history.FindLatestCurrentSnapshot();
        if (current is null)
        {
            return new PeriodSettlementAvailability { HasCurrentSnapshot = false, IsDue = false };
        }

        var openPlan = history.FindOpenPlan();
        var due = openPlan is not null && _clock.Today >= openPlan.SettlementAvailableFrom;

        return new PeriodSettlementAvailability
        {
            HasCurrentSnapshot = true,
            IsDue = due,
            CurrentSnapshot = current,
            PendingPlan = due ? openPlan : null
        };
    }

    /// <summary>Dönem kapanışı ekranı ve sihirbazı için bağlam nesnesini hazırlar.</summary>
    public Task<PeriodSettlementContext> GetContextAsync(Guid? planId = null, CancellationToken cancellationToken = default) =>
        GetContextAsync(null, planId, cancellationToken);

    /// <summary>Dönem kapanışı ekranı ve sihirbazı için bağlam nesnesini güncel finansal planla hazırlar.</summary>
    public async Task<PeriodSettlementContext> GetContextAsync(FinancialPlan? financialPlan, Guid? planId = null, CancellationToken cancellationToken = default)
    {
        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var current = history.FindLatestCurrentSnapshot() ?? throw new InvalidOperationException("Önce güncel finansal durumunu kaydetmelisin.");
        var plan = planId is null ? history.FindOpenPlan() : history.Plans.SingleOrDefault(x => x.Id == planId.Value);
        if (plan is null)
        {
            throw new InvalidOperationException("Güncellenecek dönem planı bulunamadı.");
        }

        var source = history.Snapshots.Single(x => x.Id == plan.FinancialSnapshotId);
        var revisions = history.FindFinalRevisions(plan);
        var revision = revisions.Count > 0 ? revisions[^1] : null;
        var actual = history.Actuals.SingleOrDefault(x => x.PeriodPlanSnapshotId == plan.Id);
        var comparison = actual is null ? null : _comparisonCalculator.Calculate(plan, revision, actual);
        var paymentLines = FinalPaymentLines(plan, revision);
        var plannedIncome = revision?.PlannedIncome ?? plan.PlannedIncome;
        var plannedLiving = revision?.PlannedVariableExpenseAllowance ?? plan.PlannedVariableExpenseAllowance;
        var plannedInterest = revision?.PlannedDeficitInterest ?? plan.PlannedDeficitInterest;
        var cardPayments = financialPlan is not null
            ? _instrumentService.ResolveCurrentCardPayments(financialPlan, new CashFlowPeriod(plan.PeriodStart, plan.PeriodEnd))
            : null;

        var suggestedStartingBalance = source.ProjectionOpeningBalance + plannedIncome
            - paymentLines.Sum(x => ProjectedPaymentAmount.Of(x, cardPayments ?? new Dictionary<Guid, decimal>()))
            - plannedLiving - plannedInterest;

        return new PeriodSettlementContext
        {
            Snapshot = source,
            OriginalPlan = plan,
            Revision = revision,
            RevisionCount = revisions.Count,
            Actual = actual,
            SuggestedStartingBalance = suggestedStartingBalance,
            Comparison = comparison
        };
    }

    /// <summary>Kapanış taslağının onay öncesi önizleme sonuçlarını hesaplar.</summary>
    public Task<PeriodSettlementPreview> PreviewAsync(PeriodSettlementDraft draft, CancellationToken cancellationToken = default) =>
        PreviewAsync(draft, null, cancellationToken);

    /// <summary>Kapanış taslağının onay öncesi önizleme sonuçlarını güncel finansal planla hesaplar.</summary>
    public async Task<PeriodSettlementPreview> PreviewAsync(PeriodSettlementDraft draft, FinancialPlan? financialPlan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var plan = history.Plans.SingleOrDefault(x => x.Id == draft.PeriodPlanSnapshotId) ?? throw new InvalidOperationException("Dönem planı bulunamadı.");
        var snapshot = history.Snapshots.Single(x => x.Id == plan.FinancialSnapshotId);
        var revisions = history.FindFinalRevisions(plan);
        var revision = revisions.Count > 0 ? revisions[^1] : null;
        var cardPayments = financialPlan is not null
            ? _instrumentService.ResolveCurrentCardPayments(financialPlan, new CashFlowPeriod(plan.PeriodStart, plan.PeriodEnd))
            : null;
        var actual = PeriodActualBuilder.Build(plan, snapshot, revision, FinalPaymentLines(plan, revision), draft, _clock.UtcNow, Guid.Empty, false, cardPayments);

        return new PeriodSettlementPreview
        {
            DerivedEndingBalance = actual.DerivedEndingBalance,
            ConfirmedEndingBalance = actual.ConfirmedEndingBalance,
            ReconciliationAdjustment = actual.ReconciliationAdjustment,
            Comparison = _comparisonCalculator.Calculate(plan, revision, actual),
            TotalPaymentsCount = actual.Payments.Count,
            PaidPaymentsCount = actual.Payments.Count(p => p.Status is ActualPaymentStatus.Paid or ActualPaymentStatus.DifferentAmount)
        };
    }

    /// <summary>Dönem mutabakatını kesinleştirir ve tüm paketi atomik olarak kalıcılaştırır.</summary>
    public async Task<PeriodSettlementResult> FinalizeAsync(FinancialPlan financialPlan, PeriodSettlementDraft draft, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(financialPlan);
        ArgumentNullException.ThrowIfNull(draft);

        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var current = history.FindLatestCurrentSnapshot() ?? throw new InvalidOperationException("Güncel finansal durum bulunamadı.");
        var plan = history.Plans.SingleOrDefault(x => x.Id == draft.PeriodPlanSnapshotId) ?? throw new InvalidOperationException("Dönem planı bulunamadı.");

        ValidateEligibility(history, current, plan);

        var revisions = history.FindFinalRevisions(plan);
        var revision = revisions.Count > 0 ? revisions[^1] : null;
        var lines = FinalPaymentLines(plan, revision);
        var cardPayments = _instrumentService.ResolveCurrentCardPayments(financialPlan, new CashFlowPeriod(plan.PeriodStart, plan.PeriodEnd));
        var provisional = PeriodActualBuilder.Build(plan, current, revision, lines, draft, _clock.UtcNow, Guid.Empty, true, cardPayments);

        var (bundle, instruments) = ReconcileAndBuildBundle(financialPlan, lines, provisional, plan.SettlementAvailableFrom, current.Id);
        var actual = provisional with { ResultFinancialSnapshotId = bundle.Snapshot.Id };
        var comparison = _comparisonCalculator.Calculate(plan, revision, actual);
        actual = actual with { ComparisonSummary = comparison.Summary };

        await _periodHistoryRepository.CommitPeriodSettlementAsync(CreateCommit(revision, actual, bundle, instruments), cancellationToken);

        return new PeriodSettlementResult { NewSnapshot = bundle.Snapshot, Actual = actual, Comparison = comparison, NewPlan = bundle.Plan };
    }

    private void ValidateEligibility(FinancialHistoryData history, FinancialSnapshot current, PeriodPlanSnapshot plan)
    {
        if (plan.FinancialSnapshotId != current.Id)
        {
            throw new InvalidOperationException("Yalnızca güncel plan dönemi kapatılabilir.");
        }
        if (history.Actuals.Any(x => x.PeriodPlanSnapshotId == plan.Id))
        {
            throw new InvalidOperationException("Bu dönem daha önce kaydedildi.");
        }
        if (_clock.Today < plan.SettlementAvailableFrom)
        {
            throw new InvalidOperationException("Bu dönem henüz güncellenmeye hazır değil.");
        }
    }

    private (FinancialSnapshotBundle Bundle, ReconciledFinancialInstruments Instruments) ReconcileAndBuildBundle(
        FinancialPlan financialPlan, IReadOnlyList<PeriodPlanPaymentLine> lines, PeriodActual provisional, DateOnly anchor, Guid currentSnapshotId)
    {
        var instruments = _instrumentService.Apply(financialPlan, lines, provisional.Payments, anchor);
        var updatedPlan = financialPlan with
        {
            Loans = instruments.Loans,
            PaymentPlans = instruments.PaymentPlans,
            CreditCards = instruments.CreditCards,
            PlannedLargeExpenses = instruments.LargeExpenses,
            LoanPrepayments = financialPlan.LoanPrepayments.Where(x => !instruments.RemovedLoanPrepaymentIds.Contains(x.Id)).ToArray()
        };
        var bundle = _snapshotService.Build(updatedPlan, provisional.ConfirmedEndingBalance, anchor, FinancialSnapshotSource.MonthlyUpdate, "Dönem güncellemesi", currentSnapshotId);
        return (bundle, instruments);
    }

    private static PeriodSettlementCommit CreateCommit(
        PeriodPlanRevision? revision, PeriodActual actual, FinancialSnapshotBundle bundle, ReconciledFinancialInstruments instruments) => new()
    {
        Revision = revision,
        Actual = actual,
        NewSnapshot = bundle.Snapshot,
        NewPlan = bundle.Plan,
        UpdatedSettings = bundle.UpdatedSettings,
        UpdatedLoans = instruments.Loans,
        UpdatedPaymentPlans = instruments.PaymentPlans,
        UpdatedCreditCards = instruments.CreditCards,
        UpdatedLargeExpenses = instruments.LargeExpenses,
        RemovedLoanPrepaymentIds = instruments.RemovedLoanPrepaymentIds
    };

    private static IReadOnlyList<PeriodPlanPaymentLine> FinalPaymentLines(PeriodPlanSnapshot plan, PeriodPlanRevision? revision) =>
        revision?.PaymentLines.Count > 0 ? revision.PaymentLines : plan.PaymentLines;
}
