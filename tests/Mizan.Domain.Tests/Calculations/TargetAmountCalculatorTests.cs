using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class TargetAmountCalculatorTests
{
    private readonly TargetAmountCalculator _sut = new();

    [Fact]
    public void FindFirstReached_ArtanBakiyede_HedefiIlkAsanDonemiDondurur()
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 50_000m, 100_000m);
        var p2 = CreateProjection(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10), 100_000m, 180_000m);
        var p3 = CreateProjection(new DateOnly(2026, 10, 10), new DateOnly(2026, 11, 10), 180_000m, 270_000m);
        var p4 = CreateProjection(new DateOnly(2026, 11, 10), new DateOnly(2026, 12, 10), 270_000m, 340_000m);

        // Act
        var reached = _sut.FindFirstReached([p1, p2, p3, p4], 300_000m);

        // Assert
        Assert.NotNull(reached);
        Assert.Equal(new DateOnly(2026, 11, 10), reached.PeriodStart);
        Assert.Equal(340_000m, reached.EndingBalance);
    }

    [Fact]
    public void FindFirstReached_KarisikSiradaGelenDonemleri_KronolojikSiralarVeDogruDonemiBulur()
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 0m, 50_000m);
        var p2 = CreateProjection(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10), 50_000m, 120_000m);
        var p3 = CreateProjection(new DateOnly(2026, 10, 10), new DateOnly(2026, 11, 10), 120_000m, 220_000m);

        // Act - karışık sıra veriliyor
        var reached = _sut.FindFirstReached([p3, p1, p2], 100_000m);

        // Assert
        Assert.NotNull(reached);
        Assert.Equal(new DateOnly(2026, 9, 10), reached.PeriodStart);
    }

    [Fact]
    public void FindFirstReached_ArayaNegatifDonemlerGirdiginde_ToparlanipHedefeUlasilanIlkDonemiBulur()
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 0m, -25_000m);
        var p2 = CreateProjection(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10), -25_000m, 9_000m);
        var p3 = CreateProjection(new DateOnly(2026, 10, 10), new DateOnly(2026, 11, 10), 9_000m, 60_000m);
        var p4 = CreateProjection(new DateOnly(2026, 11, 10), new DateOnly(2026, 12, 10), 60_000m, 130_000m);

        // Act
        var reached = _sut.FindFirstReached([p1, p2, p3, p4], 100_000m);

        // Assert
        Assert.NotNull(reached);
        Assert.Equal(new DateOnly(2026, 11, 10), reached.PeriodStart);
    }

    [Fact]
    public void FindFirstReached_HedefeHicUlasilamazsa_NullDondurur()
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 0m, 50_000m);

        // Act
        var reached = _sut.FindFirstReached([p1], 100_000m);

        // Assert
        Assert.Null(reached);
    }

    [Fact]
    public void FindFirstReached_BosKoleksiyonVerildiginde_NullDondurur()
    {
        // Act
        var reached = _sut.FindFirstReached([], 100_000m);

        // Assert
        Assert.Null(reached);
    }

    [Fact]
    public void FindFirstReachable_AcilisBakiyesiHedefiKarsiliyorsa_ZatenUlasildiOlarakDondurur()
    {
        // Arrange (İlk dönemin açılış bakiyesi 320.000 TL, hedef 300.000 TL)
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 320_000m, 280_000m);

        // Act
        var result = _sut.FindFirstReachable([p1], 300_000m);

        // Assert
        Assert.True(result.IsAlreadyReached);
        Assert.Null(result.FirstReachedPeriod);
        Assert.True(result.IsReached);
    }

    [Fact]
    public void FindFirstReachable_AcilistaUlasilmamissa_HedefiAsanIlkDonemiDondurur()
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 50_000m, 80_000m);
        var p2 = CreateProjection(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10), 80_000m, 150_000m);

        // Act
        var result = _sut.FindFirstReachable([p1, p2], 100_000m);

        // Assert
        Assert.False(result.IsAlreadyReached);
        Assert.NotNull(result.FirstReachedPeriod);
        Assert.Equal(new DateOnly(2026, 9, 10), result.FirstReachedPeriod.PeriodStart);
        Assert.True(result.IsReached);
    }

    [Fact]
    public void FindFirstReachable_KarisikSiradaIlkDonemAcilisi_DogruTespitEdilir()
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 350_000m, 320_000m);
        var p2 = CreateProjection(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10), 320_000m, 400_000m);

        // Act - karışık sırada veriliyor
        var result = _sut.FindFirstReachable([p2, p1], 340_000m);

        // Assert
        Assert.True(result.IsAlreadyReached);
        Assert.Null(result.FirstReachedPeriod);
        Assert.True(result.IsReached);
    }

    [Fact]
    public void FindFirstReachable_HedefeHicUlasilamazsa_UlasilmadiDondurur()
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 10_000m, 20_000m);

        // Act
        var result = _sut.FindFirstReachable([p1], 500_000m);

        // Assert
        Assert.False(result.IsAlreadyReached);
        Assert.Null(result.FirstReachedPeriod);
        Assert.False(result.IsReached);
    }

    [Fact]
    public void FindFirstReachable_BosKoleksiyonda_UlasilmadiDondurur()
    {
        // Act
        var result = _sut.FindFirstReachable([], 100_000m);

        // Assert
        Assert.False(result.IsAlreadyReached);
        Assert.Null(result.FirstReachedPeriod);
        Assert.False(result.IsReached);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    public void Metotlar_SifirVeyaNegatifHedefTutarinda_ArgumentOutOfRangeExceptionFirlatir(decimal target)
    {
        // Arrange
        var p1 = CreateProjection(new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10), 0m, 50_000m);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.FindFirstReached([p1], target));
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.FindFirstReachable([p1], target));
    }

    [Fact]
    public void Metotlar_KoleksiyonNullVerildiginde_ArgumentNullExceptionFirlatir()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.FindFirstReached(null!, 100_000m));
        Assert.Throws<ArgumentNullException>(() => _sut.FindFirstReachable(null!, 100_000m));
    }

    private static CashFlowPeriodProjection CreateProjection(
        DateOnly start,
        DateOnly end,
        decimal openingBalance,
        decimal endingBalance) => new()
    {
        Period = new CashFlowPeriod(start, end),
        OpeningBalance = openingBalance,
        EndingBalance = endingBalance
    };
}
