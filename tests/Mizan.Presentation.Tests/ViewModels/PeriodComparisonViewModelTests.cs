using Mizan.Application.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Ana sayfanın plan / şu an tablosu (S87): kart başına ödeme, KMH faizi ve yaşam giderinin tutarları gidişattan
/// olduğu gibi gelir; planı aşan şu an işaretlenir, bilinmeyen şu an uydurulmaz.
/// </summary>
public sealed class PeriodComparisonViewModelTests
{
    private static readonly DateOnly Start = new(2026, 9, 10);
    private static readonly DateOnly CardDue = new(2026, 10, 5);

    private readonly PeriodComparisonViewModel _viewModel = new();

    [Fact]
    public void Goster_KartSatirlari_PlanVeSuAnGidisatinSirasiylaGelir()
    {
        _viewModel.Show(Progress(cards:
        [
            new PeriodCardComparison(Guid.NewGuid(), "Akbank Axess", CardDue, 24233m, 26747m),
            new PeriodCardComparison(Guid.NewGuid(), "Garanti Bonus", CardDue, 0m, 0m)
        ]));

        Assert.Equal(
            [
                new PeriodCardComparisonRow("Akbank Axess", 24233m, 26747m, IsAbovePlan: true),
                new PeriodCardComparisonRow("Garanti Bonus", 0m, 0m, IsAbovePlan: false)
            ],
            _viewModel.Cards);
        Assert.True(_viewModel.HasRows);
    }

    [Theory]
    [InlineData(26747.0, true)]
    [InlineData(24233.0, false)]
    [InlineData(20000.0, false)]
    [InlineData(null, false)]
    public void Goster_KartinSuAni_YalnizPlaniAsarsaIsaretlenir(double? current, bool expected)
    {
        _viewModel.Show(Progress(cards: [new PeriodCardComparison(Guid.NewGuid(), "Akbank Axess", CardDue, 24233m, (decimal?)current)]));

        var row = Assert.Single(_viewModel.Cards);
        Assert.Equal((decimal?)current, row.Current);
        Assert.Equal(expected, row.IsAbovePlan);
    }

    [Fact]
    public void Goster_KmhFaizi_PlanVeGidisatiTasirAsimIsaretlenir()
    {
        _viewModel.Show(Progress(plannedDeficitInterest: 6831m, projectedDeficitInterest: 7200m));

        Assert.True(_viewModel.HasDeficitInterest);
        Assert.Equal(6831m, _viewModel.PlannedDeficitInterest);
        Assert.Equal(7200m, _viewModel.CurrentDeficitInterest);
        Assert.True(_viewModel.IsDeficitInterestAbovePlan);
        Assert.True(_viewModel.HasRows);
    }

    [Fact]
    public void Goster_BakiyeGirilmediyse_KmhFaizininSuAniYoktur()
    {
        _viewModel.Show(Progress(plannedDeficitInterest: 6831m, projectedDeficitInterest: null, observed: false));

        Assert.True(_viewModel.HasDeficitInterest);
        Assert.Equal(6831m, _viewModel.PlannedDeficitInterest);
        Assert.Null(_viewModel.CurrentDeficitInterest);
        Assert.False(_viewModel.IsDeficitInterestAbovePlan);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(0.0, null)]
    public void Goster_KmhFaiziSifirsa_SatirYokturKartYoksaTabloYoktur(double planned, double? projected)
    {
        _viewModel.Show(Progress(plannedDeficitInterest: (decimal)planned, projectedDeficitInterest: (decimal?)projected));

        Assert.False(_viewModel.HasDeficitInterest);
        Assert.Empty(_viewModel.Cards);
        Assert.False(_viewModel.HasRows);
    }

    [Fact]
    public void Goster_PlandaYokGidisattaVarsa_KmhSatiriGorunur()
    {
        _viewModel.Show(Progress(plannedDeficitInterest: 0m, projectedDeficitInterest: 1250m));

        Assert.True(_viewModel.HasDeficitInterest);
        Assert.True(_viewModel.IsDeficitInterestAbovePlan);
        Assert.True(_viewModel.HasRows);
    }

