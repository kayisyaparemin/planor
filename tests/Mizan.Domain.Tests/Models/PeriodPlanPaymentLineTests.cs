using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodPlanPaymentLineTests
{
    [Fact]
    public void VarsayilanAlanlar_GecerliDegerlerleBaslatilir()
    {
        var line = new PeriodPlanPaymentLine();

        Assert.NotEqual(Guid.Empty, line.Id);
        Assert.NotNull(line.Name);
        Assert.Empty(line.Name);
        Assert.NotNull(line.Detail);
        Assert.Empty(line.Detail);
        Assert.Null(line.PlannedAmount);
        Assert.False(line.IsEstimate);
    }

    [Fact]
    public void Alanlar_DogruAtanirVeKorunur()
    {
        var snapshotId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var date = new DateOnly(2026, 10, 20);

        var line = new PeriodPlanPaymentLine
        {
            PeriodPlanSnapshotId = snapshotId,
            SourceEntityId = entityId,
            SourceType = PlanPaymentSourceType.CreditCard,
            Name = "Akbank Axess",
            PlannedDate = date,
            PlannedAmount = 7_500m,
            IsEstimate = true,
            Detail = "Tahmini asgari tutar"
        };

        Assert.Equal(snapshotId, line.PeriodPlanSnapshotId);
        Assert.Equal(entityId, line.SourceEntityId);
        Assert.Equal(PlanPaymentSourceType.CreditCard, line.SourceType);
        Assert.Equal("Akbank Axess", line.Name);
        Assert.Equal(date, line.PlannedDate);
        Assert.Equal(7_500m, line.PlannedAmount);
        Assert.True(line.IsEstimate);
        Assert.Equal("Tahmini asgari tutar", line.Detail);
    }
}
