using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class FinancialPlanValidatorTests
{
    [Fact]
    public void Validate_NullPlan_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => FinancialPlanValidator.Validate(null!));
    }

    [Fact]
    public void Validate_GecerliPlan_HataFirlatmaz()
    {
        var planId = Guid.NewGuid();
        var plan = new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(15),
                PeriodVariableExpenseAllowance = 30_000m,
                ProjectionOpeningBalance = 10_000m,
                ProjectionAnchorDate = new DateOnly(2026, 10, 15),
                CreditCardCarryInterestRate = 0.05m,
                DeficitFinancingInterestRate = 0.05m
            },
            RecurringIncomes =
            [
                new RecurringIncome { Id = Guid.NewGuid(), Name = "Gelir", PaymentDay = 15, IsActive = true }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory { Amount = 50_000m, EffectiveDate = new DateOnly(2026, 1, 1) }
            ],
            AdHocIncomes =
            [
                new AdHocIncome { Amount = 15_000m, ExactDate = new DateOnly(2026, 10, 20), Description = "Prim" }
            ],
            Loans =
            [
                new Loan
                {
                    Name = "Konut Kredisi",
                    MonthlyPayment = 12_000m,
                    PaymentDay = 20,
                    NextPaymentDate = new DateOnly(2026, 10, 20),
                    RemainingInstallmentCount = 24
                }
            ],
            LoanPrepayments =
            [
                new LoanPrepayment
                {
                    Date = new DateOnly(2026, 11, 20),
                    Mode = LoanPrepaymentMode.ReduceTerm,
                    PrincipalAmount = 100_000m
                }
            ],
            PaymentPlans =
            [
                new TemporaryPaymentPlan
                {
                    Id = planId,
                    Name = "Mobilya Taksiti",
                    Installments =
                    [
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 10, 25), Amount = 10_000m },
                        new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 11, 25), Amount = 10_000m }
                    ]
                }
            ],
            CreditCards =
            [
                new CreditCard
                {
                    Name = "Bonus",
                    Limit = 50_000m,
                    StatementClosingDay = 5,
                    PaymentDueDay = 15,
                    BalanceAsOfDate = new DateOnly(2026, 10, 1),
                    MinimumPaymentRate = 0.2m
                }
            ],
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Name = "Tatil",
                    Amount = 25_000m,
                    ExactDate = new DateOnly(2026, 11, 1)
                }
            ]
        };

        var exception = Record.Exception(() => FinancialPlanValidator.Validate(plan));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_GecersizAyarlar_InvalidOperationExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings { PeriodVariableExpenseAllowance = -500m }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan));
        Assert.Equal("Dönem yaşam gideri negatif olamaz.", exception.Message);
    }

    [Fact]
    public void Validate_GecersizFaizOrani_ArgumentOutOfRangeExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings { CreditCardCarryInterestRate = 1.25m }
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => FinancialPlanValidator.Validate(plan));
        Assert.StartsWith("Kredi kartı akdi faiz oranı %0 ile %100 arasında olmalıdır.", exception.Message);
    }

    [Fact]
    public void Validate_NegatifGelirGecmisiTutari_InvalidOperationExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            IncomeHistories =
            [
                new IncomeAmountHistory { Amount = -100m, EffectiveDate = new DateOnly(2026, 1, 1) }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan));
        Assert.Equal("Gelir tutarı negatif olamaz.", exception.Message);
    }

    [Fact]
    public void Validate_NegatifAriziGelirTutari_InvalidOperationExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            AdHocIncomes =
            [
                new AdHocIncome { Amount = -50m, ExactDate = new DateOnly(2026, 10, 1), Description = "Hatalı Gelir" }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan));
        Assert.Equal("Tek seferlik arızi gelir tutarı negatif olamaz.", exception.Message);
    }

    [Fact]
    public void Validate_GecersizPlanliBuyukHarcama_InvalidOperationExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense { Name = "TV", Amount = -10_000m, ExactDate = new DateOnly(2026, 12, 1) }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan));
        Assert.Equal("Planlanan harcama tutarı sıfırdan büyük olmalıdır.", exception.Message);
    }

    [Fact]
    public void Validate_GecersizGeciciOdemePlani_InvalidOperationExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            PaymentPlans =
            [
                new TemporaryPaymentPlan { Name = string.Empty }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan));
        Assert.Equal("Ödeme planı adı boş olamaz.", exception.Message);
    }

    [Fact]
    public void Validate_GecersizKrediKartiOrani_ArgumentOutOfRangeExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            CreditCards =
            [
                new CreditCard
                {
                    Name = "Kart",
                    Limit = 10_000m,
                    StatementClosingDay = 1,
                    PaymentDueDay = 10,
                    BalanceAsOfDate = new DateOnly(2026, 10, 1),
                    MinimumPaymentRate = 1.5m
                }
            ]
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => FinancialPlanValidator.Validate(plan));
        Assert.StartsWith("Asgari ödeme oranı 0 ile 1 arasında olmalıdır.", exception.Message);
    }

    [Fact]
    public void Validate_NegatifKartLimiti_InvalidOperationExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            CreditCards =
            [
                new CreditCard
                {
                    Name = "Kart",
                    Limit = -100m,
                    StatementClosingDay = 1,
                    PaymentDueDay = 10,
                    BalanceAsOfDate = new DateOnly(2026, 10, 1)
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan));
        Assert.Equal("Kart limit ve borç bileşenleri negatif olamaz.", exception.Message);
    }

    [Fact]
    public void Validate_NegatifErkenOdemeTutari_InvalidOperationExceptionFirlatir()
    {
        var plan = new FinancialPlan
        {
            LoanPrepayments =
            [
                new LoanPrepayment
                {
                    Mode = LoanPrepaymentMode.ReduceTerm,
                    PrincipalAmount = -5_000m,
                    Date = new DateOnly(2026, 10, 1)
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan));
        Assert.Equal("Erken ödeme anapara tutarı negatif olamaz.", exception.Message);
    }

    [Fact]
    public void Validate_GecersizKrediTaksitiVeyaAdedi_InvalidOperationExceptionFirlatir()
    {
        var plan1 = new FinancialPlan
        {
            Loans =
            [
                new Loan { Name = "Kredi", MonthlyPayment = -1_000m, RemainingInstallmentCount = 10 }
            ]
        };

        var ex1 = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan1));
        Assert.Equal("Kredi aylık taksit tutarı negatif olamaz.", ex1.Message);

        var plan2 = new FinancialPlan
        {
            Loans =
            [
                new Loan { Name = "Kredi", MonthlyPayment = 1_000m, RemainingInstallmentCount = -1 }
            ]
        };

        var ex2 = Assert.Throws<InvalidOperationException>(() => FinancialPlanValidator.Validate(plan2));
        Assert.Equal("Kredi kalan taksit sayısı negatif olamaz.", ex2.Message);
    }
}
