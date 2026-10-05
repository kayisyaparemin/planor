using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Dönem kapanışı özet ekranının (EK-V11, GS31, S78) görünüm modeli testleri:
/// Dönem sonu bakiyesi, plana göre fark, yaşam harcaması ve borç gerçekleşmesi,
/// KMH faizi, bakiye değiştirme, kesinleştirme ve üç durum.
/// </summary>
public sealed class PeriodSettlementViewModelTests
{
    private static readonly DateOnly Start = new(2026, 9, 10);
    private static readonly DateOnly End = new(2026, 10, 10);
    private static readonly DateOnly SettlementDue = new(2026, 10, 10);
    private static readonly Guid PlanId = Guid.NewGuid();
    private static readonly Guid SnapshotId = Guid.NewGuid();

    private readonly FakePeriodWorkflowService _workflow = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialogs = new();
    private readonly PeriodSettlementViewModel _viewModel;

    public PeriodSettlementViewModelTests()
    {
        SetupDefaultContext();
        _viewModel = new PeriodSettlementViewModel(_workflow, _navigation, _dialogs);
    }

    [Fact]
    public async Task Yukle_KapatilacakDonemYoksa_BosDurumGosterir()
    {
        _workflow.Availability = new PeriodSettlementAvailability
        {
            HasCurrentSnapshot = true,
            IsDue = false,
            PendingPlan = null
        };

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.False(_viewModel.CanFinalize);
    }

    [Fact]
    public async Task Yukle_KapatilabilirDonem_OzetVeFarklariBasariylaDoldurur()
    {
        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.True(_viewModel.CanFinalize);
        Assert.Equal(Start, _viewModel.PeriodStart);
        Assert.Equal(new DateOnly(2026, 10, 9), _viewModel.PeriodLastDay);
        Assert.Equal(End, _viewModel.NextPeriodStart);

        // Hero: 42.180 ₺ gerçekleşen, 43.900 ₺ planlanan, −1.720 ₺ fark (geride)
        Assert.Equal(42180m, _viewModel.EndingBalance);
        Assert.Equal(43900m, _viewModel.PlannedEndingBalance);
        Assert.Equal(-1720m, _viewModel.EndingDeviation);
        Assert.True(_viewModel.IsBehindPlan);
        Assert.False(_viewModel.IsAheadOfPlan);

        // Yaşam gideri: 29.650 ₺ plan, 28.750 ₺ fiilî -> +900 ₺ tasarruf
        Assert.Equal(29650m, _viewModel.PlannedLivingSpend);
        Assert.Equal(28750m, _viewModel.ActualLivingSpend);
        Assert.Equal(900m, _viewModel.LivingDifference);
        Assert.True(_viewModel.IsLivingSaved);
        Assert.False(_viewModel.IsLivingOverspent);

        // Ödemeler: 7 ödemenin 7'si ödendi, 0 fark
        Assert.Equal(7, _viewModel.TotalPaymentsCount);
        Assert.Equal(7, _viewModel.PaidPaymentsCount);
        Assert.Equal(0m, _viewModel.PaymentsDifference);

        // Kapanış bakiyesi
        Assert.Equal(42180m, _viewModel.ConfirmedBalance);
        Assert.Equal(new DateOnly(2026, 10, 9), _viewModel.ConfirmedDate);
    }

