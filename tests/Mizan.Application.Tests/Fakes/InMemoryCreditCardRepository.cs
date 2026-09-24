using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi kredi kartı deposu implementasyonu.
/// </summary>
public sealed class InMemoryCreditCardRepository : ICreditCardRepository
{
    private readonly Dictionary<Guid, CreditCard> _cards = [];

    public Task<IReadOnlyList<CreditCard>> GetCreditCardsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<CreditCard> result = _cards.Values.OrderBy(x => x.Name).ToArray();
        return Task.FromResult(result);
    }

    public Task UpsertCreditCardAsync(CreditCard card, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cards[card.Id] = card;
        return Task.CompletedTask;
    }

    public Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cards.Remove(id);
        return Task.CompletedTask;
    }
}
