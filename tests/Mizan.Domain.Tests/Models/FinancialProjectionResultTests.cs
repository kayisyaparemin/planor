using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class FinancialProjectionResultTests
{
    private static readonly CashFlowPeriod Period1 = new(
        new DateOnly(2026, 9, 15),
        new DateOnly(2026, 10, 15));

    private static readonly CashFlowPeriod Period2 = new(
        new DateOnly(2026, 10, 15),
        new DateOnly(2026, 11, 15));

    [Fact]
    public void FaizKumulatifleri_DonemlerdekiFaizleriDogruToplar()
    {
        var periods = new List<CashFlowPeriodProjection>
        {
            new()
            {
                Period = Period1,
                CardInterestGenerated = 400m,
                DeficitFinancingInterest = 150m
            },
            new()
            {
                Period = Period2,
                CardInterestGenerated = 600m,
                DeficitFinancingInterest = 250m
            }
        };

        var plan = new PeriodObligationPlan([], [], []);
        var result = new FinancialProjectionResult(periods, plan, []);

        Assert.Equal(1_000m, result.TotalCreditCardInterest);
        Assert.Equal(400m, result.TotalDeficitFinancingInterest);
        Assert.Equal(1_400m, result.TotalInterestCost);
    }

    [Fact]
    public void ProjectionInterestSummary_From_DonemDizisindenFaizOzetiniUretir()
    {
        var periods = new List<CashFlowPeriodProjection>
        {
            new()
            {
                Period = Period1,
                CardInterestGenerated = 300m,
                DeficitFinancingInterest = 100m
            },
            new()
            {
                Period = Period2,
                CardInterestGenerated = 700m,
                DeficitFinancingInterest = 200m
            }
        };

        var summary = ProjectionInterestSummary.From(periods);

        Assert.Equal(1_000m, summary.CreditCardInterest);
        Assert.Equal(300m, summary.DeficitFinancingInterest);
        Assert.Equal(1_300m, summary.TotalInterestCost);
    }

    [Fact]
    public void ProjectionInterestSummary_From_NullGecilirseHataFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ProjectionInterestSummary.From(null!));
    }
}
