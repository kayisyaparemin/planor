using Mizan.Application.Abstractions;

namespace Mizan.Application.Services;

/// <summary>
/// Kullanıcının planında gerçekleşen kasıtlı değişiklikleri (borç ekleme, silme, gelir güncelleme vb.)
/// açık nakit akış dönemine plan revizyonu olarak kaydeden uygulama servisidir (Kural M4).
/// </summary>
public sealed class PlanChangeRecorder(
    IPlanReader planReader,
    FinancialSnapshotService snapshotService,
    HistoricalPlanRevisionService historicalPlanRevisionService) : IPlanChangeRecorder
{
    private readonly IPlanReader _planReader =
        planReader ?? throw new ArgumentNullException(nameof(planReader));
    private readonly FinancialSnapshotService _snapshotService =
        snapshotService ?? throw new ArgumentNullException(nameof(snapshotService));
    private readonly HistoricalPlanRevisionService _historicalPlanRevisionService =
        historicalPlanRevisionService ?? throw new ArgumentNullException(nameof(historicalPlanRevisionService));

    /// <inheritdoc />
    public async Task RecordChangeAsync(
        string trigger,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trigger);

        var plan = await _planReader.GetPlanAsync(cancellationToken);
        await _snapshotService.EnsureInitialSnapshotAsync(plan, cancellationToken);
        await _historicalPlanRevisionService.CaptureOpenPlanRevisionAsync(plan, trigger, cancellationToken);
    }
}
