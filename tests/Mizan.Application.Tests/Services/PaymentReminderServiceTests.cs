using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class PaymentReminderServiceTests
{
    private static readonly DateOnly InitialDate = new(2026, 9, 1);
    private static readonly DateOnly SettlementDate = new(2026, 10, 1);
    private static readonly DateTimeOffset InitialNowUtc = new(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly MutableClock _clock = new(InitialDate, InitialNowUtc);
    private readonly InMemoryPaymentReminderRepository _reminderRepo = new();
    private readonly InMemoryPeriodHistoryRepository _historyRepo = new();
    private readonly InMemoryPeriodObservationRepository _observationRepo = new();

    private PaymentReminderService CreateService(out FinancialProjectionService projectionService, out FakePlanReader planReader)
    {
        var periodCalc = new CashFlowPeriodCalculator();
        var scheduleCalc = new LoanScheduleCalculator();
        var amortCalc = new LoanAmortizationCalculator(scheduleCalc);
        var loanBuilder = new LoanPaymentScheduleBuilder(scheduleCalc, amortCalc);
        var scheduledCalc = new ScheduledPaymentCalculator();
        var cardStatementCalc = new CreditCardStatementCalculator();

        var projectionCalc = new FinancialProjectionCalculator(
            periodCalc,
            new IncomeProjectionCalculator(new IncomeResolver()),
            cardStatementCalc,
            new MandatoryPaymentCalculator(loanBuilder, scheduledCalc),
            new PeriodObligationGrouper());

        projectionService = new FinancialProjectionService(projectionCalc);
        planReader = new FakePlanReader();

        return new PaymentReminderService(
            _reminderRepo,
            _historyRepo,
            new PaymentDueCollector(_observationRepo, planReader, projectionService, cardStatementCalc));
    }

    [Fact]
    public void Yapici_NullParametreleri_Reddeder()
    {
        CreateService(out var projectionService, out var planReader);
        var cardStatementCalc = new CreditCardStatementCalculator();
        var dueCollector = new PaymentDueCollector(_observationRepo, planReader, projectionService, cardStatementCalc);

        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(null!, _historyRepo, dueCollector));
        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(_reminderRepo, null!, dueCollector));
        Assert.Throws<ArgumentNullException>(() => new PaymentReminderService(_reminderRepo, _historyRepo, null!));
        Assert.Throws<ArgumentNullException>(() => new PaymentDueCollector(null!, planReader, projectionService, cardStatementCalc));
        Assert.Throws<ArgumentNullException>(() => new PaymentDueCollector(_observationRepo, null!, projectionService, cardStatementCalc));
        Assert.Throws<ArgumentNullException>(() => new PaymentDueCollector(_observationRepo, planReader, null!, cardStatementCalc));
        Assert.Throws<ArgumentNullException>(() => new PaymentDueCollector(_observationRepo, planReader, projectionService, null!));
    }

    [Fact]
    public async Task GetModeAsync_Ve_SaveModeAsync_ModuKaydederVeOkur()
    {
        var service = CreateService(out _, out _);

        Assert.Equal(PaymentReminderMode.Off, await service.GetModeAsync());

        await service.SaveModeAsync(PaymentReminderMode.Aggressive);

        Assert.Equal(PaymentReminderMode.Aggressive, await service.GetModeAsync());
    }

    [Fact]
    public async Task GetRemindersAsync_ModKapaliyken_BosListeDoner()
    {
        var service = CreateService(out _, out _);
        await SeedCurrentPlanAsync();

        var reminders = await service.GetRemindersAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        Assert.Empty(reminders);
    }

    [Fact]
    public async Task GetRemindersAsync_ModAcikken_HatirlaticilariSiraliDoner()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);
        await service.SaveModeAsync(PaymentReminderMode.Relaxed);

        var now = new DateTime(2026, 9, 1, 10, 0, 0);
        var reminders = await service.GetRemindersAsync(now);

        Assert.NotEmpty(reminders);
        Assert.True(reminders[0].NotifyAt >= now);
    }

    [Fact]
    public async Task GetBoardAsync_ModKapaliyken_BosPanoDoner()
    {
        var service = CreateService(out _, out _);

        var board = await service.GetBoardAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        Assert.Equal(PaymentReminderMode.Off, board.Mode);
        Assert.Empty(board.Reminders);
        Assert.Empty(board.Upcoming);
        Assert.Null(board.Sample);
    }

    [Fact]
    public async Task GetBoardAsync_ModAcikken_PanoDetaylariniDoldurur()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);
        await service.SaveModeAsync(PaymentReminderMode.Relaxed);

        var now = new DateTime(2026, 9, 1, 10, 0, 0);
        var board = await service.GetBoardAsync(now);

        Assert.Equal(PaymentReminderMode.Relaxed, board.Mode);
        Assert.NotEmpty(board.Reminders);
        Assert.NotEmpty(board.Upcoming);
        Assert.NotNull(board.Sample);
    }

    [Fact]
    public async Task GetBoardAsync_KartaDonemIcindeHarcamaGirildiyse_HatirlaticiKartinGuncelOdemesiniGosterir()
    {
        // Hazırla — plan kartın 15 Eylül ödemesini 8.000 diye dondurdu; dönem içinde 2.095 harcama girildi
        var service = CreateService(out _, out var planReader);
        var card = CardWithInPeriodCharge();
        planReader.PlanToReturn = CreateSampleFinancialPlan() with { CreditCards = [card] };
        await SeedCurrentPlanAsync(planReader.PlanToReturn, CardLine(card, 8_000m));
        await service.SaveModeAsync(PaymentReminderMode.Relaxed);

        // Uygula
        var board = await service.GetBoardAsync(new DateTime(2026, 9, 4, 10, 0, 0));

        // Doğrula — hatırlatıcı ana sayfanın "şu an"ıyla aynı tutarı söyler (I23, S87-1)
        var payment = board.Upcoming.SelectMany(x => x.Payments).Single(x => x.Name == card.Name);
        Assert.Equal(10_095m, payment.Amount);
    }

    [Fact]
    public async Task GetBoardAsync_KartArtikKayitliDegilse_HatirlaticiPlandakiTutariGosterir()
    {
        // Hazırla — plandaki kart satırının kartı artık kayıtlı değil
        var service = CreateService(out _, out var planReader);
        var card = CardWithInPeriodCharge();
        planReader.PlanToReturn = CreateSampleFinancialPlan();
        await SeedCurrentPlanAsync(planReader.PlanToReturn, CardLine(card, 8_000m));
        await service.SaveModeAsync(PaymentReminderMode.Relaxed);

        // Uygula
        var board = await service.GetBoardAsync(new DateTime(2026, 9, 4, 10, 0, 0));

        // Doğrula — bugünkü tutar uydurulmaz, plandaki tutar kalır (S87-1)
        var payment = board.Upcoming.SelectMany(x => x.Payments).Single(x => x.Name == card.Name);
        Assert.Equal(8_000m, payment.Amount);
    }

    [Fact]
    public async Task RecordAnswerAsync_BosGeldiginde_HicbirSeyYapmaz()
    {
        var service = CreateService(out _, out _);

        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 5, 12, 0, 0),
            null,
            []));

        var responses = await service.GetResponsesAsync();
        Assert.Empty(responses);
    }

    [Fact]
    public async Task RecordAnswerAsync_YanitlariKaydeder_VePaidOlaninUstuneSnoozedYazmaz()
    {
        var service = CreateService(out _, out _);
        var dueKey = "loan:123:2026-09-10";

        // Önce Paid kaydet
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 5, 12, 0, 0),
            null,
            [new PaymentDue(dueKey, "Kredi", new DateOnly(2026, 9, 10), 10_000m)]));

        var stored = await service.GetResponsesAsync();
        Assert.Single(stored);
        Assert.Equal(PaymentReminderAnswerKind.Paid, stored[0].Kind);

        // Şimdi aynı anahtara Snoozed gönder
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Snoozed,
            new DateTime(2026, 9, 6, 12, 0, 0),
            new DateTime(2026, 9, 7, 9, 0, 0),
            [new PaymentDue(dueKey, "Kredi", new DateOnly(2026, 9, 10), 10_000m)]));

        var afterSnooze = await service.GetResponsesAsync();
        Assert.Single(afterSnooze);
        // Hâlâ Paid kalmalı!
        Assert.Equal(PaymentReminderAnswerKind.Paid, afterSnooze[0].Kind);
    }

    [Fact]
    public async Task UndoAnswerAsync_YanitiSiler()
    {
        var service = CreateService(out _, out _);
        var dueKey = "loan:123:2026-09-10";

        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 5, 12, 0, 0),
            null,
            [new PaymentDue(dueKey, "Kredi", new DateOnly(2026, 9, 10), 10_000m)]));

        await service.UndoAnswerAsync(dueKey);

        var responses = await service.GetResponsesAsync();
        Assert.Empty(responses);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_AcikPlanYoksa_BosListeDoner()
    {
        var service = CreateService(out _, out _);

        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        Assert.Empty(dues);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_OdenmisGozlemleri_VePaidYanitlariFiltreler()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        var snapshotPlan = await SeedCurrentPlanAsync(plan);

        var line = snapshotPlan.PaymentLines[0];
        var dueKey = PaymentReminderPlanner.DueKey(line.SourceEntityId, line.Name, line.PlannedDate);

        // 1. Hatırlatıcı yanıtı olarak Paid kaydet
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            new DateTime(2026, 9, 2, 10, 0, 0),
            null,
            [new PaymentDue(dueKey, line.Name, line.PlannedDate, line.PlannedAmount)]));

        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        // Paid yanıtı verildiği için listede olmamalı
        Assert.DoesNotContain(dues, x => x.Key == dueKey);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_OdendiIsaretliSatir_ListedeOlmaz()
    {
        // Hazırla
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        var snapshotPlan = await SeedCurrentPlanAsync(plan);
        var line = snapshotPlan.PaymentLines[0];
        var dueKey = PaymentReminderPlanner.DueKey(line.SourceEntityId, line.Name, line.PlannedDate);
        await _observationRepo.UpsertPaymentMarkAsync(new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = snapshotPlan.Id,
            PeriodPlanPaymentLineId = line.Id,
            Status = ActualPaymentStatus.Paid,
            ActualAmount = 10_000m
        });

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        // Doğrula
        Assert.DoesNotContain(dues, x => x.Key == dueKey);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_OdenmediIsaretliSatir_ListedeKalir()
    {
        // Hazırla
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        var snapshotPlan = await SeedCurrentPlanAsync(plan);
        var line = snapshotPlan.PaymentLines[0];
        var dueKey = PaymentReminderPlanner.DueKey(line.SourceEntityId, line.Name, line.PlannedDate);
        await _observationRepo.UpsertPaymentMarkAsync(new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = snapshotPlan.Id,
            PeriodPlanPaymentLineId = line.Id,
            Status = ActualPaymentStatus.Unpaid
        });

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 1, 10, 0, 0));

        // Doğrula
        Assert.Contains(dues, x => x.Key == dueKey);
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_UfukDonemiAstiginda_ProjeksiyondanVeBuyukHarcamalardanToplar()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        var expenseId = Guid.NewGuid();
        plan = plan with
        {
            PlannedLargeExpenses =
            [
                new PlannedLargeExpense
                {
                    Id = expenseId,
                    Name = "Yıllık Sigorta",
                    Amount = 15_000m,
                    Status = PlannedExpenseStatus.Planned,
                    ExactDate = new DateOnly(2026, 10, 3)
                }
            ]
        };
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);

        // 15 Eylül'de çağır: Ufuk 20 Ekim'e kadar uzanır (> 1 Ekim dönem sonu)
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 15, 10, 0, 0));

        Assert.NotEmpty(dues);
        Assert.Contains(dues, x => x.Name == "Yıllık Sigorta" && x.DueDate == new DateOnly(2026, 10, 3));
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_TaksitVadesiDonemBitisGunundeyse_ListedeYerAlir()
    {
        // Hazırla: açık dönem [1 Eylül, 1 Ekim); taksit her ayın 1'inde, ilki 1 Ekim'de.
        // Bitiş günü açık plana değil sonraki döneme aittir; hatırlatıcı onu oradan almalı.
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        var loan = plan.Loans[0] with { PaymentDay = 1, NextPaymentDate = SettlementDate };
        plan = plan with { Loans = [loan] };
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 15, 10, 0, 0));

        // Doğrula
        Assert.Contains(dues, x => x.Key == PaymentReminderPlanner.DueKey(loan.Id, loan.Name, SettlementDate));
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_BuyukHarcamaDonemBitisGunundeyse_ListedeYerAlir()
    {
        // Hazırla: açık dönem [1 Eylül, 1 Ekim); planlı büyük harcama tam 1 Ekim'de.
        var service = CreateService(out _, out var planReader);
        var expense = new PlannedLargeExpense
        {
            Id = Guid.NewGuid(),
            Name = "Okul Taksiti",
            Amount = 25_000m,
            Status = PlannedExpenseStatus.Planned,
            ExactDate = SettlementDate
        };
        var plan = CreateSampleFinancialPlan() with { PlannedLargeExpenses = [expense] };
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 15, 10, 0, 0));

        // Doğrula
        Assert.Contains(dues, x => x.Key == PaymentReminderPlanner.DueKey(expense.Id, expense.Name, SettlementDate));
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_UfukTamDonemBitisindeBitiyorsa_BitisGunuVadesiListededir()
    {
        // Hazırla: kurulumdan sonra açık dönem [1 Eylül, 1 Ekim) ileride başlıyor; bugün 27 Ağustos,
        // 35 günlük ufuk tam 1 Ekim'de bitiyor. Taksit 1 Ekim'de.
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        var loan = plan.Loans[0] with { PaymentDay = 1, NextPaymentDate = SettlementDate };
        plan = plan with { Loans = [loan] };
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 8, 27, 10, 0, 0));

        // Doğrula
        Assert.Single(dues, x => x.Key == PaymentReminderPlanner.DueKey(loan.Id, loan.Name, SettlementDate));
    }

    [Fact]
    public async Task GetUpcomingPaymentDuesAsync_AcikPlandaBitisGunuSatiriOdendiyse_ProjeksiyondanGeriGelmez()
    {
        // Hazırla: eski uygulamadan içe aktarılan açık plan (başlangıç, bitiş] modelinden geldiği için bitiş
        // gününe (1 Ekim) satır taşıyabilir. O satır ödendi işaretli; aynı taksiti projeksiyon da üretir.
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        var loan = plan.Loans[0] with { PaymentDay = 1, NextPaymentDate = SettlementDate };
        plan = plan with { Loans = [loan] };
        planReader.PlanToReturn = plan;
        var endDayLine = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            SourceType = PlanPaymentSourceType.Loan,
            SourceEntityId = loan.Id,
            Name = loan.Name,
            PlannedDate = SettlementDate,
            PlannedAmount = loan.MonthlyPayment
        };
        var snapshotPlan = await SeedCurrentPlanAsync(plan, endDayLine);
        await _observationRepo.UpsertPaymentMarkAsync(new PeriodPaymentMark
        {
            PeriodPlanSnapshotId = snapshotPlan.Id,
            PeriodPlanPaymentLineId = endDayLine.Id,
            Status = ActualPaymentStatus.Paid,
            ActualAmount = loan.MonthlyPayment
        });

        // Uygula
        var dues = await service.GetUpcomingPaymentDuesAsync(new DateTime(2026, 9, 15, 10, 0, 0));

        // Doğrula
        Assert.DoesNotContain(dues, x => x.Key == PaymentReminderPlanner.DueKey(loan.Id, loan.Name, SettlementDate));
    }

    [Fact]
    public async Task GetBoardAsync_DonemIlkGunuVadeliOdemeErtelendiginde_TakipHatirlatmasiKurulur()
    {
        var service = await CreateServiceWithOpenPlanAsync();
        var snoozedAt = new DateTime(2026, 9, 1, 10, 0, 0);
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Snoozed,
            snoozedAt,
            PaymentReminderPlanner.SnoozeUntil(snoozedAt),
            [AnsweredDue(InitialDate)]));

        var board = await service.GetBoardAsync(snoozedAt.AddMinutes(30));

        Assert.Contains(board.Reminders, x => x.Key == "20260901-ertele");
        Assert.Contains(board.Snoozed, x => x.DueDate == InitialDate);
    }

    [Fact]
    public async Task GetBoardAsync_DonemIlkGunuVadeliOdemeOdendiginde_PanodaOdenmisGorunur()
    {
        var service = await CreateServiceWithOpenPlanAsync();
        var answeredAt = new DateTime(2026, 9, 1, 10, 0, 0);
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            answeredAt,
            null,
            [AnsweredDue(InitialDate)]));

        var board = await service.GetBoardAsync(answeredAt.AddMinutes(30));

        Assert.Contains(board.Paid, x => x.DueDate == InitialDate);
    }

    [Fact]
    public async Task GetBoardAsync_OncekiDonemSonGunuVadeliCevaplar_PanoyaGirmez()
    {
        var service = await CreateServiceWithOpenPlanAsync();
        var previousPeriodLastDay = InitialDate.AddDays(-1);
        var answeredAt = new DateTime(2026, 9, 1, 10, 0, 0);
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid, answeredAt, null, [AnsweredDue(previousPeriodLastDay)]));
        await service.RecordAnswerAsync(new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Snoozed,
            answeredAt,
            PaymentReminderPlanner.SnoozeUntil(answeredAt),
            [AnsweredDue(previousPeriodLastDay)]));

        var board = await service.GetBoardAsync(answeredAt.AddMinutes(30));

        Assert.Empty(board.Paid);
        Assert.Empty(board.Snoozed);
        Assert.DoesNotContain(board.Reminders, x => x.Key.EndsWith("-ertele", StringComparison.Ordinal));
    }

    private async Task<PaymentReminderService> CreateServiceWithOpenPlanAsync()
    {
        var service = CreateService(out _, out var planReader);
        var plan = CreateSampleFinancialPlan();
        planReader.PlanToReturn = plan;
        await SeedCurrentPlanAsync(plan);
        await service.SaveModeAsync(PaymentReminderMode.Relaxed);
        return service;
    }

    // Dönem kapanışında ödenmeyen yükümlülük yeni dönemin ilk gününe devreder (S27);
    // dönem sınırına en sık düşen hatırlatıcı cevabı budur.
    // PeriodProgressServiceTests'teki kartla aynı: 8.000 ekstreye girmemiş harcama + 3 Eylül'de 2.095, vade 15 Eylül.
    private static CreditCard CardWithInPeriodCharge() => new()
    {
        Name = "Bonus",
        Limit = 50_000m,
        MinimumPaymentRate = 0.40m,
        PaymentStrategy = CreditCardPaymentStrategy.FullStatement,
        StatementClosingDay = 5,
        PaymentDueDay = 15,
        BalanceAsOfDate = InitialDate,
        UnbilledSpending = 8_000m,
        Charges = [new CardCharge { PostingDate = new DateOnly(2026, 9, 3), Amount = 2_095m }]
    };

    private static PeriodPlanPaymentLine CardLine(CreditCard card, decimal plannedAmount) => new()
    {
        Id = Guid.NewGuid(),
        SourceType = PlanPaymentSourceType.CreditCard,
        SourceEntityId = card.Id,
        Name = card.Name,
        PlannedDate = new DateOnly(2026, 9, 15),
        PlannedAmount = plannedAmount
    };

    private static PaymentDue AnsweredDue(DateOnly dueDate) =>
        new(PaymentReminderPlanner.DueKey(Guid.NewGuid(), "Devreden Kredi", dueDate), "Devreden Kredi", dueDate, 10_000m);

    private async Task<PeriodPlanSnapshot> SeedCurrentPlanAsync(
        FinancialPlan? plan = null,
        PeriodPlanPaymentLine? extraLine = null)
    {
        var sampleLine = new PeriodPlanPaymentLine
        {
            Id = Guid.NewGuid(),
            PeriodPlanSnapshotId = Guid.Empty,
            SourceType = PlanPaymentSourceType.Loan,
            SourceEntityId = Guid.NewGuid(),
            Name = "Kira / Kredi",
            PlannedDate = new DateOnly(2026, 9, 10),
            PlannedAmount = 10_000m
        };

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
            PaymentLines = extraLine is null ? [sampleLine] : [sampleLine, extraLine]
        };

        await _historyRepo.SaveCurrentFinancialSnapshotAsync(snapshot, frozenPlan);
        return frozenPlan;
    }

    private static FinancialPlan CreateSampleFinancialPlan()
    {
        var incomeId = Guid.NewGuid();
        var loanId = Guid.NewGuid();

        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                PeriodVariableExpenseAllowance = 20_000m,
                ProjectionAnchorDate = InitialDate,
                ProjectionOpeningBalance = 50_000m,
                DeficitFinancingInterestRate = 0.05m
            },
            RecurringIncomes =
            [
                new RecurringIncome
                {
                    Id = incomeId,
                    Name = "Maaş",
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
            ],
            Loans =
            [
                new Loan
                {
                    Id = loanId,
                    Name = "İhtiyaç Kredisi",
                    Bank = "İş Bankası",
                    MonthlyPayment = 10_000m,
                    PaymentDay = 10,
                    NextPaymentDate = new DateOnly(2026, 9, 10),
                    RemainingInstallmentCount = 12
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

        public void SetDate(DateOnly today)
        {
            Today = today;
            UtcNow = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }
    }
}
