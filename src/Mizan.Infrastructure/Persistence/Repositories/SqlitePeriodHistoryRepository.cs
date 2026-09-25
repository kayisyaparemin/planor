using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dondurulmuş dönem planları, finansal durum anlık görüntüleri, plan revizyonları
/// ve dönem gerçekleşmelerini SQLite veritabanında saklayan somut depo adaptörü.
/// </summary>
public sealed class SqlitePeriodHistoryRepository(SQLiteAsyncConnection connection) : IPeriodHistoryRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<FinancialHistoryData> GetFinancialHistoryAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var snapsTask = _connection.Table<FinancialSnapshotEntity>().ToListAsync();
        var plansTask = _connection.Table<PeriodPlanSnapshotEntity>().ToListAsync();
        var planPaymentsTask = _connection.Table<PeriodPlanPaymentLineEntity>().ToListAsync();
        var planIncomesTask = _connection.Table<PeriodPlanIncomeLineEntity>().ToListAsync();
        var revsTask = _connection.Table<PeriodPlanRevisionEntity>().ToListAsync();
        var revPaymentsTask = _connection.Table<PeriodPlanRevisionPaymentLineEntity>().ToListAsync();
        var revIncomesTask = _connection.Table<PeriodPlanRevisionIncomeLineEntity>().ToListAsync();
        var actualsTask = _connection.Table<PeriodActualEntity>().ToListAsync();
        var actPaymentsTask = _connection.Table<ActualPaymentEntity>().ToListAsync();
        var actFlowsTask = _connection.Table<ActualFlowEntity>().ToListAsync();
        var actLivingTask = _connection.Table<ActualLivingBreakdownEntity>().ToListAsync();

        await Task.WhenAll(
            snapsTask, plansTask, planPaymentsTask, planIncomesTask, revsTask,
            revPaymentsTask, revIncomesTask, actualsTask, actPaymentsTask, actFlowsTask, actLivingTask);

        var planPayments = await planPaymentsTask;
        var planIncomes = await planIncomesTask;
        var revPayments = await revPaymentsTask;
        var revIncomes = await revIncomesTask;
        var actPayments = await actPaymentsTask;
        var actFlows = await actFlowsTask;
        var actLiving = await actLivingTask;

        return new FinancialHistoryData(
            (await snapsTask).Select(PeriodPlanEntityMapper.MapSnapshot).OrderBy(s => s.SnapshotDate).ThenBy(s => s.CreatedAtUtc).ToArray(),
            (await plansTask).Select(p => PeriodPlanEntityMapper.MapPlan(p, planPayments.Where(x => x.PeriodPlanSnapshotId == p.Id), planIncomes.Where(x => x.PeriodPlanSnapshotId == p.Id))).OrderBy(p => p.PeriodStart).ToArray(),
            (await revsTask).Select(r => PeriodPlanEntityMapper.MapRevision(r, revPayments.Where(x => x.PeriodPlanRevisionId == r.Id), revIncomes.Where(x => x.PeriodPlanRevisionId == r.Id))).OrderBy(r => r.CreatedAtUtc).ToArray(),
            (await actualsTask).Select(a => PeriodActualEntityMapper.MapActual(a, actPayments.Where(x => x.PeriodActualId == a.Id), actFlows.Where(x => x.PeriodActualId == a.Id), actLiving.Where(x => x.PeriodActualId == a.Id))).OrderBy(a => a.PeriodStart).ToArray());
    }

    /// <inheritdoc />
    public async Task SaveCurrentFinancialSnapshotAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        UserSettings? updatedSettings = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.RunInTransactionAsync(conn =>
        {
            conn.Execute("UPDATE financial_snapshots SET IsCurrent = 0 WHERE IsCurrent = 1");
            if (updatedSettings is not null)
            {
                PeriodSettlementWriter.SaveSettings(conn, updatedSettings);
            }

            PeriodPlanEntityWriter.InsertSnapshot(conn, snapshot with { IsCurrent = true });
            PeriodPlanEntityWriter.InsertPlan(conn, plan);
        });
    }

    /// <inheritdoc />
    public async Task ReplacePendingFinancialSnapshotPlanAsync(
        FinancialSnapshot snapshot,
        PeriodPlanSnapshot plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.RunInTransactionAsync(conn =>
        {
            var snapIdStr = snapshot.Id.ToString();
            var current = conn.Find<FinancialSnapshotEntity>(snapIdStr);
            if (current is null || !current.IsCurrent)
            {
                throw new InvalidOperationException("Yalnızca güncel snapshot planı düzeltilebilir.");
            }

            var oldPlans = conn.Table<PeriodPlanSnapshotEntity>().Where(p => p.FinancialSnapshotId == snapIdStr).ToList();
            if (oldPlans.Any(p => conn.Table<PeriodActualEntity>().Any(a => a.PeriodPlanSnapshotId == p.Id)))
            {
                throw new InvalidOperationException("Tamamlanmış dönem planı değiştirilemez.");
            }

            foreach (var oldPlan in oldPlans)
            {
                conn.Delete<PeriodPlanSnapshotEntity>(oldPlan.Id);
            }

            conn.Execute("DELETE FROM financial_snapshots WHERE Id = ?", snapIdStr);
            PeriodPlanEntityWriter.InsertSnapshot(conn, snapshot with { IsCurrent = true });
            PeriodPlanEntityWriter.InsertPlan(conn, plan);
        });
    }

    /// <inheritdoc />
    public async Task SavePeriodPlanRevisionAsync(
        PeriodPlanRevision revision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(revision);
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.RunInTransactionAsync(conn =>
        {
            var planIdStr = revision.PeriodPlanSnapshotId.ToString();
            var plan = conn.Find<PeriodPlanSnapshotEntity>(planIdStr);
            if (plan is null)
            {
                throw new InvalidOperationException("Revize edilecek dönem planı bulunamadı.");
            }

            if (conn.Table<PeriodActualEntity>().Any(a => a.PeriodPlanSnapshotId == planIdStr))
            {
                throw new InvalidOperationException("Tamamlanmış dönem planı revize edilemez.");
            }

            PeriodPlanEntityWriter.InsertRevision(conn, revision);
        });
    }

    /// <inheritdoc />
    public async Task CommitPeriodSettlementAsync(
        PeriodSettlementCommit commit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commit);
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.RunInTransactionAsync(conn =>
        {
            ValidateCommitPreconditions(conn, commit);

            if (commit.Revision is not null)
            {
                PeriodPlanEntityWriter.InsertRevision(conn, commit.Revision);
            }

            PeriodActualEntityWriter.InsertActual(conn, commit.Actual);
            PeriodSettlementWriter.SaveInstruments(conn, commit);
            PeriodSettlementWriter.SaveSettings(conn, commit.UpdatedSettings);

            conn.Execute("UPDATE financial_snapshots SET IsCurrent = 0 WHERE IsCurrent = 1");
            PeriodPlanEntityWriter.InsertSnapshot(conn, commit.NewSnapshot with { IsCurrent = true });
            PeriodPlanEntityWriter.InsertPlan(conn, commit.NewPlan);
        });
    }

    private static void ValidateCommitPreconditions(SQLiteConnection conn, PeriodSettlementCommit commit)
    {
        var current = conn.Table<FinancialSnapshotEntity>().FirstOrDefault(s => s.IsCurrent);
        if (current is null || current.Id != commit.Actual.SourceFinancialSnapshotId.ToString())
        {
            throw new InvalidOperationException("Güncel finansal durum değişti. Dönemi yeniden açın.");
        }

        var planIdStr = commit.Actual.PeriodPlanSnapshotId.ToString();
        if (conn.Table<PeriodActualEntity>().Any(a => a.PeriodPlanSnapshotId == planIdStr))
        {
            throw new InvalidOperationException("Bu dönem daha önce kaydedildi.");
        }
    }
}
