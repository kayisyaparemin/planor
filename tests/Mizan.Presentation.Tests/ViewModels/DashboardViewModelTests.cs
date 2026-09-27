using System.Collections.ObjectModel;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Ana sayfa görünüm modelinin dönem gidişatı yükleme, bütçe oranı hesaplama,
/// gözlem kaydetme ve gezinme aksiyonlarını doğrulayan birim testleri.
/// </summary>
public sealed class DashboardViewModelTests : IDisposable
{
    private readonly FakePeriodProgressService _progressService = new();
    private readonly FakePeriodWorkflowService _workflowService = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakePaymentReminderService _reminderService = new();
    private readonly FakePaymentReminderScheduler _scheduler = new();
    private readonly FakeDialogService _dialog = new();
    private readonly SabitSaat _clock = new(new DateOnly(2026, 9, 27));
    private readonly ProfileService _profileService;
    private readonly ReminderCardViewModel _reminders;
    private readonly DashboardViewModel _viewModel;

    public DashboardViewModelTests()
    {
        _profileService = new ProfileService(new FakeProfileRepository(), new FakeProfileStoreSwitch(), _clock);
        _reminders = new ReminderCardViewModel(_reminderService, _scheduler, _dialog, _clock, _profileService);
        _viewModel = new DashboardViewModel(
            _progressService,
            _workflowService,
            _navigation,
            _reminders);
    }

    [Fact]
    public async Task YukleAsync_AcikDonemVarsa_GidisatVeButceOraniDoldurulur()
    {
        _progressService.CurrentProgress = CreateSampleProgress();

        await _viewModel.LoadAsync();

        Assert.Equal(new DateOnly(2026, 9, 15), _viewModel.PeriodStart);
        Assert.Equal(new DateOnly(2026, 10, 15), _viewModel.PeriodEnd);
        Assert.Equal(41723m, _viewModel.ProjectedEndingBalance);
        Assert.Equal(40554m, _viewModel.PlannedEndingBalance);
        Assert.Equal(0.68, _viewModel.BudgetRatio, 2);
        Assert.Equal(ScreenState.Content, _viewModel.State);
    }

