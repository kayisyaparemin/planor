using Mizan.Application.Abstractions;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kredi kartı tanımları, ekstre verileri, ödeme tercihi geçmişi ve dönemsel kart ödeme planlarının
/// doğrulanmasını, depolanmasını ve açık dönem plan revizyonunun tetiklenmesini yöneten kullanım senaryosu servisi.
/// </summary>
public sealed class CreditCardObligationService(
    ICreditCardRepository repository,
    IClock clock,
    CreditCardPaymentPreferenceResolver paymentPreferenceResolver,
    IPlanChangeRecorder planChangeRecorder) : ICreditCardObligationService
{
    private const string PlanChangeTrigger = "Kart planı değişti";

    private readonly ICreditCardRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly CreditCardPaymentPreferenceResolver _paymentPreferenceResolver = paymentPreferenceResolver ?? throw new ArgumentNullException(nameof(paymentPreferenceResolver));
    private readonly IPlanChangeRecorder _planChangeRecorder = planChangeRecorder ?? throw new ArgumentNullException(nameof(planChangeRecorder));

    /// <inheritdoc />
    public async Task SaveCreditCardAsync(CreditCard card, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(card);
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);
        ObligationValidation.ValidateCreditCard(normalized);
        normalized = await AppendPaymentPreferenceHistoryAsync(normalized, cancellationToken);
        _paymentPreferenceResolver.Validate(normalized.PaymentPreferences);

        await _repository.UpsertCreditCardAsync(normalized, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(PlanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveCreditCardStatementAsync(
        Guid creditCardId,
        CreditCardStatement statement,
        CurrentStatementPaymentPlan paymentPlan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(statement);
        ArgumentNullException.ThrowIfNull(paymentPlan);
        cancellationToken.ThrowIfCancellationRequested();

        var card = await FindCardOrThrowAsync(creditCardId, cancellationToken);
        var now = _clock.UtcNow;
        var normalizedStatement = statement with
        {
            CreditCardId = creditCardId,
            CreatedAt = statement.CreatedAt == default ? now : statement.CreatedAt,
            UpdatedAt = now
        };
        var normalizedPlan = paymentPlan.Mode == CurrentStatementPaymentMode.Custom
            ? paymentPlan
            : paymentPlan with { CustomAmount = null };

        await SaveCreditCardAsync(card with
        {
            CurrentStatement = normalizedStatement,
            CurrentStatementPaymentPlan = normalizedPlan
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _repository.DeleteCreditCardAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(PlanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveCreditCardPaymentPlanAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CreditCardPaymentType paymentType,
        decimal? amount = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var card = await FindCardOrThrowAsync(creditCardId, cancellationToken);

        if (paymentType == CreditCardPaymentType.FixedAmount && amount is null or <= 0m)
        {
            throw new InvalidOperationException("Özel ödeme tutarı sıfırdan büyük olmalıdır.");
        }

        var existing = card.PaymentPlans.FirstOrDefault(x => x.DueDate == dueDate);
        var paymentPlan = new CreditCardPaymentPlan
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            CreditCardId = creditCardId,
            DueDate = dueDate,
            PaymentType = paymentType,
            Amount = paymentType == CreditCardPaymentType.FixedAmount ? amount : null
        };

        await SaveCreditCardAsync(card with
        {
            PaymentPlans = card.PaymentPlans
                .Where(x => x.DueDate != dueDate)
                .Append(paymentPlan)
                .OrderBy(x => x.DueDate)
                .ToArray()
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetStatementPaymentModeAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CreditCardPaymentType paymentType,
        CancellationToken cancellationToken = default)
    {
        if (paymentType == CreditCardPaymentType.FixedAmount)
        {
            throw new InvalidOperationException("Bu ekrandan yalnızca asgari veya tamamı seçilebilir.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var card = await FindCardOrThrowAsync(creditCardId, cancellationToken);

        if (card.CurrentStatement is { } statement && statement.DueDate == dueDate)
        {
            var planMode = paymentType == CreditCardPaymentType.Minimum
                ? CurrentStatementPaymentMode.Minimum
                : CurrentStatementPaymentMode.Full;

            await SaveCreditCardAsync(card with
            {
                CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan { Mode = planMode }
            }, cancellationToken);
            return;
        }

        await SaveCreditCardPaymentPlanAsync(creditCardId, dueDate, paymentType, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveCreditCardPaymentPlanAsync(Guid creditCardId, DateOnly dueDate, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var card = await FindCardOrThrowAsync(creditCardId, cancellationToken);

        await SaveCreditCardAsync(card with
        {
            PaymentPlans = card.PaymentPlans.Where(x => x.DueDate != dueDate).ToArray()
        }, cancellationToken);
    }

    private async Task<CreditCard> FindCardOrThrowAsync(Guid creditCardId, CancellationToken cancellationToken) =>
        (await _repository.GetCreditCardsAsync(cancellationToken)).SingleOrDefault(x => x.Id == creditCardId)
        ?? throw new InvalidOperationException("Kredi kartı bulunamadı.");

    private async Task<CreditCard> AppendPaymentPreferenceHistoryAsync(CreditCard card, CancellationToken cancellationToken)
    {
        var existing = (await _repository.GetCreditCardsAsync(cancellationToken)).FirstOrDefault(x => x.Id == card.Id);
        var history = existing?.PaymentPreferences ?? [];

        if (card.CurrentStatement is not { } statement || card.CurrentStatementPaymentPlan is not { } plan)
        {
            return card with { PaymentPreferences = history };
        }

        var effective = _paymentPreferenceResolver.Resolve(statement.StatementDate, history);
        if (_paymentPreferenceResolver.RepresentsSameDecision(effective, plan))
        {
            return card with { PaymentPreferences = history };
        }

        var appended = new CreditCardPaymentPreference
        {
            CreditCardId = card.Id,
            Mode = plan.Mode,
            CustomAmount = plan.Mode == CurrentStatementPaymentMode.Custom ? plan.CustomAmount : null,
            EffectiveFromStatementDate = statement.StatementDate,
            CreatedAt = _clock.UtcNow
        };

        return card with { PaymentPreferences = history.Append(appended).ToArray() };
    }
}
