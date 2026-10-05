using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class ObligationValidationTests
{
    private readonly FixedTestClock _clock = new(
        new DateOnly(2026, 9, 24),
        new DateTimeOffset(2026, 9, 24, 15, 30, 0, TimeSpan.Zero));

    [Fact]
    public void NormalizeCreditCard_EksikBakiyeTarihini_ClockTodayIleDoldurur()
    {
        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            Name = "Bonus",
            BalanceAsOfDate = default
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.Equal(_clock.Today, normalized.BalanceAsOfDate);
    }

    [Fact]
    public void NormalizeCreditCard_MevcutBakiyeTarihiniKorumalidir()
    {
        var existingDate = new DateOnly(2026, 8, 15);
        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            Name = "Bonus",
            BalanceAsOfDate = existingDate
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.Equal(existingDate, normalized.BalanceAsOfDate);
    }

    [Fact]
    public void NormalizeCreditCard_EkstreCreditCardIdVeGuncellemeUtcZamaniniDoldurur()
    {
        var cardId = Guid.NewGuid();
        var statement = new CreditCardStatement
        {
            CreditCardId = Guid.Empty,
            StatementDate = new DateOnly(2026, 9, 1),
            DueDate = new DateOnly(2026, 9, 11),
            StatementAmount = 5_000m,
            MinimumPaymentAmount = 2_000m,
            UpdatedAt = default
        };
        var card = new CreditCard
        {
            Id = cardId,
            CurrentStatement = statement
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.NotNull(normalized.CurrentStatement);
        Assert.Equal(cardId, normalized.CurrentStatement.CreditCardId);
        Assert.Equal(_clock.UtcNow, normalized.CurrentStatement.UpdatedAt);
    }

    [Fact]
    public void NormalizeCreditCard_CurrentStatementYoksa_NullKalir()
    {
        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            CurrentStatement = null
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.Null(normalized.CurrentStatement);
    }

    [Fact]
    public void NormalizeCreditCard_CustomModHaricinde_CustomAmountAlaniniTemizler()
    {
        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Minimum,
                CustomAmount = 2_500m
            }
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.NotNull(normalized.CurrentStatementPaymentPlan);
        Assert.Equal(CurrentStatementPaymentMode.Minimum, normalized.CurrentStatementPaymentPlan.Mode);
        Assert.Null(normalized.CurrentStatementPaymentPlan.CustomAmount);
    }

    [Fact]
    public void NormalizeCreditCard_CustomModOldugunda_CustomAmountAlaniniKorur()
    {
        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Custom,
                CustomAmount = 3_000m
            }
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.NotNull(normalized.CurrentStatementPaymentPlan);
        Assert.Equal(CurrentStatementPaymentMode.Custom, normalized.CurrentStatementPaymentPlan.Mode);
        Assert.Equal(3_000m, normalized.CurrentStatementPaymentPlan.CustomAmount);
    }

    [Fact]
    public void NormalizeCreditCard_HarcamalariPostingDateGoreSiralayip_CreditCardIdKenetler()
    {
        var cardId = Guid.NewGuid();
        var c1 = new CardCharge
        {
            CreditCardId = Guid.Empty,
            PostingDate = new DateOnly(2026, 9, 30),
            Amount = 100m
        };
        var c2 = new CardCharge
        {
            CreditCardId = Guid.Empty,
            PostingDate = new DateOnly(2026, 9, 15),
            Amount = 200m
        };
        var card = new CreditCard
        {
            Id = cardId,
            Charges = [c1, c2]
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.Equal(2, normalized.Charges.Count);
        Assert.Equal(new DateOnly(2026, 9, 15), normalized.Charges[0].PostingDate);
        Assert.Equal(new DateOnly(2026, 9, 30), normalized.Charges[1].PostingDate);
        Assert.All(normalized.Charges, c => Assert.Equal(cardId, c.CreditCardId));
    }

    [Fact]
    public void NormalizeCreditCard_OdemePlanlariniDueDateGoreSiralayip_CreditCardIdKenetler()
    {
        var cardId = Guid.NewGuid();
        var p1 = new CreditCardPaymentPlan
        {
            CreditCardId = Guid.Empty,
            DueDate = new DateOnly(2026, 10, 10),
            PaymentType = CreditCardPaymentType.Minimum
        };
        var p2 = new CreditCardPaymentPlan
        {
            CreditCardId = Guid.Empty,
            DueDate = new DateOnly(2026, 9, 10),
            PaymentType = CreditCardPaymentType.FullStatement
        };
        var card = new CreditCard
        {
            Id = cardId,
            PaymentPlans = [p1, p2]
        };

        var normalized = ObligationValidation.NormalizeCreditCard(card, _clock);

        Assert.Equal(2, normalized.PaymentPlans.Count);
        Assert.Equal(new DateOnly(2026, 9, 10), normalized.PaymentPlans[0].DueDate);
        Assert.Equal(new DateOnly(2026, 10, 10), normalized.PaymentPlans[1].DueDate);
        Assert.All(normalized.PaymentPlans, p => Assert.Equal(cardId, p.CreditCardId));
    }

    [Fact]
    public void NormalizePaymentPlan_TaksitleriVadeyeGoreSiralarVePlanIdEsitler()
    {
        var planId = Guid.NewGuid();
        var inst1 = new TemporaryPaymentInstallment
        {
            PlanId = Guid.Empty,
            DueDate = new DateOnly(2026, 11, 20),
            Amount = 1_500m
        };
        var inst2 = new TemporaryPaymentInstallment
        {
            PlanId = Guid.Empty,
            DueDate = new DateOnly(2026, 10, 20),
            Amount = 1_500m
        };
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Senet",
            Installments = [inst1, inst2]
        };

        var normalized = ObligationValidation.NormalizePaymentPlan(plan);

        Assert.Equal(2, normalized.Installments.Count);
        Assert.Equal(new DateOnly(2026, 10, 20), normalized.Installments[0].DueDate);
        Assert.Equal(new DateOnly(2026, 11, 20), normalized.Installments[1].DueDate);
        Assert.All(normalized.Installments, i => Assert.Equal(planId, i.PlanId));
    }

    [Fact]
    public void ValidateCreditCard_GecersizKartOldugunda_HataFirlatir()
    {
        var invalidCard = new CreditCard
        {
            BalanceAsOfDate = default // Gecersiz: bakiye tarihi yok
        };

        Assert.Throws<InvalidOperationException>(() => ObligationValidation.ValidateCreditCard(invalidCard));
    }

    [Fact]
    public void ValidateCreditCard_GecerliKartOldugunda_BasariylaTamamlanir()
    {
        var validCard = new CreditCard
        {
            BalanceAsOfDate = new DateOnly(2026, 9, 24),
            StatementClosingDay = 15,
            PaymentDueDay = 25,
            Limit = 50_000m,
            MinimumPaymentRate = 0.20m
        };

        ObligationValidation.ValidateCreditCard(validCard);
    }

    private sealed class FixedTestClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today { get; } = today;
        public DateTimeOffset UtcNow { get; } = utcNow;
        public DateTimeOffset Now => UtcNow;
    }
}
