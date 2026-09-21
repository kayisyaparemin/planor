using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class MandatoryPaymentCalculatorTests
{
    private readonly MandatoryPaymentCalculator _calculator;

    public MandatoryPaymentCalculatorTests()
    {
        var scheduleCalculator = new LoanScheduleCalculator();
        var amortizationCalculator = new LoanAmortizationCalculator(scheduleCalculator);
        var loanScheduleBuilder = new LoanPaymentScheduleBuilder(scheduleCalculator, amortizationCalculator);
        var scheduledPaymentCalculator = new ScheduledPaymentCalculator();
        _calculator = new MandatoryPaymentCalculator(loanScheduleBuilder, scheduledPaymentCalculator);
    }

    [Fact]
    public void Yapici_NullBagimlilik_ArgumentNullExceptionFirlatir()
    {
        var scheduleCalculator = new LoanScheduleCalculator();
        var amortizationCalculator = new LoanAmortizationCalculator(scheduleCalculator);
        var builder = new LoanPaymentScheduleBuilder(scheduleCalculator, amortizationCalculator);
        var scheduledCalculator = new ScheduledPaymentCalculator();

        Assert.Throws<ArgumentNullException>(() => new MandatoryPaymentCalculator(null!, scheduledCalculator));
        Assert.Throws<ArgumentNullException>(() => new MandatoryPaymentCalculator(builder, null!));
    }

    [Fact]
    public void BuildObligations_AktifOlmayanKrediyi_ListeyeDahilEtmez()
    {
        var inactiveLoan = new Loan
        {
            Id = Guid.NewGuid(),
            Bank = "Ziraat",
            Name = "İhtiyaç Kredisi",
            MonthlyPayment = 1_500m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 10, 15),
            RemainingInstallmentCount = 5,
            IsActive = false
        };

        var obligations = _calculator.BuildObligations([inactiveLoan], [], []);
        Assert.Empty(obligations);
    }

    [Fact]
    public void BuildObligations_NormalKrediyi_DogruAlanlarVeSiraylaListeler()
    {
        var loanId = Guid.NewGuid();
        var loan = new Loan
        {
            Id = loanId,
            Bank = "Garanti",
            Name = "Taşıt",
            MonthlyPayment = 10_000m,
            PaymentDay = 20,
            NextPaymentDate = new DateOnly(2026, 10, 20),
            RemainingInstallmentCount = 2,
            IsActive = true
        };

        var obligations = _calculator.BuildObligations([loan], [], []);

        Assert.Equal(2, obligations.Count);
        Assert.Equal("Garanti Taşıt", obligations[0].Name);
        Assert.Equal(ObligationType.Loan, obligations[0].Type);
        Assert.Equal(new DateOnly(2026, 10, 20), obligations[0].DueDate);
        Assert.Equal(10_000m, obligations[0].Amount);
        Assert.False(obligations[0].IsFinalPayment);
        Assert.Equal(loanId, obligations[0].PaymentId);

        Assert.True(obligations[1].IsFinalPayment);
        Assert.Equal(new DateOnly(2026, 11, 20), obligations[1].DueDate);
    }

    [Fact]
    public void BuildObligations_ErkenKapamaliKredide_KapanisSonrasiTaksitleriListelemez()
    {
        var loanId = Guid.NewGuid();
        var loan = new Loan
        {
            Id = loanId,
            Bank = "İş Bankası",
            Name = "Konut",
            MonthlyPayment = 20_000m,
            PaymentDay = 10,
            NextPaymentDate = new DateOnly(2026, 10, 10),
            RemainingInstallmentCount = 12,
            RemainingDebt = 200_000m,
            IsActive = true
        };

        var prepaymentId = Guid.NewGuid();
        var prepayment = new LoanPrepayment
        {
            Id = prepaymentId,
            LoanId = loanId,
            Date = new DateOnly(2026, 10, 15),
            Mode = LoanPrepaymentMode.FullClosure,
            PrincipalAmount = null
        };

        var obligations = _calculator.BuildObligations([loan], [prepayment], [], []);

        Assert.Equal(2, obligations.Count);
        Assert.Equal(new DateOnly(2026, 10, 10), obligations[0].DueDate);
        Assert.Equal(ObligationType.Loan, obligations[0].Type);
        Assert.False(obligations[0].IsFinalPayment);

        Assert.Equal(new DateOnly(2026, 10, 15), obligations[1].DueDate);
        Assert.Equal("İş Bankası Konut · erken kapama", obligations[1].Name);
        Assert.True(obligations[1].IsFinalPayment);
        Assert.Equal(prepaymentId, obligations[1].PaymentId);
        Assert.Equal("Erken ödeme: anapara + işleyen faiz", obligations[1].Detail);
    }

    [Fact]
    public void BuildObligations_KrediVadeliPlanVeKartOdemelerini_KronolojikBirlestirir()
    {
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            Bank = "Akbank",
            Name = "İhtiyaç",
            MonthlyPayment = 3_000m,
            PaymentDay = 25,
            NextPaymentDate = new DateOnly(2026, 10, 25),
            RemainingInstallmentCount = 1,
            IsActive = true
        };

        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Senet",
            Kind = PaymentPlanKind.Temporary,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 10, 5),
                    Amount = 4_000m,
                    IsPaid = false
                }
            ]
        };

        var cardPayment = new ObligationItem(
            "Axess Kart",
            ObligationType.CreditCard,
            new DateOnly(2026, 10, 15),
            7_000m);

        var obligations = _calculator.BuildObligations([loan], [plan], [cardPayment]);

        Assert.Equal(3, obligations.Count);
        Assert.Equal("Senet", obligations[0].Name);
        Assert.Equal(new DateOnly(2026, 10, 5), obligations[0].DueDate);

        Assert.Equal("Axess Kart", obligations[1].Name);
        Assert.Equal(new DateOnly(2026, 10, 15), obligations[1].DueDate);

        Assert.Equal("Akbank İhtiyaç", obligations[2].Name);
        Assert.Equal(new DateOnly(2026, 10, 25), obligations[2].DueDate);
    }

    [Fact]
    public void Summarize_TumKategorileri_DogruAltToplamlarlaHesaplar()
    {
        var items = new[]
        {
            new ObligationItem("Kredi", ObligationType.Loan, new DateOnly(2026, 10, 1), 1_000m),
            new ObligationItem("Kart", ObligationType.CreditCard, new DateOnly(2026, 10, 5), 2_000m),
            new ObligationItem("Geçici", ObligationType.TemporaryPayment, new DateOnly(2026, 10, 10), 3_000m),
            new ObligationItem("Taksit", ObligationType.InstallmentPayment, new DateOnly(2026, 10, 15), 4_000m),
            new ObligationItem("Periyodik", ObligationType.OtherScheduledPayment, new DateOnly(2026, 10, 20), 5_000m)
        };

        var summary = _calculator.Summarize(items);

        Assert.Equal(1_000m, summary.LoanPayments);
        Assert.Equal(2_000m, summary.CreditCardPayments);
        Assert.Equal(3_000m, summary.TemporaryPayments);
        Assert.Equal(4_000m, summary.InstallmentPayments);
        Assert.Equal(5_000m, summary.OtherScheduledPayments);
        Assert.Equal(15_000m, summary.Total);
        Assert.Equal(5, summary.Items.Count);
    }

    [Fact]
    public void Summarize_BuyukHarcamaKalemini_ZorunluToplamlaraDahilEtmez()
    {
        var items = new[]
        {
            new ObligationItem("Kredi", ObligationType.Loan, new DateOnly(2026, 10, 1), 5_000m),
            new ObligationItem("Tatil", ObligationType.PlannedLargeExpense, new DateOnly(2026, 10, 10), 50_000m)
        };

        var summary = _calculator.Summarize(items);

        Assert.Equal(5_000m, summary.LoanPayments);
        Assert.Equal(5_000m, summary.Total);
        var single = Assert.Single(summary.Items);
        Assert.Equal("Kredi", single.Name);
    }

    [Fact]
    public void Summarize_BosListeVerildiginde_SifirToplamlarUretir()
    {
        var summary = _calculator.Summarize([]);

        Assert.Empty(summary.Items);
        Assert.Equal(0m, summary.LoanPayments);
        Assert.Equal(0m, summary.CreditCardPayments);
        Assert.Equal(0m, summary.TemporaryPayments);
        Assert.Equal(0m, summary.InstallmentPayments);
        Assert.Equal(0m, summary.OtherScheduledPayments);
        Assert.Equal(0m, summary.Total);
    }
}
