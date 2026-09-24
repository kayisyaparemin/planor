using Mizan.Application.Models;
using Mizan.Application.Services;

namespace Mizan.Application.Tests.Services;

public sealed class PaymentReminderFormatterTests
{
    private static readonly DateOnly Due = new(2026, 9, 18);

    [Theory]
    [InlineData(PaymentReminderMode.Off, "gönderilmez")]
    [InlineData(PaymentReminderMode.Relaxed, "tek bildirim")]
    [InlineData(PaymentReminderMode.Aggressive, "dört bildirim")]
    public void Describe_TumModlarIcinTurkceAciklamaUretir(PaymentReminderMode mode, string expectedFragment)
    {
        var text = PaymentReminderFormatter.Describe(mode);
        Assert.Contains(expectedFragment, text);
    }

    [Theory]
    [InlineData(0, "bugün")]
    [InlineData(1, "yarın")]
    [InlineData(2, "2 gün sonra")]
    [InlineData(-1, "dün")]
    [InlineData(-3, "3 gün önce")]
    public void RelativeDay_BuguneGoreGoreceliGunAdlandirir(int offsetDays, string expected)
    {
        var today = new DateOnly(2026, 9, 15);
        var targetDate = today.AddDays(offsetDays);
        Assert.Equal(expected, PaymentReminderFormatter.RelativeDay(targetDate, today));
    }

    [Fact]
    public void SnoozeText_BugunVeYarinIcinSaatliMetinUretir()
    {
        var now = new DateTime(2026, 9, 15, 14, 0, 0);
        var todayUntil = new DateTime(2026, 9, 15, 17, 0, 0);
        Assert.Equal("Yeniden hatırlatma: bugün 17:00", PaymentReminderFormatter.SnoozeText(todayUntil, now));

        var nightNow = new DateTime(2026, 9, 15, 20, 0, 0);
        var tomorrowUntil = new DateTime(2026, 9, 16, 9, 0, 0);
        Assert.Equal("Yeniden hatırlatma: yarın 09:00", PaymentReminderFormatter.SnoozeText(tomorrowUntil, nightNow));
    }

    [Fact]
    public void What_TekilTutarliVeTutarsizOdemeler_DogruUretilir()
    {
        var singleKnown = new[] { new PaymentDue("k1", "Taşıt Kredisi", Due, 7_374.59m) };
        Assert.Equal("Taşıt Kredisi · 7.374,59 TL", PaymentReminderFormatter.What(singleKnown));

        var singleUnknown = new[] { new PaymentDue("kart", "Bonus Kart", Due, null) };
        Assert.Equal("Bonus Kart · tutarı henüz belli değil", PaymentReminderFormatter.What(singleUnknown));
    }

    [Fact]
    public void What_CokluOdemeler_UcIsimdenSonrasiniSayar()
    {
        var payments = new[]
        {
            new PaymentDue("1", "A", Due, 500m),
            new PaymentDue("2", "B", Due, 400m),
            new PaymentDue("3", "C", Due, 300m),
            new PaymentDue("4", "D", Due, 200m),
            new PaymentDue("5", "E", Due, null)
        };

        var text = PaymentReminderFormatter.What(payments);
        Assert.Equal("5 ödeme · toplam 1.400,00 TL: A, B, C ve 2 ödeme daha", text);
    }

    [Fact]
    public void Preview_BildirimleriGunBazindaGruplarVeBuguneGoreZamaniGosterir()
    {
        var now = new DateTime(2026, 9, 15, 14, 37, 0);
        var reminder1 = new PaymentReminder
        {
            Key = "20260918-gun",
            NotifyAt = new DateTime(2026, 9, 18, 9, 0, 0),
            Title = "Bugün ödeme günü",
            Message = "Burgan Ameliyat · 7.375,00 TL",
            DueDate = Due,
            Payments = [new PaymentDue("b", "Burgan Ameliyat", Due, 7_375m)]
        };

        var reminder2Early = new PaymentReminder
        {
            Key = "20260920-3gun",
            NotifyAt = new DateTime(2026, 9, 17, 10, 0, 0),
            Title = "3 gün sonra ödeme var",
            Message = "Eminevim · 28.167,00 TL",
            DueDate = new DateOnly(2026, 9, 20),
            Payments = [new PaymentDue("e", "Eminevim", new DateOnly(2026, 9, 20), 28_167m)]
        };

        var reminder2Day = new PaymentReminder
        {
            Key = "20260920-gun",
            NotifyAt = new DateTime(2026, 9, 20, 9, 0, 0),
            Title = "Bugün ödeme günü",
            Message = "Eminevim · 28.167,00 TL",
            DueDate = new DateOnly(2026, 9, 20),
            Payments = [new PaymentDue("e", "Eminevim", new DateOnly(2026, 9, 20), 28_167m)]
        };

        var days = PaymentReminderFormatter.Preview([reminder1, reminder2Early, reminder2Day], now);

        Assert.Equal(2, days.Count);
        Assert.Equal("18 Eylül Cuma · 3 gün sonra", days[0].When);
        Assert.Equal("Burgan Ameliyat · 7.375,00 TL", days[0].What);
        Assert.Equal("Bildirim: ödeme günü 09:00", days[0].Schedule);

        Assert.Equal("20 Eylül Pazar · 5 gün sonra", days[1].When);
        Assert.Equal("Eminevim · 28.167,00 TL", days[1].What);
        Assert.Equal("Bildirimler: 3 gün önce 10:00 · ödeme günü 09:00", days[1].Schedule);
    }
}