    [Fact]
    public async Task YukleAsync_GozlemVarsa_GozlemAlanlariDoldurulur()
    {
        _progressService.CurrentProgress = CreateSampleProgress();

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasObservation);
        Assert.Equal(12400m, _viewModel.LastObservedBalance);
        Assert.Equal(new DateOnly(2026, 9, 20), _viewModel.LastObservedOn);
    }

    [Fact]
    public async Task YukleAsync_KalanOdemelerVarsa_RemainingLinesListesiDoldurulur()
    {
        _progressService.CurrentProgress = CreateSampleProgress();

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasRemainingLines);
        Assert.Equal(3, _viewModel.RemainingLines.Count);
        Assert.Equal(6450m, _viewModel.RemainingPlannedTotal);
        Assert.Equal("Giyim", _viewModel.RemainingLines[0].Name);
        Assert.Equal(4500m, _viewModel.RemainingLines[0].Amount);
        Assert.False(_viewModel.HasOverflow);
    }

    [Fact]
    public async Task YukleAsync_UctenFazlaKalanOdemeVarsa_IlkUcuGosterilirKalaniTasmaSayilir()
    {
        _progressService.CurrentProgress = CreateSampleProgress(lineCount: 5);

        await _viewModel.LoadAsync();

        Assert.Equal(3, _viewModel.RemainingLines.Count);
        Assert.Equal(5, _viewModel.RemainingCount);
        Assert.Equal(2, _viewModel.OverflowCount);
        Assert.True(_viewModel.HasOverflow);
    }

    [Fact]
    public async Task YukleAsync_AcikDonemVarsa_KalanButceVeHarcananHamDegerOlarakSunulur()
    {
        _progressService.CurrentProgress = CreateSampleProgress();

        await _viewModel.LoadAsync();

        Assert.Equal(6800m, _viewModel.RemainingVariableExpenseAllowance);
        Assert.Equal(3200m, _viewModel.ObservedLivingSpend);
        Assert.Equal(12, _viewModel.ElapsedDays);
        Assert.Equal(30, _viewModel.TotalDays);
    }

    [Fact]
    public async Task YukleAsync_GidisatOkunamazsa_ScreenStateErrorOlur()
    {
        _progressService.Failure = new InvalidOperationException("Gidişat okunamadı.");

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task YukleAsync_Surerken_ScreenStateLoadingOlur()
    {
        var pending = new TaskCompletionSource<PeriodProgress?>();
        _progressService.Pending = pending;

        var load = _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Loading, _viewModel.State);
        pending.SetResult(null);
        await load;
    }

    [Fact]
    public async Task YukleAsync_PlanYoksa_ScreenStateEmptyOlur()
    {
        _progressService.CurrentProgress = null;

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.Null(_viewModel.ProjectedEndingBalance);
    }

    [Fact]
    public async Task GozlemKaydetAsync_GecerliTutarGirilince_WorkflowaIletilirVeYenidenYuklenir()
    {
        _progressService.CurrentProgress = CreateSampleProgress();
        _viewModel.CurrentBalanceInput = "15000";

        await _viewModel.SaveObservationAsync();

        Assert.Equal(15000m, _workflowService.LastObservedBalance);
        Assert.Equal(string.Empty, _viewModel.CurrentBalanceInput);
    }

    [Fact]
    public async Task YukleAsync_DonemKapanisiGeldiyse_AlertGosterilir()
    {
        _progressService.CurrentProgress = CreateSampleProgress();
        _workflowService.Availability = new PeriodSettlementAvailability
        {
            HasCurrentSnapshot = true,
            IsDue = true,
            CurrentSnapshot = null,
            PendingPlan = null
        };

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasActiveAlert);
    }

    [Fact]
    public async Task GezinmeKomutlari_DogruRotalariAcar()
    {
        await _viewModel.ClosePeriodAsync();
        Assert.Equal(Routes.PeriodSettlement, _navigation.LastNavigatedRoute);

        await _viewModel.OpenProjectionAsync();
        Assert.Equal(Routes.Projection, _navigation.LastNavigatedRoute);

        await _viewModel.OpenHistoryAsync();
        Assert.Equal(Routes.History, _navigation.LastNavigatedRoute);

        await _viewModel.OpenFinancialStructureAsync();
        Assert.Equal(Routes.FinancialStructure, _navigation.LastNavigatedRoute);

        await _viewModel.OpenSimulationAsync();
        Assert.Equal(Routes.Simulation, _navigation.LastNavigatedRoute);

        await _viewModel.OpenSettingsAsync();
        Assert.Equal(Routes.Settings, _navigation.LastNavigatedRoute);
    }

    private static PeriodProgress CreateSampleProgress(int lineCount = 3)
    {
        var planId = Guid.NewGuid();
        var snoozedId = Guid.NewGuid();
        var lines = new List<PeriodPlanPaymentLine>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PeriodPlanSnapshotId = planId,
                PlannedDate = new DateOnly(2026, 9, 21),
                Name = "Giyim",
                PlannedAmount = 4500m,
                SourceType = PlanPaymentSourceType.InstallmentPayment,
                Detail = "Taksit",
                IsEstimate = false
            },
            new()
            {
                Id = snoozedId,
                PeriodPlanSnapshotId = planId,
                PlannedDate = new DateOnly(2026, 9, 24),
                Name = "Market",
                PlannedAmount = 1200m,
                SourceType = PlanPaymentSourceType.OtherScheduledPayment,
                Detail = string.Empty,
                IsEstimate = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                PeriodPlanSnapshotId = planId,
                PlannedDate = new DateOnly(2026, 9, 27),
                Name = "Denizbank",
                PlannedAmount = 750m,
                SourceType = PlanPaymentSourceType.CreditCard,
                Detail = "Ekstre",
                IsEstimate = false
            }
        };
        for (var i = lines.Count; i < lineCount; i++)
        {
            lines.Add(new()
            {
                Id = Guid.NewGuid(),
                PeriodPlanSnapshotId = planId,
                PlannedDate = new DateOnly(2026, 10, 1).AddDays(i),
                Name = $"Ek ödeme {i}",
                PlannedAmount = 100m,
                SourceType = PlanPaymentSourceType.OtherScheduledPayment,
                Detail = string.Empty,
                IsEstimate = false
            });
        }

        return new PeriodProgress
        {
            PeriodPlanSnapshotId = planId,
            PeriodStart = new DateOnly(2026, 9, 15),
            PeriodEnd = new DateOnly(2026, 10, 15),
            Today = new DateOnly(2026, 9, 27),
            ElapsedDays = 12,
            TotalDays = 30,
            RevisionCount = 0,
            PlannedIncome = 50000m,
            PlannedMandatoryPayments = 15000m,
            PlannedEndingBalance = 40554m,
            PlannedVariableExpenseAllowance = 10000m,
            PlannedDeficitInterest = 0m,
            ObservedLivingSpend = 3200m,
            RemainingVariableExpenseAllowance = 6800m,
            ProjectedDeficitInterest = 0m,
            ProjectedEndingBalance = 41723m,
            Cards = [],
            Observation = new PeriodObservation
            {
                PeriodPlanSnapshotId = planId,
                ObservedOn = new DateOnly(2026, 9, 20),
                ObservedBalance = 12400m,
                ObservedLivingSpend = 3200m
            },
            RemainingLines = lines,
            RemainingPlannedTotal = 6450m,
            IsClosable = true,
            SnoozedLineIds = new HashSet<Guid> { snoozedId }
        };
    }

    public void Dispose()
    {
        _profileService.Dispose();
    }
}
