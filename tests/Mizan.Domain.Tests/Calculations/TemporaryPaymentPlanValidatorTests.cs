using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class TemporaryPaymentPlanValidatorTests
{
    [Fact]
    public void Validate_GecerliPlan_HataFirlatmaz()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Eminevim",
            Kind = PaymentPlanKind.Temporary,
            OriginalAmount = 100_000m,
            TotalRepaymentAmount = 111_827m,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 9, 20),
                    Amount = 28_167.40m
                }
            ]
        };

        var exception = Record.Exception(() => TemporaryPaymentPlanValidator.Validate(plan));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_PlanNullIse_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => TemporaryPaymentPlanValidator.Validate(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_PlanAdiBosVeyaWhitespaceIse_InvalidOperationExceptionFirlatir(string? invalidName)
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = invalidName!,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 9, 20),
                    Amount = 1_000m
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TemporaryPaymentPlanValidator.Validate(plan));
        Assert.Contains("adı boş olamaz", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TaksitYoksa_InvalidOperationExceptionFirlatir()
    {
        var plan = new TemporaryPaymentPlan
        {
            Name = "Bos Plan",
            Installments = []
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TemporaryPaymentPlanValidator.Validate(plan));
        Assert.Contains("en az bir taksit", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void Validate_TaksitTutariSifirVeyaNegatifIse_InvalidOperationExceptionFirlatir(decimal invalidAmount)
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Gecersiz Taksitli",
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 9, 20),
                    Amount = invalidAmount
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TemporaryPaymentPlanValidator.Validate(plan));
        Assert.Contains("sıfırdan büyük olmalıdır", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TaksitVadesiGecersizIse_InvalidOperationExceptionFirlatir()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Gecersiz Vade",
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = planId,
                    DueDate = default,
                    Amount = 1_000m
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TemporaryPaymentPlanValidator.Validate(plan));
        Assert.Contains("vade tarihi", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TaksitPlanIdEslesmiyorsa_InvalidOperationExceptionFirlatir()
    {
        var planId = Guid.NewGuid();
        var differentPlanId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Uyumsuz Plan Id",
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = differentPlanId,
                    DueDate = new DateOnly(2026, 9, 20),
                    Amount = 1_000m
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TemporaryPaymentPlanValidator.Validate(plan));
        Assert.Contains("eşleşmiyor", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Validate_OrijinalBorcBelirtilmisVeSifirVeyaNegatifIse_InvalidOperationExceptionFirlatir(decimal invalidAmount)
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Gecersiz Orijinal Borc",
            OriginalAmount = invalidAmount,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 9, 20),
                    Amount = 1_000m
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TemporaryPaymentPlanValidator.Validate(plan));
        Assert.Contains("Orijinal borç", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Validate_ToplamGeriOdemeBelirtilmisVeSifirVeyaNegatifIse_InvalidOperationExceptionFirlatir(decimal invalidAmount)
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Gecersiz Geri Odeme",
            TotalRepaymentAmount = invalidAmount,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 9, 20),
                    Amount = 1_000m
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TemporaryPaymentPlanValidator.Validate(plan));
        Assert.Contains("Toplam geri ödeme", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
