using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests;

public sealed class SimulationRequestValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullName_ThrowsInvalidOperationException(string? name)
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = name!,
            Amount = 10_000m,
            StartDate = new DateOnly(2026, 10, 1)
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("Plan adı gereklidir", exception.Message);
    }

    [Fact]
    public void Validate_FutureIncome_BeforeAnchorDate_ThrowsInvalidOperationException()
    {
        var anchor = new DateOnly(2026, 10, 15);
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.FutureIncome,
            Name = "Erken Prim",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 10)
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request, anchor));
        Assert.Contains("15.10.2026", exception.Message);
        Assert.Contains("önce olamaz", exception.Message);
    }

    [Fact]
    public void Validate_FutureIncome_OnOrAfterAnchorDate_Succeeds()
    {
        var anchor = new DateOnly(2026, 10, 15);
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.FutureIncome,
            Name = "Zamanında Prim",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 15)
        };

        SimulationRequestValidator.Validate(request, anchor);
    }

    [Fact]
    public void Validate_LoanEarlyClosure_WithoutLoanId_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.LoanEarlyClosure,
            Name = "Erken Kapama",
            Amount = 0m,
            StartDate = new DateOnly(2026, 11, 1),
            LoanId = null
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("bir kredi seçmelisin", exception.Message);
    }

    [Fact]
    public void Validate_LoanPartialPrepayment_WithoutLoanId_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.LoanPartialPrepayment,
            Name = "Ara Ödeme",
            Amount = 20_000m,
            StartDate = new DateOnly(2026, 11, 1),
            LoanId = null,
            PrepaymentMode = LoanPrepaymentMode.ReduceTerm
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("bir kredi seçmelisin", exception.Message);
    }

    [Fact]
    public void Validate_LoanPartialPrepayment_WithoutValidPrepaymentMode_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.LoanPartialPrepayment,
            Name = "Ara Ödeme",
            Amount = 20_000m,
            StartDate = new DateOnly(2026, 11, 1),
            LoanId = Guid.NewGuid(),
            PrepaymentMode = null
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("vadenin mi taksitin mi", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_NonZeroAmountRequired_ForPurchasesAndLoans(decimal amount)
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashPurchase,
            Name = "Harcama",
            Amount = amount,
            StartDate = new DateOnly(2026, 10, 1)
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("0'dan büyük", exception.Message);
    }

    [Fact]
    public void Validate_ZeroAmountAllowed_ForCreditCardPaymentModeAndLoanEarlyClosure()
    {
        var cardRequest = new SimulationRequest
        {
            Type = SimulationScenarioType.CreditCardPaymentMode,
            Name = "Asgari Öde",
            Amount = 0m,
            StartDate = new DateOnly(2026, 10, 1),
            CreditCardId = Guid.NewGuid(),
            CardPaymentType = CreditCardPaymentType.Minimum
        };

        var loanCloseRequest = new SimulationRequest
        {
            Type = SimulationScenarioType.LoanEarlyClosure,
            Name = "Krediyi Kapat",
            Amount = 0m,
            StartDate = new DateOnly(2026, 10, 1),
            LoanId = Guid.NewGuid()
        };

        SimulationRequestValidator.Validate(cardRequest);
        SimulationRequestValidator.Validate(loanCloseRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(121)]
    public void Validate_PaymentCountOutOfRange_ThrowsArgumentOutOfRangeException(int count)
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CreditCardInstallmentPurchase,
            Name = "Taksitli Alışveriş",
            Amount = 10_000m,
            StartDate = new DateOnly(2026, 10, 1),
            PaymentCount = count,
            CreditCardId = Guid.NewGuid()
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("1 ile 120 arasında", exception.Message);
    }

    [Fact]
    public void Validate_CreditCardPaymentMode_WithoutCardId_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CreditCardPaymentMode,
            Name = "Kart Ödeme Modu",
            Amount = 0m,
            StartDate = new DateOnly(2026, 10, 1),
            CreditCardId = null,
            CardPaymentType = CreditCardPaymentType.FullStatement
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("kredi kartı seçmelisin", exception.Message);
    }

    [Fact]
    public void Validate_CreditCardPaymentMode_FixedAmount_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CreditCardPaymentMode,
            Name = "Kart Ödeme Modu",
            Amount = 0m,
            StartDate = new DateOnly(2026, 10, 1),
            CreditCardId = Guid.NewGuid(),
            CardPaymentType = CreditCardPaymentType.FixedAmount
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("asgari veya tamamı", exception.Message);
    }

    [Fact]
    public void Validate_FinancingLoan_WithoutTotalRepayment_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.FinancingLoan,
            Name = "Taşıt Kredisi",
            Amount = 100_000m,
            StartDate = new DateOnly(2026, 10, 1),
            PaymentCount = 12,
            FirstPaymentDate = new DateOnly(2026, 11, 1),
            TotalRepaymentAmount = null
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("toplam geri ödeme gereklidir", exception.Message);
    }

    [Fact]
    public void Validate_FinancingLoan_TotalRepaymentLessThanAmount_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.FinancingLoan,
            Name = "Taşıt Kredisi",
            Amount = 100_000m,
            StartDate = new DateOnly(2026, 10, 1),
            PaymentCount = 12,
            FirstPaymentDate = new DateOnly(2026, 11, 1),
            TotalRepaymentAmount = 90_000m
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("ana tutardan düşük olamaz", exception.Message);
    }

    [Fact]
    public void Validate_FinancingLoan_WithoutFirstPaymentDate_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.FinancingLoan,
            Name = "Taşıt Kredisi",
            Amount = 100_000m,
            StartDate = new DateOnly(2026, 10, 1),
            PaymentCount = 12,
            TotalRepaymentAmount = 140_000m,
            FirstPaymentDate = null
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("ilk ödeme tarihi gereklidir", exception.Message);
    }

    [Fact]
    public void Validate_FirstPaymentDate_BeforeStartDate_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.CashDebt,
            Name = "Borç",
            Amount = 50_000m,
            StartDate = new DateOnly(2026, 10, 15),
            PaymentCount = 5,
            FirstPaymentDate = new DateOnly(2026, 10, 1)
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("başlangıç tarihinden önce olamaz", exception.Message);
    }

    [Fact]
    public void Validate_IncomeChange_WithoutRecurringIncomeId_ThrowsInvalidOperationException()
    {
        var request = new SimulationRequest
        {
            Type = SimulationScenarioType.IncomeChange,
            Name = "Maaş Zammı",
            Amount = 120_000m,
            StartDate = new DateOnly(2027, 1, 1),
            RecurringIncomeId = null
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(request));
        Assert.Contains("düzenli gelir akışı seçmelisin", exception.Message);
    }

    [Fact]
    public void ValidateList_EmptyList_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(Array.Empty<SimulationRequest>()));
        Assert.Contains("en az bir koşul", exception.Message);
    }

    [Fact]
    public void ValidateList_ConflictingIncomeChange_SameStreamAndDate_ThrowsInvalidOperationException()
    {
        var incomeId = Guid.NewGuid();
        var requests = new[]
        {
            new SimulationRequest
            {
                Type = SimulationScenarioType.IncomeChange,
                Name = "Birinci Zam",
                Amount = 100_000m,
                StartDate = new DateOnly(2027, 1, 1),
                RecurringIncomeId = incomeId
            },
            new SimulationRequest
            {
                Type = SimulationScenarioType.IncomeChange,
                Name = "İkinci Zam",
                Amount = 120_000m,
                StartDate = new DateOnly(2027, 1, 1),
                RecurringIncomeId = incomeId
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => SimulationRequestValidator.Validate(requests));
        Assert.Contains("iki farklı gelir değişikliği var", exception.Message);
    }

    [Fact]
    public void ValidateList_DifferentStreamsSameDate_IncomeChange_Succeeds()
    {
        var firstIncomeId = Guid.NewGuid();
        var secondIncomeId = Guid.NewGuid();
        var requests = new[]
        {
            new SimulationRequest
            {
                Type = SimulationScenarioType.IncomeChange,
                Name = "Birinci Akış Zam",
                Amount = 100_000m,
                StartDate = new DateOnly(2027, 1, 1),
                RecurringIncomeId = firstIncomeId
            },
            new SimulationRequest
            {
                Type = SimulationScenarioType.IncomeChange,
                Name = "İkinci Akış Zam",
                Amount = 40_000m,
                StartDate = new DateOnly(2027, 1, 1),
                RecurringIncomeId = secondIncomeId
            }
        };

        SimulationRequestValidator.Validate(requests);
    }
}
