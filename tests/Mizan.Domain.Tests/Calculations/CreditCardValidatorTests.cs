using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CreditCardValidatorTests
{
    [Fact]
    public void Validate_GecerliKart_HatasizGecer()
    {
        var card = GecerliKartUret();
        var exception = Record.Exception(() => CreditCardValidator.Validate(card));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullKart_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => CreditCardValidator.Validate(null!));
    }

    [Fact]
    public void Validate_BakiyeTarihiVarsayilanIse_InvalidOperationExceptionFirlatir()
    {
        var card = GecerliKartUret() with { BalanceAsOfDate = default };
        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Validate_KesimGunuGecersizIse_HataFirlatir(int day)
    {
        var card = GecerliKartUret() with { StatementClosingDay = day };
        Assert.Throws<ArgumentOutOfRangeException>(() => CreditCardValidator.Validate(card));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Validate_SonOdemeGunuGecersizIse_HataFirlatir(int day)
    {
        var card = GecerliKartUret() with { PaymentDueDay = day };
        Assert.Throws<ArgumentOutOfRangeException>(() => CreditCardValidator.Validate(card));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Validate_AsgariOdemeOraniAralikDisiIse_HataFirlatir(decimal rate)
    {
        var card = GecerliKartUret() with { MinimumPaymentRate = rate };
        Assert.Throws<ArgumentOutOfRangeException>(() => CreditCardValidator.Validate(card));
    }

    [Fact]
    public void Validate_NegatifBorcBilesenleri_InvalidOperationExceptionFirlatir()
    {
        var card1 = GecerliKartUret() with { CarriedBalance = -1m };
        var card2 = GecerliKartUret() with { UnbilledSpending = -1m };
        var card3 = GecerliKartUret() with
        {
            Charges = [new CardCharge { Amount = -10m, PostingDate = new DateOnly(2026, 9, 1) }]
        };

        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card1));
        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card2));
        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card3));
    }

    [Fact]
    public void Validate_SabitOdemeStratejisindeTutarSifirVeyaNullIse_HataFirlatir()
    {
        var card1 = GecerliKartUret() with
        {
            PaymentStrategy = CreditCardPaymentStrategy.FixedAmount,
            FixedPaymentAmount = null
        };
        var card2 = GecerliKartUret() with
        {
            PaymentStrategy = CreditCardPaymentStrategy.FixedAmount,
            FixedPaymentAmount = 0m
        };

        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card1));
        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card2));
    }

    [Fact]
    public void Validate_YedekSabitTutarStratejisindeTutarSifirVeyaNullIse_HataFirlatir()
    {
        var card = GecerliKartUret() with
        {
            ProjectionFallbackStrategy = ProjectionFallbackStrategy.FixedAmount,
            ProjectionFallbackFixedAmount = 0m
        };

        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card));
    }

    [Fact]
    public void Validate_MukerrerVadeOzelPlani_HataFirlatir()
    {
        var dueDate = new DateOnly(2026, 9, 5);
        var card = GecerliKartUret() with
        {
            PaymentPlans =
            [
                new CreditCardPaymentPlan { DueDate = dueDate, PaymentType = CreditCardPaymentType.Minimum },
                new CreditCardPaymentPlan { DueDate = dueDate, PaymentType = CreditCardPaymentType.FullStatement }
            ]
        };

        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card));
    }

    [Fact]
    public void Validate_EkstreAsgariTutarEkstreTutariniAsarsa_HataFirlatir()
    {
        var card = GecerliKartUret() with
        {
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 25),
                DueDate = new DateOnly(2026, 9, 5),
                StatementAmount = 10_000m,
                MinimumPaymentAmount = 15_000m
            }
        };

        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card));
    }

    [Fact]
    public void Validate_EkstreYokkenEkstrePlaniVarsa_HataFirlatir()
    {
        var card = GecerliKartUret() with
        {
            CurrentStatement = null,
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Minimum
            }
        };

        Assert.Throws<InvalidOperationException>(() => CreditCardValidator.Validate(card));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.05)]
    [InlineData(1.0)]
    public void ValidateInterestRate_GecerliOranlar_SorunsuzGecer(decimal rate)
    {
        var exception = Record.Exception(() => CreditCardValidator.ValidateInterestRate(rate));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void ValidateInterestRate_AralikDisiOran_HataFirlatir(decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreditCardValidator.ValidateInterestRate(rate));
    }

    private static CreditCard GecerliKartUret() => new()
    {
        Name = "Test Kart",
        Bank = "Test Bank",
        Limit = 50_000m,
        CarriedBalance = 5_000m,
        UnbilledSpending = 3_000m,
        BalanceAsOfDate = new DateOnly(2026, 8, 1),
        StatementClosingDay = 25,
        PaymentDueDay = 5,
        MinimumPaymentRate = 0.40m
    };
}
