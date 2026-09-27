using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Kart yazma portunun sahtesi. Depo verilirse kaydedilen ekstreyi ve planı karta yazar;
/// böylece yeniden yükleme gerçek akıştaki gibi yeni hâli görür.
/// </summary>
internal sealed class FakeCreditCardObligationService(FakeCreditCardRepository? repository = null) : ICreditCardObligationService
{
    public CreditCard? LastSavedCard { get; private set; }
    public (Guid CardId, CreditCardStatement Statement, CurrentStatementPaymentPlan Plan)? LastSavedStatement { get; private set; }
    public (Guid CardId, DateOnly DueDate, CreditCardPaymentType PaymentType)? LastSavedPaymentMode { get; private set; }
    public (Guid CardId, DateOnly DueDate)? LastRemovedPlan { get; private set; }
    public Exception? SaveException { get; set; }

    public Task SaveCreditCardAsync(CreditCard card, CancellationToken cancellationToken = default)
    {
        if (SaveException is not null)
        {
            return Task.FromException(SaveException);
        }

        LastSavedCard = card;
        if (repository is not null)
        {
            repository.Cards[card.Id] = card;
        }

        return Task.CompletedTask;
    }

    public Task SaveCreditCardStatementAsync(
        Guid creditCardId,
        CreditCardStatement statement,
        CurrentStatementPaymentPlan paymentPlan,
        CancellationToken cancellationToken = default)
    {
        if (SaveException is not null)
        {
            return Task.FromException(SaveException);
        }

        LastSavedStatement = (creditCardId, statement, paymentPlan);
        if (repository is not null && repository.Cards.TryGetValue(creditCardId, out var card))
        {
            repository.Cards[creditCardId] = card with { CurrentStatement = statement, CurrentStatementPaymentPlan = paymentPlan };
        }

        return Task.CompletedTask;
    }

    public Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SaveCreditCardPaymentPlanAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CreditCardPaymentType paymentType,
        decimal? amount = null,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetStatementPaymentModeAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CreditCardPaymentType paymentType,
        CancellationToken cancellationToken = default)
    {
        LastSavedPaymentMode = (creditCardId, dueDate, paymentType);
        return Task.CompletedTask;
    }

    public Task RemoveCreditCardPaymentPlanAsync(
        Guid creditCardId,
        DateOnly dueDate,
        CancellationToken cancellationToken = default)
    {
        LastRemovedPlan = (creditCardId, dueDate);
        return Task.CompletedTask;
    }
}
