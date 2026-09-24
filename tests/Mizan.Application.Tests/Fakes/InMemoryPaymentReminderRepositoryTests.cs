using Mizan.Application.Models;

namespace Mizan.Application.Tests.Fakes;

public sealed class InMemoryPaymentReminderRepositoryTests
{
    [Fact]
    public async Task PaymentReminderRepository_ModVeYanitIslemleri_DogruCalismalidir()
    {
        var repo = new InMemoryPaymentReminderRepository();

        // Varsayılan mod Off olmalıdır
        var initialMode = await repo.GetModeAsync();
        Assert.Equal(PaymentReminderMode.Off, initialMode);

        // Mod kaydetme
        await repo.SaveModeAsync(PaymentReminderMode.Aggressive);
        var updatedMode = await repo.GetModeAsync();
        Assert.Equal(PaymentReminderMode.Aggressive, updatedMode);

        // Yanıt ekleme
        var response1 = new PaymentReminderResponse
        {
            DueKey = "kredi-20261015",
            Name = "Konut Kredisi",
            DueDate = new DateOnly(2026, 10, 15),
            Amount = 15_000m,
            Kind = PaymentReminderAnswerKind.Paid,
            AnsweredAt = new DateTime(2026, 10, 15, 9, 30, 0),
            SnoozedUntil = null
        };

        var response2 = new PaymentReminderResponse
        {
            DueKey = "kart-20261020",
            Name = "Bonus Kart",
            DueDate = new DateOnly(2026, 10, 20),
            Amount = 5_000m,
            Kind = PaymentReminderAnswerKind.Snoozed,
            AnsweredAt = new DateTime(2026, 10, 20, 9, 0, 0),
            SnoozedUntil = new DateTime(2026, 10, 20, 12, 0, 0)
        };

        await repo.UpsertResponsesAsync([response1, response2]);
        var responses = await repo.GetResponsesAsync();
        Assert.Equal(2, responses.Count);

        // Tekil silme
        await repo.DeleteResponseAsync("kredi-20261015");
        var remainingResponses = await repo.GetResponsesAsync();
        var remaining = Assert.Single(remainingResponses);
        Assert.Equal("kart-20261020", remaining.DueKey);
    }
}
