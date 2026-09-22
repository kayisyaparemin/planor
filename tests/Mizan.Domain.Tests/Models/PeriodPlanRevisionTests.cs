using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodPlanRevisionTests
{
    [Fact]
    public void PlannedInterest_KartVeAcikFaizleriniToplar()
    {
        var revision = new PeriodPlanRevision
        {
            PlannedCardInterest = 850m,
            PlannedDeficitInterest = 150m
        };

        Assert.Equal(1_000m, revision.PlannedInterest);
    }

    [Theory]
    [InlineData(-100, true)]
    [InlineData(0, false)]
    [InlineData(2500, false)]
    public void HasDeficit_KapanisNegatifseTrue_PozitifseFalse(
        decimal plannedEndingBalance,
        bool expectedHasDeficit)
    {
        var revision = new PeriodPlanRevision
        {
            PlannedEndingBalance = plannedEndingBalance
        };

        Assert.Equal(expectedHasDeficit, revision.HasDeficit);
    }

    [Fact]
    public void PaymentLines_VarsayilanOlarakBosListeyleBaslatilir()
    {
        var revision = new PeriodPlanRevision();

        Assert.NotNull(revision.PaymentLines);
        Assert.Empty(revision.PaymentLines);
    }
}
