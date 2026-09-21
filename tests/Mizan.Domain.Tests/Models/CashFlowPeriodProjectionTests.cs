using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class CashFlowPeriodProjectionTests
{
    private static readonly CashFlowPeriod DefaultPeriod = new(
        new DateOnly(2026, 9, 15),
        new DateOnly(2026, 10, 15));

    [Fact]
    public void PeriodStartVePeriodEnd_PeriodNesnesiyleBirebirEslenir()
    {
        var projection = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod
        };

        Assert.Equal(new DateOnly(2026, 9, 15), projection.PeriodStart);
        Assert.Equal(new DateOnly(2026, 10, 15), projection.PeriodEnd);
    }

    [Fact]
    public void CarryOverDeficit_AcilisBakiyesiPozitifVeyaSifirsa_SifirDondurur()
    {
        var projection = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            OpeningBalance = 5_000m
        };

        Assert.Equal(0m, projection.CarryOverDeficit);
        Assert.False(projection.HasCarryOverDeficit);
    }

    [Fact]
    public void CarryOverDeficit_AcilisBakiyesiNegatifse_MutlakDegerDondurur()
    {
        var projection = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            OpeningBalance = -12_500m
        };

        Assert.Equal(12_500m, projection.CarryOverDeficit);
        Assert.True(projection.HasCarryOverDeficit);
    }

    [Fact]
    public void AvailableAfterCarryOverDeficit_ZorunluSonrasiVeAcikFarkiniDondurur()
    {
        var projection = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            OpeningBalance = -4_000m,
            AvailableAfterMandatory = 15_000m
        };

        Assert.Equal(11_000m, projection.AvailableAfterCarryOverDeficit);
    }

    [Fact]
    public void DeficitPrincipal_FaizOncesiBakiyeNegatifse_MutlakDegeriniDondurur()
    {
        var negative = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            EndingBalanceBeforeDeficitInterest = -8_200m
        };

        var positive = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            EndingBalanceBeforeDeficitInterest = 1_500m
        };

        Assert.Equal(8_200m, negative.DeficitPrincipal);
        Assert.Equal(0m, positive.DeficitPrincipal);
    }

    [Fact]
    public void TotalInterestGenerated_KartFaiziVeAcikFaiziniToplar()
    {
        var projection = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            CardInterestGenerated = 750.50m,
            DeficitFinancingInterest = 249.50m
        };

        Assert.Equal(1_000m, projection.TotalInterestGenerated);
    }

    [Theory]
    [InlineData(0, 5000, 0)]          // Devreden açık yok
    [InlineData(-5000, -1000, 0)]      // Devreden açık var ama cari dönem zararda
    [InlineData(-5000, 0, 0)]          // Devreden açık var ama cari dönem nötr
    [InlineData(-5000, 3000, 3000)]    // Kısmi kapatma (fazla açıktan küçük)
    [InlineData(-5000, 7000, 5000)]    // Tam kapatma (fazla açıktan büyük)
    public void DeficitCoveredThisPeriod_DonemNetFazlasiylaKapatilanAcigiHesaplar(
        decimal openingBalance,
        decimal estimatedSurplus,
        decimal expectedCovered)
    {
        var projection = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            OpeningBalance = openingBalance,
            EstimatedSurplus = estimatedSurplus
        };

        Assert.Equal(expectedCovered, projection.DeficitCoveredThisPeriod);
    }

    [Fact]
    public void RecoveredCarryOverDeficit_AciklaBaslayipPozitifBiterseTrue_AksiHaldeFalse()
    {
        var recovered = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            OpeningBalance = -3_000m,
            EndingBalance = 500m
        };

        var stillInDeficit = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            OpeningBalance = -3_000m,
            EndingBalance = -100m
        };

        var noDeficit = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod,
            OpeningBalance = 2_000m,
            EndingBalance = 4_000m
        };

        Assert.True(recovered.RecoveredCarryOverDeficit);
        Assert.False(stillInDeficit.RecoveredCarryOverDeficit);
        Assert.False(noDeficit.RecoveredCarryOverDeficit);
        Assert.Equal(100m, stillInDeficit.RemainingCarryOverDeficit);
        Assert.Equal(0m, recovered.RemainingCarryOverDeficit);
    }

    [Fact]
    public void KoleksiyonOzellikleri_VarsayilanOlarakBosListeDondurur()
    {
        var projection = new CashFlowPeriodProjection
        {
            Period = DefaultPeriod
        };

        Assert.NotNull(projection.IncomeItems);
        Assert.Empty(projection.IncomeItems);
        Assert.NotNull(projection.MandatoryItems);
        Assert.Empty(projection.MandatoryItems);
        Assert.NotNull(projection.LargeExpenseItems);
        Assert.Empty(projection.LargeExpenseItems);
        Assert.NotNull(projection.CardPaymentStatuses);
        Assert.Empty(projection.CardPaymentStatuses);
    }
}
