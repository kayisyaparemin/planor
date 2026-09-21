using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CreditCardStatementCalculatorTests
{
    private readonly CreditCardStatementCalculator _calculator = new();

    [Fact]
    public void Project_StatementCountSifirVeyaNegatifse_BosListeDondurur()
    {
        var card = CreateValidCard();

        var result = _calculator.Project(card, 0);

        Assert.Empty(result);
    }

    [Fact]
    public void Project_GecersizFaizOraninda_HataFirlatir()
    {
        var card = CreateValidCard();

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Project(card, 1, carryInterestRate: -0.01m));
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.Project(card, 1, carryInterestRate: 1.01m));
    }

    [Fact]
    public void Project_DevredenBorcVeDonemIciHarcama_FaizVeAsgariyiDogruHesaplar()
    {
        // 35.000 devreden borç, %5 faiz = 1.750 carry faizi; 59.000 yeni harcama
        // Toplam ekstre = 35.000 + 1.750 + 59.000 = 95.750; %40 asgari = 38.300
        var card = CreateValidCard() with
        {
            CarriedBalance = 35_000m,
            UnbilledSpending = 59_000m,
            PaymentStrategy = CreditCardPaymentStrategy.Minimum
        };

        var statement = Assert.Single(_calculator.Project(card, 1, carryInterestRate: 0.05m));

        Assert.Equal(1_750m, statement.CarryInterest);
        Assert.Equal(95_750m, statement.StatementBalance);
        Assert.Equal(38_300m, statement.MinimumPayment);
        Assert.Equal(38_300m, statement.Payment);
        Assert.Equal(57_450m, statement.CarriedAfterPayment);
        Assert.Equal(57_450m, statement.NextCarriedBalance);
    }

    [Fact]
    public void Project_AsgariOdeme_KurusYuvarlamasindaAwayFromZeroKullanir()
    {
        var card = CreateValidCard() with
        {
            CarriedBalance = 10.0125m,
            MinimumPaymentRate = 0.40m,
            PaymentStrategy = CreditCardPaymentStrategy.Minimum
        };

        // 10,0125 + 0,50 faiz = 10,5125; %40 = 4,205 -> AwayFromZero ile 4,21
        var statement = Assert.Single(_calculator.Project(card, 1, carryInterestRate: 0.05m));

        Assert.Equal(4.21m, statement.MinimumPayment);
    }

    [Fact]
    public void Project_TamOdemeYapildiginda_SonrakiEkstreyeBorcVeFaizDevretmez()
    {
        var card = CreateValidCard() with
        {
            CarriedBalance = 60_000m,
            UnbilledSpending = 17_900m,
            PaymentStrategy = CreditCardPaymentStrategy.FullStatement
        };

        var statements = _calculator.Project(card, 2, carryInterestRate: 0.05m);

        // İlk ekstrede devreden borca faiz işletilir ve tamamı ödenir
        Assert.Equal(3_000m, statements[0].CarryInterest);
        Assert.Equal(80_900m, statements[0].StatementBalance);
        Assert.Equal(80_900m, statements[0].Payment);
        Assert.Equal(0m, statements[0].NextCarriedBalance);

        // Borç kapandığı için ikinci ekstrede devreden bakiye ve faiz sıfırdır
        Assert.Equal(0m, statements[1].CarryInterest);
        Assert.Equal(0m, statements[1].StatementBalance);
        Assert.Equal(0m, statements[1].NextCarriedBalance);
    }

    [Fact]
    public void Project_AsgariOdemede_FaizBilesikEtkiyleSonrakiEkstreyeIsler()
    {
        var card = CreateValidCard() with
        {
            CarriedBalance = 100_000m,
            MinimumPaymentRate = 0.40m,
            PaymentStrategy = CreditCardPaymentStrategy.Minimum
        };

        var statements = _calculator.Project(card, 2, carryInterestRate: 0.05m);

        // İlk ekstre: 100.000 + 5.000 faiz = 105.000; 42.000 ödeme -> 63.000 kalan
        Assert.Equal(5_000m, statements[0].CarryInterest);
        Assert.Equal(63_000m, statements[0].NextCarriedBalance);

        // İkinci ekstre: 63.000 üzerinden %5 faiz = 3.150; 66.150 ekstre; %40 = 26.460 ödeme -> 39.690 kalan
        Assert.Equal(3_150m, statements[1].CarryInterest);
        Assert.Equal(66_150m, statements[1].StatementBalance);
        Assert.Equal(26_460m, statements[1].Payment);
        Assert.Equal(39_690m, statements[1].NextCarriedBalance);
    }

    [Fact]
    public void Project_KesilmisEkstreVarsa_BankaFaiziZatenIslenmistirVeTekrarFaizEklenmez()
    {
        // I11 invariant kuralı: Kesilmiş ekstrede faiz banka tarafından eklenmiştir;
        // StatementAmount nihai tutardır, üzerine eklenmez. NewCharges ise 0 kabul edilir.
        var id = Guid.NewGuid();
        var card = new CreditCard
        {
            Id = id,
            Name = "Axess",
            BalanceAsOfDate = new DateOnly(2026, 8, 28),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            MinimumPaymentRate = 0.40m,
            PaymentStrategy = CreditCardPaymentStrategy.AskEachStatement,
            CurrentStatement = new CreditCardStatement
            {
                CreditCardId = id,
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = 100_804.94m,
                MinimumPaymentAmount = 40_321.97m,
                NextStatementDate = new DateOnly(2026, 9, 28),
                NextDueDate = new DateOnly(2026, 10, 8)
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Minimum
            }
        };

        var statement = Assert.Single(_calculator.Project(card, 1, carryInterestRate: 0.05m));

        Assert.True(statement.IsActualStatement);
        Assert.Equal(0m, statement.CarryInterest);
        Assert.Equal(0m, statement.NewCharges);
        Assert.Equal(100_804.94m, statement.StatementBalance);
        Assert.Equal(40_321.97m, statement.Payment);
        Assert.Equal(60_482.97m, statement.CarriedAfterPayment);
    }

    [Fact]
    public void Project_KesilmisEkstreSonrasindakiHarcamalar_SonrakiEkstreyeDuser()
    {
        var id = Guid.NewGuid();
        var card = new CreditCard
        {
            Id = id,
            Name = "Garanti",
            BalanceAsOfDate = new DateOnly(2026, 8, 24),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            MinimumPaymentRate = 0.40m,
            CurrentStatement = new CreditCardStatement
            {
                CreditCardId = id,
                StatementDate = new DateOnly(2026, 8, 24),
                DueDate = new DateOnly(2026, 9, 3),
                StatementAmount = 15_000m,
                MinimumPaymentAmount = 6_000m,
                NextStatementDate = new DateOnly(2026, 9, 25),
                NextDueDate = new DateOnly(2026, 10, 5)
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Minimum
            },
            PaymentStrategy = CreditCardPaymentStrategy.Minimum,
            Charges =
            [
                new CardCharge
                {
                    PostingDate = new DateOnly(2026, 9, 10),
                    Amount = 2_500m
                }
            ]
        };

        var statements = _calculator.Project(card, 2, carryInterestRate: 0m);

        // İlk ekstre: bankanın 15.000 TL'lik ekstresi
        Assert.Equal(15_000m, statements[0].StatementBalance);
        Assert.Equal(0m, statements[0].NewCharges);

        // İkinci ekstre: 9.000 devreden bakiye + 2.500 yeni harcama = 11.500
        Assert.Equal(new DateOnly(2026, 9, 25), statements[1].StatementCloseDate);
        Assert.Equal(2_500m, statements[1].NewCharges);
        Assert.Equal(11_500m, statements[1].StatementBalance);
    }

    private static CreditCard CreateValidCard() => new()
    {
        Name = "Test Kart",
        BalanceAsOfDate = new DateOnly(2026, 8, 1),
        StatementClosingDay = 25,
        PaymentDueDay = 5,
        MinimumPaymentRate = 0.40m
    };
}
