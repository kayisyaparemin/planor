using Mizan.Application.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Hatırlatıcı çocuk kartının yükleme, işletim sistemi bildirim senkronizasyonu,
/// ödedim ve erteleme aksiyonlarını doğrulayan birim testleri.
/// </summary>
public sealed class ReminderCardViewModelTests
{
    private readonly FakePaymentReminderService _service = new();
    private readonly FakePaymentReminderScheduler _scheduler = new();
    private readonly FakeDialogService _dialog = new();
    private readonly SabitSaat _clock = new(new DateOnly(2026, 9, 27));
    private readonly ReminderCardViewModel _viewModel;
    private readonly Guid _profileId = Guid.NewGuid();

    public ReminderCardViewModelTests()
    {
        _viewModel = new ReminderCardViewModel(_service, _scheduler, _dialog, _clock);
    }

    [Fact]
    public async Task YukleAsync_HatirlaticiYoksa_HasActiveReminderFalseVeKartGizlidir()
    {
        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Upcoming = [],
            Snoozed = []
        };

        await _viewModel.LoadAsync(_profileId);

        Assert.False(_viewModel.HasActiveReminder);
        Assert.Null(_viewModel.ActiveReminder);
        Assert.Null(_viewModel.PaymentName);
        Assert.Null(_viewModel.Amount);
        Assert.Null(_viewModel.DueDate);
    }

    [Fact]
    public async Task YukleAsync_VadesiGelenOdemeVarsa_ActiveReminderDoldurulurVeHasActiveReminderTrue()
    {
        var today = _clock.Today;
        var payment = new PaymentDue("loan-1", "Konut Kredisi", today, 12500m);
        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Upcoming = [new PaymentReminderDay(today, "Bugün", "Konut Kredisi · 12.500,00 ₺", "10:00", [payment])]
        };

        await _viewModel.LoadAsync(_profileId);

        Assert.True(_viewModel.HasActiveReminder);
        Assert.NotNull(_viewModel.ActiveReminder);
        Assert.Equal("loan-1", _viewModel.ActiveReminder.DueKey);
        Assert.Equal("Konut Kredisi", _viewModel.PaymentName);
        Assert.Equal("Konut Kredisi", _viewModel.Title);
        Assert.Equal(12500m, _viewModel.Amount);
        Assert.Equal(today, _viewModel.DueDate);
        Assert.False(_viewModel.IsSnoozed);
    }

    [Fact]
    public async Task YukleAsync_GecikmisOdemeVarsa_ActiveReminderDoldurulur()
    {
        var pastDate = _clock.Today.AddDays(-2);
        var payment = new PaymentDue("card-1", "Bonus Kart", pastDate, 4500m);
        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Upcoming = [new PaymentReminderDay(pastDate, "2 gün önce", "Bonus Kart · 4.500,00 ₺", "10:00", [payment])]
        };

        await _viewModel.LoadAsync(_profileId);

        Assert.True(_viewModel.HasActiveReminder);
        Assert.Equal("Bonus Kart", _viewModel.PaymentName);
        Assert.Equal(4500m, _viewModel.Amount);
        Assert.Equal(pastDate, _viewModel.DueDate);
    }

    [Fact]
    public async Task YukleAsync_ErtelenenOdemeSuresiDolduysa_ActiveReminderOlarakSunulur()
    {
        var today = _clock.Today;
        var snoozedResponse = new PaymentReminderResponse
        {
            DueKey = "expense-1",
            Name = "İnternet Faturası",
            DueDate = today,
            Amount = 350m,
            Kind = PaymentReminderAnswerKind.Snoozed,
            AnsweredAt = _clock.UtcNow.DateTime.AddHours(-4),
            SnoozedUntil = _clock.UtcNow.DateTime.AddHours(-1)
        };
        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Snoozed = [snoozedResponse]
        };

        await _viewModel.LoadAsync(_profileId);

        Assert.True(_viewModel.HasActiveReminder);
        Assert.NotNull(_viewModel.ActiveReminder);
        Assert.Equal("İnternet Faturası", _viewModel.PaymentName);
        Assert.Equal(350m, _viewModel.Amount);
        Assert.True(_viewModel.IsSnoozed);
    }

    [Fact]
    public async Task YukleAsync_SchedulerIleBildirimleriSenkronizeEder()
    {
        var reminder = new PaymentReminder
        {
            Key = "rem-1",
            NotifyAt = _clock.UtcNow.DateTime.AddDays(1),
            Title = "Yarın Ödeme Var",
            Message = "Kira 20.000 ₺",
            DueDate = _clock.Today.AddDays(1),
            Payments = [new PaymentDue("rent-1", "Kira", _clock.Today.AddDays(1), 20000m)]
        };

        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Reminders = [reminder]
        };

        await _viewModel.LoadAsync(_profileId);

        Assert.True(_scheduler.ScheduledReminders.ContainsKey(_profileId));
        Assert.Single(_scheduler.ScheduledReminders[_profileId]);
        Assert.Equal("rem-1", _scheduler.ScheduledReminders[_profileId][0].Key);
    }

    [Fact]
    public async Task ApplyPendingAnswersAsync_BildirimCevaplariniKaydederVeSchedulerOnaylar()
    {
        var due = new PaymentDue("card-1", "Kart Borcu", _clock.Today, 5000m);
        var answer = new PaymentReminderAnswer(PaymentReminderAnswerKind.Paid, _clock.UtcNow.DateTime, null, [due]);
        _scheduler.PendingAnswers = [answer];

        await _viewModel.ApplyPendingAnswersAsync(_profileId);

        Assert.Single(_service.RecordedAnswers);
        Assert.Equal(PaymentReminderAnswerKind.Paid, _service.RecordedAnswers[0].Kind);
        Assert.Equal(1, _scheduler.AcknowledgedCount);
        Assert.Empty(_scheduler.PendingAnswers);
    }

    [Fact]
    public async Task MarkAsPaidAsync_Cagrilinca_PaidKaydederKartiKapatirVeAnswersChangedAtesler()
    {
        var today = _clock.Today;
        var payment = new PaymentDue("loan-1", "İhtiyaç Kredisi", today, 7000m);
        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Upcoming = [new PaymentReminderDay(today, "Bugün", "İhtiyaç Kredisi", "10:00", [payment])]
        };
        await _viewModel.LoadAsync(_profileId);

        var eventFired = false;
        _viewModel.AnswersChanged += (_, _) => eventFired = true;

        await _viewModel.MarkAsPaidCommand.ExecuteAsync(null);

        Assert.False(_viewModel.HasActiveReminder);
        Assert.Null(_viewModel.ActiveReminder);
        Assert.Single(_service.RecordedAnswers);
        Assert.Equal(PaymentReminderAnswerKind.Paid, _service.RecordedAnswers[0].Kind);
        Assert.Equal("loan-1", _service.RecordedAnswers[0].Payments[0].Key);
        Assert.True(eventFired);
    }

    [Fact]
    public async Task SnoozeAsync_Cagrilinca_SnoozedKaydederKartiKapatirVeAnswersChangedAtesler()
    {
        var today = _clock.Today;
        var payment = new PaymentDue("loan-1", "İhtiyaç Kredisi", today, 7000m);
        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Upcoming = [new PaymentReminderDay(today, "Bugün", "İhtiyaç Kredisi", "10:00", [payment])]
        };
        await _viewModel.LoadAsync(_profileId);

        var eventFired = false;
        _viewModel.AnswersChanged += (_, _) => eventFired = true;

        await _viewModel.SnoozeCommand.ExecuteAsync(null);

        Assert.False(_viewModel.HasActiveReminder);
        Assert.Null(_viewModel.ActiveReminder);
        Assert.Single(_service.RecordedAnswers);
        Assert.Equal(PaymentReminderAnswerKind.Snoozed, _service.RecordedAnswers[0].Kind);
        Assert.NotNull(_service.RecordedAnswers[0].SnoozedUntil);
        Assert.True(eventFired);
    }

    [Fact]
    public async Task MarkAsPaidAsync_ActiveReminderYokken_HicbirIslemYapmaz()
    {
        var eventFired = false;
        _viewModel.AnswersChanged += (_, _) => eventFired = true;

        await _viewModel.MarkAsPaidCommand.ExecuteAsync(null);

        Assert.Empty(_service.RecordedAnswers);
        Assert.False(eventFired);
    }

    [Fact]
    public async Task SnoozeAsync_ActiveReminderYokken_HicbirIslemYapmaz()
    {
        var eventFired = false;
        _viewModel.AnswersChanged += (_, _) => eventFired = true;

        await _viewModel.SnoozeCommand.ExecuteAsync(null);

        Assert.Empty(_service.RecordedAnswers);
        Assert.False(eventFired);
    }

    [Fact]
    public async Task LoadAsync_CokluAcilOdemeVarsa_IlkiniSecer()
    {
        var today = _clock.Today;
        var p1 = new PaymentDue("loan-1", "Kredi 1", today.AddDays(-1), 1000m);
        var p2 = new PaymentDue("loan-2", "Kredi 2", today, 2000m);
        _service.CurrentBoard = new PaymentReminderBoard
        {
            Mode = PaymentReminderMode.Relaxed,
            Upcoming =
            [
                new PaymentReminderDay(today.AddDays(-1), "Dün", "Kredi 1", "10:00", [p1]),
                new PaymentReminderDay(today, "Bugün", "Kredi 2", "10:00", [p2])
            ]
        };

        await _viewModel.LoadAsync(_profileId);

        Assert.True(_viewModel.HasActiveReminder);
        Assert.Equal("loan-1", _viewModel.ActiveReminder?.DueKey);
        Assert.Equal("Kredi 1", _viewModel.PaymentName);
    }
}
