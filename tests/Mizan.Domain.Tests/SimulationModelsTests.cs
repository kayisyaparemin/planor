using Mizan.Domain.Models;

namespace Mizan.Domain.Tests;

public sealed class SimulationModelsTests
{
    [Fact]
    public void SimulationImpactRow_CalculatesDifferencesCorrectly()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1));
        var baseline = new CashFlowPeriodProjection
        {
            Period = period,
            MandatoryOutflow = 20_000m,
            EstimatedSurplus = 15_000m,
            EndingBalance = 40_000m,
            CardInterestGenerated = 500m,
            DeficitFinancingInterest = 0m
        };
        var scenario = new CashFlowPeriodProjection
        {
            Period = period,
            MandatoryOutflow = 26_000m,
            EstimatedSurplus = 9_000m,
            EndingBalance = 33_500m,
            CardInterestGenerated = 500m,
            DeficitFinancingInterest = 300m
        };

        var row = new SimulationImpactRow(baseline, scenario);

        Assert.Equal(period, row.Period);
        Assert.Equal(new DateOnly(2026, 9, 1), row.PeriodStart);
        Assert.Equal(6_000m, row.MandatoryOutflowDifference);
        Assert.Equal(-6_000m, row.SurplusDifference);
        Assert.Equal(-6_500m, row.ProjectedBalanceDifference);
        Assert.Equal(300m, row.InterestDifference);
    }

    [Fact]
    public void SimulationResult_CalculatesAdditionalInterestCost_WhenScenarioGeneratesMoreInterest()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1));
        var baseline = new[]
        {
            new CashFlowPeriodProjection
            {
                Period = period,
                CardInterestGenerated = 1_000m,
                DeficitFinancingInterest = 200m
            }
        };
        var scenario = new[]
        {
            new CashFlowPeriodProjection
            {
                Period = period,
                CardInterestGenerated = 1_000m,
                DeficitFinancingInterest = 700m
            }
        };
        var rows = new[] { new SimulationImpactRow(baseline[0], scenario[0]) };
        var risk = new SimulationRiskSummary
        {
            LowestPeriod = period,
            EndingProjectedBalance = 10_000m
        };

        var result = new SimulationResult(baseline, scenario, rows, risk);

        Assert.Equal(1_200m, result.BaselineInterest.TotalInterestCost);
        Assert.Equal(1_700m, result.ScenarioInterest.TotalInterestCost);
        Assert.Equal(500m, result.AdditionalInterestCost);
        Assert.Equal(0m, result.InterestSaving);
    }

    [Fact]
    public void SimulationResult_CalculatesInterestSaving_WhenScenarioReducesInterest()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1));
        var baseline = new[]
        {
            new CashFlowPeriodProjection
            {
                Period = period,
                CardInterestGenerated = 3_000m,
                DeficitFinancingInterest = 0m
            }
        };
        var scenario = new[]
        {
            new CashFlowPeriodProjection
            {
                Period = period,
                CardInterestGenerated = 1_200m,
                DeficitFinancingInterest = 0m
            }
        };
        var rows = new[] { new SimulationImpactRow(baseline[0], scenario[0]) };
        var risk = new SimulationRiskSummary
        {
            LowestPeriod = period,
            EndingProjectedBalance = 50_000m
        };

        var result = new SimulationResult(baseline, scenario, rows, risk);

        Assert.Equal(3_000m, result.BaselineInterest.TotalInterestCost);
        Assert.Equal(1_200m, result.ScenarioInterest.TotalInterestCost);
        Assert.Equal(-1_800m, result.AdditionalInterestCost);
        Assert.Equal(1_800m, result.InterestSaving);
    }

    [Fact]
    public void SimulationRiskSummary_FirstDeficitPeriod_AliasesFirstNegativeProjectedBalancePeriod()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1));
        var risk = new SimulationRiskSummary
        {
            LowestPeriod = period,
            FirstNegativeProjectedBalancePeriod = period,
            FirstNegativeSurplusPeriod = period,
            EndingProjectedBalance = -5_000m,
            MaximumCarryOverDeficit = 5_000m
        };

        Assert.Equal(period, risk.FirstDeficitPeriod);
        Assert.Equal(period, risk.FirstNegativeProjectedBalancePeriod);
    }

    [Fact]
    public void LoanPrepaymentImpact_StoresMetricsAccurately()
    {
        var loanId = Guid.NewGuid();
        var impact = new LoanPrepaymentImpact
        {
            LoanId = loanId,
            LoanName = "İhtiyaç Kredisi",
            PrepaidAmount = 50_000m,
            InterestSaving = 18_400m,
            BaselineEndDate = new DateOnly(2028, 6, 15),
            ScenarioEndDate = new DateOnly(2027, 10, 15),
            BaselineMonthlyPayment = 8_200m,
            ScenarioMonthlyPayment = null
        };

        Assert.Equal(loanId, impact.LoanId);
        Assert.Equal("İhtiyaç Kredisi", impact.LoanName);
        Assert.Equal(50_000m, impact.PrepaidAmount);
        Assert.Equal(18_400m, impact.InterestSaving);
        Assert.Equal(new DateOnly(2028, 6, 15), impact.BaselineEndDate);
        Assert.Equal(new DateOnly(2027, 10, 15), impact.ScenarioEndDate);
        Assert.Equal(8_200m, impact.BaselineMonthlyPayment);
        Assert.Null(impact.ScenarioMonthlyPayment);
    }
}
