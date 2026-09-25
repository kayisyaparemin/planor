using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Presenters;

namespace Mizan.Presentation.Tests;

public sealed class SimulatorInsightServiceTests
{
    private readonly SimulatorInsightService _service = new();

    [Fact]
    public void InterestComparison_KeepsCardAndDeficitInterestApart()
    {
        var rows = SimulatorInsightService.BuildInterestComparison(
            new ProjectionInterestSummary(7_081m, 6_236m),
            new ProjectionInterestSummary(941m, 10_858m));

        Assert.Equal(3, rows.Count);

        var card = rows[0];
        Assert.Equal(-6_140m, card.DifferenceAmount);
        Assert.True(card.IsSaving);
        Assert.False(card.IsExtra);
        Assert.False(card.IsTotal);

        var deficit = rows[1];
        Assert.Equal(4_622m, deficit.DifferenceAmount);
        Assert.True(deficit.IsExtra);
        Assert.StartsWith("+", deficit.Difference);

        var total = rows[2];
        Assert.True(total.IsTotal);
        Assert.Equal(-1_518m, total.DifferenceAmount);
        Assert.True(total.IsSaving);
        Assert.Contains("→", total.Transition);
    }

    [Fact]
    public void InterestComparison_CountsFinancingCostAsInterest()
    {
        var rows = SimulatorInsightService.BuildInterestComparison(
            new ProjectionInterestSummary(7_081m, 6_236m),
            new ProjectionInterestSummary(7_081m, 0m),
            scenarioFinancingCost: 30_000m);

        Assert.Equal(4, rows.Count);

        var financing = rows[2];
        Assert.Equal("Kredi finansman maliyeti", financing.Label);
        Assert.Equal(30_000m, financing.DifferenceAmount);
        Assert.True(financing.IsExtra);

        var total = rows[3];
        Assert.True(total.IsTotal);
        Assert.Equal(23_764m, total.DifferenceAmount);
        Assert.True(total.IsExtra);
        Assert.Contains("37.081,00", total.Transition);
    }

    [Fact]
    public void InterestComparison_OmitsFinancingRowWhenThereIsNoLoan()
    {
        var rows = SimulatorInsightService.BuildInterestComparison(
            new ProjectionInterestSummary(7_081m, 6_236m),
            new ProjectionInterestSummary(941m, 10_858m));

        Assert.Equal(3, rows.Count);
        Assert.DoesNotContain(rows, x => x.Label == "Kredi finansman maliyeti");
    }

    [Fact]
    public void InterestComparison_SaysUnchangedInsteadOfZero()
    {
        var rows = SimulatorInsightService.BuildInterestComparison(
            new ProjectionInterestSummary(7_324.30m, 0m),
            new ProjectionInterestSummary(7_324.30m, 0m));

        Assert.All(rows, row =>
        {
            Assert.Equal(0m, row.DifferenceAmount);
            Assert.Equal("Değişmiyor", row.Difference);
            Assert.False(row.IsSaving);
            Assert.False(row.IsExtra);
        });
        Assert.Equal("7.324,30 TL → 7.324,30 TL", rows[0].Transition);
    }

    [Fact]
    public void PeriodNeed_ComposesCashRequirementsWithoutOpeningState()
    {
        var row = Row(
            new DateOnly(2027, 3, 10),
            opening: 50_000m,
            income: 100_000m,
            loan: 10_000m,
            creditCard: 20_000m,
            temporary: 5_000m,
            installment: 7_000m,
            other: 3_000m,
            living: 30_000m,
            large: 15_000m,
            deficitInterest: 1_000m,
            ending: 59_000m);

        var view = _service.Build([row]).Periods[0];

        Assert.Equal(91_000m, view.NeedTotal);
        Assert.Equal(9_000m, view.IncomeCoverage);
        Assert.Equal("Gelirlerden kalan", view.CoverageLabel);
        Assert.DoesNotContain(view.NeedBreakdown, x => x.Label.Contains("Dönem başı", StringComparison.Ordinal));
        Assert.Equal(91_000m, view.NeedBreakdown.Single(x => x.Label == "Toplam").Amount);
    }

