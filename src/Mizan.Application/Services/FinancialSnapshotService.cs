using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Finansal durum anlık görüntülerinin (FinancialSnapshot) ve buna bağlı dondurulmuş dönem
/// planlarının yaşam döngüsünü, ilk kurulumunu (onboarding) ve dönem başlangıcı dondurma
/// işlemlerini yöneten uygulama servisidir.
/// </summary>
public sealed class FinancialSnapshotService(
    IPeriodHistoryRepository periodHistoryRepository,
    IClock clock,
    PeriodPlanSnapshotService planSnapshotService,
    CashFlowPeriodCalculator periodCalculator)
{
    private readonly IPeriodHistoryRepository _periodHistoryRepository =
        periodHistoryRepository ?? throw new ArgumentNullException(nameof(periodHistoryRepository));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly PeriodPlanSnapshotService _planSnapshotService =
        planSnapshotService ?? throw new ArgumentNullException(nameof(planSnapshotService));
    private readonly CashFlowPeriodCalculator _periodCalculator =
        periodCalculator ?? throw new ArgumentNullException(nameof(periodCalculator));

    /// <summary>
    /// Finansal plan için henüz hiçbir durum kaydı yoksa ilk başlangıç durumunu dondurarak oluşturur;
    /// mevcut bekleyen durum varsa ve takvim döngüsü bozulmuşsa onarır, aksi halde güncel durumu getirir.
    /// </summary>
    public async Task<FinancialSnapshot?> EnsureInitialSnapshotAsync(
        FinancialPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var current = history.FindLatestCurrentSnapshot();
        if (current is not null)
        {
            return await HandleExistingSnapshotAsync(plan, history, current, cancellationToken);
        }

        if (!plan.CanBuildProjection)
        {
            return null;
        }

        var bundle = Build(
            plan,
            plan.Settings.ProjectionOpeningBalance,
            plan.Settings.ProjectionAnchorDate,
            FinancialSnapshotSource.Initial,
            "İlk güncel finansal durum",
            null);

        await _periodHistoryRepository.SaveCurrentFinancialSnapshotAsync(
            bundle.Snapshot,
            bundle.Plan,
            cancellationToken: cancellationToken);

        return bundle.Snapshot;
    }

    /// <summary>
    /// Belirtilen finansal plan, açılış bakiyesi ve referans tarihi ile yeni bir güncel finansal durum
    /// ve dondurulmuş dönem planı oluşturur ve kalıcı olarak kaydeder.
    /// </summary>
    public async Task<FinancialSnapshotBundle> CreateCurrentSnapshotAsync(
        FinancialPlan plan,
        decimal startingBalance,
        DateOnly snapshotDate,
        FinancialSnapshotSource source,
        string note,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var history = await _periodHistoryRepository.GetFinancialHistoryAsync(cancellationToken);
        var previous = history.FindLatestCurrentSnapshot();
        var bundle = Build(plan, startingBalance, snapshotDate, source, note, previous?.Id);

        await _periodHistoryRepository.SaveCurrentFinancialSnapshotAsync(
            bundle.Snapshot,
            bundle.Plan,
            bundle.UpdatedSettings,
            cancellationToken);

        return bundle;
    }

    /// <summary>
    /// Veritabanına kaydetmeksizin bellek üzerinde yeni bir finansal durum, dondurulmuş plan
    /// ve güncellenen ayarlar paketini (bundle) inşa eder.
    /// </summary>
    public FinancialSnapshotBundle Build(
        FinancialPlan plan,
        decimal startingBalance,
        DateOnly snapshotDate,
        FinancialSnapshotSource source,
        string note,
        Guid? previousSnapshotId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!plan.CanBuildProjection)
        {
            throw new InvalidOperationException(
                "Güncel durum için çapa tarihi ve en az bir finansal başlangıç dayanağı gereklidir.");
        }

        var updatedSettings = plan.Settings with
        {
            ProjectionOpeningBalance = startingBalance,
            ProjectionAnchorDate = snapshotDate
        };
        var snapshotPlan = plan with { Settings = updatedSettings };
        var now = _clock.UtcNow;

        var snapshot = new FinancialSnapshot
        {
            SnapshotDate = snapshotDate,
            ProjectionAnchorDate = snapshotDate,
            ProjectionOpeningBalance = startingBalance,
            Anchor = updatedSettings.PeriodAnchor,
            PreviousSnapshotId = previousSnapshotId,
            Source = source,
            IsCurrent = true,
            CreatedAtUtc = now,
            Note = (note ?? string.Empty).Trim()
        };

        var frozenPlan = _planSnapshotService.Freeze(snapshotPlan, snapshot, now);
        snapshot = snapshot with { NextSettlementDate = frozenPlan.SettlementAvailableFrom };

        return new FinancialSnapshotBundle(snapshot, frozenPlan, updatedSettings);
    }

    private async Task<FinancialSnapshot> HandleExistingSnapshotAsync(
        FinancialPlan plan,
        FinancialHistoryData history,
        FinancialSnapshot current,
        CancellationToken cancellationToken)
    {
        var expectedSettlementDate = _periodCalculator.GetNextSettlementDate(current.SnapshotDate, current.Anchor);
        var pendingPlan = history.FindOpenPlan();

        var requiresCadenceRepair = pendingPlan is not null &&
            (current.NextSettlementDate != expectedSettlementDate ||
             pendingPlan.PeriodStart != current.SnapshotDate ||
             pendingPlan.PeriodEnd != expectedSettlementDate ||
             pendingPlan.SettlementAvailableFrom != expectedSettlementDate);

        if (!requiresCadenceRepair)
        {
            return current;
        }

        var correctedSnapshot = current with { NextSettlementDate = expectedSettlementDate };
        var snapshotPlan = plan with
        {
            Settings = plan.Settings with
            {
                ProjectionOpeningBalance = current.ProjectionOpeningBalance,
                ProjectionAnchorDate = current.SnapshotDate,
                PeriodAnchor = current.Anchor
            }
        };

        var correctedPlan = _planSnapshotService.Freeze(snapshotPlan, correctedSnapshot, _clock.UtcNow);
        await _periodHistoryRepository.ReplacePendingFinancialSnapshotPlanAsync(
            correctedSnapshot,
            correctedPlan,
            cancellationToken);

        return correctedSnapshot;
    }
}
