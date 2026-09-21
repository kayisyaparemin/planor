using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class CashFlowPeriodTests
{
    [Fact]
    public void Constructor_GecerliTarihlerle_OzellikleriDogruAyarla()
    {
        var start = new DateOnly(2026, 9, 10);
        var end = new DateOnly(2026, 10, 10);

        var period = new CashFlowPeriod(start, end);

        Assert.Equal(start, period.Start);
        Assert.Equal(end, period.End);
        Assert.Equal(30, period.DayCount);
    }

    [Fact]
    public void Constructor_BitisBaslangicaEsitse_HataFirlatir()
    {
        var date = new DateOnly(2026, 9, 10);

        var ex = Assert.Throws<ArgumentException>(() => new CashFlowPeriod(date, date));
        Assert.Equal("end", ex.ParamName);
    }

    [Fact]
    public void Constructor_BitisBaslangictanOnceyse_HataFirlatir()
    {
        var start = new DateOnly(2026, 9, 10);
        var end = new DateOnly(2026, 9, 9);

        var ex = Assert.Throws<ArgumentException>(() => new CashFlowPeriod(start, end));
        Assert.Equal("end", ex.ParamName);
    }

    [Fact]
    public void Contains_YariAcikAralikKuraliniUygular()
    {
        var start = new DateOnly(2026, 9, 10);
        var end = new DateOnly(2026, 10, 10);
        var period = new CashFlowPeriod(start, end);

        // Başlangıç günü dahildir
        Assert.True(period.Contains(start));

        // Dönem içi bir gün dahildir
        Assert.True(period.Contains(new DateOnly(2026, 9, 25)));

        // Bitiş gününden önceki son gün dahildir
        Assert.True(period.Contains(new DateOnly(2026, 10, 9)));

        // Bitiş günü dahil DEĞİLDİR (sonraki döneme aittir)
        Assert.False(period.Contains(end));

        // Başlangıçtan önceki ve bitişten sonraki günler dahil değildir
        Assert.False(period.Contains(new DateOnly(2026, 9, 9)));
        Assert.False(period.Contains(new DateOnly(2026, 10, 11)));
    }
}
