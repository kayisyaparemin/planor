using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Presenters;

namespace Mizan.Presentation.Tests;

public sealed class CashFlowPeriodDetailPresenterTests
{
    private readonly CashFlowPeriodDetailPresenter _presenter = new();

    [Fact]
    public void Build_MapsEngineValuesWithoutChangingProjectionResults()
    {
        var row = CreateSampleRow(
            openingBalance: 10_000m,
            totalIncome: 115_000m,
            mandatoryOutflow: 50_000m,
            surplus: 65_000m,
            endingBalance: 75_000m);

        var detail = _presenter.Build(row);

        Assert.Equal(115_000m, detail.IncomeSummary.Amount);
        Assert.Equal("115.000 TL", detail.IncomeSummary.AmountText);
        Assert.Equal(50_000m, detail.MandatorySummary.Amount);
        Assert.Equal(65_000m, detail.NetSurplusSummary.Amount);
        Assert.Equal(75_000m, detail.EndingSummary.Amount);
        Assert.Equal("20 Ağustos 2026 Dönemi", detail.PeriodTitle);
        Assert.Equal("20 Ağustos – 20 Eylül", detail.PeriodWindowText);
    }

    [Fact]
    public void Build_CardRowsCarryTheirCardAndCurrentPaymentMode()
    {
        var cardId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 9, 5);
        var period = new CashFlowPeriod(new DateOnly(2026, 8, 20), new DateOnly(2026, 9, 20));

        var row = CreateSampleRow(
            mandatoryItems:
            [
                new ObligationItem(
                    "Garanti Bonus",
                    ObligationType.CreditCard,
                    dueDate,
                    15_000m)
                {
                    PaymentId = cardId
                }
            ],
            cardStatuses:
            [
                new CreditCardPaymentProjectionStatus
                {
                    CardId = cardId,
                    CardName = "Garanti Bonus",
                    PaymentDueDate = dueDate,
                    Payment = 15_000m,
                    PaymentType = CreditCardPaymentType.Minimum,
                    AssignedPeriodDate = period.Start
                }
            ]);

        var detail = _presenter.Build(row);

