using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class TemporaryPaymentInstallmentTests
{
    [Fact]
    public void Yapici_GecerliDegerlerle_OzellikleriDogruAtar()
    {
        var id = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 10, 15);
        const decimal amount = 12_500.75m;

        var installment = new TemporaryPaymentInstallment
        {
            Id = id,
            PlanId = planId,
            DueDate = dueDate,
            Amount = amount,
            IsPaid = true
        };

        Assert.Equal(id, installment.Id);
        Assert.Equal(planId, installment.PlanId);
        Assert.Equal(dueDate, installment.DueDate);
        Assert.Equal(amount, installment.Amount);
        Assert.True(installment.IsPaid);
    }

    [Fact]
    public void Varsayilanlar_IsPaidFalseVeYeniIdAtar()
    {
        var installment = new TemporaryPaymentInstallment();

        Assert.NotEqual(Guid.Empty, installment.Id);
        Assert.False(installment.IsPaid);
        Assert.Equal(0m, installment.Amount);
        Assert.Equal(default, installment.DueDate);
        Assert.Equal(Guid.Empty, installment.PlanId);
    }

    [Fact]
    public void RecordTipi_DegerEsitliginiDestekler()
    {
        var id = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 11, 20);

        var first = new TemporaryPaymentInstallment
        {
            Id = id,
            PlanId = planId,
            DueDate = dueDate,
            Amount = 5_000m,
            IsPaid = false
        };

        var second = new TemporaryPaymentInstallment
        {
            Id = id,
            PlanId = planId,
            DueDate = dueDate,
            Amount = 5_000m,
            IsPaid = false
        };

        Assert.Equal(first, second);
    }
}
