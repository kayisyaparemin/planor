using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class PeriodObligationGrouperTests
{
    private readonly PeriodObligationGrouper _grouper = new();

    private static CashFlowPeriod[] CreatePeriods() =>
    [
        new(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10)),
        new(new DateOnly(2026, 10, 10), new DateOnly(2026, 11, 10))
    ];

    [Fact]
    public void Group_NullDonemler_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _grouper.Group(null!, []));
    }

    [Fact]
    public void Group_NullYukumlulukler_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _grouper.Group(CreatePeriods(), null!));
    }

    [Fact]
    public void Group_DonemListesiBosOldugunda_ArgumentExceptionFirlatir()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            _grouper.Group([], []));

        Assert.Contains("En az bir dönem", ex.Message);
    }

    [Fact]
    public void Group_DogalDonemsellik_VadesiDonemAraliginaDusenKalemleriDogruDonemeAtar()
    {
        var periods = CreatePeriods();
        var obligations = new[]
        {
            new ObligationItem("Kira", ObligationType.OtherScheduledPayment, new DateOnly(2026, 9, 10), 10000m),
            new ObligationItem("Fatura", ObligationType.TemporaryPayment, new DateOnly(2026, 10, 9), 500m),
            new ObligationItem("Kart", ObligationType.CreditCard, new DateOnly(2026, 10, 10), 4000m),
            new ObligationItem("Kredi", ObligationType.Loan, new DateOnly(2026, 11, 9), 7500m)
        };

        var plan = _grouper.Group(periods, obligations);

        Assert.Equal(2, plan.Groups.Count);
        Assert.Equal(2, plan.Groups[0].Items.Count);
        Assert.Collection(
            plan.Groups[0].Items,
            x => Assert.Equal("Kira", x.Name),
            x => Assert.Equal("Fatura", x.Name));

        Assert.Equal(2, plan.Groups[1].Items.Count);
        Assert.Collection(
            plan.Groups[1].Items,
            x => Assert.Equal("Kart", x.Name),
            x => Assert.Equal("Kredi", x.Name));
    }

    [Fact]
    public void Group_IlkDonemOncesiOdemeler_PreFirstPeriodItemsListesineToplanir()
    {
        var periods = CreatePeriods();
        var obligations = new[]
        {
            new ObligationItem("Eski Borç", ObligationType.OtherScheduledPayment, new DateOnly(2026, 9, 5), 1000m),
            new ObligationItem("Kira", ObligationType.OtherScheduledPayment, new DateOnly(2026, 9, 15), 10000m)
        };

        var plan = _grouper.Group(periods, obligations);

        Assert.Single(plan.PreFirstPeriodItems);
        Assert.Equal("Eski Borç", plan.PreFirstPeriodItems[0].Name);
        Assert.Single(plan.Groups[0].Items);
    }

    [Fact]
    public void Group_SonDonemSonrasiOdemeler_PostHorizonItemsListesineToplanir()
    {
        var periods = CreatePeriods();
        var obligations = new[]
        {
            new ObligationItem("Gelecek Taksit", ObligationType.Loan, new DateOnly(2026, 11, 10), 5000m),
            new ObligationItem("Uzak Taksit", ObligationType.Loan, new DateOnly(2026, 12, 1), 5000m)
        };

        var plan = _grouper.Group(periods, obligations);

        Assert.Equal(2, plan.PostHorizonItems.Count);
        Assert.Collection(
            plan.PostHorizonItems,
            x => Assert.Equal("Gelecek Taksit", x.Name),
            x => Assert.Equal("Uzak Taksit", x.Name));
    }

    [Fact]
    public void Group_PeriodObligationGroup_ZorunluVeBuyukHarcamaKalemleriniDogruAyristirir()
    {
        var periods = CreatePeriods();
        var obligations = new[]
        {
            new ObligationItem("Kredi", ObligationType.Loan, new DateOnly(2026, 9, 15), 5000m),
            new ObligationItem("Telefon", ObligationType.PlannedLargeExpense, new DateOnly(2026, 9, 20), 20000m)
        };

        var plan = _grouper.Group(periods, obligations);
        var group = plan.Groups[0];

        Assert.Equal(25000m, group.TotalAmount);
        var mandatory = Assert.Single(group.MandatoryItems);
        Assert.Equal("Kredi", mandatory.Name);
        var largeExpense = Assert.Single(group.LargeExpenseItems);
        Assert.Equal("Telefon", largeExpense.Name);
    }

    [Fact]
    public void Group_TekilAtama_HicbirOdemeMukerrerlesmezVeKaybolmaz()
    {
        var periods = CreatePeriods();
        var obligations = new[]
        {
            new ObligationItem("Önce", ObligationType.TemporaryPayment, new DateOnly(2026, 9, 1), 100m),
            new ObligationItem("D1", ObligationType.Loan, new DateOnly(2026, 9, 15), 200m),
            new ObligationItem("D2", ObligationType.CreditCard, new DateOnly(2026, 10, 15), 300m),
            new ObligationItem("Sonra", ObligationType.TemporaryPayment, new DateOnly(2026, 11, 15), 400m)
        };

        var plan = _grouper.Group(periods, obligations);

        var assignedTotal = plan.Groups.Sum(g => g.Items.Count) +
                            plan.PreFirstPeriodItems.Count +
                            plan.PostHorizonItems.Count;

        Assert.Equal(obligations.Length, assignedTotal);
    }

    [Fact]
    public void PeriodObligationPlan_IndexerVeFindGroup_DogruCalisir()
    {
        var periods = CreatePeriods();
        var obligations = new[]
        {
            new ObligationItem("Kira", ObligationType.OtherScheduledPayment, new DateOnly(2026, 9, 15), 10000m)
        };

        var plan = _grouper.Group(periods, obligations);

        var groupByIndexer = plan[periods[0]];
        Assert.Equal(periods[0], groupByIndexer.Period);

        var foundGroup = plan.FindGroup(new DateOnly(2026, 9, 20));
        Assert.NotNull(foundGroup);
        Assert.Equal(periods[0], foundGroup.Period);

        var outsideGroup = plan.FindGroup(new DateOnly(2026, 9, 1));
        Assert.Null(outsideGroup);

        var missingPeriod = new CashFlowPeriod(new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 1));
        Assert.Throws<KeyNotFoundException>(() => plan[missingPeriod]);
    }

    [Fact]
    public void Group_OdemelerKronolojikSiralanir()
    {
        var periods = CreatePeriods();
        var obligations = new[]
        {
            new ObligationItem("B Harcaması", ObligationType.TemporaryPayment, new DateOnly(2026, 9, 20), 100m),
            new ObligationItem("A Harcaması", ObligationType.Loan, new DateOnly(2026, 9, 12), 200m),
            new ObligationItem("C Harcaması", ObligationType.CreditCard, new DateOnly(2026, 9, 12), 300m)
        };

        var plan = _grouper.Group(periods, obligations);
        var items = plan.Groups[0].Items;

        Assert.Equal("A Harcaması", items[0].Name);
        Assert.Equal("C Harcaması", items[1].Name);
        Assert.Equal("B Harcaması", items[2].Name);
    }
}
