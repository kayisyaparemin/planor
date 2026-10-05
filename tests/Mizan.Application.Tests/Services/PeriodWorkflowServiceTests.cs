using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PeriodWorkflowServiceTests
{
    private static readonly DateOnly InitialDate = new(2026, 9, 1);
    private static readonly DateOnly SettlementDate = new(2026, 10, 1);
    private static readonly DateTimeOffset InitialNowUtc = new(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly MutableClock _clock = new(InitialDate, InitialNowUtc);
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();
    private readonly InMemoryPeriodObservationRepository _observationRepo = new();

    private PeriodWorkflowService CreateService(out PeriodSettlementService settlementService, out FakePlanReader planReader)
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var cardStatementCalc = new CreditCardStatementCalculator();
        var cardReconciler = new CreditCardActualPaymentReconciler();
        var loanReconciler = new LoanInstrumentReconciler(amortCalc, loanBuilder);

        var projectionCalc = new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            cardStatementCalc,
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());

        var planSnapshotService = new PeriodPlanSnapshotService(projectionCalc, periodCalc);
        var snapshotService = new FinancialSnapshotService(_historyRepo, _clock, planSnapshotService, periodCalc);
        var instrumentService = new FinancialInstrumentReconciliationService(
            cardReconciler,
            cardStatementCalc,
            loanReconciler);
        var comparisonCalc = new PlanActualComparisonCalculator();

        settlementService = new PeriodSettlementService(
            _historyRepo,
            _clock,
            snapshotService,
            instrumentService,
            comparisonCalc);

        planReader = new FakePlanReader();

        return new PeriodWorkflowService(
            _historyRepo,
            _observationRepo,
            planReader,
            settlementService,
            _clock);
    }

    [Fact]
    public void Yapici_NullParametreleri_Reddeder()
    {
        CreateService(out var settlementService, out var planReader);

        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(null!, _observationRepo, planReader, settlementService, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, null!, planReader, settlementService, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, _observationRepo, null!, settlementService, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, _observationRepo, planReader, null!, _clock));
        Assert.Throws<ArgumentNullException>(() => new PeriodWorkflowService(_historyRepo, _observationRepo, planReader, settlementService, null!));
    }

    [Fact]
    public async Task GetSettlementAvailabilityAsync_TarihceYokken_HazirDegilDoner()
    {
        var service = CreateService(out _, out _);

        var availability = await service.GetSettlementAvailabilityAsync();

        Assert.False(availability.HasCurrentSnapshot);
        Assert.False(availability.IsDue);
    }

    [Fact]
    public async Task GetSettlementAvailabilityAsync_CapaGunuGeldiginde_HazirDoner()
    {
        var service = CreateService(out _, out _);
        await SeedCurrentPlanAsync();
        _clock.SetDate(SettlementDate);

        var availability = await service.GetSettlementAvailabilityAsync();

        Assert.True(availability.HasCurrentSnapshot);
        Assert.True(availability.IsDue);
        Assert.NotNull(availability.PendingPlan);
    }

    [Fact]
    public async Task GetSettlementContextAsync_AcikPlanin_BaglaminiGetirir()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var context = await service.GetSettlementContextAsync();

        Assert.NotNull(context.OriginalPlan);
        Assert.Equal(plan.Id, context.OriginalPlan.Id);
        Assert.True(context.SuggestedStartingBalance > 0m);
    }

    [Fact]
    public async Task GetObservedSettlementDraftAsync_GozlemYoksa_NullDoner()
    {
        var service = CreateService(out _, out _);

        var draft = await service.GetObservedSettlementDraftAsync(Guid.NewGuid());

        Assert.Null(draft);
    }

    [Fact]
    public async Task GetObservedSettlementDraftAsync_GozlemVeIsaretVarsa_TaslagiUretir_FlowsBosKalir()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        var lineId = plan.PaymentLines[0].Id;

        await _observationRepo.UpsertPeriodObservationAsync(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedBalance = 42_000m
        });
        await _observationRepo.UpsertPaymentMarkAsync(new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = plan.Id,
            PeriodPlanPaymentLineId = lineId,
            Status = ActualPaymentStatus.Paid,
            ActualAmount = 5_000m,
            ActualPaymentDate = new DateOnly(2026, 9, 15),
            Note = "Kira ödendi"
        });

        var draft = await service.GetObservedSettlementDraftAsync(plan.Id);

        Assert.NotNull(draft);
        Assert.Equal(plan.Id, draft.PeriodPlanSnapshotId);
        Assert.Equal(42_000m, draft.ConfirmedEndingBalance);
        Assert.Equal(0m, draft.ActualLivingSpend);
        Assert.Equal(0m, draft.ActualInterest);
        Assert.Equal(string.Empty, draft.ActualNote);
        Assert.Empty(draft.Flows);
        Assert.Single(draft.Payments);
        Assert.Equal(lineId, draft.Payments[0].PeriodPlanPaymentLineId);
        Assert.Equal(5_000m, draft.Payments[0].ActualAmount);
        Assert.Equal(ActualPaymentStatus.Paid, draft.Payments[0].Status);
        Assert.Equal("Kira ödendi", draft.Payments[0].Note);
    }

    [Fact]
    public async Task GetObservedSettlementDraftAsync_YalnizIsaretVarsa_BakiyesizTaslakUretir()
    {
        // Hazırla — kullanıcı yalnız ödemeyi işaretledi, hiç bakiye girmedi
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        await _observationRepo.UpsertPaymentMarkAsync(new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = plan.Id,
            PeriodPlanPaymentLineId = plan.PaymentLines[0].Id,
            Status = ActualPaymentStatus.Paid,
            ActualAmount = 10_000m
        });

        // Uygula
        var draft = await service.GetObservedSettlementDraftAsync(plan.Id);

        // Doğrula
        Assert.NotNull(draft);
        Assert.Null(draft.ConfirmedEndingBalance);
        Assert.Single(draft.Payments);
    }


    [Fact]
    public async Task PreviewSettlementAsync_TaslagiHesaplar()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ConfirmedEndingBalance = 45_000m,
            ActualLivingSpend = 7_000m
        };

        var preview = await service.PreviewSettlementAsync(draft);

        Assert.NotNull(preview);
        Assert.Equal(45_000m, preview.ConfirmedEndingBalance);
        Assert.NotNull(preview.Comparison);
    }

    [Fact]
    public async Task FinalizeSettlementAsync_DonemiKapatir_GozlemleriSilmez()
    {
        var service = CreateService(out _, out var planReader);
        var samplePlan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = samplePlan;

        var plan = await SeedCurrentPlanAsync();
        _clock.SetDate(SettlementDate);

        await _observationRepo.UpsertPeriodObservationAsync(new PeriodObservation
        {
            PeriodPlanSnapshotId = plan.Id,
            ObservedBalance = 48_000m
        });

        var draft = new PeriodSettlementDraft
        {
            PeriodPlanSnapshotId = plan.Id,
            ConfirmedEndingBalance = 48_000m,
            ActualLivingSpend = 6_000m,
            Payments =
            [
                new ActualPaymentDraft
                {
                    PeriodPlanPaymentLineId = plan.PaymentLines[0].Id,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = plan.PaymentLines[0].PlannedAmount!.Value,
                    ActualPaymentDate = new DateOnly(2026, 9, 10)
                }
            ]
        };

        var result = await service.FinalizeSettlementAsync(draft);

        Assert.NotNull(result);
        Assert.NotNull(result.NewSnapshot);
        Assert.NotNull(result.NewPlan);
        Assert.Equal(SettlementDate, result.NewPlan.PeriodStart);

        Assert.Single(await _observationRepo.GetPeriodObservationsAsync(plan.Id));
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_AcikPlanYoksa_HataFirlatir()
    {
        var service = CreateService(out _, out _);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ObserveCurrentBalanceAsync(30_000m));
        Assert.Contains("güncel bir dönem planı", ex.Message);
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_GozlemYoksa_YeniKayitOlusturur()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var observation = await service.ObserveCurrentBalanceAsync(25_000m);

        Assert.Equal(plan.Id, observation.PeriodPlanSnapshotId);
        Assert.Equal(25_000m, observation.ObservedBalance);
        Assert.Equal(InitialDate, observation.ObservedOn);

        var stored = Assert.Single(await _observationRepo.GetPeriodObservationsAsync(plan.Id));
        Assert.Equal(25_000m, stored.ObservedBalance);
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_FarkliGundeIkinciGiris_YeniNoktaEkler()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        await service.ObserveCurrentBalanceAsync(20_000m);
        _clock.SetDate(new DateOnly(2026, 9, 10));

        var ikinci = await service.ObserveCurrentBalanceAsync(18_500m);

        Assert.Equal(new DateOnly(2026, 9, 10), ikinci.ObservedOn);
        var kayitlar = await _observationRepo.GetPeriodObservationsAsync(plan.Id);
        Assert.Equal(20_000m, kayitlar[0].ObservedBalance);
        Assert.Equal(18_500m, kayitlar[1].ObservedBalance);
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_AyniGundeIkinciGiris_OncekininYerineGecer()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        await service.ObserveCurrentBalanceAsync(20_000m);
        await service.ObserveCurrentBalanceAsync(18_500m);

        var kayit = Assert.Single(await _observationRepo.GetPeriodObservationsAsync(plan.Id));
        Assert.Equal(18_500m, kayit.ObservedBalance);
    }


    [Fact]
    public async Task ObserveCurrentBalanceAsync_GeriyeTarihliGiris_SeriyeKendiGunuyleGirer()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        _clock.SetDate(new DateOnly(2026, 9, 20));
        await service.ObserveCurrentBalanceAsync(20_000m);

        var geriye = await service.ObserveCurrentBalanceAsync(30_000m, new DateOnly(2026, 9, 10));

        Assert.Equal(new DateOnly(2026, 9, 10), geriye.ObservedOn);
        var kayitlar = await _observationRepo.GetPeriodObservationsAsync(plan.Id);
        Assert.Equal([new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 20)], kayitlar.Select(x => x.ObservedOn));
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_GelecekGun_Reddeder()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        _clock.SetDate(new DateOnly(2026, 9, 10));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ObserveCurrentBalanceAsync(20_000m, new DateOnly(2026, 9, 11)));
        Assert.Empty(await _observationRepo.GetPeriodObservationsAsync(plan.Id));
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_DonemBasindanOncekiGun_Reddeder()
    {
        var service = CreateService(out _, out _);
        await SeedCurrentPlanAsync();
        _clock.SetDate(new DateOnly(2026, 9, 10));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ObserveCurrentBalanceAsync(20_000m, new DateOnly(2026, 8, 31)));
        Assert.Contains("dönemin dışında", hata.Message);
    }

    [Fact]
    public async Task ObserveCurrentBalanceAsync_KapanisiErtelenmisDonem_OnceKapanisiIster()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        _clock.SetDate(new DateOnly(2026, 10, 3));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ObserveCurrentBalanceAsync(20_000m, new DateOnly(2026, 9, 30)));
        Assert.Contains("kapanış", hata.Message);
        Assert.Empty(await _observationRepo.GetPeriodObservationsAsync(plan.Id));
    }

    [Fact]
    public async Task ObservePaymentAsync_SatirPlandaYoksa_HataFirlatir()
    {
        var service = CreateService(out _, out _);
        await SeedCurrentPlanAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ObservePaymentAsync(Guid.NewGuid(), ActualPaymentStatus.Paid, 1_000m));
        Assert.Contains("bulunamadı", ex.Message);
    }

    [Fact]
    public async Task ObservePaymentAsync_SatirPlandaysa_IsaretiKaydeder()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        var lineId = plan.PaymentLines[0].Id;

        var mark = await service.ObservePaymentAsync(
            lineId,
            ActualPaymentStatus.Paid,
            5_000m,
            new DateOnly(2026, 9, 5),
            "  Açıklama notu  ");

        Assert.Equal(plan.Id, mark.PeriodPlanSnapshotId);
        Assert.Equal(lineId, mark.PeriodPlanPaymentLineId);
        Assert.Equal(5_000m, mark.ActualAmount);
        Assert.Equal(new DateOnly(2026, 9, 5), mark.ActualPaymentDate);
        Assert.Equal("Açıklama notu", mark.Note);
        Assert.Equal([mark], await _observationRepo.GetPaymentMarksAsync(plan.Id));
    }

    [Fact]
    public async Task ObservePaymentAsync_TarihVerilmediyse_BugunuYazar()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var mark = await service.ObservePaymentAsync(plan.PaymentLines[0].Id, ActualPaymentStatus.Paid, 10_000m);

        Assert.Equal(InitialDate, mark.ActualPaymentDate);
    }

    [Fact]
    public async Task ObservePaymentAsync_SatirSonRevizyondaysa_IsaretiKaydeder()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        var newRevisionLineId = Guid.NewGuid();
        var revision = new PeriodPlanRevision
        {
            PeriodPlanSnapshotId = plan.Id,
            RevisionNumber = 1,
            CreatedAtUtc = _clock.UtcNow,
            PaymentLines =
            [
                new PeriodPlanPaymentLine
                {
                    Id = newRevisionLineId,
                    PeriodPlanSnapshotId = plan.Id,
                    SourceType = PlanPaymentSourceType.Loan,
                    SourceEntityId = Guid.NewGuid(),
                    Name = "Yeni Revize Kredi",
                    PlannedDate = new DateOnly(2026, 9, 20),
                    PlannedAmount = 3_000m
                }
            ]
        };
        await _historyRepo.SavePeriodPlanRevisionAsync(revision);

        var mark = await service.ObservePaymentAsync(newRevisionLineId, ActualPaymentStatus.Paid, 3_000m);

        Assert.Equal(newRevisionLineId, mark.PeriodPlanPaymentLineId);
    }

    [Fact]
    public async Task ObservePaymentAsync_AyniSatiraIkinciIsaret_OncekininYerineGecer()
    {
        // Hazırla
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        var lineId = plan.PaymentLines[0].Id;
        await service.ObservePaymentAsync(lineId, ActualPaymentStatus.Paid, 10_000m);

        // Uygula — kullanıcı fikrini değiştirdi
        await service.ObservePaymentAsync(lineId, ActualPaymentStatus.Unpaid, 0m);

        // Doğrula
        var marks = await _observationRepo.GetPaymentMarksAsync(plan.Id);
        Assert.Equal(ActualPaymentStatus.Unpaid, Assert.Single(marks).Status);
    }

    /// <summary>I87: işaret gözlemden bağımsızdır (S68-8).</summary>
    [Fact]
    public async Task ObservePaymentAsync_GozlemVarken_GozlemiDegistirmez()
    {
        // Hazırla — 1 Eylül'de 20.000 girildi, sonra 10 Eylül'de 5 Eylül'deki ödeme işaretlenecek
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();
        var gozlem = await service.ObserveCurrentBalanceAsync(20_000m);
        _clock.SetDate(new DateOnly(2026, 9, 10));

        // Uygula
        await service.ObservePaymentAsync(
            plan.PaymentLines[0].Id, ActualPaymentStatus.Paid, 10_000m, new DateOnly(2026, 9, 5));

        // Doğrula
        Assert.Equal(gozlem, Assert.Single(await _observationRepo.GetPeriodObservationsAsync(plan.Id)));
    }

    [Fact]
    public async Task ObservePaymentAsync_GozlemYokken_GozlemOlusturmaz()
    {
        var service = CreateService(out _, out _);
        var plan = await SeedCurrentPlanAsync();

        await service.ObservePaymentAsync(plan.PaymentLines[0].Id, ActualPaymentStatus.Paid, 10_000m);

        Assert.Empty(await _observationRepo.GetPeriodObservationsAsync(plan.Id));
    }


    private async Task<PeriodPlanSnapshot> SeedCurrentPlanAsync()
    {
        var snapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SnapshotDate = InitialDate,
            ProjectionAnchorDate = InitialDate,
            ProjectionOpeningBalance = 50_000m,
            Anchor = new PeriodAnchor(1),
            Source = FinancialSnapshotSource.Initial,
            IsCurrent = true,
            CreatedAtUtc = InitialNowUtc
        };

        var frozenPlan = new PeriodPlanSnapshot
        {
            Id = Guid.NewGuid(),
            FinancialSnapshotId = snapshot.Id,
            PeriodStart = InitialDate,
            PeriodEnd = SettlementDate,
            SettlementAvailableFrom = SettlementDate,
            OpeningBalance = 50_000m,
            PlannedIncome = 60_000m,
            PlannedVariableExpenseAllowance = 20_000m,
            CreatedAtUtc = InitialNowUtc,
            PaymentLines =
            [
                new PeriodPlanPaymentLine
                {
                    Id = Guid.NewGuid(),
                    PeriodPlanSnapshotId = Guid.Empty,
                    SourceType = PlanPaymentSourceType.Loan,
                    SourceEntityId = Guid.NewGuid(),
                    Name = "Kira / Kredi",
                    PlannedDate = new DateOnly(2026, 9, 10),
                    PlannedAmount = 10_000m
                }
            ]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);
        return frozenPlan;
    }

    private static FinancialPlan CreateSampleFinancialPlan()
    {
        var incomeId = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionOpeningBalance = 50_000m,
                ProjectionAnchorDate = InitialDate,
                PeriodVariableExpenseAllowance = 20_000m
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = incomeId,
                    Name = "Gelir",
                    PaymentDay = 1,
                    IsActive = true
                }
            ],
            IncomeHistories =
            [
                new IncomeAmountHistory
                {
                    RecurringIncomeId = incomeId,
                    Amount = 60_000m,
                    EffectiveDate = InitialDate
                }
            ]
        };
    }

    private sealed class FakePlanReader : IPlanReader
    {
        public FinancialPlan PlanToReturn { get; set; } = new();

        public Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(PlanToReturn);

        public Task<ProjectionQueryPlan> GetProjectionPlanAsync(DateOnly asOf, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProjectionQueryPlan(PlanToReturn, null));
    }

    private sealed class MutableClock(DateOnly today, DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today { get; private set; } = today;
        public DateTimeOffset UtcNow { get; private set; } = utcNow;
        public DateTimeOffset Now => UtcNow;

        public void SetDate(DateOnly today)
        {
            Today = today;
            UtcNow = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }
    }
}
