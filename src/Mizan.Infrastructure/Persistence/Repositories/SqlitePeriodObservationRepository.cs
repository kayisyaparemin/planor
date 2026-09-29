using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Devam eden açık nakit akış döneminin anlık gözlem defterini ve ödeme işaretlerini
/// SQLite veritabanında saklayan somut depo adaptörü.
/// </summary>
public sealed class SqlitePeriodObservationRepository(SQLiteAsyncConnection connection) : IPeriodObservationRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<PeriodObservation?> GetPeriodObservationAsync(Guid periodPlanSnapshotId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var planKey = periodPlanSnapshotId.ToString();
        var row = await _connection.Table<PeriodObservationEntity>().FirstOrDefaultAsync(x => x.PeriodPlanSnapshotId == planKey);
        return row is null ? null : MapObservation(row);
    }

    /// <inheritdoc />
    public async Task UpsertPeriodObservationAsync(PeriodObservation observation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(observation);
        cancellationToken.ThrowIfCancellationRequested();

        var planKey = observation.PeriodPlanSnapshotId.ToString();
        var obsKey = observation.Id.ToString();

        await _connection.RunInTransactionAsync(conn =>
        {
            var existing = conn.Table<PeriodObservationEntity>().FirstOrDefault(x => x.PeriodPlanSnapshotId == planKey);
            if (existing is not null && existing.Id != obsKey)
            {
                conn.Execute("DELETE FROM period_observations WHERE Id = ?", existing.Id);
            }

            conn.Upsert(ToEntity(observation));
        });
    }

    /// <inheritdoc />
    public async Task DeletePeriodObservationAsync(Guid periodPlanSnapshotId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var planKey = periodPlanSnapshotId.ToString();
        await _connection.ExecuteAsync("DELETE FROM period_observations WHERE PeriodPlanSnapshotId = ?", planKey);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PeriodPaymentMark>> GetPaymentMarksAsync(Guid periodPlanSnapshotId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var planKey = periodPlanSnapshotId.ToString();
        var rows = await _connection.Table<PeriodPaymentMarkEntity>()
            .Where(x => x.PeriodPlanSnapshotId == planKey)
            .ToListAsync();
        return rows.Select(MapMark).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertPaymentMarkAsync(PeriodPaymentMark mark, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mark);
        cancellationToken.ThrowIfCancellationRequested();

        var entity = ToEntity(mark);
        await _connection.RunInTransactionAsync(conn =>
        {
            // Aynı satıra başka kimlikle konmuş işaret yeni işaretin yerine geçer; işaretin çocuğu yoktur.
            conn.Execute(
                "DELETE FROM period_payment_marks WHERE PeriodPlanSnapshotId = ? AND PeriodPlanPaymentLineId = ? AND Id <> ?",
                entity.PeriodPlanSnapshotId, entity.PeriodPlanPaymentLineId, entity.Id);
            conn.Upsert(entity);
        });
    }

    private static PeriodObservation MapObservation(PeriodObservationEntity row) =>
        new()
        {
            Id = Guid.Parse(row.Id),
            PeriodPlanSnapshotId = Guid.Parse(row.PeriodPlanSnapshotId),
            ObservedOn = DateOnly.ParseExact(row.ObservedOn, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ObservedBalance = row.ObservedBalance,
            ObservedLivingSpend = row.ObservedLivingSpend,
            Note = row.Note,
            CreatedAtUtc = DateTimeOffset.Parse(row.CreatedAtUtc, CultureInfo.InvariantCulture),
            UpdatedAtUtc = DateTimeOffset.Parse(row.UpdatedAtUtc, CultureInfo.InvariantCulture)
        };

    private static PeriodPaymentMark MapMark(PeriodPaymentMarkEntity row) =>
        new()
        {
            Id = Guid.Parse(row.Id),
            PeriodPlanSnapshotId = Guid.Parse(row.PeriodPlanSnapshotId),
            PeriodPlanPaymentLineId = Guid.Parse(row.PeriodPlanPaymentLineId),
            Status = (ActualPaymentStatus)row.Status,
            ActualAmount = row.ActualAmount,
            ActualPaymentDate = string.IsNullOrWhiteSpace(row.ActualPaymentDate) ? null : DateOnly.ParseExact(row.ActualPaymentDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Note = row.Note
        };

    private static PeriodObservationEntity ToEntity(PeriodObservation obs) =>
        new()
        {
            Id = obs.Id.ToString(),
            PeriodPlanSnapshotId = obs.PeriodPlanSnapshotId.ToString(),
            ObservedOn = obs.ObservedOn.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ObservedBalance = obs.ObservedBalance,
            ObservedLivingSpend = obs.ObservedLivingSpend,
            Note = obs.Note,
            CreatedAtUtc = obs.CreatedAtUtc.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            UpdatedAtUtc = obs.UpdatedAtUtc.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture)
        };

    private static PeriodPaymentMarkEntity ToEntity(PeriodPaymentMark mark) =>
        new()
        {
            Id = mark.Id.ToString(),
            PeriodPlanSnapshotId = mark.PeriodPlanSnapshotId.ToString(),
            PeriodPlanPaymentLineId = mark.PeriodPlanPaymentLineId.ToString(),
            Status = (int)mark.Status,
            ActualAmount = mark.ActualAmount,
            ActualPaymentDate = mark.ActualPaymentDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Note = mark.Note
        };
}
