using Mizan.Application.Services;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kapanmış dönem ayrıntısı ekranının (EK-V12, S68-6, 7, GS32) görünüm modeli testleri:
/// Dönem sonu gerçekleşmesi, bakiye çizgisi (trend), farkın kaynağı dökümü,
/// gerçekleşen ödemeler listesi ve yerinde genişletme/daraltma.
/// </summary>
public sealed class HistoryDetailViewModelTests
{
    private static readonly DateOnly Start = new(2026, 9, 10);
    private static readonly DateOnly End = new(2026, 10, 10);

    private readonly FakePeriodHistoryRepository _historyRepo = new();
    private readonly FakePeriodObservationRepository _observationRepo = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly HistoryDetailViewModel _viewModel;

    public HistoryDetailViewModelTests()
    {
        var calculator = new PlanActualComparisonCalculator();
        var queryService = new HistoryQueryService(_historyRepo, calculator);
        _viewModel = new HistoryDetailViewModel(queryService, _observationRepo, _navigation);
    }

    [Fact]
    public async Task Yukle_GecersizActualId_BosDurumGosterir()
    {
        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(ScreenState.Empty, _viewModel.State);
    }

    [Fact]
    public async Task Yukle_GecerliDonem_TumAlanlariVeGrafigiDoldurur()
    {
        var actualId = SetupClosedPeriodWithPaymentsAndObservations();

        await _viewModel.LoadAsync(actualId);

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(Start, _viewModel.PeriodStart);
        Assert.Equal(new DateOnly(2026, 10, 9), _viewModel.PeriodLastDay);

        // Bakiye ve fark
        Assert.Equal(42180m, _viewModel.ActualEndingBalance);
        Assert.Equal(43900m, _viewModel.PlannedEndingBalance);
        Assert.Equal(-1720m, _viewModel.Difference);
        Assert.True(_viewModel.IsDifferenceNegative);
        Assert.False(_viewModel.IsDifferencePositive);
        Assert.Equal(15000m, _viewModel.OpeningBalance);

        // Grafik (S68-6, 7)
        Assert.NotNull(_viewModel.Trend);
        Assert.NotNull(_viewModel.Trend.PlanLevel);
        Assert.Equal(43900m, _viewModel.Trend.PlanLevel.Value);
        Assert.NotNull(_viewModel.Trend.Markers);
        Assert.Equal(2, _viewModel.Trend.Markers.Points.Count);
        Assert.NotEmpty(_viewModel.Trend.Series.Points);

        // Farkın kaynağı (S4)
        Assert.Equal(29650m, _viewModel.PlannedLivingSpend);
        Assert.Equal(28750m, _viewModel.ActualLivingSpend);
        Assert.Equal(900m, _viewModel.LivingDifference);
        Assert.True(_viewModel.IsLivingDifferencePositive);

        Assert.Equal(10000m, _viewModel.PlannedMandatory);
        Assert.Equal(10000m, _viewModel.ActualMandatory);
        Assert.Equal(5, _viewModel.TotalPaymentCount);
        Assert.Equal(5, _viewModel.PaidPaymentCount);

        Assert.Equal(250m, _viewModel.ActualInterest);
        Assert.True(_viewModel.HasActualInterest);

        Assert.Equal(-50m, _viewModel.ReconciliationAdjustment);
        Assert.True(_viewModel.HasReconciliationAdjustment);

        // Ödemeler (5 ödeme > 4 limit => ilk 4 gösterilir, 1 gizli)
        Assert.True(_viewModel.HasPayments);
        Assert.Equal(4, _viewModel.Payments.Count);
        Assert.Equal(1, _viewModel.HiddenPaymentCount);
        Assert.True(_viewModel.HasHiddenPayments);
        Assert.False(_viewModel.IsExpanded);
    }

    [Fact]
    public async Task OdemeleriAcKapa_GizliOdemeleriGosterirVeKapatir()
    {
        var actualId = SetupClosedPeriodWithPaymentsAndObservations();
        await _viewModel.LoadAsync(actualId);

        // Genişlet
        _viewModel.TogglePayments();
        Assert.Equal(5, _viewModel.Payments.Count);
        Assert.Equal(0, _viewModel.HiddenPaymentCount);
        Assert.False(_viewModel.HasHiddenPayments);
        Assert.True(_viewModel.IsExpanded);

        // Tekrar daralt
        _viewModel.TogglePayments();
        Assert.Equal(4, _viewModel.Payments.Count);
        Assert.Equal(1, _viewModel.HiddenPaymentCount);
        Assert.True(_viewModel.HasHiddenPayments);
        Assert.False(_viewModel.IsExpanded);
    }

    [Fact]
    public async Task GeriDon_NavigasyonServisiniCagirir()
    {
        await _viewModel.GoBackAsync();

        Assert.True(_navigation.NavigateBackCalled);
    }

    private Guid SetupClosedPeriodWithPaymentsAndObservations()
    {
        var startSnapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = Start,
            ProjectionOpeningBalance = 15000m
        };
        var resultSnapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = End,
            ProjectionOpeningBalance = 42180m
        };
        var plan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = startSnapshot.Id,
            PeriodStart = Start,
            PeriodEnd = End,
            OpeningBalance = 15000m,
            PlannedEndingBalance = 43900m,
            PlannedVariableExpenseAllowance = 29650m,
            PlannedMandatoryPayments = 10000m
        };

        var payments = new List<ActualPayment>
        {
            new() { Name = "Kira", PlannedDate = new DateOnly(2026, 9, 15), PlannedAmount = 4000m, ActualAmount = 4000m, Status = ActualPaymentStatus.Paid },
            new() { Name = "Elektrik", PlannedDate = new DateOnly(2026, 9, 18), PlannedAmount = 500m, ActualAmount = 500m, Status = ActualPaymentStatus.Paid },
            new() { Name = "İnternet", PlannedDate = new DateOnly(2026, 9, 20), PlannedAmount = 300m, ActualAmount = 300m, Status = ActualPaymentStatus.Paid },
            new() { Name = "Kredi Kartı", PlannedDate = new DateOnly(2026, 9, 25), PlannedAmount = 3000m, ActualAmount = 3200m, Status = ActualPaymentStatus.DifferentAmount },
            new() { Name = "Doğalgaz", PlannedDate = new DateOnly(2026, 9, 28), PlannedAmount = 2200m, ActualAmount = 2000m, Status = ActualPaymentStatus.DifferentAmount }
        };

        var actual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = plan.Id,
            SourceFinancialSnapshotId = startSnapshot.Id,
            ResultFinancialSnapshotId = resultSnapshot.Id,
            PeriodStart = Start,
            PeriodEnd = End,
            ConfirmedEndingBalance = 42180m,
            ActualLivingSpend = 28750m,
            ActualMandatoryPayments = 10000m,
            ActualInterest = 250m,
            ReconciliationAdjustment = -50m,
            Payments = payments
        };

        _historyRepo.Snapshots.Add(startSnapshot);
        _historyRepo.Snapshots.Add(resultSnapshot);
        _historyRepo.Plans.Add(plan);
        _historyRepo.Actuals.Add(actual);

        // Gözlemler (S68-6, 7)
        _observationRepo.Observations.Add(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedOn = new DateOnly(2026, 9, 15),
            ObservedBalance = 18000m,
            RecordedAtUtc = DateTimeOffset.UtcNow
        });
        _observationRepo.Observations.Add(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedOn = new DateOnly(2026, 9, 25),
            ObservedBalance = 25000m,
            RecordedAtUtc = DateTimeOffset.UtcNow
        });

        return actual.Id;
    }
}
