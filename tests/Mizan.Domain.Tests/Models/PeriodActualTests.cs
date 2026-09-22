using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodActualTests
{
    private static readonly DateOnly PeriodStart = new(2026, 10, 10);
    private static readonly DateOnly PeriodEnd = new(2026, 11, 10);

    [Fact]
    public void TotalActualOutflows_TumFiiliGiderleriFaizleriVePlansizOdemeleriToplar()
    {
        var actual = new PeriodActual
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            ActualMandatoryPayments = 14_000m,
            ActualLivingSpend = 9_500m,
            ActualLargeExpenses = 3_000m,
            ActualInterest = 650m,
            UnplannedPayments = 1_200m
        };

        // 14000 + 9500 + 3000 + 650 + 1200 = 28350
        Assert.Equal(28_350m, actual.TotalActualOutflows);
    }

    [Theory]
    [InlineData(-1000, true)]
    [InlineData(-0.01, true)]
    [InlineData(0, false)]
    [InlineData(2500, false)]
    public void HasDeficit_TeyitEdilenBakiyeNegatifseTrue_SifirVeyaPozitifseFalse(
        decimal confirmedEndingBalance,
        bool expectedHasDeficit)
    {
        var actual = new PeriodActual
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            ConfirmedEndingBalance = confirmedEndingBalance
        };

        Assert.Equal(expectedHasDeficit, actual.HasDeficit);
    }

    [Theory]
    [InlineData(2026, 10, 5, false)]  // S19 kurulum-çapa ara penceresi (dönem öncesi)
    [InlineData(2026, 10, 9, false)]  // Dönem başlangıcı öncesi
    [InlineData(2026, 10, 10, true)]  // Dönem başlangıç çapa günü (dahil)
    [InlineData(2026, 10, 25, true)]  // Dönem içi
    [InlineData(2026, 11, 9, true)]   // Dönem son günü
    [InlineData(2026, 11, 10, false)] // Sonraki dönem başlangıcı / dönem sonu (hariç)
    [InlineData(2026, 11, 15, false)] // Dönem sonrası
    public void ContainsDate_YariAcikAralikKuraliniUygular(
        int year,
        int month,
        int day,
        bool expectedResult)
    {
        var actual = new PeriodActual
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd
        };

        var date = new DateOnly(year, month, day);
        Assert.Equal(expectedResult, actual.ContainsDate(date));
    }

    [Fact]
    public void ReconciliationAdjustment_TeyitEdilenVeTuretilenBakiyeFarkiniYansitir()
    {
        var actual = new PeriodActual
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd,
            DerivedEndingBalance = 12_500m,
            ConfirmedEndingBalance = 12_200m,
            ReconciliationAdjustment = -300m
        };

        Assert.Equal(-300m, actual.ReconciliationAdjustment);
        Assert.Equal(actual.ConfirmedEndingBalance - actual.DerivedEndingBalance, actual.ReconciliationAdjustment);
    }

    [Fact]
    public void Koleksiyonlar_VarsayilanOlarakBosListelerleBaslatilir()
    {
        var actual = new PeriodActual
        {
            PeriodStart = PeriodStart,
            PeriodEnd = PeriodEnd
        };

        Assert.NotNull(actual.Payments);
        Assert.Empty(actual.Payments);
        Assert.NotNull(actual.Flows);
        Assert.Empty(actual.Flows);
        Assert.NotNull(actual.LivingBreakdown);
        Assert.Empty(actual.LivingBreakdown);
    }

    [Theory]
    [InlineData(ActualPaymentStatus.Paid, true)]
    [InlineData(ActualPaymentStatus.DifferentAmount, true)]
    [InlineData(ActualPaymentStatus.Unpaid, false)]
    public void ActualPayment_IsSettled_OdenmeDurumunuDogruBelirtir(
        ActualPaymentStatus status,
        bool expectedIsSettled)
    {
        var payment = new ActualPayment
        {
            Status = status,
            ActualAmount = status == ActualPaymentStatus.Unpaid ? 0m : 5_000m
        };

        Assert.Equal(expectedIsSettled, payment.IsSettled);
    }

    [Theory]
    [InlineData(1000.0, 1000.0, 0.0)]      // Tam ödeme
    [InlineData(1000.0, 1200.0, 200.0)]    // Fazla ödeme
    [InlineData(1000.0, 800.0, -200.0)]    // Eksik ödeme
    [InlineData(null, 500.0, 500.0)]       // Planda tutarsızdı, 500 ödendi
    [InlineData(1000.0, 0.0, -1000.0)]     // Ödenmedi
    public void ActualPayment_Variance_FiiliVePlanlananFarkiniDondurur(
        double? plannedAmount,
        double actualAmount,
        double expectedVariance)
    {
        var payment = new ActualPayment
        {
            PlannedAmount = (decimal?)plannedAmount,
            ActualAmount = (decimal)actualAmount
        };

        Assert.Equal((decimal)expectedVariance, payment.Variance);
    }

    [Fact]
    public void ActualFlow_IsIncomeVeIsPayment_TureGoreDogruSonucVerir()
    {
        var incomeFlow = new ActualFlow
        {
            Type = ActualFlowType.UnplannedIncome,
            Amount = 1_500m
        };
        var paymentFlow = new ActualFlow
        {
            Type = ActualFlowType.UnplannedPayment,
            Amount = 800m
        };

        Assert.True(incomeFlow.IsIncome);
        Assert.False(incomeFlow.IsPayment);

        Assert.False(paymentFlow.IsIncome);
        Assert.True(paymentFlow.IsPayment);
    }

    [Fact]
    public void ActualLivingBreakdown_AlanlariDogruTutar()
    {
        var breakdownId = Guid.NewGuid();
        var actualId = Guid.NewGuid();

        var item = new ActualLivingBreakdown
        {
            Id = breakdownId,
            PeriodActualId = actualId,
            Category = "Market",
            Amount = 4_250.75m
        };

        Assert.Equal(breakdownId, item.Id);
        Assert.Equal(actualId, item.PeriodActualId);
        Assert.Equal("Market", item.Category);
        Assert.Equal(4_250.75m, item.Amount);
    }

    [Fact]
    public void EnumDegerleri_BeklenenSabitleriTasir()
    {
        Assert.Equal(0, (int)ActualPaymentStatus.Paid);
        Assert.Equal(1, (int)ActualPaymentStatus.DifferentAmount);
        Assert.Equal(2, (int)ActualPaymentStatus.Unpaid);

        Assert.Equal(0, (int)ActualFlowType.UnplannedPayment);
        Assert.Equal(1, (int)ActualFlowType.UnplannedIncome);
    }
}
