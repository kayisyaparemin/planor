using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class TemporaryPaymentPlanTests
{
    [Fact]
    public void TotalInstallmentAmount_TumTaksitlerinTutariniToplar()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Eminevim",
            Installments =
            [
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 9, 20), Amount = 28_167.40m, IsPaid = true },
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 10, 20), Amount = 28_167.40m, IsPaid = false },
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 11, 20), Amount = 55_492.20m, IsPaid = false }
            ]
        };

        Assert.Equal(111_827.00m, plan.TotalInstallmentAmount);
    }

    [Fact]
    public void RemainingAmount_YalnizcaOdenmemisTaksitleriToplar()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Eminevim",
            Installments =
            [
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 9, 20), Amount = 28_167.40m, IsPaid = true },
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 10, 20), Amount = 28_167.40m, IsPaid = false },
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 11, 20), Amount = 55_492.20m, IsPaid = false }
            ]
        };

        Assert.Equal(83_659.60m, plan.RemainingAmount);
    }

    [Fact]
    public void RemainingAmount_TumTaksitlerOdenmisse_SifirDondurur()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Senet",
            Installments =
            [
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 9, 15), Amount = 10_000m, IsPaid = true },
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 10, 15), Amount = 10_000m, IsPaid = true }
            ]
        };

        Assert.Equal(0m, plan.RemainingAmount);
    }

    [Fact]
    public void RemainingInstallmentCount_YalnizcaOdenmemisTaksitSayisiniDondurur()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Kurs Taksiti",
            Installments =
            [
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 9, 1), Amount = 5_000m, IsPaid = true },
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 10, 1), Amount = 5_000m, IsPaid = false },
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 11, 1), Amount = 5_000m, IsPaid = false }
            ]
        };

        Assert.Equal(2, plan.RemainingInstallmentCount);
    }

    [Fact]
    public void IsCompleted_EnAzBirTaksitVarVeTumuOdenmisse_TrueDondurur()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Kapanmis Plan",
            Installments =
            [
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 8, 1), Amount = 3_000m, IsPaid = true }
            ]
        };

        Assert.True(plan.IsCompleted);
    }

    [Fact]
    public void IsCompleted_OdenmemisTaksitVarsaVeyaTaksitYoksa_FalseDondurur()
    {
        var planWithUnpaid = new TemporaryPaymentPlan
        {
            Installments = [new TemporaryPaymentInstallment { Amount = 1_000m, IsPaid = false }]
        };
        var planWithoutInstallments = new TemporaryPaymentPlan { Installments = [] };

        Assert.False(planWithUnpaid.IsCompleted);
        Assert.False(planWithoutInstallments.IsCompleted);
    }

    [Fact]
    public void NextInstallment_OdenmemisTaksitlerArasindanVadesiEnYakinOlaniDondurur()
    {
        var planId = Guid.NewGuid();
        var second = new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 10, 15), Amount = 4_000m, IsPaid = false };
        var third = new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 11, 15), Amount = 4_000m, IsPaid = false };
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Installments =
            [
                new TemporaryPaymentInstallment { PlanId = planId, DueDate = new DateOnly(2026, 9, 15), Amount = 4_000m, IsPaid = true },
                third,
                second
            ]
        };

        Assert.NotNull(plan.NextInstallment);
        Assert.Equal(new DateOnly(2026, 10, 15), plan.NextInstallment.DueDate);
        Assert.Equal(second.Id, plan.NextInstallment.Id);
    }

    [Fact]
    public void NextInstallment_TumTaksitlerOdenmisse_NullDondurur()
    {
        var plan = new TemporaryPaymentPlan
        {
            Installments = [new TemporaryPaymentInstallment { Amount = 2_000m, IsPaid = true }]
        };

        Assert.Null(plan.NextInstallment);
    }

    [Fact]
    public void Normalize_KarisikTarihliTaksitleri_KronolojikSiralarVePlanIdEsler()
    {
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Senet",
            Installments =
            [
                new TemporaryPaymentInstallment { PlanId = Guid.Empty, DueDate = new DateOnly(2026, 12, 1), Amount = 5_000m },
                new TemporaryPaymentInstallment { PlanId = Guid.Empty, DueDate = new DateOnly(2026, 10, 1), Amount = 5_000m },
                new TemporaryPaymentInstallment { PlanId = Guid.Empty, DueDate = new DateOnly(2026, 11, 1), Amount = 5_000m }
            ]
        };

        var normalized = plan.Normalize();

        Assert.Equal(3, normalized.Installments.Count);
        Assert.Equal(new DateOnly(2026, 10, 1), normalized.Installments[0].DueDate);
        Assert.Equal(new DateOnly(2026, 11, 1), normalized.Installments[1].DueDate);
        Assert.Equal(new DateOnly(2026, 12, 1), normalized.Installments[2].DueDate);
        Assert.All(normalized.Installments, x => Assert.Equal(planId, x.PlanId));
    }
}