    [Fact]
    public void IncomeInsufficient_DoesNotBecomeFinancingDeficitWhenOpeningCoversGap()
    {
        var row = Row(
            new DateOnly(2027, 3, 10),
            opening: 50_000m,
            income: 100_000m,
            mandatory: 90_000m,
            living: 30_000m,
            ending: 30_000m);

        var summary = _service.Build([row]);
        var view = summary.Periods[0];

        Assert.Equal(120_000m, view.NeedTotal);
        Assert.Equal(-20_000m, view.IncomeCoverage);
        Assert.Equal("Gelirlerin karşılamadığı", view.CoverageLabel);
        Assert.NotNull(summary.FirstIncomeInsufficientPeriod);
        Assert.Null(summary.FirstDeficitPeriod);
        Assert.Contains(summary.NarrativeInsights, x => x.Contains("dönem başı durumundan", StringComparison.Ordinal));
        Assert.DoesNotContain(summary.NarrativeInsights, x =>
            x.Contains("Mevcut", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("Yeni Plan", StringComparison.OrdinalIgnoreCase) ||
            x.Contains("fark", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FinancingDeficit_ReportsRecoveryInsideHorizon()
    {
        var september = Row(
            new DateOnly(2026, 9, 10),
            opening: 0m,
            income: 50_000m,
            mandatory: 40_000m,
            living: 35_000m,
            deficitInterest: 1_250m,
            ending: -26_250m);
        var october = Row(
            new DateOnly(2026, 10, 10),
            opening: -26_250m,
            income: 70_000m,
            mandatory: 20_000m,
            living: 40_000m,
            ending: -16_250m);
        var november = Row(
            new DateOnly(2026, 11, 10),
            opening: -16_250m,
            income: 80_000m,
            mandatory: 10_000m,
            living: 30_000m,
            ending: 23_750m);

        var summary = _service.Build([september, october, november]);

        Assert.Equal(september.PeriodStart, summary.FirstDeficitPeriod?.Projection.PeriodStart);
        Assert.Equal(november.PeriodStart, summary.DeficitRecoveryPeriod?.Projection.PeriodStart);
        Assert.Contains(summary.NarrativeInsights, x => x.Contains("finansman açığı oluşuyor", StringComparison.Ordinal));
        Assert.Contains(summary.NarrativeInsights, x => x.Contains("kapanması bekleniyor", StringComparison.Ordinal));
    }

    [Fact]
    public void HighestNeedLowestEndingAndRecovery_AreDeterministicAndDeduplicated()
    {
        var january = Row(
            new DateOnly(2027, 1, 10),
            opening: 70_000m,
            income: 100_000m,
            mandatory: 40_000m,
            living: 30_000m,
            ending: 100_000m);
        var february = Row(
            new DateOnly(2027, 2, 10),
            opening: 100_000m,
            income: 100_000m,
            mandatory: 95_000m,
            living: 45_000m,
            ending: 60_000m);
        var march = Row(
            new DateOnly(2027, 3, 10),
            opening: 60_000m,
            income: 100_000m,
            mandatory: 40_000m,
            living: 30_000m,
            ending: 90_000m);
        var april = Row(
            new DateOnly(2027, 4, 10),
            opening: 90_000m,
            income: 100_000m,
            mandatory: 30_000m,
            living: 30_000m,
            ending: 130_000m);

        var summary = _service.Build([january, february, march, april]);

        Assert.Equal(february.PeriodStart, summary.HighestNeedPeriod.Projection.PeriodStart);
        Assert.Equal(february.PeriodStart, summary.LowestEndingPeriod.Projection.PeriodStart);
        Assert.Equal(march.PeriodStart, summary.BurdenReliefPeriod?.Projection.PeriodStart);
        Assert.All(summary.Periods, period => Assert.True(period.InsightChips.Count <= 2));
    }

    [Fact]
    public void RepresentativeTimelineStory_DoesNotNarrateScenarioInputs()
    {
        var rows = new List<CashFlowPeriodProjection>();
        for (var i = 1; i <= 12; i++)
        {
            var date = new DateOnly(2026, 8, 20).AddMonths(i - 1);
            rows.Add(Row(
                date,
                opening: 50_000m,
                income: 80_000m,
                mandatory: 35_000m,
                living: 25_000m,
                ending: 70_000m));
        }

        var summary = _service.Build(rows);
        var text = string.Join(" ", summary.NarrativeInsights);

        Assert.DoesNotContain("ekledin", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("taksit ek", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mevcut Plan", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Yeni Plan", text, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(summary.NarrativeInsights);
        Assert.NotEmpty(summary.Periods);
    }

    [Fact]
    public void Build_ThrowsWhenScenarioIsEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => _service.Build([]));
    }

    private static CashFlowPeriodProjection Row(
        DateOnly start,
        decimal opening,
        decimal income,
        decimal mandatory = 0m,
        decimal loan = 0m,
        decimal creditCard = 0m,
        decimal temporary = 0m,
        decimal installment = 0m,
        decimal other = 0m,
        decimal living = 0m,
        decimal large = 0m,
        decimal deficitInterest = 0m,
        decimal ending = 0m)
    {
        var mandatoryTotal = mandatory == 0m
            ? loan + creditCard + temporary + installment + other
            : mandatory;
        var end = start.AddMonths(1);
        return new CashFlowPeriodProjection
        {
            Period = new CashFlowPeriod(start, end),
            OpeningBalance = opening,
            TotalIncome = income,
            RecurringIncomeTotal = income,
            AdHocIncomeTotal = 0m,
            MandatoryOutflow = mandatoryTotal,
            LoanPayments = loan,
            CreditCardPayments = creditCard,
            TemporaryPayments = temporary,
            InstallmentPayments = installment,
            OtherScheduledPayments = other,
            VariableExpenseAllowance = living,
            PlannedLargeCashExpenses = large,
            DeficitFinancingInterest = deficitInterest,
            EndingBalanceBeforeDeficitInterest = ending + deficitInterest,
            EndingBalance = ending,
            HasDeficit = ending < 0m,
            IncomeItems = [],
            MandatoryItems = [],
            LargeExpenseItems = [],
            CardPaymentStatuses = []
        };
    }
}
