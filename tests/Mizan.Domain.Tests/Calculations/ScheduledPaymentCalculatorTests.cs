using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class ScheduledPaymentCalculatorTests
{
    private readonly ScheduledPaymentCalculator _calculator = new();

    [Fact]
    public void GetItems_NullPlanListesi_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => _calculator.GetItems(null!));
    }

    [Fact]
    public void GetItems_BosPlanListesi_BosKoleksiyonDondurur()
    {
        var items = _calculator.GetItems([]);
        Assert.Empty(items);
    }

    [Fact]
    public void GetItems_TumTaksitleriOdenmisPlan_HicKalemUretmez()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Ödenmiş Plan",
            Kind = PaymentPlanKind.Temporary,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 10, 1),
                    Amount = 1_000m,
                    IsPaid = true
                }
            ]
        };

        var items = _calculator.GetItems([plan]);
        Assert.Empty(items);
    }

    [Fact]
    public void GetItems_YalnizcaOdenmemisTaksitleriUretir()
    {
        var planId = Guid.NewGuid();
        var unpaidId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Mobilya Taksiti",
            Kind = PaymentPlanKind.Installment,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 9, 15),
                    Amount = 2_500m,
                    IsPaid = true
                },
                new TemporaryPaymentInstallment
                {
                    Id = unpaidId,
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 10, 15),
                    Amount = 2_500m,
                    IsPaid = false
                }
            ]
        };

        var items = _calculator.GetItems([plan]);

        var item = Assert.Single(items);
        Assert.Equal("Mobilya Taksiti", item.Name);
        Assert.Equal(ObligationType.InstallmentPayment, item.Type);
        Assert.Equal(new DateOnly(2026, 10, 15), item.DueDate);
        Assert.Equal(2_500m, item.Amount);
        Assert.True(item.IsFinalPayment);
        Assert.Equal(unpaidId, item.PaymentId);
    }

    [Theory]
    [InlineData(PaymentPlanKind.Temporary, ObligationType.TemporaryPayment)]
    [InlineData(PaymentPlanKind.Installment, ObligationType.InstallmentPayment)]
    [InlineData(PaymentPlanKind.Recurring, ObligationType.OtherScheduledPayment)]
    [InlineData(PaymentPlanKind.OtherScheduled, ObligationType.OtherScheduledPayment)]
    public void GetItems_FarkliPlanTurlerini_DogruObligationTypeIleEsler(
        PaymentPlanKind kind,
        ObligationType expectedType)
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Test Planı",
            Kind = kind,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 10, 1),
                    Amount = 500m,
                    IsPaid = false
                }
            ]
        };

        var items = _calculator.GetItems([plan]);

        var item = Assert.Single(items);
        Assert.Equal(expectedType, item.Type);
    }

    [Fact]
    public void GetItems_CokluTaksitte_YalnizcaSonTaksitFinalPaymentIsaretlenir()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Elden Borç",
            Kind = PaymentPlanKind.Temporary,
            Installments =
            [
                new TemporaryPaymentInstallment
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 10, 10),
                    Amount = 3_000m,
                    IsPaid = false
                },
                new TemporaryPaymentInstallment
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    DueDate = new DateOnly(2026, 11, 10),
                    Amount = 3_000m,
                    IsPaid = false
                }
            ]
        };

        var items = _calculator.GetItems([plan]);

        Assert.Equal(2, items.Count);
        Assert.False(items[0].IsFinalPayment);
        Assert.True(items[1].IsFinalPayment);
    }
}
