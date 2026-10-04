using Mizan.Application.Services;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Geçmiş dönemler ekranının (EK-V12, GS32) görünüm modeli testleri:
/// Boş durum, dönemler ve özetin dolması, ayrıntıya yönlendirme ve hata durumu.
/// </summary>
public sealed class HistoryViewModelTests
{
    private readonly FakePeriodHistoryRepository _repository = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly HistoryQueryService _queryService;
    private readonly HistoryViewModel _viewModel;

    public HistoryViewModelTests()
    {
        var calculator = new PlanActualComparisonCalculator();
        _queryService = new HistoryQueryService(_repository, calculator);
        _viewModel = new HistoryViewModel(_queryService, _navigation);
    }

    [Fact]
    public async Task Yukle_KapanmisDonemYoksa_BosDurumGosterir()
    {
        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.False(_viewModel.HasSummary);
        Assert.Empty(_viewModel.Periods);
    }

    [Fact]
    public async Task Yukle_KapanmisDonemlerVarsa_DonemleriVeOzetiDoldurur()
    {
        var (plan1, actual1) = AddClosedPeriod(
            start: new DateOnly(2026, 8, 10),
            end: new DateOnly(2026, 9, 10),
            opening: 10000m,
            plannedEnding: 15000m,
            confirmedEnding: 16000m); // +1000 fark (planın üzerinde)

        var (plan2, actual2) = AddClosedPeriod(
            start: new DateOnly(2026, 9, 10),
            end: new DateOnly(2026, 10, 10),
            opening: 16000m,
            plannedEnding: 20000m,
            confirmedEnding: 19000m); // -1000 fark (planın altında)

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.True(_viewModel.HasSummary);
        Assert.Equal(2, _viewModel.SummaryPeriodCount);
        Assert.Equal(2, _viewModel.Periods.Count);

        // En yeni dönem başta (Eylül-Ekim)
        var row0 = _viewModel.Periods[0];
        Assert.Equal(actual2.Id, row0.ActualId);
        Assert.Equal(new DateOnly(2026, 9, 10), row0.PeriodStart);
        Assert.Equal(new DateOnly(2026, 10, 9), row0.PeriodLastDay);
        Assert.Equal(19000m, row0.ActualEndingBalance);
        Assert.Equal(20000m, row0.PlannedEndingBalance);
        Assert.Equal(-1000m, row0.Difference);
        Assert.True(row0.IsDifferenceNegative);
        Assert.False(row0.IsDifferencePositive);
        Assert.Equal("Planın altında", row0.StatusText);
        Assert.Equal("Negative", row0.StatusSemantic);

        // İkinci dönem (Ağustos-Eylül)
        var row1 = _viewModel.Periods[1];
        Assert.Equal(actual1.Id, row1.ActualId);
        Assert.Equal(new DateOnly(2026, 8, 10), row1.PeriodStart);
        Assert.Equal(new DateOnly(2026, 9, 9), row1.PeriodLastDay);
        Assert.Equal(16000m, row1.ActualEndingBalance);
        Assert.Equal(15000m, row1.PlannedEndingBalance);
        Assert.Equal(1000m, row1.Difference);
        Assert.True(row1.IsDifferencePositive);
        Assert.False(row1.IsDifferenceNegative);
        Assert.Equal("Planın üzerinde", row1.StatusText);
        Assert.Equal("Positive", row1.StatusSemantic);
    }

    [Fact]
    public async Task DetayAc_GecerliDonemleCagrildiginda_AyrintiSayfasinaGider()
    {
        var row = new HistoryPeriodRow
        {
            ActualId = Guid.NewGuid(),
            PeriodStart = new DateOnly(2026, 9, 10),
            PeriodLastDay = new DateOnly(2026, 10, 9),
            ActualEndingBalance = 42180m,
            PlannedEndingBalance = 43900m,
            Difference = -1720m,
            StatusText = "Planın altında",
            StatusSemantic = "Negative"
        };

        await _viewModel.OpenDetailAsync(row);

        Assert.Equal(Routes.HistoryDetail, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal(row.ActualId, _navigation.LastParameters[Routes.HistoryActualIdParameter]);
    }

    private (PeriodPlanSnapshot Plan, PeriodActual Actual) AddClosedPeriod(
        DateOnly start,
        DateOnly end,
        decimal opening,
        decimal plannedEnding,
        decimal confirmedEnding)
    {
        var startSnapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = start,
            ProjectionOpeningBalance = opening
        };
        var resultSnapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = end,
            ProjectionOpeningBalance = confirmedEnding
        };
        var plan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = startSnapshot.Id,
            PeriodStart = start,
            PeriodEnd = end,
            OpeningBalance = opening,
            PlannedEndingBalance = plannedEnding
        };
        var actual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = plan.Id,
            SourceFinancialSnapshotId = startSnapshot.Id,
            ResultFinancialSnapshotId = resultSnapshot.Id,
            PeriodStart = start,
            PeriodEnd = end,
            ConfirmedEndingBalance = confirmedEnding
        };

        _repository.Snapshots.Add(startSnapshot);
        _repository.Snapshots.Add(resultSnapshot);
        _repository.Plans.Add(plan);
        _repository.Actuals.Add(actual);

        return (plan, actual);
    }
}
