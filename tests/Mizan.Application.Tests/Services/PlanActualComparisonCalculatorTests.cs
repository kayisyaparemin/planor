using System.Globalization;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PlanActualComparisonCalculatorTests
{
    private readonly PlanActualComparisonCalculator _calculator = new();

    [Fact]
    public void Calculate_Revizyonsuz_OrijinalPlanaGore12KategoriyiKarsilastirir()
    {
        var plan = CreateSamplePlan();
        var actual = CreateSampleActual();

        var comparison = _calculator.Calculate(plan, null, actual);

        Assert.Equal(12, comparison.Lines.Count);
        AssertLine(comparison, "Gelir", 102_500m, 105_500m, 3_000m);
        AssertLine(comparison, "Krediler", 10_000m, 12_000m, 2_000m);
        AssertLine(comparison, "Kredi kartları", 4_000m, 4_000m, 0m);
        AssertLine(comparison, "Geçici ödemeler", 5_000m, 0m, -5_000m);
        AssertLine(comparison, "Taksitli ödemeler", 1_000m, 1_000m, 0m);
        AssertLine(comparison, "Diğer planlı ödemeler", 500m, 750m, 250m);
        AssertLine(comparison, "Zorunlu ödemeler", 20_500m, 17_750m, -2_750m);
        AssertLine(comparison, "Büyük ödemeler", 7_000m, 7_000m, 0m);
        AssertLine(comparison, "Yaşam giderleri", 20_322.58m, 24_500m, 4_177.42m);
        AssertLine(comparison, "Faiz", 300.25m, 450m, 149.75m);
        AssertLine(comparison, "Plan dışı ödemeler", 0m, 1_200m, 1_200m);
        AssertLine(comparison, "Dönem düzeltmesi", 0m, 850m, 850m);

        Assert.Equal(104_877.17m, comparison.PlannedEndingBalance);
        Assert.Equal(111_000m, comparison.ActualEndingBalance);
        Assert.Equal(6_122.83m, comparison.Difference);
        Assert.Equal(
            "Dönem sonu finansal durumun planın 6.122,83 TL üzerinde gerçekleşti. " +
            "En belirgin fark Geçici ödemeler kaleminde 5.000,00 TL oldu.",
            comparison.Summary);
    }

    [Fact]
    public void Calculate_Revizyonlu_RevizePlanDegerleriniKullanir()
    {
        var plan = CreateSamplePlan();
        var revision = new PeriodPlanRevision
        {
            PeriodPlanSnapshotId = plan.Id,
            RevisionNumber = 1,
            PlannedIncome = 110_000m,
            PlannedLoanPayments = 11_000m,
            PlannedCardPayments = 4_500m,
            PlannedTemporaryPayments = 6_000m,
            PlannedInstallmentPayments = 1_500m,
            PlannedOtherScheduledPayments = 600m,
            PlannedMandatoryPayments = 23_600m,
            PlannedLargeExpenses = 8_000m,
            PlannedVariableExpenseAllowance = 21_000m,
            PlannedCardInterest = 100m,
            PlannedDeficitInterest = 50m,
            PlannedEndingBalance = 107_350m
        };

        var comparison = _calculator.Calculate(plan, revision, CreateSampleActual());

        AssertLine(comparison, "Gelir", 110_000m, 105_500m, -4_500m);
        AssertLine(comparison, "Krediler", 11_000m, 12_000m, 1_000m);
        AssertLine(comparison, "Kredi kartları", 4_500m, 4_000m, -500m);
        AssertLine(comparison, "Geçici ödemeler", 6_000m, 0m, -6_000m);
        AssertLine(comparison, "Taksitli ödemeler", 1_500m, 1_000m, -500m);
        AssertLine(comparison, "Diğer planlı ödemeler", 600m, 750m, 150m);
        AssertLine(comparison, "Zorunlu ödemeler", 23_600m, 17_750m, -5_850m);
        AssertLine(comparison, "Büyük ödemeler", 8_000m, 7_000m, -1_000m);
        AssertLine(comparison, "Yaşam giderleri", 21_000m, 24_500m, 3_500m);
        AssertLine(comparison, "Faiz", 150m, 450m, 300m);

        Assert.Equal(107_350m, comparison.PlannedEndingBalance);
        Assert.Equal(3_650m, comparison.Difference);
        Assert.EndsWith(
            "En belirgin fark Geçici ödemeler kaleminde 6.000,00 TL oldu.",
            comparison.Summary);
    }

    [Fact]
    public void Calculate_FarkSifirOldugunda_AyniSeviyedeCumlesiUretir()
    {
        var plan = CreateSamplePlan();
        var actual = CreateSampleActual() with { ConfirmedEndingBalance = plan.PlannedEndingBalance };

        var comparison = _calculator.Calculate(plan, null, actual);

        Assert.Equal(0m, comparison.Difference);
        Assert.Equal("Dönem sonu finansal durumun planla aynı seviyede gerçekleşti.", comparison.Summary);
    }

    [Fact]
    public void Calculate_IngilizceCihazdaBile_TurkceParaFormatiUretir()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var plan = CreateSamplePlan();
            var actual = CreateSampleActual() with { ConfirmedEndingBalance = 103_642.61m };

            var comparison = _calculator.Calculate(plan, null, actual);

            Assert.Equal(-1_234.56m, comparison.Difference);
            Assert.StartsWith("Dönem sonu finansal durumun planın 1.234,56 TL altında gerçekleşti.", comparison.Summary);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Calculate_SebepBulurken_ZorunluToplamVeDonemDuzeltmesiniHaricTutar()
    {
        var plan = new PeriodPlanSnapshot
        {
            PlannedMandatoryPayments = 10_000m,
            PlannedEndingBalance = 1_000m
        };
        var actual = new PeriodActual
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualMandatoryPayments = 90_000m,
            ReconciliationAdjustment = 50_000m,
            ConfirmedEndingBalance = 1_500m
        };

        var comparison = _calculator.Calculate(plan, null, actual);

        Assert.Equal(
            "Dönem sonu finansal durumun planın 500,00 TL üzerinde gerçekleşti.",
            comparison.Summary);
    }

    [Fact]
    public void Calculate_EnBuyukFarkiSebepOlarakSecer_EsitlikteListedekiIlkKalemKazanir()
    {
        var plan = new PeriodPlanSnapshot
        {
            PlannedIncome = 10_000m,
            PlannedVariableExpenseAllowance = 5_000m,
            PlannedEndingBalance = 5_000m
        };
        var actual = new PeriodActual
        {
            PeriodPlanSnapshotId = plan.Id,
            ActualIncome = 8_000m,
            ActualLivingSpend = 7_000m,
            UnplannedPayments = 1_999.99m,
            ConfirmedEndingBalance = 1_000m
        };

        var comparison = _calculator.Calculate(plan, null, actual);

        Assert.EndsWith(
            "En belirgin fark Gelir kaleminde 2.000,00 TL oldu.",
            comparison.Summary);
    }

    [Fact]
    public void Calculate_NullPlanVeyaActualGeldiginde_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(null!, null, new PeriodActual()));
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(new PeriodPlanSnapshot(), null, null!));
    }

    private static PeriodPlanSnapshot CreateSamplePlan() => new()
    {
        PeriodStart = new DateOnly(2026, 8, 20),
        PeriodEnd = new DateOnly(2026, 9, 20),
        SettlementAvailableFrom = new DateOnly(2026, 9, 20),
        PlannedIncome = 102_500m,
        PlannedLoanPayments = 10_000m,
        PlannedCardPayments = 4_000m,
        PlannedTemporaryPayments = 5_000m,
        PlannedInstallmentPayments = 1_000m,
        PlannedOtherScheduledPayments = 500m,
        PlannedMandatoryPayments = 20_500m,
        PlannedLargeExpenses = 7_000m,
        PlannedVariableExpenseAllowance = 20_322.58m,
        PlannedCardInterest = 250.25m,
        PlannedDeficitInterest = 50m,
        PlannedEndingBalance = 104_877.17m
    };

    private static PeriodActual CreateSampleActual() => new()
    {
        ActualIncome = 105_500m,
        ActualLoanPayments = 12_000m,
        ActualCardPayments = 4_000m,
        ActualTemporaryPayments = 0m,
        ActualInstallmentPayments = 1_000m,
        ActualOtherScheduledPayments = 750m,
        ActualMandatoryPayments = 17_750m,
        ActualLargeExpenses = 7_000m,
        ActualLivingSpend = 24_500m,
        ActualInterest = 450m,
        UnplannedIncome = 3_000m,
        UnplannedPayments = 1_200m,
        DerivedEndingBalance = 110_150m,
        ConfirmedEndingBalance = 111_000m,
        ReconciliationAdjustment = 850m
    };

    private static void AssertLine(
        PlanActualComparison comparison,
        string category,
        decimal planned,
        decimal actual,
        decimal difference)
    {
        var line = Assert.Single(comparison.Lines, x => x.Category == category);
        Assert.Equal(planned, line.Planned);
        Assert.Equal(actual, line.Actual);
        Assert.Equal(difference, line.Difference);
    }
}