        var cardRow = Assert.Single(detail.PaymentRows, x => x.Category == "Kredi Kartı" && x.Amount is not null);
        Assert.Equal(cardId, cardRow.CreditCardId);
        Assert.Equal(CreditCardPaymentType.Minimum, cardRow.CardPaymentType);
        Assert.True(cardRow.IsMinimumSelected);
        Assert.False(cardRow.IsFullStatementSelected);
        Assert.True(cardRow.CanChangeCardPaymentMode);
    }

    [Fact]
    public void Build_SimulationScenario_DoesNotOfferPaymentModeChange()
    {
        var cardId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 9, 5);
        var period = new CashFlowPeriod(new DateOnly(2026, 8, 20), new DateOnly(2026, 9, 20));

        var row = CreateSampleRow(
            mandatoryItems:
            [
                new ObligationItem(
                    "Garanti Bonus",
                    ObligationType.CreditCard,
                    dueDate,
                    15_000m)
                {
                    PaymentId = cardId
                }
            ],
            cardStatuses:
            [
                new CreditCardPaymentProjectionStatus
                {
                    CardId = cardId,
                    CardName = "Garanti Bonus",
                    PaymentDueDate = dueDate,
                    Payment = 15_000m,
                    PaymentType = CreditCardPaymentType.Minimum,
                    AssignedPeriodDate = period.Start
                }
            ]);

        var detail = _presenter.Build(row, isSimulationScenario: true);

        Assert.All(detail.PaymentRows, x => Assert.False(x.CanChangeCardPaymentMode));
    }

    [Fact]
    public void Build_HidesZeroCategoriesAndCreatesIndependentPaymentRows()
    {
        var row = CreateSampleRow(
            loanPayments: 20_000m,
            creditCardPayments: 0m,
            temporaryPayments: 10_000m,
            mandatoryItems:
            [
                new ObligationItem("Konut Kredisi", ObligationType.Loan, new DateOnly(2026, 8, 25), 20_000m),
                new ObligationItem("Mobilya Taksiti", ObligationType.TemporaryPayment, new DateOnly(2026, 9, 1), 10_000m)
            ]);

        var detail = _presenter.Build(row);

        Assert.Collection(
            detail.MandatoryRows,
            item => Assert.Equal("Krediler", item.Label),
            item => Assert.Equal("Geçici Ödeme Planları", item.Label));
        Assert.All(detail.MandatoryRows, item => Assert.NotEqual(0m, item.Amount));
        Assert.Equal(2, detail.PaymentRows.Count);
    }

    [Fact]
    public void Build_ComparisonRowsComputesSemanticDeltasCorrectly()
    {
        var baseline = CreateSampleRow(mandatoryOutflow: 40_000m, surplus: 60_000m, endingBalance: 70_000m);
        var scenario = CreateSampleRow(mandatoryOutflow: 50_000m, surplus: 50_000m, endingBalance: 60_000m);

        var detail = _presenter.Build(scenario, baseline);

        Assert.True(detail.HasComparison);
        Assert.Collection(
            detail.ComparisonRows,
            mandatory =>
            {
                Assert.Equal("Zorunlu", mandatory.Label);
                Assert.Equal(10_000m, mandatory.Difference);
            },
            surplus =>
            {
                Assert.Equal("Dönem neti", surplus.Label);
                Assert.Equal(-10_000m, surplus.Difference);
                Assert.True(surplus.IsUnfavorable);
            },
            interest =>
            {
                Assert.Equal("Faiz yükü", interest.Label);
                Assert.Equal(0m, interest.Difference);
            },
            ending =>
            {
                Assert.Equal("Dönem sonu durumu", ending.Label);
                Assert.Equal(-10_000m, ending.Difference);
                Assert.True(ending.IsUnfavorable);
            });
    }

    [Fact]
    public void Build_ShowsInterestAndCarryDeficitOnlyWhenRelevant()
    {
        var rowWithoutDeficit = CreateSampleRow(
            openingBalance: 10_000m,
            cardInterest: 1_200m,
            endingBalance: 20_000m);
        var rowWithDeficit = CreateSampleRow(
            openingBalance: -8_000m,
            surplus: 10_000m,
            endingBalance: 2_000m);

        var detail1 = _presenter.Build(rowWithoutDeficit);
        var detail2 = _presenter.Build(rowWithDeficit);

        Assert.True(detail1.HasInterestRows);
        Assert.False(detail1.HasDeficit);

        Assert.True(detail2.HasDeficit);
        Assert.Equal(8_000m, detail2.Deficit!.OpeningDeficit);
        Assert.Equal(8_000m, detail2.Deficit.CoveredThisPeriod);
    }

    [Fact]
    public void Build_MismatchedBaselineAndScenario_ThrowsInvalidOperationException()
    {
        var baseline = CreateSampleRow(periodStart: new DateOnly(2026, 8, 20));
        var scenario = CreateSampleRow(periodStart: new DateOnly(2026, 9, 20));

        Assert.Throws<InvalidOperationException>(() => _presenter.Build(scenario, baseline));
    }

    [Fact]
    public void Build_SimulationScenarioMode_SetsScenarioFlagAndBreakdown()
    {
        var row = CreateSampleRow(
            totalIncome: 120_000m,
            mandatoryOutflow: 50_000m,
            surplus: 70_000m,
            endingBalance: 80_000m);

        var detail = _presenter.Build(row, isSimulationScenario: true);

        Assert.True(detail.IsSimulationScenario);
        Assert.Equal("DÖNEM GELİRLERİ", detail.IncomeSummary.Label);
        Assert.Equal(50_000m, detail.PeriodNeedSummary.Amount);
        Assert.Equal(70_000m, detail.IncomeCoverageSummary.Amount);
        Assert.Contains(detail.NeedBreakdownRows, x => x.Label == "Toplam" && x.Amount == 50_000m);
    }

    [Theory]
    [InlineData(100_000, 80_000, 50_000, "Dönem gelirlerinden 20.000,00 TL kalıyor.")]
    [InlineData(80_000, 80_000, 50_000, "Dönem gelirleri bu ayki toplam ihtiyacı tam karşılıyor.")]
    [InlineData(70_000, 80_000, 20_000, "Bu ay dönem gelirlerin ihtiyacın 10.000,00 TL altında kalıyor. Fark dönem başındaki finansal durumundan karşılanıyor.")]
    [InlineData(70_000, 80_000, -10_000, "Bu dönem sonunda yaklaşık 10.000,00 TL finansman açığı oluşuyor.")]
    public void Build_IncomeCoverageMessages_ReflectFinancialStatus(
        decimal totalIncome,
        decimal mandatoryOutflow,
        decimal endingBalance,
        string expectedMessage)
    {
        var row = CreateSampleRow(
            totalIncome: totalIncome,
            mandatoryOutflow: mandatoryOutflow,
            endingBalance: endingBalance);

        var detail = _presenter.Build(row);

        Assert.Equal(expectedMessage, detail.IncomeCoverageMessage);
    }

    [Fact]
    public void DetailMetric_AmountText_FormatsPositiveSignAndDecimals()
    {
        var metricWithPlus = new DetailMetric("Artış", 1_500.5m, DetailSemanticType.Income) { ShowPositiveSign = true };
        var metricWithoutPlus = new DetailMetric("Sabit", 1_500.5m, DetailSemanticType.Income) { ShowPositiveSign = false };

        Assert.Equal("+1.500,50 TL", metricWithPlus.AmountText);
        Assert.Equal("1.500,50 TL", metricWithoutPlus.AmountText);
    }

    [Fact]
    public void DetailDeficitCallout_Message_DistinguishesRecoveredAndRemaining()
    {
        var recovered = new DetailDeficitCallout(5_000m, 5_000m, 0m, IsRecovered: true);
        var remaining = new DetailDeficitCallout(10_000m, 3_000m, 7_000m, IsRecovered: false);

        Assert.Equal("Bu dönemde açık tamamen kapanıyor.", recovered.Message);
        Assert.Equal("Dönem sonuna devreden açık: 7.000,00 TL", remaining.Message);
        Assert.True(recovered.HasCoveredAmount);
        Assert.Equal("Bu dönem karşılanan: 5.000,00 TL", recovered.CoveredText);
    }

    private static CashFlowPeriodProjection CreateSampleRow(
        DateOnly? periodStart = null,
        decimal openingBalance = 0m,
        decimal totalIncome = 100_000m,
        decimal mandatoryOutflow = 0m,
        decimal loanPayments = 0m,
        decimal creditCardPayments = 0m,
        decimal temporaryPayments = 0m,
        decimal surplus = 0m,
        decimal endingBalance = 0m,
        decimal cardInterest = 0m,
        decimal deficitInterest = 0m,
        IReadOnlyList<ObligationItem>? mandatoryItems = null,
        IReadOnlyList<CreditCardPaymentProjectionStatus>? cardStatuses = null)
    {
        var start = periodStart ?? new DateOnly(2026, 8, 20);
        var period = new CashFlowPeriod(start, start.AddMonths(1));

        return new CashFlowPeriodProjection
        {
            Period = period,
            OpeningBalance = openingBalance,
            TotalIncome = totalIncome,
            RecurringIncomeTotal = totalIncome,
            AdHocIncomeTotal = 0m,
            MandatoryOutflow = mandatoryOutflow,
            LoanPayments = loanPayments,
            CreditCardPayments = creditCardPayments,
            TemporaryPayments = temporaryPayments,
            InstallmentPayments = 0m,
            PlannedLargeCashExpenses = 0m,
            OtherScheduledPayments = 0m,
            VariableExpenseAllowance = 0m,
            DeficitFinancingInterest = deficitInterest,
            CardInterestGenerated = cardInterest,
            EstimatedSurplus = surplus,
            EndingBalance = endingBalance,
            MandatoryItems = mandatoryItems ?? [],
            LargeExpenseItems = [],
            CardPaymentStatuses = cardStatuses ?? [],
            IncomeItems = []
        };
    }
}
