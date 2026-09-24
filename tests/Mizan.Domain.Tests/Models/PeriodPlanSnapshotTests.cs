using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodPlanSnapshotTests
{
    private static readonly DateOnly PeriodStart = new(2026, 10, 15);
    private static readonly DateOnly PeriodEnd = new(2026, 11, 15);

    [Fact]
    public void PlannedInterest_KartVeAcikFaizleriniToplar()
    {
        var plan = new PeriodPlanSnapshot
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            PlannedCardInterest = 1_200.50m,
            PlannedDeficitInterest = 349.50m
        };

        Assert.Equal(1_550m, plan.PlannedInterest);
    }

    [Theory]
    [InlineData(10000, 15000, 5000)]
    [InlineData(20000, 12000, -8000)]
    [InlineData(5000, -3000, -8000)]
    [InlineData(0, 0, 0)]
    public void PlannedNetChange_KapanisVeAcilisFarkiniDondurur(
        decimal openingBalance,
        decimal plannedEndingBalance,
        decimal expectedChange)
    {
        var plan = new PeriodPlanSnapshot
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            OpeningBalance = openingBalance,
            PlannedEndingBalance = plannedEndingBalance
        };

        Assert.Equal(expectedChange, plan.PlannedNetChange);
    }

    [Fact]
    public void TotalPlannedOutflows_TumGiderleriVeFaizleriToplar()
    {
        var plan = new PeriodPlanSnapshot
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            PlannedMandatoryPayments = 15_000m,
            PlannedVariableExpenseAllowance = 8_000m,
            PlannedLargeExpenses = 5_000m,
            PlannedCardInterest = 400m,
            PlannedDeficitInterest = 100m
        };

        // 15000 + 8000 + 5000 + 400 + 100 = 28500
        Assert.Equal(28_500m, plan.TotalPlannedOutflows);
    }

    [Theory]
    [InlineData(-500, true)]
    [InlineData(-0.01, true)]
    [InlineData(0, false)]
    [InlineData(1500, false)]
    public void HasDeficit_KapanisNegatifseTrue_SifirVeyaPozitifseFalse(
        decimal plannedEndingBalance,
        bool expectedHasDeficit)
    {
        var plan = new PeriodPlanSnapshot
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            PlannedEndingBalance = plannedEndingBalance
        };

        Assert.Equal(expectedHasDeficit, plan.HasDeficit);
    }

    [Theory]
    [InlineData(2026, 10, 14, false)] // Başlangıç öncesi
    [InlineData(2026, 10, 15, true)]  // Başlangıç günü (dahil)
    [InlineData(2026, 10, 25, true)]  // Dönem içi
    [InlineData(2026, 11, 14, true)]  // Bitiş öncesi son gün
    [InlineData(2026, 11, 15, false)] // Bitiş günü (hariç)
    [InlineData(2026, 11, 16, false)] // Dönem sonrası
    public void ContainsDate_YariAcikAralikKuraliniUygular(
        int year,
        int month,
        int day,
        bool expectedResult)
    {
        var plan = new PeriodPlanSnapshot
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd
        };

        var date = new DateOnly(year, month, day);
        Assert.Equal(expectedResult, plan.ContainsDate(date));
    }

    [Fact]
    public void PaymentLines_VarsayilanOlarakBosListeyleBaslatilir()
    {
        var plan = new PeriodPlanSnapshot
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd
        };

        Assert.NotNull(plan.PaymentLines);
        Assert.Empty(plan.PaymentLines);
    }

    [Fact]
    public void IncomeLines_VarsayilanOlarakBosListeyleBaslatilir()
    {
        var plan = new PeriodPlanSnapshot
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd
        };

        Assert.NotNull(plan.IncomeLines);
        Assert.Empty(plan.IncomeLines);
    }
}
