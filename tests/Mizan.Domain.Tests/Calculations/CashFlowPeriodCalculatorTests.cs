using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CashFlowPeriodCalculatorTests
{
    private readonly CashFlowPeriodCalculator _calculator = new();

    [Fact]
    public void GetPeriod_TarihTamCapaGunuOldugunda_OAyinCapaGunundeBaslayanDonemiDondurur()
    {
        var anchor = new PeriodAnchor(10);
        var date = new DateOnly(2026, 9, 10);

        var period = _calculator.GetPeriod(date, anchor);

        Assert.Equal(new DateOnly(2026, 9, 10), period.Start);
        Assert.Equal(new DateOnly(2026, 10, 10), period.End);
    }

    [Fact]
    public void GetPeriod_TarihCapaGunundenOnceyse_BirOncekiAydanBaslayanDonemiDondurur()
    {
        var anchor = new PeriodAnchor(10);
        var date = new DateOnly(2026, 9, 9);

        var period = _calculator.GetPeriod(date, anchor);

        Assert.Equal(new DateOnly(2026, 8, 10), period.Start);
        Assert.Equal(new DateOnly(2026, 9, 10), period.End);
    }

    [Fact]
    public void GetPeriod_TarihCapaGunundenSonraysa_MevcutAydanBaslayanDonemiDondurur()
    {
        var anchor = new PeriodAnchor(10);
        var date = new DateOnly(2026, 9, 25);

        var period = _calculator.GetPeriod(date, anchor);

        Assert.Equal(new DateOnly(2026, 9, 10), period.Start);
        Assert.Equal(new DateOnly(2026, 10, 10), period.End);
    }

    [Theory]
    [InlineData(2027, 1, 31, 31, 2027, 1, 31, 2027, 2, 28)]
    [InlineData(2027, 2, 28, 31, 2027, 2, 28, 2027, 3, 31)]
    [InlineData(2028, 1, 31, 31, 2028, 1, 31, 2028, 2, 29)]
    [InlineData(2028, 2, 29, 31, 2028, 2, 29, 2028, 3, 31)]
    public void GetPeriod_KisaAylarVeArtikYilda_AySonunaKenetlenirVeUzunAydaGeriKazanir(
        int year,
        int month,
        int day,
        int anchorDay,
        int expectedStartYear,
        int expectedStartMonth,
        int expectedStartDay,
        int expectedEndYear,
        int expectedEndMonth,
        int expectedEndDay)
    {
        var anchor = new PeriodAnchor(anchorDay);
        var date = new DateOnly(year, month, day);

        var period = _calculator.GetPeriod(date, anchor);

        Assert.Equal(new DateOnly(expectedStartYear, expectedStartMonth, expectedStartDay), period.Start);
        Assert.Equal(new DateOnly(expectedEndYear, expectedEndMonth, expectedEndDay), period.End);
    }

    [Fact]
    public void GetPeriods_BelirtilenAdette_SirasizBozulmayanVeKesintisizDonemDizisiUretir()
    {
        var anchor = new PeriodAnchor(15);
        var asOf = new DateOnly(2026, 9, 20);

        var periods = _calculator.GetPeriods(asOf, anchor, 3);

        Assert.Equal(3, periods.Count);

        // Birbirini takip eden dönemler kesintisiz olmalı (Öncekinin End'i sonrakinin Start'ına eşit)
        Assert.Equal(new DateOnly(2026, 9, 15), periods[0].Start);
        Assert.Equal(new DateOnly(2026, 10, 15), periods[0].End);

        Assert.Equal(periods[0].End, periods[1].Start);
        Assert.Equal(new DateOnly(2026, 11, 15), periods[1].End);

        Assert.Equal(periods[1].End, periods[2].Start);
        Assert.Equal(new DateOnly(2026, 12, 15), periods[2].End);
    }

    [Fact]
    public void GetPeriods_KisaAydanGecenSeride_TercihEdilenGunuGeriKazanarakUretir()
    {
        var anchor = new PeriodAnchor(31);
        var asOf = new DateOnly(2027, 1, 31);

        var periods = _calculator.GetPeriods(asOf, anchor, 3);

        Assert.Equal(3, periods.Count);
        Assert.Equal(new DateOnly(2027, 1, 31), periods[0].Start);
        Assert.Equal(new DateOnly(2027, 2, 28), periods[0].End);

        Assert.Equal(new DateOnly(2027, 2, 28), periods[1].Start);
        Assert.Equal(new DateOnly(2027, 3, 31), periods[1].End);

        Assert.Equal(new DateOnly(2027, 3, 31), periods[2].Start);
        Assert.Equal(new DateOnly(2027, 4, 30), periods[2].End);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(61)]
    public void GetPeriods_GecersizAdetVerildiginde_HataFirlatir(int invalidCount)
    {
        var anchor = new PeriodAnchor(10);
        var asOf = new DateOnly(2026, 9, 10);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.GetPeriods(asOf, anchor, invalidCount));

        Assert.Equal("count", ex.ParamName);
    }

    [Fact]
    public void GetFirstPeriodStartOnOrAfter_TarihTamDonemBasiysa_KendisiniDondurur()
    {
        var anchor = new PeriodAnchor(10);
        var date = new DateOnly(2026, 9, 10);

        var result = _calculator.GetFirstPeriodStartOnOrAfter(date, anchor);

        Assert.Equal(new DateOnly(2026, 9, 10), result);
    }

    [Fact]
    public void GetFirstPeriodStartOnOrAfter_TarihDonemIciyse_SonrakiDonemBasiniDondurur()
    {
        var anchor = new PeriodAnchor(10);
        var date = new DateOnly(2026, 9, 15);

        var result = _calculator.GetFirstPeriodStartOnOrAfter(date, anchor);

        Assert.Equal(new DateOnly(2026, 10, 10), result);
    }

    [Theory]
    [InlineData(2026, 8, 20, 10, 2026, 9, 10)]
    [InlineData(2026, 9, 10, 10, 2026, 10, 10)]
    [InlineData(2026, 9, 11, 10, 2026, 10, 10)]
    [InlineData(2027, 1, 31, 31, 2027, 2, 28)]
    [InlineData(2027, 2, 28, 31, 2027, 3, 31)]
    [InlineData(2028, 1, 31, 31, 2028, 2, 29)]
    public void GetNextSettlementDate_SnapshotTarihindenSonrakiIlkDonemKapanisiniDondurur(
        int snapshotYear,
        int snapshotMonth,
        int snapshotDay,
        int anchorDay,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        var anchor = new PeriodAnchor(anchorDay);
        var snapshot = new DateOnly(snapshotYear, snapshotMonth, snapshotDay);

        var result = _calculator.GetNextSettlementDate(snapshot, anchor);

        Assert.Equal(new DateOnly(expectedYear, expectedMonth, expectedDay), result);
        Assert.True(result > snapshot);
    }
}