    [Fact]
    public async Task Yukle_KMHFaiziVarsa_FaizSatiriGorunur()
    {
        // 120 ₺ KMH faizi olan önizleme
        _workflow.Preview = new PeriodSettlementPreview
        {
            DerivedEndingBalance = 42180m,
            ConfirmedEndingBalance = 42180m,
            ReconciliationAdjustment = 0m,
            Comparison = new PlanActualComparison(
                43900m, 42180m, -1720m, "Özet",
                [
                    new PlanActualComparisonLine("Yaşam giderleri", 29650m, 28750m, -900m),
                    new PlanActualComparisonLine("Faiz", 0m, 120m, 120m)
                ])
        };

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasDeficitInterest);
        Assert.Equal(120m, _viewModel.ActualDeficitInterest);
    }

    [Fact]
    public async Task BakiyeDegistir_GecerliTutarGirilince_OnizlemeyiGunceller()
    {
        await _viewModel.LoadAsync();

        _dialogs.NextPromptResponse = "45000";
        _workflow.Preview = new PeriodSettlementPreview
        {
            DerivedEndingBalance = 42180m,
            ConfirmedEndingBalance = 45000m,
            ReconciliationAdjustment = 2820m,
            Comparison = new PlanActualComparison(
                43900m, 45000m, 1100m, "Özet",
                [
                    new PlanActualComparisonLine("Yaşam giderleri", 29650m, 28750m, -900m)
                ])
        };

        await _viewModel.ChangeBalanceAsync();

        Assert.Equal(45000m, _viewModel.ConfirmedBalance);
        Assert.Equal(45000m, _viewModel.EndingBalance);
        Assert.Equal(1100m, _viewModel.EndingDeviation);
        Assert.True(_viewModel.IsAheadOfPlan);
    }

    [Fact]
    public async Task DonemiKapat_FinalizeCagrilirVeAnaSayfayaDoner()
    {
        await _viewModel.LoadAsync();

        await _viewModel.ClosePeriodAsync();

        Assert.NotNull(_workflow.FinalizedDraft);
        Assert.Equal(PlanId, _workflow.FinalizedDraft!.PeriodPlanSnapshotId);
        Assert.Equal(Routes.Dashboard, _navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task Ertele_AnaSayfayaDoner()
    {
        await _viewModel.LoadAsync();

        await _viewModel.DismissAsync();

        Assert.Equal(Routes.Dashboard, _navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task Yukle_HataOlustugunda_HataDurumunaGecer()
    {
        _workflow.Availability = new PeriodSettlementAvailability
        {
            HasCurrentSnapshot = true,
            IsDue = true,
            PendingPlan = new PeriodPlanSnapshot
            {
                Id = PlanId,
                FinancialSnapshotId = SnapshotId,
                PeriodStart = Start,
                PeriodEnd = End,
                SettlementAvailableFrom = SettlementDue,
                OpeningBalance = 30000m,
                PlannedIncome = 70000m,
                PlannedVariableExpenseAllowance = 29650m,
                CreatedAtUtc = DateTimeOffset.UtcNow
            }
        };
        _workflow.Context = null; // GetSettlementContextAsync exception fırlatır

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Error, _viewModel.State);
    }

    [Fact]
    public async Task Yukle_YalnizBakiyeGozlemiVarsa_OdemeDurumuSayisiVeKapanisBakiyesiDogrudur()
    {
        // Kullanıcı yalnız bakiye girmiş, ödeme işareti koymamış (Payments = [])
        _workflow.ObservedDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = PlanId,
            Payments = [],
            ActualLivingSpend = 28750m,
            ActualInterest = 0m,
            ConfirmedEndingBalance = 42180m
        };

        await _viewModel.LoadAsync();

        // 7 ödemenin 7'si sayılmalı (0/0 kalmamalı)
        Assert.Equal(7, _viewModel.TotalPaymentsCount);
        Assert.Equal(7, _viewModel.PaidPaymentsCount);
    }

    private void SetupDefaultContext()
    {
        var plan = new PeriodPlanSnapshot
        {
            Id = PlanId,
            FinancialSnapshotId = SnapshotId,
            PeriodStart = Start,
            PeriodEnd = End,
            SettlementAvailableFrom = SettlementDue,
            OpeningBalance = 30000m,
            PlannedIncome = 70000m,
            PlannedVariableExpenseAllowance = 29650m,
            PlannedEndingBalance = 43900m,
            PaymentLines = Enumerable.Range(1, 7).Select(i => new PeriodPlanPaymentLine
            {
                Id = Guid.NewGuid(),
                PeriodPlanSnapshotId = PlanId,
                SourceEntityId = Guid.NewGuid(),
                SourceType = PlanPaymentSourceType.Loan,
                Name = $"Ödeme {i}",
                PlannedDate = Start.AddDays(i * 3),
                PlannedAmount = 1000m
            }).ToArray(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var snapshot = new FinancialSnapshot
        {
            Id = SnapshotId,
            SnapshotDate = Start,
            ProjectionAnchorDate = Start,
            ProjectionOpeningBalance = 30000m,
            Anchor = new PeriodAnchor(10),
            Source = FinancialSnapshotSource.MonthlyUpdate,
            IsCurrent = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _workflow.Availability = new PeriodSettlementAvailability
        {
            HasCurrentSnapshot = true,
            IsDue = true,
            CurrentSnapshot = snapshot,
            PendingPlan = plan
        };

        _workflow.Context = new PeriodSettlementContext
        {
            Snapshot = snapshot,
            OriginalPlan = plan,
            Revision = null,
            RevisionCount = 0,
            Actual = null,
            SuggestedStartingBalance = 42180m,
            Comparison = null
        };

        _workflow.ObservedDraft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = PlanId,
            Payments = plan.PaymentLines.Select(l => new ActualPaymentDraft
            {
                PeriodPlanPaymentLineId = l.Id,
                Status = ActualPaymentStatus.Paid,
                ActualAmount = 1000m,
                ActualPaymentDate = l.PlannedDate
            }).ToArray(),
            ActualLivingSpend = 28750m,
            ActualInterest = 0m,
            ConfirmedEndingBalance = 42180m
        };

        _workflow.Preview = new PeriodSettlementPreview
        {
            DerivedEndingBalance = 42180m,
            ConfirmedEndingBalance = 42180m,
            ReconciliationAdjustment = 0m,
            Comparison = new PlanActualComparison(
                43900m, 42180m, -1720m, "Özet",
                [
                    new PlanActualComparisonLine("Yaşam giderleri", 29650m, 28750m, -900m),
                    new PlanActualComparisonLine("Faiz", 0m, 0m, 0m)
                ]),
            TotalPaymentsCount = 7,
            PaidPaymentsCount = 7
        };
    }
}
