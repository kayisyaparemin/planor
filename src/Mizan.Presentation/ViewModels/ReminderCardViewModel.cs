using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Vadesi gelen veya ertelenmiş tekil ödeme hatırlatıcısını sunan ve
/// "Ödedim" ya da "Ertele" aksiyonlarını yöneten çocuk görünüm modelidir.
/// </summary>
public sealed partial class ReminderCardViewModel : ViewModelBase
{
    private readonly IPaymentReminderService _reminderService;
    private readonly IPaymentReminderScheduler _scheduler;
    private readonly IDialogService _dialogService;
    private readonly IClock _clock;

    /// <summary>Kullanıcının verdiği yanıtlar sebebiyle ödeme durumları değiştiğinde tetiklenir.</summary>
    public event EventHandler? AnswersChanged;

    [ObservableProperty]
    private ReminderItem? activeReminder;

    [ObservableProperty]
    private bool hasActiveReminder;

    [ObservableProperty]
    private string? paymentName;

    [ObservableProperty]
    private decimal? amount;

    [ObservableProperty]
    private DateOnly? dueDate;

    [ObservableProperty]
    private bool isSnoozed;

    /// <summary>Kart başlığı olarak ödeme adını sunar.</summary>
    public string? Title => PaymentName;

    /// <summary>Gerekli bağımlılıklarla çocuk görünüm modelini başlatır.</summary>
    public ReminderCardViewModel(
        IPaymentReminderService reminderService,
        IPaymentReminderScheduler scheduler,
        IDialogService dialogService,
        IClock clock)
    {
        _reminderService = reminderService ?? throw new ArgumentNullException(nameof(reminderService));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>İşletim sistemi bildirim düğmelerinden gelen yanıtları kalıcı deftere işler.</summary>
    public async Task ApplyPendingAnswersAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var pending = _scheduler.ReadPendingAnswers(profileId);
        if (pending.Count == 0)
        {
            return;
        }

        foreach (var answer in pending)
        {
            await _reminderService.RecordAnswerAsync(answer, cancellationToken);
        }

        _scheduler.AcknowledgeAnswers(profileId, pending.Count);
    }

    /// <summary>Profilin hatırlatıcılarını işletim sistemiyle eşitler ve aktif ödemeyi yükler.</summary>
    public async Task LoadAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await ApplyPendingAnswersAsync(profileId, cancellationToken);

        var now = _clock.UtcNow.DateTime;
        var today = _clock.Today;
        var board = await _reminderService.GetBoardAsync(now, cancellationToken);

        _scheduler.Schedule(profileId, board.Reminders);

        var urgent = FindUrgentReminder(board, now, today);
        if (urgent is not null)
        {
            SetActive(urgent);
        }
        else
        {
            ClearActive();
        }
    }

    private static ReminderItem? FindUrgentReminder(PaymentReminderBoard board, DateTime now, DateOnly today)
    {
        var snoozed = board.Snoozed
            .FirstOrDefault(s => (s.SnoozedUntil is null || s.SnoozedUntil <= now) && s.DueDate <= today);

        if (snoozed is not null)
        {
            return new ReminderItem
            {
                DueKey = snoozed.DueKey,
                Name = snoozed.Name,
                DueDate = snoozed.DueDate,
                Amount = snoozed.Amount,
                IsSnoozed = true,
                SnoozedUntil = snoozed.SnoozedUntil
            };
        }

        var payment = board.Upcoming
            .Where(d => d.DueDate <= today)
            .SelectMany(d => d.Payments)
            .FirstOrDefault();

        return payment is null ? null : new ReminderItem
        {
            DueKey = payment.Key,
            Name = payment.Name,
            DueDate = payment.DueDate,
            Amount = payment.Amount,
            IsSnoozed = false,
            SnoozedUntil = null
        };
    }

    /// <summary>Aktif hatırlatıcıyı ödendi olarak işaretler ve ana sayfayı günceller.</summary>
    [RelayCommand]
    private async Task MarkAsPaidAsync(CancellationToken cancellationToken = default)
    {
        if (ActiveReminder is not { } reminder)
        {
            return;
        }

        var due = new PaymentDue(reminder.DueKey, reminder.Name, reminder.DueDate, reminder.Amount);
        var answer = new PaymentReminderAnswer(PaymentReminderAnswerKind.Paid, _clock.UtcNow.DateTime, null, [due]);
        await _reminderService.RecordAnswerAsync(answer, cancellationToken);

        ClearActive();
        AnswersChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Aktif hatırlatıcıyı 3 saat sonraya öteler ve kartı ekrandan kaldırır.</summary>
    [RelayCommand]
    private async Task SnoozeAsync(CancellationToken cancellationToken = default)
    {
        if (ActiveReminder is not { } reminder)
        {
            return;
        }

        var now = _clock.UtcNow.DateTime;
        var snoozeUntil = PaymentReminderPlanner.SnoozeUntil(now);
        var due = new PaymentDue(reminder.DueKey, reminder.Name, reminder.DueDate, reminder.Amount);
        var answer = new PaymentReminderAnswer(PaymentReminderAnswerKind.Snoozed, now, snoozeUntil, [due]);
        await _reminderService.RecordAnswerAsync(answer, cancellationToken);

        ClearActive();
        AnswersChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetActive(ReminderItem item)
    {
        ActiveReminder = item;
        HasActiveReminder = true;
        PaymentName = item.Name;
        Amount = item.Amount;
        DueDate = item.DueDate;
        IsSnoozed = item.IsSnoozed;
        OnPropertyChanged(nameof(Title));
    }

    private void ClearActive()
    {
        ActiveReminder = null;
        HasActiveReminder = false;
        PaymentName = null;
        Amount = null;
        DueDate = null;
        IsSnoozed = false;
        OnPropertyChanged(nameof(Title));
    }
}
