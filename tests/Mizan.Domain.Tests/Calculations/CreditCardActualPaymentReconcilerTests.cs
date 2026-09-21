using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CreditCardActualPaymentReconcilerTests
{
    private readonly CreditCardActualPaymentReconciler _reconciler = new();

    [Fact]
    public void Apply_TamOdeme_MevcutEkstreVePlaniTemizler_DevredenBakiyeSifirOlur()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m);
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 15_000m);

        var result = _reconciler.Apply(card, statement, 15_000m);

        Assert.Null(result.CurrentStatement);
        Assert.Null(result.CurrentStatementPaymentPlan);
        Assert.Equal(0m, result.CarriedBalance);
        Assert.Equal(0m, result.UnbilledSpending);
        Assert.Equal(new DateOnly(2026, 8, 29), result.BalanceAsOfDate);
    }

    [Fact]
    public void Apply_KismiOdeme_YalnizcaKalanAnaparayiDevreder_FaizKapitalizeEdilmez()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m);
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 15_000m);

        var result = _reconciler.Apply(card, statement, 10_000m);

        Assert.Null(result.CurrentStatement);
        Assert.Equal(5_000m, result.CarriedBalance);
    }

    [Fact]
    public void Apply_FazlaOdeme_DevredenBakiyeyiNegatifeDusurmez_SifirYapar()
    {
        var card = CreateTestCard(statementAmount: 10_000m, carriedBalance: 0m);
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 10_000m);

        var result = _reconciler.Apply(card, statement, 12_000m);

        Assert.Equal(0m, result.CarriedBalance);
    }

    [Fact]
    public void Apply_SifirOdeme_TumEkstreBorcunuDevredenBakiyeyeAktarir()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m);
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 15_000m);

        var result = _reconciler.Apply(card, statement, 0m);

        Assert.Equal(15_000m, result.CarriedBalance);
    }

    [Fact]
    public void Apply_NegatifOdemeTutari_ArgumentOutOfRangeExceptionFirlatir()
    {
        var card = CreateTestCard(statementAmount: 10_000m, carriedBalance: 0m);
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 10_000m);

        Assert.Throws<ArgumentOutOfRangeException>(() => _reconciler.Apply(card, statement, -500m));
    }

    [Fact]
    public void Apply_EkstreTarihindeVeOncesindeIslenmisHarcamalariDuser_SonrakileriKorur()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m) with
        {
            Charges =
            [
                new CardCharge { PostingDate = new DateOnly(2026, 8, 26), Amount = 1_000m },
                new CardCharge { PostingDate = new DateOnly(2026, 8, 28), Amount = 2_000m },
                new CardCharge { PostingDate = new DateOnly(2026, 9, 1), Amount = 3_000m }
            ]
        };
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 15_000m);

        var result = _reconciler.Apply(card, statement, 15_000m);

        var remaining = Assert.Single(result.Charges);
        Assert.Equal(new DateOnly(2026, 9, 1), remaining.PostingDate);
        Assert.Equal(3_000m, remaining.Amount);
    }

    [Fact]
    public void Apply_KapananEkstreninOzelOdemePlaniTemizlenir_DigerVadelerdekiPlanlarKorunur()
    {
        var dueDate = new DateOnly(2026, 9, 7);
        var otherDueDate = new DateOnly(2026, 10, 8);
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m) with
        {
            PaymentPlans =
            [
                new CreditCardPaymentPlan { DueDate = dueDate, PaymentType = CreditCardPaymentType.FixedAmount, Amount = 10_000m },
                new CreditCardPaymentPlan { DueDate = otherDueDate, PaymentType = CreditCardPaymentType.Minimum }
            ]
        };
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: dueDate, balance: 15_000m);

        var result = _reconciler.Apply(card, statement, 10_000m);

        var remainingPlan = Assert.Single(result.PaymentPlans);
        Assert.Equal(otherDueDate, remainingPlan.DueDate);
    }

    [Fact]
    public void Apply_BankaninBildirdigiSonrakiKesimVeVadeTarihleriniYeniDonemeAktarir()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m);
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 15_000m);

        var result = _reconciler.Apply(card, statement, 15_000m);

        Assert.Equal(new DateOnly(2026, 9, 28), result.KnownNextStatementDate);
        Assert.Equal(new DateOnly(2026, 10, 8), result.KnownNextDueDate);
    }

    [Fact]
    public void Apply_EkstreProjeksiyonuKoleksiyonuVerildiginde_VadeyeUygunEkstreyiBularakUygular()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m);
        var statement1 = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 15_000m);
        var statement2 = CreateProjection(closeDate: new DateOnly(2026, 9, 28), dueDate: new DateOnly(2026, 10, 8), balance: 8_000m);

        var result = _reconciler.Apply(card, [statement1, statement2], new DateOnly(2026, 9, 7), 15_000m);

        Assert.Equal(0m, result.CarriedBalance);
        Assert.Null(result.CurrentStatement);
    }

    [Fact]
    public void Apply_KoleksiyondaVadeyleEslesenEkstreYoksa_InvalidOperationExceptionFirlatir()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m);
        var statement = CreateProjection(closeDate: new DateOnly(2026, 8, 28), dueDate: new DateOnly(2026, 9, 7), balance: 15_000m);

        Assert.Throws<InvalidOperationException>(() =>
            _reconciler.Apply(card, [statement], new DateOnly(2026, 12, 31), 5_000m));
    }

    [Fact]
    public void Apply_EkstreProjeksiyonundaBakiyeNullIse_InvalidOperationExceptionFirlatir()
    {
        var card = CreateTestCard(statementAmount: 15_000m, carriedBalance: 0m);
        var statement = new CreditCardStatementProjection
        {
            StatementCloseDate = new DateOnly(2026, 8, 28),
            PaymentDueDate = new DateOnly(2026, 9, 7),
            StatementBalance = null
        };

        Assert.Throws<InvalidOperationException>(() => _reconciler.Apply(card, statement, 5_000m));
    }

    private static CreditCard CreateTestCard(decimal statementAmount, decimal carriedBalance)
    {
        var id = Guid.NewGuid();
        return new CreditCard
        {
            Id = id,
            Name = "Test Kart",
            BalanceAsOfDate = new DateOnly(2026, 8, 28),
            StatementClosingDay = 28,
            PaymentDueDay = 7,
            MinimumPaymentRate = 0.40m,
            CarriedBalance = carriedBalance,
            UnbilledSpending = 1_500m,
            CurrentStatement = new CreditCardStatement
            {
                CreditCardId = id,
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = statementAmount,
                MinimumPaymentAmount = statementAmount * 0.40m,
                NextStatementDate = new DateOnly(2026, 9, 28),
                NextDueDate = new DateOnly(2026, 10, 8)
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Full
            }
        };
    }

    private static CreditCardStatementProjection CreateProjection(DateOnly closeDate, DateOnly dueDate, decimal balance) =>
        new()
        {
            StatementCloseDate = closeDate,
            PaymentDueDate = dueDate,
            StatementBalance = balance,
            MinimumPayment = balance * 0.40m,
            Payment = balance,
            OpeningCarriedBalance = 0m,
            NewCharges = 0m,
            CarryInterest = 0m,
            NextCarriedBalance = 0m
        };
}
