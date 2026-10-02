using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class IncomePlanServiceTests
{
    private static readonly DateOnly Today = new(2026, 10, 12);

    private readonly InMemoryRecurringIncomeRepository _recurringIncomeRepository = new();
    private readonly InMemoryAdHocIncomeRepository _adHocIncomeRepository = new();
    private readonly FakePlanChangeRecorder _changeRecorder = new();
    private readonly SabitSaat _clock = new(Today);

    private IncomePlanService CreateSut() =>
        new(_recurringIncomeRepository, _adHocIncomeRepository, _changeRecorder, _clock);

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IncomePlanService(null!, _adHocIncomeRepository, _changeRecorder, _clock));
        Assert.Throws<ArgumentNullException>(() =>
            new IncomePlanService(_recurringIncomeRepository, null!, _changeRecorder, _clock));
        Assert.Throws<ArgumentNullException>(() =>
            new IncomePlanService(_recurringIncomeRepository, _adHocIncomeRepository, null!, _clock));
        Assert.Throws<ArgumentNullException>(() =>
            new IncomePlanService(_recurringIncomeRepository, _adHocIncomeRepository, _changeRecorder, null!));
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_TutarEklenipPlanliDegisiklikSilinir_TekYazmadaTekRevizyonla()
    {
        var sut = CreateSut();
        var (income, current, planned) = await SeedIncomeAsync();
        var raise = new IncomeAmountHistory { Amount = 15_000m, EffectiveDate = new DateOnly(2027, 2, 20) };

        await sut.SaveRecurringIncomeAsync(income, [raise], [planned.Id]);

        var saved = await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync();
        Assert.Equal([(current.Id, 12_500m), (raise.Id, 15_000m)], saved.Select(x => (x.Id, x.Amount)));
        Assert.All(saved, x => Assert.Equal(income.Id, x.RecurringIncomeId));
        Assert.Equal(2, _recurringIncomeRepository.CombinedWriteCount);
        Assert.Equal(["Gelir planı değişti"], _changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_YururlugeGirmisTutarSilinmekIstenirse_HicbirSeyYazmaz()
    {
        var sut = CreateSut();
        var (income, current, _) = await SeedIncomeAsync();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SaveRecurringIncomeAsync(income with { Name = "Dükkân kirası" }, [], [current.Id]));

        Assert.Equal("Yürürlüğe girmiş tutar silinmez.", error.Message);
        Assert.Equal("Kira geliri", Assert.Single(await _recurringIncomeRepository.GetRecurringIncomesAsync()).Name);
        Assert.Equal(2, (await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync()).Count);
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_AyniTarihteIkinciTutar_HicbirSeyYazmaz()
    {
        var sut = CreateSut();
        var (income, _, planned) = await SeedIncomeAsync();
        var sameDay = new IncomeAmountHistory { Amount = 15_000m, EffectiveDate = planned.EffectiveDate };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveRecurringIncomeAsync(income, [sameDay], []));

        Assert.Equal("Bu tarihte zaten bir tutar var.", error.Message);
        Assert.Equal(2, (await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync()).Count);
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_TutarDunTarihliyse_HicbirSeyYazmaz()
    {
        var sut = CreateSut();
        var income = new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 };
        var backdated = new IncomeAmountHistory { Amount = 12_500m, EffectiveDate = Today.AddDays(-1) };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveRecurringIncomeAsync(income, [backdated], []));

        Assert.Empty(await _recurringIncomeRepository.GetRecurringIncomesAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_SonTutarSilinipYenisiEklenmezse_HicbirSeyYazmaz()
    {
        var sut = CreateSut();
        var income = new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 };
        var first = new IncomeAmountHistory { Amount = 1_250m, EffectiveDate = Today };
        await sut.SaveRecurringIncomeAsync(income, [first], []);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveRecurringIncomeAsync(income, [], [first.Id]));

        Assert.Equal("Gelirin en az bir tutarı kalmalı.", error.Message);
        Assert.Single(await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync());
        Assert.Single(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_BaskaGelirinTutariSilinmekIstenirse_HicbirSeyYazmaz()
    {
        var sut = CreateSut();
        var (income, _, _) = await SeedIncomeAsync();
        var other = new RecurringIncome { Name = "Emekli aylığı", PaymentDay = 5 };
        var otherAmount = new IncomeAmountHistory { Amount = 9_000m, EffectiveDate = new DateOnly(2027, 3, 5) };
        await sut.SaveRecurringIncomeAsync(other, [otherAmount], []);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveRecurringIncomeAsync(income, [], [otherAmount.Id]));

        Assert.Equal(3, (await _recurringIncomeRepository.GetIncomeAmountHistoriesAsync()).Count);
    }

    [Fact]
    public async Task SaveRecurringIncomeAsync_YeniGelirVeIlkTutari_TekYazmadaTekRevizyonlaKaydeder()
    {
        var sut = CreateSut();
        var income = new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 };
        var firstAmount = new IncomeAmountHistory { Amount = 12_500m, EffectiveDate = new DateOnly(2026, 10, 12) };

        await sut.SaveRecurringIncomeAsync(income, [firstAmount], []);

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

        await sut.SaveRecurringIncomeAsync(income with { Name = "Dükkân kirası" }, [], []);

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

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.SaveRecurringIncomeAsync(income, [], []));
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveRecurringIncomeAsync(income, [firstAmount], []));
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

    // Yürürlükte bir tutar (Temmuz'dan beri) ve ileri tarihli bir değişiklik (Ocak) taşıyan kayıtlı gelir.
    private async Task<(RecurringIncome Income, IncomeAmountHistory Current, IncomeAmountHistory Planned)> SeedIncomeAsync()
    {
        var income = new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 };
        var current = new IncomeAmountHistory { RecurringIncomeId = income.Id, Amount = 12_500m, EffectiveDate = new DateOnly(2026, 7, 20) };
        var planned = new IncomeAmountHistory { RecurringIncomeId = income.Id, Amount = 14_000m, EffectiveDate = new DateOnly(2027, 1, 20) };
        await _recurringIncomeRepository.UpsertRecurringIncomeWithAmountsAsync(income, [current, planned], []);
        return (income, current, planned);
    }

    private sealed class SabitSaat(DateOnly bugun) : IClock
    {
        public DateOnly Today { get; } = bugun;
        public DateTimeOffset UtcNow { get; } = new(bugun.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
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
