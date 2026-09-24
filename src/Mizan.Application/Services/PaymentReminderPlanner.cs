using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Ödeme yükümlülüklerinden bildirim takvimi, dilimler ve erteleme zamanlarını üreten saf hesaplayıcı motor.
/// </summary>
public static class PaymentReminderPlanner
{
    /// <summary>Kaç gün ilerisi kurulur. Uygulama her açıldığında yenilenir.</summary>
    public const int HorizonDays = 35;

    /// <summary>"Ertele" bu kadar sonra yeniden hatırlatır.</summary>
    public static readonly TimeSpan SnoozeDelay = TimeSpan.FromHours(3);

    private sealed record Slot(
        string Code,
        int DaysBefore,
        TimeOnly At,
        string Title);

    private static readonly Slot DayOf = new("gun", 0, new TimeOnly(9, 0), "Bugün ödeme günü");

    private static readonly IReadOnlyList<Slot> Relaxed = [DayOf];

    private static readonly IReadOnlyList<Slot> Aggressive =
    [
        new("3gun", 3, new TimeOnly(10, 0), "3 gün sonra ödeme var"),
        new("1gun", 1, new TimeOnly(20, 0), "Yarın ödeme günü"),
        DayOf,
        new("aksam", 0, new TimeOnly(18, 0), "Ödemeyi unutma, bugün son gün")
    ];

    private static readonly TimeOnly QuietStarts = new(22, 0);
    private static readonly TimeOnly QuietEnds = new(8, 0);
    private static readonly TimeOnly Morning = new(9, 0);

    /// <summary>
    /// Ödemenin hatırlatıcı defterindeki anahtarını üretir: kaynak kimliği ve vade.
    /// Plan satırı kimliği kullanılmaz; revizyon ve dönem kapanışı anahtarı bozmaz.
    /// </summary>
    public static string DueKey(Guid sourceId, string name, DateOnly date) =>
        sourceId == Guid.Empty
            ? $"{name}-{date:yyyyMMdd}"
            : $"{sourceId:N}-{date:yyyyMMdd}";

    /// <summary>
    /// "Ertele" aksiyonu seçildiğinde yeniden hatırlatma zamanını hesaplar; gece sessizliği saatlerine düşmez.
    /// </summary>
    public static DateTime SnoozeUntil(DateTime now)
    {
        var candidate = now + SnoozeDelay;
        var time = TimeOnly.FromDateTime(candidate);
        var date = DateOnly.FromDateTime(candidate);
        if (time >= QuietStarts)
        {
            return date.AddDays(1).ToDateTime(Morning);
        }

        return time < QuietEnds
            ? date.ToDateTime(Morning)
            : candidate;
    }

    /// <summary>
    /// Ertelenen ödemelerin yeniden hatırlatma bildirimlerini üretir: vade günü başına tek bildirim,
    /// o günün en geç erteleme saatinde.
    /// </summary>
    public static IReadOnlyList<PaymentReminder> FollowUps(
        IEnumerable<PaymentReminderResponse> snoozed,
        DateTime now) =>
        snoozed
            .Where(x => x.Kind == PaymentReminderAnswerKind.Snoozed &&
                        x.SnoozedUntil is { } until &&
                        until > now)
            .GroupBy(x => x.DueDate)
            .Select(day =>
            {
                var payments = day
                    .Select(x => new PaymentDue(x.DueKey, x.Name, x.DueDate, x.Amount))
                    .OrderByDescending(x => x.Amount ?? 0m)
                    .ThenBy(x => x.Name, StringComparer.Create(PaymentReminderFormatter.TurkishCulture, false))
                    .ToArray();
                return new PaymentReminder
                {
                    Key = $"{day.Key:yyyyMMdd}-ertele",
                    NotifyAt = day.Max(x => x.SnoozedUntil!.Value),
                    Title = "Ertelediğin ödeme",
                    Message = $"{PaymentReminderFormatter.What(payments)} · {day.Key.ToString("d MMMM dddd", PaymentReminderFormatter.TurkishCulture)}",
                    DueDate = day.Key,
                    Payments = payments
                };
            })
            .OrderBy(x => x.NotifyAt)
            .ToArray();

    /// <summary>
    /// "Deneme bildirimi gönder" için sıradaki ilk ödeme gününün bildirimini hemen çalacak şekilde üretir.
    /// </summary>
    public static PaymentReminder? Sample(
        IEnumerable<PaymentDue> dues,
        DateTime now)
    {
        var today = DateOnly.FromDateTime(now);
        var day = dues
            .Where(x => x.DueDate >= today)
            .GroupBy(x => x.DueDate)
            .OrderBy(x => x.Key)
            .FirstOrDefault();
        if (day is null)
        {
            return null;
        }

        var payments = day
            .GroupBy(x => x.Key)
            .Select(x => x.First())
            .OrderByDescending(x => x.Amount ?? 0m)
            .ThenBy(x => x.Name, StringComparer.Create(PaymentReminderFormatter.TurkishCulture, false))
            .ToArray();
        return new PaymentReminder
        {
            Key = $"{day.Key:yyyyMMdd}-deneme",
            NotifyAt = now,
            Title = "Deneme bildirimi",
            Message = $"{PaymentReminderFormatter.What(payments)} · {day.Key.ToString("d MMMM dddd", PaymentReminderFormatter.TurkishCulture)} ({PaymentReminderFormatter.RelativeDay(day.Key, today)})",
            DueDate = day.Key,
            Payments = payments
        };
    }

    /// <summary>
    /// Seçilen hatırlatıcı moduna göre yaklaşan ödemelerin bildirim takvimini üretir.
    /// </summary>
    public static IReadOnlyList<PaymentReminder> Plan(
        PaymentReminderMode mode,
        IEnumerable<PaymentDue> dues,
        DateTime now,
        int horizonDays = HorizonDays)
    {
        var slots = mode switch
        {
            PaymentReminderMode.Relaxed => Relaxed,
            PaymentReminderMode.Aggressive => Aggressive,
            _ => []
        };
        if (slots.Count == 0)
        {
            return [];
        }

        var today = DateOnly.FromDateTime(now);
        var last = today.AddDays(horizonDays);
        return dues
            .Where(x => x.DueDate >= today && x.DueDate <= last)
            .GroupBy(x => x.Key)
            .Select(x => x.First())
            .GroupBy(x => x.DueDate)
            .SelectMany(day =>
            {
                var payments = day
                    .OrderByDescending(x => x.Amount ?? 0m)
                    .ThenBy(x => x.Name, StringComparer.Create(PaymentReminderFormatter.TurkishCulture, false))
                    .ToArray();
                return slots.Select(slot => Build(day.Key, payments, slot));
            })
            .Where(x => x.NotifyAt > now)
            .OrderBy(x => x.NotifyAt)
            .ToArray();
    }

    private static PaymentReminder Build(
        DateOnly dueDate,
        IReadOnlyList<PaymentDue> payments,
        Slot slot)
    {
        var notifyAt = dueDate
            .AddDays(-slot.DaysBefore)
            .ToDateTime(slot.At);
        var what = PaymentReminderFormatter.What(payments);
        var message = slot.DaysBefore == 0
            ? what
            : $"{what} · {dueDate.ToString("d MMMM dddd", PaymentReminderFormatter.TurkishCulture)}";
        return new PaymentReminder
        {
            Key = $"{dueDate:yyyyMMdd}-{slot.Code}",
            NotifyAt = notifyAt,
            Title = slot.Title,
            Message = message,
            DueDate = dueDate,
            Payments = payments
        };
    }
}
