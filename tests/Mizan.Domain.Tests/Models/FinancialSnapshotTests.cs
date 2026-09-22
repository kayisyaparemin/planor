using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class FinancialSnapshotTests
{
    [Theory]
    [InlineData(FinancialSnapshotSource.Initial, true)]
    [InlineData(FinancialSnapshotSource.MonthlyUpdate, false)]
    [InlineData(FinancialSnapshotSource.Recovery, false)]
    public void IsInitial_SourceInitialOldugundaTrue_DigerlerindeFalse(
        FinancialSnapshotSource source,
        bool expectedResult)
    {
        var snapshot = new FinancialSnapshot
        {
            Source = source
        };

        Assert.Equal(expectedResult, snapshot.IsInitial);
    }

    [Fact]
    public void Anchor_VarsayilanDegeri1OlanGecerliAnchorDondurur()
    {
        var snapshot = new FinancialSnapshot();

        Assert.NotNull(snapshot.Anchor);
        Assert.Equal(1, snapshot.Anchor.DayOfMonth);
    }

    [Fact]
    public void VarsayilanAlanlar_GecerliDegerlerleBaslatilir()
    {
        var snapshot = new FinancialSnapshot();

        Assert.NotEqual(Guid.Empty, snapshot.Id);
        Assert.NotNull(snapshot.Note);
        Assert.Empty(snapshot.Note);
    }
}
