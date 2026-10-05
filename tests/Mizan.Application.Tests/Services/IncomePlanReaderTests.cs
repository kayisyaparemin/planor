using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class IncomePlanReaderTests
{
    [Fact]
    public async Task ReadIncomesAsync_TumGelirAkislariniVeGecmisleriniPaketler()
    {
        var recurringRepo = new InMemoryRecurringIncomeRepository();
        var adHocRepo = new InMemoryAdHocIncomeRepository();

        var recurringIncome = new RecurringIncome
        {
            Id = Guid.NewGuid(),
            Name = "Gelir",
            IsActive = true,
            PaymentDay = 15
        };
        var history = new IncomeAmountHistory
        {
            RecurringIncomeId = recurringIncome.Id,
            EffectiveDate = new DateOnly(2026, 1, 1),
            Amount = 50_000m
        };
        await recurringRepo.UpsertRecurringIncomeAsync(recurringIncome);
        await recurringRepo.UpsertIncomeAmountHistoryAsync(history);

        var adHoc = new AdHocIncome
        {
            Id = Guid.NewGuid(),
            Description = "Performans Primi",
            Amount = 25_000m,
            ExactDate = new DateOnly(2026, 10, 20)
        };
        await adHocRepo.UpsertAdHocIncomeAsync(adHoc);

        var reader = new IncomePlanReader(recurringRepo, adHocRepo);

        var bundle = await reader.ReadIncomesAsync();

        Assert.NotNull(bundle);
        Assert.Single(bundle.RecurringIncomes);
        Assert.Equal("Gelir", bundle.RecurringIncomes[0].Name);
        Assert.Single(bundle.IncomeHistories);
        Assert.Equal(50_000m, bundle.IncomeHistories[0].Amount);
        Assert.Single(bundle.AdHocIncomes);
        Assert.Equal("Performans Primi", bundle.AdHocIncomes[0].Description);
    }
}