    [Fact]
    public void Goster_YasamGideri_PlanlananVeHarcananTutarGelir()
    {
        _viewModel.Show(Progress(observed: true));

        Assert.Equal(30000m, _viewModel.PlannedLivingExpense);
        Assert.Equal(22269m, _viewModel.SpentLivingExpense);
    }

    [Fact]
    public void Goster_BakiyeGirilmediyse_YasamGiderininYalnizPlaniVardir()
    {
        _viewModel.Show(Progress(observed: false));

        Assert.Equal(30000m, _viewModel.PlannedLivingExpense);
        Assert.Null(_viewModel.SpentLivingExpense);
    }

    [Fact]
    public void Goster_IkinciKez_OncekiSatirlarKalmaz()
    {
        _viewModel.Show(Progress(cards:
        [
            new PeriodCardComparison(Guid.NewGuid(), "Akbank Axess", CardDue, 24233m, 26747m),
            new PeriodCardComparison(Guid.NewGuid(), "Garanti Bonus", CardDue, 0m, 0m)
        ]));

        _viewModel.Show(Progress(cards: [new PeriodCardComparison(Guid.NewGuid(), "Garanti Bonus", CardDue, 0m, 0m)]));

        Assert.Equal("Garanti Bonus", Assert.Single(_viewModel.Cards).Name);
    }

    [Fact]
    public void Temizle_TabloVeTutarlarBosalir()
    {
        _viewModel.Show(Progress(
            cards: [new PeriodCardComparison(Guid.NewGuid(), "Akbank Axess", CardDue, 24233m, 26747m)],
            plannedDeficitInterest: 6831m,
            projectedDeficitInterest: 7200m));

        _viewModel.Clear();

        Assert.Empty(_viewModel.Cards);
        Assert.False(_viewModel.HasRows);
        Assert.False(_viewModel.HasDeficitInterest);
        Assert.False(_viewModel.IsDeficitInterestAbovePlan);
        Assert.Null(_viewModel.PlannedDeficitInterest);
        Assert.Null(_viewModel.CurrentDeficitInterest);
        Assert.Null(_viewModel.PlannedLivingExpense);
        Assert.Null(_viewModel.SpentLivingExpense);
    }

    private static PeriodProgress Progress(
        IReadOnlyList<PeriodCardComparison>? cards = null,
        decimal plannedDeficitInterest = 0m,
        decimal? projectedDeficitInterest = 0m,
        bool observed = true)
    {
        var planId = Guid.NewGuid();
        var end = new DateOnly(2026, 10, 10);
        return new PeriodProgress
        {
            PeriodPlanSnapshotId = planId,
            PeriodStart = Start,
            PeriodEnd = end,
            Today = new DateOnly(2026, 9, 30),
            ElapsedDays = 20,
            TotalDays = 30,
            RevisionCount = 0,
            PlannedIncome = 50000m,
            PlannedMandatoryPayments = 15000m,
            PlannedEndingBalance = -167552m,
            PlannedVariableExpenseAllowance = 30000m,
            PlannedDeficitInterest = plannedDeficitInterest,
            ObservedLivingSpend = observed ? 22269m : null,
            RemainingVariableExpenseAllowance = observed ? 7731m : null,
            ProjectedDeficitInterest = observed ? projectedDeficitInterest : null,
            ProjectedEndingBalance = observed ? -167552m : null,
            Pace = null,
            Cards = cards ?? [],
            Observation = null,
            Observations = [],
            Path = new PeriodBalancePath([new(Start, 0m)], [new(Start, 0m), new(end, -167552m)]),
            RemainingPayments = [],
            IsClosable = false,
            SnoozedLineIds = new HashSet<Guid>()
        };
    }
}
