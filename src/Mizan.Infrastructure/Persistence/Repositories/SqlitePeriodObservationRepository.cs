using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Devam eden açık nakit akış döneminin anlık gözlem defterini
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
        if (row is null)
        {
            return null;
        }

        var payments = await _connection.Table<PeriodObservationPaymentEntity>()
            .Where(p => p.PeriodObservationId == row.Id)
            .ToListAsync();

        return MapObservation(row, payments);
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
                conn.Execute("DELETE FROM period_observation_payments WHERE PeriodObservationId = ?", existing.Id);
                conn.Execute("DELETE FROM period_observations WHERE Id = ?", existing.Id);
            }

            conn.InsertOrReplace(ToEntity(observation));
            conn.Execute("DELETE FROM period_observation_payments WHERE PeriodObservationId = ?", obsKey);
            foreach (var payment in observation.Payments)
            {
                conn.Insert(ToPaymentEntity(obsKey, payment));
            }
        });
    }

    /// <inheritdoc />
    public async Task DeletePeriodObservationAsync(Guid periodPlanSnapshotId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var planKey = periodPlanSnapshotId.ToString();
        await _connection.ExecuteAsync("DELETE FROM period_observations WHERE PeriodPlanSnapshotId = ?", planKey);
    }

    private static PeriodObservation MapObservation(PeriodObservationEntity row, List<PeriodObservationPaymentEntity> payments) =>
        new()
        {
            Id = Guid.Parse(row.Id),
            PeriodPlanSnapshotId = Guid.Parse(row.PeriodPlanSnapshotId),
            ObservedOn = DateOnly.ParseExact(row.ObservedOn, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            ObservedBalance = row.ObservedBalance,
            ObservedLivingSpend = row.ObservedLivingSpend,
            Note = row.Note,
            CreatedAtUtc = DateTimeOffset.Parse(row.CreatedAtUtc, CultureInfo.InvariantCulture),
            UpdatedAtUtc = DateTimeOffset.Parse(row.UpdatedAtUtc, CultureInfo.InvariantCulture),
            Payments = payments.Select(MapPayment).ToArray()
        };

    private static PeriodObservationPayment MapPayment(PeriodObservationPaymentEntity p) =>
        new()
        {
            Id = Guid.Parse(p.Id),
            PeriodObservationId = Guid.Parse(p.PeriodObservationId),
            PeriodPlanPaymentLineId = Guid.Parse(p.PeriodPlanPaymentLineId),
            Status = (ActualPaymentStatus)p.Status,
            ActualAmount = p.ActualAmount,
            ActualPaymentDate = string.IsNullOrWhiteSpace(p.ActualPaymentDate) ? null : DateOnly.ParseExact(p.ActualPaymentDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Note = p.Note
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

    private static PeriodObservationPaymentEntity ToPaymentEntity(string obsKey, PeriodObservationPayment payment) =>
        new()
        {
            Id = payment.Id.ToString(),
            PeriodObservationId = obsKey,
            PeriodPlanPaymentLineId = payment.PeriodPlanPaymentLineId.ToString(),
            Status = (int)payment.Status,
            ActualAmount = payment.ActualAmount,
            ActualPaymentDate = payment.ActualPaymentDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            Note = payment.Note
        };
}
