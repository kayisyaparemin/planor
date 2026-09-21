using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class CreditCardTests
{
    [Fact]
    public void KnownTotalDebt_KesilmisEkstreYokken_DevredenVeDonemIciVeGelecekHarcamalariToplar()
    {
        var cardId = Guid.NewGuid();
        var card = new CreditCard
        {
            Id = cardId,
            Name = "Axess",
            Bank = "Akbank",
            Limit = 100_000m,
            CarriedBalance = 15_000m,
            UnbilledSpending = 25_000m,
            BalanceAsOfDate = new DateOnly(2026, 8, 20),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            MinimumPaymentRate = 0.40m,
            CurrentStatement = null,
            Charges =
            [
                new CardCharge
                {
                    CreditCardId = cardId,
                    PostingDate = new DateOnly(2026, 9, 25),
                    Amount = 5_000m
                },
                new CardCharge
                {
                    CreditCardId = cardId,
                    PostingDate = new DateOnly(2026, 10, 25),
                    Amount = 3_000m
                }
            ]
        };

        Assert.Equal(48_000m, card.KnownTotalDebt);
    }

    [Fact]
    public void KnownTotalDebt_KesilmisEkstreVarken_EkstreTutariniVeYalnizcaSonrakiHarcamalariToplar()
    {
        var cardId = Guid.NewGuid();
        var statementDate = new DateOnly(2026, 8, 25);
        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            Bank = "Garanti",
            Limit = 80_000m,
            CarriedBalance = 0m,
            UnbilledSpending = 0m,
            BalanceAsOfDate = statementDate,
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            MinimumPaymentRate = 0.40m,
            CurrentStatement = new CreditCardStatement
            {
                CreditCardId = cardId,
                StatementDate = statementDate,
                DueDate = new DateOnly(2026, 9, 5),
                StatementAmount = 30_000m,
                MinimumPaymentAmount = 12_000m
            },
            Charges =
            [
                new CardCharge
                {
                    CreditCardId = cardId,
                    PostingDate = new DateOnly(2026, 8, 20), // Ekstre öncesi, dahil edilmemeli
                    Amount = 4_000m
                },
                new CardCharge
                {
                    CreditCardId = cardId,
                    PostingDate = statementDate, // Ekstre günü, dahil edilmemeli
                    Amount = 2_000m
                },
                new CardCharge
                {
                    CreditCardId = cardId,
                    PostingDate = new DateOnly(2026, 9, 25), // Ekstre sonrası, dahil edilmeli
                    Amount = 7_500m
                },
                new CardCharge
                {
                    CreditCardId = cardId,
                    PostingDate = new DateOnly(2026, 10, 25), // Ekstre sonrası, dahil edilmeli
                    Amount = 5_000m
                }
            ]
        };

        // 30_000 + 7_500 + 5_000 = 42_500
        Assert.Equal(42_500m, card.KnownTotalDebt);
    }

    [Fact]
    public void AvailableLimit_ToplamLimittenGuncelBorcuCikarir()
    {
        var card = new CreditCard
        {
            Limit = 50_000m,
            CarriedBalance = 10_000m,
            UnbilledSpending = 5_000m,
            BalanceAsOfDate = new DateOnly(2026, 8, 1),
            StatementClosingDay = 20,
            PaymentDueDay = 30,
            MinimumPaymentRate = 0.40m
        };

        Assert.Equal(35_000m, card.AvailableLimit);
    }

    [Fact]
    public void AvailableLimit_BorcLimitiAstiginda_NegatifDegerUretir()
    {
        var card = new CreditCard
        {
            Limit = 20_000m,
            CarriedBalance = 15_000m,
            UnbilledSpending = 10_000m,
            BalanceAsOfDate = new DateOnly(2026, 8, 1),
            StatementClosingDay = 20,
            PaymentDueDay = 30,
            MinimumPaymentRate = 0.20m
        };

        Assert.Equal(-5_000m, card.AvailableLimit);
    }

    [Fact]
    public void VarsayilanDegerler_BeklenenDegerlerleBaslatilir()
    {
        var card = new CreditCard();

        Assert.True(card.IsActive);
        Assert.Equal(CreditCardPaymentStrategy.AskEachStatement, card.PaymentStrategy);
        Assert.Equal(ProjectionFallbackStrategy.None, card.ProjectionFallbackStrategy);
        Assert.Empty(card.Charges);
        Assert.Empty(card.PaymentPlans);
        Assert.Empty(card.PaymentPreferences);
    }
}
