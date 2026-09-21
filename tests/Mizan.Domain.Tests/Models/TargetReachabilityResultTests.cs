using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class TargetReachabilityResultTests
{
    [Fact]
    public void IsReached_ZatenUlasilmissa_TrueDondurur()
    {
        var result = new TargetReachabilityResult(true, null);

        Assert.True(result.IsAlreadyReached);
        Assert.Null(result.FirstReachedPeriod);
        Assert.True(result.IsReached);
    }

    [Fact]
    public void IsReached_IlkUlasilanDonemVarsa_TrueDondurur()
    {
        var period = new CashFlowPeriodProjection
        {
            Period = new CashFlowPeriod(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10))
        };
        var result = new TargetReachabilityResult(false, period);

        Assert.False(result.IsAlreadyReached);
        Assert.NotNull(result.FirstReachedPeriod);
        Assert.True(result.IsReached);
    }

    [Fact]
    public void IsReached_UlasilmamisVeDonemYoksa_FalseDondurur()
    {
        var result = new TargetReachabilityResult(false, null);

        Assert.False(result.IsAlreadyReached);
        Assert.Null(result.FirstReachedPeriod);
        Assert.False(result.IsReached);
    }
}
