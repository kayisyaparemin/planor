using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kullanıcının What-If simülatöründe oluşturduğu varsayımsal plan taslaklarını
/// SQLite veritabanında saklayan somut depo adaptörü.
/// </summary>
public sealed class SqliteSimulationDraftRepository(SQLiteAsyncConnection connection) : ISimulationDraftRepository
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public async Task<IReadOnlyList<SimulationDraft>> GetDraftsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var drafts = await _connection.Table<SimulationDraftEntity>().ToListAsync();
        var conditions = await _connection.Table<SimulationDraftConditionEntity>().ToListAsync();

        return drafts
            .Select(d => MapDraft(d, conditions.Where(c => c.DraftId == d.Id).OrderBy(c => c.Position)))
            .OrderByDescending(d => d.UpdatedAt)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<SimulationDraft?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var draftIdStr = id.ToString();
        var draft = await _connection.Table<SimulationDraftEntity>().FirstOrDefaultAsync(d => d.Id == draftIdStr);
        if (draft is null)
        {
            return null;
        }

        var conditions = await _connection.Table<SimulationDraftConditionEntity>()
            .Where(c => c.DraftId == draftIdStr)
            .OrderBy(c => c.Position)
            .ToListAsync();

        return MapDraft(draft, conditions);
    }

    /// <inheritdoc />
    public async Task UpsertDraftAsync(SimulationDraft draft, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);
        cancellationToken.ThrowIfCancellationRequested();

        var draftIdStr = draft.Id.ToString();
        await _connection.RunInTransactionAsync(conn =>
        {
            conn.InsertOrReplace(new SimulationDraftEntity
            {
                Id = draftIdStr,
                Name = draft.Name,
                CreatedAt = draft.CreatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
                UpdatedAt = draft.UpdatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture)
            });

            conn.Execute("DELETE FROM simulation_draft_conditions WHERE DraftId = ?", draftIdStr);
            for (var index = 0; index < draft.Conditions.Count; index++)
            {
                conn.Insert(ToEntity(draftIdStr, index, draft.Conditions[index]));
            }
        });
    }

    /// <inheritdoc />
    public async Task DeleteDraftAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.DeleteAsync<SimulationDraftEntity>(id.ToString());
    }

    private static SimulationDraft MapDraft(SimulationDraftEntity entity, IEnumerable<SimulationDraftConditionEntity> conditions) =>
        new(
            Guid.Parse(entity.Id),
            entity.Name,
            DateTimeOffset.Parse(entity.CreatedAt, CultureInfo.InvariantCulture),
            DateTimeOffset.Parse(entity.UpdatedAt, CultureInfo.InvariantCulture),
            conditions.Select(ToDomainCondition).ToArray());

    private static SimulationDraftCondition ToDomainCondition(SimulationDraftConditionEntity entity) =>
        new(
            new SimulationRequest
            {
                ScenarioId = Guid.Parse(entity.ScenarioId),
                Type = (SimulationScenarioType)entity.Type,
                Name = entity.Name,
                Amount = entity.Amount,
                StartDate = DateOnly.ParseExact(entity.StartDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PaymentCount = entity.PaymentCount,
                FirstPaymentDate = string.IsNullOrWhiteSpace(entity.FirstPaymentDate) ? null : DateOnly.ParseExact(entity.FirstPaymentDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                CreditCardId = string.IsNullOrWhiteSpace(entity.CreditCardId) ? null : Guid.Parse(entity.CreditCardId),
                TotalRepaymentAmount = entity.TotalRepaymentAmount,
                RecurringIncomeId = string.IsNullOrWhiteSpace(entity.RecurringIncomeId) ? null : Guid.Parse(entity.RecurringIncomeId),
                CardPaymentType = (CreditCardPaymentType?)entity.CardPaymentType,
                AppliesToAllStatements = entity.AppliesToAllStatements,
                LoanId = string.IsNullOrWhiteSpace(entity.LoanId) ? null : Guid.Parse(entity.LoanId),
                PrepaymentMode = (LoanPrepaymentMode?)entity.PrepaymentMode
            },
            entity.IsEnabled);

    private static SimulationDraftConditionEntity ToEntity(string draftId, int position, SimulationDraftCondition condition)
    {
        var req = condition.Request;
        return new SimulationDraftConditionEntity
        {
            Id = Guid.NewGuid().ToString(),
            DraftId = draftId,
            Position = position,
            IsEnabled = condition.IsEnabled,
            Type = (int)req.Type,
            Name = req.Name,
            Amount = req.Amount,
            StartDate = req.StartDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            PaymentCount = req.PaymentCount,
            FirstPaymentDate = req.FirstPaymentDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            CreditCardId = req.CreditCardId?.ToString(),
            TotalRepaymentAmount = req.TotalRepaymentAmount,
            RecurringIncomeId = req.RecurringIncomeId?.ToString(),
            ScenarioId = req.ScenarioId.ToString(),
            CardPaymentType = (int?)req.CardPaymentType,
            AppliesToAllStatements = req.AppliesToAllStatements,
            LoanId = req.LoanId?.ToString(),
            PrepaymentMode = (int?)req.PrepaymentMode
        };
    }
}
