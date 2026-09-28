using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kredi ve kredi kartı haricindeki vadeli veya taksitli geçici borç planlarını
/// SQLite veritabanında saklayan ve yöneten somut depo adaptörü.
/// </summary>
public sealed class SqliteTemporaryPaymentPlanRepository(SQLiteAsyncConnection connection) : ITemporaryPaymentPlanRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<IReadOnlyList<TemporaryPaymentPlan>> GetPaymentPlansAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var plans = await _connection.Table<PaymentPlanEntity>().ToListAsync();
        var installments = await _connection.Table<PaymentInstallmentEntity>().ToListAsync();

        return plans.Select(row => new TemporaryPaymentPlan
        {
            Id = Guid.Parse(row.Id),
            Name = row.Name,
            Kind = Enum.IsDefined(typeof(PaymentPlanKind), row.Kind) ? (PaymentPlanKind)row.Kind : PaymentPlanKind.Temporary,
            OriginalAmount = row.OriginalAmount,
            TotalRepaymentAmount = row.TotalRepaymentAmount,
            Installments = installments
                .Where(x => x.PlanId == row.Id)
                .Select(i => new TemporaryPaymentInstallment
                {
                    Id = Guid.Parse(i.Id),
                    PlanId = Guid.Parse(i.PlanId),
                    DueDate = DateOnly.ParseExact(i.DueDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                    Amount = i.Amount,
                    IsPaid = i.IsPaid
                })
                .OrderBy(x => x.DueDate)
                .ToArray()
        }).ToArray();
    }

    /// <inheritdoc />
    public async Task UpsertPaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.RunInTransactionAsync(conn => PaymentPlanEntityWriter.Save(conn, plan));
    }

    /// <inheritdoc />
    public async Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<PaymentPlanEntity>(id.ToString());
    }
}
