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
    public async Task SaveRecurringIncomeAsync_YeniGelirVeIlkTutari_TekYazmadaTekRevizyonlaKaydeder()
    {
        var sut = CreateSut();
        var income = new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 };
        var firstAmount = new IncomeAmountHistory { Amount = 12_500m, EffectiveDate = new DateOnly(2026, 10, 12) };

        await sut.SaveRecurringIncomeAsync(income, [firstAmount]);

        Assert.Equal(income, Assert.Single(await _recurringIncomeRepository.GetRecurringIncomesAsync()));
        var saved = Assert.Single(await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync());
        Assert.Equal(income.Id, saved.RecurringIncomeId);
        Assert.Equal(12_500m, saved.Amount);
        Assert.Equal(1, _recurringIncomeRepository.CombinedWriteCount);
        Assert.Equal(["Gelir planı değişti"], _changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_EklenecekTutarYoksa_YalnizGeliriYazarTekRevizyonDogurur()
    {
        var sut = CreateSut();
        var income = new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 };

        await sut.SaveRecurringIncomeAsync(income with { Name = "Dükkân kirası" }, []);

        Assert.Equal("Dükkân kirası", Assert.Single(await _recurringIncomeRepository.GetRecurringIncomesAsync()).Name);
        Assert.Empty(await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
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

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.SaveRecurringIncomeAsync(income, []));
        Assert.Empty(await _recurringIncomeRepository.GetRecurringIncomesAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public async Task SaveRecurringIncomeAsync_TutarSifirVeyaEksiyse_HicbirSeyYazmaz(decimal amount)
    {
        var sut = CreateSut();
        var income = new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 };
        var firstAmount = new IncomeAmountHistory { Amount = amount, EffectiveDate = new DateOnly(2026, 10, 12) };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveRecurringIncomeAsync(income, [firstAmount]));
        Assert.Empty(await _recurringIncomeRepository.GetRecurringIncomesAsync());
        Assert.Empty(await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync());
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
