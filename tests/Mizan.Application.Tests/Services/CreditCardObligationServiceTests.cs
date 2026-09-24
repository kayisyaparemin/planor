using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class CreditCardObligationServiceTests
{
    private readonly InMemoryCreditCardRepository _repository = new();
    private readonly FakePlanChangeRecorder _changeRecorder = new();
    private readonly CreditCardPaymentPreferenceResolver _preferenceResolver = new();
    private readonly TestClock _clock = new(
        new DateOnly(2026, 9, 25),
        new DateTimeOffset(2026, 9, 25, 10, 0, 0, TimeSpan.Zero));

    private CreditCardObligationService CreateSut() =>
        new(_repository, _clock, _preferenceResolver, _changeRecorder);

    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CreditCardObligationService(null!, _clock, _preferenceResolver, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new CreditCardObligationService(_repository, null!, _preferenceResolver, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new CreditCardObligationService(_repository, _clock, null!, _changeRecorder));
        Assert.Throws<ArgumentNullException>(() =>
            new CreditCardObligationService(_repository, _clock, _preferenceResolver, null!));
    }

    [Fact]
    public async Task SaveCreditCardAsync_ValidCard_NormalizesValidatesSavesAndTriggersChange()
    {
        var sut = CreateSut();
        var card = CreateBaseCard();

        await sut.SaveCreditCardAsync(card);

        var savedCards = await _repository.GetCreditCardsAsync();
        Assert.Single(savedCards);
        var saved = savedCards[0];
        Assert.Equal(card.Id, saved.Id);
        Assert.Equal(_clock.Today, saved.BalanceAsOfDate);
        Assert.Single(_changeRecorder.RecordedTriggers);
        Assert.Equal("Kart planı değişti", _changeRecorder.RecordedTriggers[0]);
    }

    [Fact]
    public async Task SaveCreditCardAsync_InvalidCard_ThrowsAndDoesNotSaveOrTrigger()
    {
        var sut = CreateSut();
        var card = CreateBaseCard() with { Limit = -100m };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SaveCreditCardAsync(card));

        var savedCards = await _repository.GetCreditCardsAsync();
        Assert.Empty(savedCards);
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveCreditCardAsync_FirstStatementDecision_AppendsPaymentPreference()
    {
        var sut = CreateSut();
        var statement = new CreditCardStatement
        {
            StatementDate = new DateOnly(2026, 9, 10),
            DueDate = new DateOnly(2026, 9, 20),
            StatementAmount = 5_000m,
            MinimumPaymentAmount = 1_000m
        };
        var plan = new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Minimum };
        var card = CreateBaseCard() with
        {
            CurrentStatement = statement,
            CurrentStatementPaymentPlan = plan
        };

        await sut.SaveCreditCardAsync(card);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.Single(saved.PaymentPreferences);
        var pref = saved.PaymentPreferences[0];
        Assert.Equal(CurrentStatementPaymentMode.Minimum, pref.Mode);
        Assert.Equal(statement.StatementDate, pref.EffectiveFromStatementDate);
        Assert.Equal(_clock.UtcNow, pref.CreatedAt);
    }

    [Fact]
    public async Task SaveCreditCardAsync_SameStatementDecision_DoesNotDuplicatePaymentPreference()
    {
        var sut = CreateSut();
        var statement = new CreditCardStatement
        {
            StatementDate = new DateOnly(2026, 9, 10),
            DueDate = new DateOnly(2026, 9, 20),
            StatementAmount = 5_000m,
            MinimumPaymentAmount = 1_000m
        };
        var plan = new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Minimum };
        var card = CreateBaseCard() with
        {
            CurrentStatement = statement,
            CurrentStatementPaymentPlan = plan
        };

        await sut.SaveCreditCardAsync(card);
        // İkinci kez aynı kart ve tercihle kaydediyoruz
        await sut.SaveCreditCardAsync(card);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.Single(saved.PaymentPreferences);
    }

    [Fact]
    public async Task SaveCreditCardAsync_DifferentStatementDecision_AppendsNewPaymentPreference()
    {
        var sut = CreateSut();
        var statement = new CreditCardStatement
        {
            StatementDate = new DateOnly(2026, 9, 10),
            DueDate = new DateOnly(2026, 9, 20),
            StatementAmount = 5_000m,
            MinimumPaymentAmount = 1_000m
        };
        var card = CreateBaseCard() with
        {
            CurrentStatement = statement,
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Minimum }
        };

        await sut.SaveCreditCardAsync(card);

        // Tercihi Full olarak değiştirip tekrar kaydediyoruz
        var updatedCard = card with
        {
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Full }
        };
        await sut.SaveCreditCardAsync(updatedCard);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.Equal(2, saved.PaymentPreferences.Count);
        Assert.Equal(CurrentStatementPaymentMode.Minimum, saved.PaymentPreferences[0].Mode);
        Assert.Equal(CurrentStatementPaymentMode.Full, saved.PaymentPreferences[1].Mode);
    }

    [Fact]
    public async Task SaveCreditCardStatementAsync_CardNotFound_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var statement = new CreditCardStatement
        {
            StatementDate = new DateOnly(2026, 9, 10),
            DueDate = new DateOnly(2026, 9, 20),
            StatementAmount = 3_000m,
            MinimumPaymentAmount = 600m
        };
        var plan = new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Full };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SaveCreditCardStatementAsync(Guid.NewGuid(), statement, plan));
        Assert.Equal("Kredi kartı bulunamadı.", ex.Message);
    }

    [Fact]
    public async Task SaveCreditCardStatementAsync_ValidStatementAndPlan_NormalizesAndSaves()
    {
        var sut = CreateSut();
        var card = CreateBaseCard();
        await _repository.UpsertCreditCardAsync(card);

        var statement = new CreditCardStatement
        {
            StatementDate = new DateOnly(2026, 9, 10),
            DueDate = new DateOnly(2026, 9, 20),
            StatementAmount = 10_000m,
            MinimumPaymentAmount = 2_000m
        };
        var plan = new CurrentStatementPaymentPlan
        {
            Mode = CurrentStatementPaymentMode.Custom,
            CustomAmount = 5_000m
        };

        await sut.SaveCreditCardStatementAsync(card.Id, statement, plan);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.NotNull(saved.CurrentStatement);
        Assert.Equal(card.Id, saved.CurrentStatement.CreditCardId);
        Assert.Equal(_clock.UtcNow, saved.CurrentStatement.UpdatedAt);
        Assert.NotNull(saved.CurrentStatementPaymentPlan);
        Assert.Equal(5_000m, saved.CurrentStatementPaymentPlan.CustomAmount);
        Assert.Single(saved.PaymentPreferences);
        Assert.Equal(5_000m, saved.PaymentPreferences[0].CustomAmount);
    }

    [Fact]
    public async Task SaveCreditCardStatementAsync_NonCustomMode_ClearsCustomAmount()
    {
        var sut = CreateSut();
        var card = CreateBaseCard();
        await _repository.UpsertCreditCardAsync(card);

        var statement = new CreditCardStatement
        {
            StatementDate = new DateOnly(2026, 9, 10),
            DueDate = new DateOnly(2026, 9, 20),
            StatementAmount = 4_000m,
            MinimumPaymentAmount = 800m
        };
        var plan = new CurrentStatementPaymentPlan
        {
            Mode = CurrentStatementPaymentMode.Full,
            CustomAmount = 999m // Custom değil ama değer girilmiş
        };

        await sut.SaveCreditCardStatementAsync(card.Id, statement, plan);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.NotNull(saved.CurrentStatementPaymentPlan);
        Assert.Null(saved.CurrentStatementPaymentPlan.CustomAmount);
    }

    [Fact]
    public async Task DeleteCreditCardAsync_CallsRepositoryAndTriggersChange()
    {
        var sut = CreateSut();
        var card = CreateBaseCard();
        await _repository.UpsertCreditCardAsync(card);

        await sut.DeleteCreditCardAsync(card.Id);

        var cards = await _repository.GetCreditCardsAsync();
        Assert.Empty(cards);
        Assert.Contains("Kart planı değişti", _changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveCreditCardPaymentPlanAsync_CardNotFound_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SaveCreditCardPaymentPlanAsync(
                Guid.NewGuid(),
                new DateOnly(2026, 10, 15),
                CreditCardPaymentType.Minimum));
        Assert.Equal("Kredi kartı bulunamadı.", ex.Message);
    }

    [Fact]
    public async Task SaveCreditCardPaymentPlanAsync_FixedAmountWithoutPositiveAmount_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        var card = CreateBaseCard();
        await _repository.UpsertCreditCardAsync(card);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SaveCreditCardPaymentPlanAsync(
                card.Id,
                new DateOnly(2026, 10, 15),
                CreditCardPaymentType.FixedAmount,
                amount: 0m));
        Assert.Equal("Özel ödeme tutarı sıfırdan büyük olmalıdır.", ex.Message);
    }

    [Fact]
    public async Task SaveCreditCardPaymentPlanAsync_ValidPlan_AddsOrUpdatesPlanInDueDateOrder()
    {
        var sut = CreateSut();
        var card = CreateBaseCard();
        await _repository.UpsertCreditCardAsync(card);

        var dueDate2 = new DateOnly(2026, 11, 15);
        var dueDate1 = new DateOnly(2026, 10, 15);

        // Önce Kasım, sonra Ekim ekliyoruz (sıralamayı görmek için)
        await sut.SaveCreditCardPaymentPlanAsync(card.Id, dueDate2, CreditCardPaymentType.Minimum);
        await sut.SaveCreditCardPaymentPlanAsync(card.Id, dueDate1, CreditCardPaymentType.FixedAmount, 2_500m);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.Equal(2, saved.PaymentPlans.Count);
        Assert.Equal(dueDate1, saved.PaymentPlans[0].DueDate);
        Assert.Equal(2_500m, saved.PaymentPlans[0].Amount);
        Assert.Equal(dueDate2, saved.PaymentPlans[1].DueDate);

        // Ekim planını güncelliyoruz
        await sut.SaveCreditCardPaymentPlanAsync(card.Id, dueDate1, CreditCardPaymentType.FullStatement);
        saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.Equal(2, saved.PaymentPlans.Count);
        Assert.Equal(CreditCardPaymentType.FullStatement, saved.PaymentPlans[0].PaymentType);
        Assert.Null(saved.PaymentPlans[0].Amount);
    }

    [Fact]
    public async Task SetStatementPaymentModeAsync_FixedAmount_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SetStatementPaymentModeAsync(
                Guid.NewGuid(),
                new DateOnly(2026, 10, 15),
                CreditCardPaymentType.FixedAmount));
        Assert.Equal("Bu ekrandan yalnızca asgari veya tamamı seçilebilir.", ex.Message);
    }

    [Fact]
    public async Task SetStatementPaymentModeAsync_MatchingStatementDueDate_UpdatesCurrentStatementPaymentPlan()
    {
        var sut = CreateSut();
        var statementDueDate = new DateOnly(2026, 9, 20);
        var card = CreateBaseCard() with
        {
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 9, 10),
                DueDate = statementDueDate,
                StatementAmount = 3_000m,
                MinimumPaymentAmount = 600m
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Minimum
            }
        };
        await _repository.UpsertCreditCardAsync(card);

        await sut.SetStatementPaymentModeAsync(card.Id, statementDueDate, CreditCardPaymentType.FullStatement);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.NotNull(saved.CurrentStatementPaymentPlan);
        Assert.Equal(CurrentStatementPaymentMode.Full, saved.CurrentStatementPaymentPlan.Mode);
    }

    [Fact]
    public async Task SetStatementPaymentModeAsync_NonMatchingStatementDueDate_DelegatesToSavePaymentPlan()
    {
        var sut = CreateSut();
        var otherDueDate = new DateOnly(2026, 10, 20);
        var card = CreateBaseCard();
        await _repository.UpsertCreditCardAsync(card);

        await sut.SetStatementPaymentModeAsync(card.Id, otherDueDate, CreditCardPaymentType.Minimum);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.Single(saved.PaymentPlans);
        Assert.Equal(otherDueDate, saved.PaymentPlans[0].DueDate);
        Assert.Equal(CreditCardPaymentType.Minimum, saved.PaymentPlans[0].PaymentType);
    }

    [Fact]
    public async Task RemoveCreditCardPaymentPlanAsync_RemovesTargetPlanAndSaves()
    {
        var sut = CreateSut();
        var targetDueDate = new DateOnly(2026, 10, 15);
        var otherDueDate = new DateOnly(2026, 11, 15);
        var card = CreateBaseCard() with
        {
            PaymentPlans =
            [
                new CreditCardPaymentPlan { DueDate = targetDueDate, PaymentType = CreditCardPaymentType.Minimum },
                new CreditCardPaymentPlan { DueDate = otherDueDate, PaymentType = CreditCardPaymentType.FullStatement }
            ]
        };
        await _repository.UpsertCreditCardAsync(card);

        await sut.RemoveCreditCardPaymentPlanAsync(card.Id, targetDueDate);

        var saved = (await _repository.GetCreditCardsAsync())[0];
        Assert.Single(saved.PaymentPlans);
        Assert.Equal(otherDueDate, saved.PaymentPlans[0].DueDate);
        Assert.Contains("Kart planı değişti", _changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task RemoveCreditCardPaymentPlanAsync_CardNotFound_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RemoveCreditCardPaymentPlanAsync(Guid.NewGuid(), new DateOnly(2026, 10, 15)));
        Assert.Equal("Kredi kartı bulunamadı.", ex.Message);
    }

    [Fact]
    public async Task SaveCreditCardAsync_NullCard_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SaveCreditCardAsync(null!));
    }

    [Fact]
    public async Task SaveCreditCardStatementAsync_NullArguments_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var statement = new CreditCardStatement();
        var plan = new CurrentStatementPaymentPlan();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.SaveCreditCardStatementAsync(Guid.NewGuid(), null!, plan));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.SaveCreditCardStatementAsync(Guid.NewGuid(), statement, null!));
    }

    private static CreditCard CreateBaseCard() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Kartı",
        Bank = "Test Bankası",
        Limit = 50_000m,
        BalanceAsOfDate = new DateOnly(2026, 9, 25),
        StatementClosingDay = 10,
        PaymentDueDay = 20,
        MinimumPaymentRate = 0.20m,
        PaymentStrategy = CreditCardPaymentStrategy.Minimum
    };

    private sealed class FakePlanChangeRecorder : IPlanChangeRecorder
    {
        public List<string> RecordedTriggers { get; } = [];

        public Task RecordChangeAsync(string trigger, CancellationToken cancellationToken = default)
        {
            RecordedTriggers.Add(trigger);
            return Task.CompletedTask;
        }
    }

    private sealed class TestClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => utcNow;
    }
}
