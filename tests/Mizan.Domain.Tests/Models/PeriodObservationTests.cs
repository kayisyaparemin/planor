using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodObservationTests
{
    [Fact]
    public void PeriodObservation_VarsayilanDegerler_BeklenenSekildedir()
    {
        // Act
        var observation = new PeriodObservation();

        // Assert
        Assert.NotEqual(Guid.Empty, observation.Id);
        Assert.Equal(Guid.Empty, observation.PeriodPlanSnapshotId);
        Assert.Equal(default, observation.ObservedOn);
        Assert.Null(observation.ObservedBalance);
        Assert.False(observation.HasObservedBalance);
        Assert.Equal(0m, observation.ObservedLivingSpend);
        Assert.Equal(string.Empty, observation.Note);
    }

    [Fact]
    public void PeriodObservation_HasObservedBalance_BakiyeGirilipGirilmediginiDogrular()
    {
        // Arrange & Act
        var emptyObservation = new PeriodObservation { ObservedBalance = null };
        var zeroObservation = new PeriodObservation { ObservedBalance = 0m };
        var negativeObservation = new PeriodObservation { ObservedBalance = -107_150m };
        var positiveObservation = new PeriodObservation { ObservedBalance = 25_000m };

        // Assert
        Assert.False(emptyObservation.HasObservedBalance);
        Assert.True(zeroObservation.HasObservedBalance);
        Assert.True(negativeObservation.HasObservedBalance);
        Assert.True(positiveObservation.HasObservedBalance);
    }

}
