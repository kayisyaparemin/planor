using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kullanıcının finansal planını ve projeksiyona hazır durumunu veri depolarından okuyan uygulama servisidir.
/// Salt okuma yapar, hiçbir koşulda veritabanına veri yazmaz veya yan etki üretmez (Kural M4, Düğüm T5).
/// </summary>
public sealed class PlanReader(
    IUserSettingsRepository userSettingsRepository,
    IncomePlanReader incomePlanReader,
    FinancialInstrumentReader financialInstrumentReader,
    IPeriodHistoryRepository periodHistoryRepository,
    ProjectionBoundaryResolver projectionBoundaryResolver) : IPlanReader
{
    private readonly IUserSettingsRepository _userSettingsRepository =
        userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
    private readonly IncomePlanReader _incomePlanReader =
        incomePlanReader ?? throw new ArgumentNullException(nameof(incomePlanReader));
    private readonly FinancialInstrumentReader _financialInstrumentReader =
        financialInstrumentReader ?? throw new ArgumentNullException(nameof(financialInstrumentReader));
    private readonly IPeriodHistoryRepository _periodHistoryRepository =
        periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
    private readonly ProjectionBoundaryResolver _projectionBoundaryResolver =
        projectionBoundaryResolver ?? throw new ArgumentNullException(nameof(projectionBoundaryResolver));

    /// <inheritdoc />
    public async Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default)
    {
        var settingsTask = _userSettingsRepository.GetSettingsAsync(cancellationToken);
        var incomesTask = _incomePlanReader.ReadIncomesAsync(cancellationToken);
        var instrumentsTask = _financialInstrumentReader.ReadInstrumentsAsync(cancellationToken);

        await Task.WhenAll(settingsTask, incomesTask, instrumentsTask);

        var settings = await settingsTask;
        var incomes = await incomesTask;
        var instruments = await instrumentsTask;

        return new FinancialPlan
        {
            Settings = settings,
            RecurringIncomes = incomes.RecurringIncomes,
            IncomeHistories = incomes.IncomeHistories,
            AdHocIncomes = incomes.AdHocIncomes,
            Loans = instruments.Loans,
            LoanPrepayments = instruments.LoanPrepayments,
            PaymentPlans = instruments.PaymentPlans,
            CreditCards = instruments.CreditCards,
            PlannedLargeExpenses = instruments.PlannedLargeExpenses
        };
    }

    /// <inheritdoc />
    public async Task<ProjectionQueryPlan> GetProjectionPlanAsync(
        DateOnly asOf,
        CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(cancellationToken);
        if (!plan.CanBuildProjection)
        {
            return new ProjectionQueryPlan(plan, null);
        }

        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var currentSnapshot = history.FindLatestCurrentSnapshot();
        if (currentSnapshot is null)
        {
            return new ProjectionQueryPlan(plan, null);
        }

        var boundary = _projectionBoundaryResolver.Resolve(history, currentSnapshot, plan.Settings, asOf);
        var openPlan = history.FindOpenPlan();

        if (openPlan is not null)
        {
            plan = FilterOpenPeriodCardCharges(plan, openPlan);
        }

        return new ProjectionQueryPlan(ApplyProjectionBoundary(plan, boundary), boundary);
    }

    private static FinancialPlan FilterOpenPeriodCardCharges(FinancialPlan plan, PeriodPlanSnapshot openPlan) =>
        plan with
        {
            CreditCards = plan.CreditCards
                .Select(card => card with
                {
                    Charges = card.Charges
                        .Where(charge => !openPlan.ContainsDate(charge.PostingDate))
                        .ToArray()
                })
                .ToArray()
        };

    private static FinancialPlan ApplyProjectionBoundary(FinancialPlan plan, ProjectionBoundary boundary) =>
        plan with
        {
            Settings = plan.Settings with
            {
                ProjectionOpeningBalance = boundary.StartingBalance,
                ProjectionAnchorDate = boundary.ProjectionAnchorDate
            }
        };
}
