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
        Assert.Equal(0m, observation.ObservedBalance);
        Assert.Equal(default, observation.RecordedAtUtc);
    }
}
