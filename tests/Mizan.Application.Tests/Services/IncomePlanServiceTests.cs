using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class IncomePlanServiceTests
{
    private readonly InMemoryRecurringIncomeRepository _recurringIncomeRepository = new();
    private readonly InMemoryAdHocIncomeRepository _adHocIncomeRepository = new();
    private readonly FakePlanChangeRecorder _changeRecorder = new();

    private IncomePlanService CreateSut() =>
        new(_recurringIncomeRepository, _adHocIncomeRepository, _changeRecorder);

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IncomePlanService(null!, _adHocIncomeRepository, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new IncomePlanService(_recurringIncomeRepository, null!, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new IncomePlanService(_recurringIncomeRepository, _adHocIncomeRepository, null!));
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_ValidIncome_SavesAndTriggersChange()
    {
        var sut = CreateSut();
        var incomeId = Guid.NewGuid();
        var income = new RecurringIncome
        {
            Id = incomeId,
            Name = "Maaş Geliri",
            PaymentDay = 15
        };

        await sut.SaveRecurringIncomeAsync(income);

        var saved = await _recurringIncomeRepository.GetRecurringIncomesAsync();
        Assert.Single(saved);
        Assert.Equal(incomeId, saved[0].Id);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Gelir planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public async Task SaveRecurringIncomeAsync_InvalidPaymentDay_ThrowsArgumentOutOfRangeException(int day)
    {
        var sut = CreateSut();
        var income = new RecurringIncome
        {
            Name = "Kira Geliri",
            PaymentDay = day
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.SaveRecurringIncomeAsync(income));
        Assert.Empty(await _recurringIncomeRepository.GetRecurringIncomesAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task DeleteRecurringIncomeAsync_ValidId_DeletesAndTriggersChange()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();
        await _recurringIncomeRepository.UpsertRecurringIncomeAsync(new RecurringIncome
        {
            Id = id,
            Name = "Silinecek Gelir",
            PaymentDay = 15
        });

        await sut.DeleteRecurringIncomeAsync(id);

        Assert.Empty(await _recurringIncomeRepository.GetRecurringIncomesAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Gelir planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task SaveIncomeAmountHistoryAsync_ValidHistory_SavesAndTriggersChange()
    {
        var sut = CreateSut();
        var history = new IncomeAmountHistory
        {
            Id = Guid.NewGuid(),
            RecurringIncomeId = Guid.NewGuid(),
            EffectiveDate = new DateOnly(2026, 1, 1),
            Amount = 50_000m
        };

        await sut.SaveIncomeAmountHistoryAsync(history);

        var saved = await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync();
        Assert.Single(saved);
        Assert.Equal(history.Id, saved[0].Id);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Gelir planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public async Task SaveIncomeAmountHistoryAsync_ZeroOrNegativeAmount_ThrowsInvalidOperationException(decimal amount)
    {
        var sut = CreateSut();
        var history = new IncomeAmountHistory
        {
            Id = Guid.NewGuid(),
            RecurringIncomeId = Guid.NewGuid(),
            EffectiveDate = new DateOnly(2026, 1, 1),
            Amount = amount
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveIncomeAmountHistoryAsync(history));
        Assert.Empty(await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task DeleteIncomeAmountHistoryAsync_ValidId_DeletesAndTriggersChange()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();
        await _recurringIncomeRepository.UpsertIncomeAmountHistoryAsync(new IncomeAmountHistory
        {
            Id = id,
            RecurringIncomeId = Guid.NewGuid(),
            EffectiveDate = new DateOnly(2026, 1, 1),
            Amount = 20_000m
        });

        await sut.DeleteIncomeAmountHistoryAsync(id);

        Assert.Empty(await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Gelir planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task SaveAdHocIncomeAsync_ValidIncome_SavesAndTriggersChange()
    {
        var sut = CreateSut();
        var income = new AdHocIncome
        {
            Id = Guid.NewGuid(),
            Description = "Yıl Sonu Primi",
            Amount = 30_000m,
            ExactDate = new DateOnly(2026, 12, 25)
        };

        await sut.SaveAdHocIncomeAsync(income);

        var saved = await _adHocIncomeRepository.GetAdHocIncomesAsync();
        Assert.Single(saved);
        Assert.Equal(income.Id, saved[0].Id);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Gelir planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public async Task SaveAdHocIncomeAsync_ZeroOrNegativeAmount_ThrowsInvalidOperationException(decimal amount)
    {
        var sut = CreateSut();
        var income = new AdHocIncome
        {
            Description = "Geçersiz Prim",
            Amount = amount,
            ExactDate = new DateOnly(2026, 12, 25)
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveAdHocIncomeAsync(income));
        Assert.Empty(await _adHocIncomeRepository.GetAdHocIncomesAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task DeleteAdHocIncomeAsync_ValidId_DeletesAndTriggersChange()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();
        await _adHocIncomeRepository.UpsertAdHocIncomeAsync(new AdHocIncome
        {
            Id = id,
            Description = "Silinecek Prim",
            Amount = 10_000m,
            ExactDate = new DateOnly(2026, 12, 25)
        });

        await sut.DeleteAdHocIncomeAsync(id);

        Assert.Empty(await _adHocIncomeRepository.GetAdHocIncomesAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Gelir planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    private sealed class FakePlanChangeRecorder : IPlanChangeRecorder
    {
        public List<string> RecordedTriggers { get; } = [];

        public Task RecordChangeAsync(string trigger, CancellationToken cancellationToken = default)
        {
            RecordedTriggers.Add(trigger);
            return Task.CompletedTask;
        }
    }
}
