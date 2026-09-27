using Mizan.Application.Abstractions;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

internal sealed class FakeCreditCardRepository : ICreditCardRepository
{
    public Dictionary<Guid, CreditCard> Cards { get; } = [];

    public Exception? ReadException { get; set; }

    public Task<IReadOnlyList<CreditCard>> GetCreditCardsAsync(CancellationToken cancellationToken = default) =>
        ReadException is null
            ? Task.FromResult<IReadOnlyList<CreditCard>>(Cards.Values.ToList())
            : Task.FromException<IReadOnlyList<CreditCard>>(ReadException);

    public Task UpsertCreditCardAsync(CreditCard card, CancellationToken cancellationToken = default)
    {
        Cards[card.Id] = card;
        return Task.CompletedTask;
    }

    public Task DeleteCreditCardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Cards.Remove(id);
        return Task.CompletedTask;
    }
}
