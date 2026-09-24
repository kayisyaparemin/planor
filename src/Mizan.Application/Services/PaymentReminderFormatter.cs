using System.Globalization;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Bildirim ve arayüz kartları için Türkçe tarih, tutar, başlık ve durum metinlerini üreten yardımcı sınıf.
/// </summary>
public static class PaymentReminderFormatter
{
    private const int NamesInMessage = 3;

    /// <summary>Türkçe kültür formatlayıcısı.</summary>
    public static readonly CultureInfo TurkishCulture =
        CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Kullanıcıya gösterilen hatırlatıcı modunun davranış açıklamasını döndürür.
    /// </summary>
    public static string Describe(PaymentReminderMode mode) => mode switch
    {
        PaymentReminderMode.Relaxed =>
            "Ödeme günü sabah 09:00'da tek bildirim.",
        PaymentReminderMode.Aggressive =>
            "3 gün önce, bir gün önce akşam 20:00, ödeme günü sabah 09:00 ve akşam 18:00'de; ödeme başına dört bildirim.",
        _ => "Ödeme günlerinde bildirim gönderilmez."
    };

    /// <summary>
    /// Ertelenen ödemenin ne zaman yeniden hatırlatılacağını kullanıcı dilinde döndürür.
    /// </summary>
    public static string SnoozeText(DateTime until, DateTime now) =>
        $"Yeniden hatırlatma: {RelativeDay(DateOnly.FromDateTime(until), DateOnly.FromDateTime(now))} {until.ToString("HH:mm", TurkishCulture)}";

    /// <summary>
    /// Bir günün bugüne göre göreceli adını döndürür: bugün, yarın, 3 gün sonra, dün, 2 gün önce.
    /// </summary>
    public static string RelativeDay(DateOnly date, DateOnly today) =>
        (date.DayNumber - today.DayNumber) switch
        {
            0 => "bugün",
            1 => "yarın",
            -1 => "dün",
            > 1 and var ahead => $"{ahead} gün sonra",
            var behind => $"{-behind} gün önce"
        };

    /// <summary>
    /// Ödemeleri konsolide eden Türkçe başlık ve tutar metnini üretir.
    /// </summary>
    public static string What(IReadOnlyList<PaymentDue> payments) =>
        payments.Count == 1
            ? $"{payments[0].Name} · {AmountText(payments[0].Amount)}"
            : $"{payments.Count} ödeme · toplam {Money(payments.Sum(x => x.Amount ?? 0m))}: {Names(payments)}";

    /// <summary>
    /// Kurulan bildirimleri arayüz kartında ödeme günü başına bir satıra toplar ve önizleme listesini üretir.
    /// </summary>
    public static IReadOnlyList<PaymentReminderDay> Preview(
        IEnumerable<PaymentReminder> reminders,
        DateTime now)
    {
        var today = DateOnly.FromDateTime(now);
        return reminders
            .GroupBy(x => x.DueDate)
            .OrderBy(x => x.Key)
            .Select(day =>
            {
                var ordered = day.OrderBy(x => x.NotifyAt).ToArray();
                var payments = ordered[0].Payments;
                return new PaymentReminderDay(
                    day.Key,
                    $"{day.Key.ToString("d MMMM dddd", TurkishCulture)} · {RelativeDay(day.Key, today)}",
                    What(payments),
                    Schedule(day.Key, ordered),
                    payments);
            })
            .ToArray();
    }

    private static string Schedule(
        DateOnly dueDate,
        IReadOnlyList<PaymentReminder> reminders)
    {
        var parts = reminders
            .GroupBy(x => dueDate.DayNumber - DateOnly.FromDateTime(x.NotifyAt).DayNumber)
            .Select(group =>
            {
                var label = group.Key switch
                {
                    0 => "ödeme günü",
                    1 => "bir gün önce",
                    var days => $"{days} gün önce"
                };
                var times = string.Join(
                    " ve ",
                    group.Select(x => x.NotifyAt.ToString("HH:mm", TurkishCulture)));
                return $"{label} {times}";
            });
        return $"{(reminders.Count == 1 ? "Bildirim" : "Bildirimler")}: {string.Join(" · ", parts)}";
    }

    private static string Names(IReadOnlyList<PaymentDue> payments)
    {
        var names = string.Join(", ", payments.Take(NamesInMessage).Select(x => x.Name));
        return payments.Count > NamesInMessage
            ? $"{names} ve {payments.Count - NamesInMessage} ödeme daha"
            : names;
    }

    private static string AmountText(decimal? amount) =>
        amount is decimal value
            ? Money(value)
            : "tutarı henüz belli değil";

    private static string Money(decimal value) =>
        $"{value.ToString("N2", TurkishCulture)} TL";
}
