using System.Globalization;
using Mizan.Application.Models;
using Mizan.Application.Services;

namespace Mizan.Application.Tests.Services;

public sealed class PaymentReminderPlannerTests
{
    private static readonly DateOnly Due = new(2026, 9, 18);

    private static readonly DateTime[] AggressiveExpectedTimes =
    [
        new(2026, 9, 15, 10, 0, 0),
        new(2026, 9, 17, 20, 0, 0),
        new(2026, 9, 18, 9, 0, 0),
        new(2026, 9, 18, 18, 0, 0)
    ];

    private static readonly string[] AggressiveExpectedKeys =
    [
        "20260918-3gun",
        "20260918-1gun",
        "20260918-gun",
        "20260918-aksam"
    ];

    private static readonly string[] ExpectedFollowUpKeys = ["k2", "k1"];

    [Fact]
    public void DueKey_KaynakKimligiVarsaOnuKullanir_YoksaIsimVeVadeKullanir()
    {
        var sourceId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        Assert.Equal("11111111222233334444555555555555-20260918", PaymentReminderPlanner.DueKey(sourceId, "Kredi", Due));
        Assert.Equal("Kira-20260918", PaymentReminderPlanner.DueKey(Guid.Empty, "Kira", Due));
    }

    [Fact]
    public void Off_BildirimUretmez()
    {
        var reminders = PaymentReminderPlanner.Plan(
            PaymentReminderMode.Off,
            [new PaymentDue("k1", "Kredi", Due, 1_000m)],
            new DateTime(2026, 9, 15, 12, 0, 0));

        Assert.Empty(reminders);
    }

    [Fact]
    public void Relaxed_OdemeGunuSabahiTekBildirimUretir()
    {
        var dues = new[] { new PaymentDue("k1", "Taşıt Kredisi", Due, 7_374.59m) };
        var reminders = PaymentReminderPlanner.Plan(
            PaymentReminderMode.Relaxed,
            dues,
            new DateTime(2026, 9, 15, 12, 0, 0));

        var reminder = Assert.Single(reminders);
        Assert.Equal(new DateTime(2026, 9, 18, 9, 0, 0), reminder.NotifyAt);
        Assert.Equal("20260918-gun", reminder.Key);
        Assert.Equal("Bugün ödeme günü", reminder.Title);
        Assert.Equal("Taşıt Kredisi · 7.374,59 TL", reminder.Message);
        Assert.Equal(Due, reminder.DueDate);
    }

    [Fact]
    public void Aggressive_DortBildirimUretir_VeErkenBildirimlerVadeyiTasir()
    {
        var dues = new[] { new PaymentDue("k1", "Taşıt Kredisi", Due, 7_374.59m) };
        var reminders = PaymentReminderPlanner.Plan(
            PaymentReminderMode.Aggressive,
            dues,
            new DateTime(2026, 9, 15, 9, 30, 0));

        Assert.Equal(4, reminders.Count);
        Assert.Equal(AggressiveExpectedTimes, reminders.Select(x => x.NotifyAt));
        Assert.Equal(AggressiveExpectedKeys, reminders.Select(x => x.Key));

        Assert.Equal("3 gün sonra ödeme var", reminders[0].Title);
        Assert.Equal("Taşıt Kredisi · 7.374,59 TL · 18 Eylül Cuma", reminders[0].Message);

        Assert.Equal("Yarın ödeme günü", reminders[1].Title);
        Assert.Equal("Taşıt Kredisi · 7.374,59 TL · 18 Eylül Cuma", reminders[1].Message);

        Assert.Equal("Bugün ödeme günü", reminders[2].Title);
        Assert.Equal("Taşıt Kredisi · 7.374,59 TL", reminders[2].Message);

        Assert.Equal("Ödemeyi unutma, bugün son gün", reminders[3].Title);
    }

    [Theory]
    [InlineData("2026-09-15T10:00:00", 3)]
    [InlineData("2026-09-17T19:59:00", 3)]
    [InlineData("2026-09-17T20:00:00", 2)]
    [InlineData("2026-09-18T09:00:00", 1)]
    [InlineData("2026-09-18T12:00:00", 1)]
    [InlineData("2026-09-18T18:00:00", 0)]
    public void GecmisSaatliDilimler_Kurulmaz(string nowString, int expectedCount)
    {
        var now = DateTime.Parse(nowString, CultureInfo.InvariantCulture);
        var reminders = PaymentReminderPlanner.Plan(
            PaymentReminderMode.Aggressive,
            [new PaymentDue("k1", "Kredi", Due, 1_000m)],
            now);

        Assert.Equal(expectedCount, reminders.Count);
        Assert.All(reminders, x => Assert.True(x.NotifyAt > now));
    }

    [Fact]
    public void AyniGuneDusenOdemeler_TekBildirimdeBirlestirilir_TutaraGoreAzalanSiralanir()
    {
        var dues = new[]
        {
            new PaymentDue("k1", "Taşıt Kredisi", Due, 7_374.59m),
            new PaymentDue("k2", "İhtiyaç Kredisi", Due, 14_501.23m)
        };

        var reminders = PaymentReminderPlanner.Plan(
            PaymentReminderMode.Relaxed,
            dues,
            new DateTime(2026, 9, 15, 12, 0, 0));

        var reminder = Assert.Single(reminders);
        Assert.Equal("2 ödeme · toplam 21.875,82 TL: İhtiyaç Kredisi, Taşıt Kredisi", reminder.Message);
        Assert.Equal(2, reminder.Payments.Count);
    }

    [Fact]
    public void OtuzBesGunlukUfuk_DisindakiOdemeler_Filtrelenir()
    {
        var today = new DateOnly(2026, 9, 15);
        var dues = new[]
        {
            new PaymentDue("gecmis", "Dün", today.AddDays(-1), 100m),
            new PaymentDue("bugun", "Bugün", today, 100m),
            new PaymentDue("sinir", "35 Gün Sonra", today.AddDays(35), 100m),
            new PaymentDue("ufuk_disi", "36 Gün Sonra", today.AddDays(36), 100m)
        };

        var reminders = PaymentReminderPlanner.Plan(
            PaymentReminderMode.Relaxed,
            dues,
            new DateTime(2026, 9, 15, 8, 0, 0));

        var expectedDates = new[] { today, today.AddDays(35) };
        Assert.Equal(expectedDates, reminders.Select(x => x.DueDate));
    }

    [Theory]
    [InlineData("2026-09-15T14:00:00", "2026-09-15T17:00:00")]
    [InlineData("2026-09-15T18:59:00", "2026-09-15T21:59:00")]
    // 22:00 ve sonrası gece sessizliği: ertesi sabah 09:00
    [InlineData("2026-09-15T19:00:00", "2026-09-16T09:00:00")]
    [InlineData("2026-09-15T23:30:00", "2026-09-16T09:00:00")]
    // Gece yarısından sabah 08:00'e kadar basılırsa aynı sabah 09:00
    [InlineData("2026-09-16T03:00:00", "2026-09-16T09:00:00")]
    [InlineData("2026-09-16T04:59:00", "2026-09-16T09:00:00")]
    [InlineData("2026-09-16T05:00:00", "2026-09-16T08:00:00")]
    // Yıl sonu geçişi
    [InlineData("2026-12-31T21:00:00", "2027-01-01T09:00:00")]
    public void SnoozeUntil_UcSaatSonraUretir_AmaGeceSessizligindeErtesiSabahDokuzaKayar(string nowString, string expectedString)
    {
        var now = DateTime.Parse(nowString, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedString, CultureInfo.InvariantCulture);
        Assert.Equal(expected, PaymentReminderPlanner.SnoozeUntil(now));
    }

    [Fact]
    public void FollowUps_ErtelenenlerinYenidenHatirlatmasiniUretir()
    {
        var now = new DateTime(2026, 9, 18, 10, 0, 0);
        var snoozed = new[]
        {
            new PaymentReminderResponse
            {
                DueKey = "k1",
                Name = "Burgan",
                DueDate = Due,
                Amount = 7_375m,
                Kind = PaymentReminderAnswerKind.Snoozed,
                AnsweredAt = now,
                SnoozedUntil = new DateTime(2026, 9, 18, 12, 0, 0)
            },
            new PaymentReminderResponse
            {
                DueKey = "k2",
                Name = "Eminevim",
                DueDate = Due,
                Amount = 28_167m,
                Kind = PaymentReminderAnswerKind.Snoozed,
                AnsweredAt = now,
                SnoozedUntil = new DateTime(2026, 9, 18, 13, 0, 0)
            },
            new PaymentReminderResponse
            {
                DueKey = "k3",
                Name = "Eski",
                DueDate = Due.AddDays(-1),
                Amount = 100m,
                Kind = PaymentReminderAnswerKind.Snoozed,
                AnsweredAt = now,
                SnoozedUntil = new DateTime(2026, 9, 18, 9, 0, 0) // Saati geçmiş
            },
            new PaymentReminderResponse
            {
                DueKey = "k4",
                Name = "Süresiz",
                DueDate = Due.AddDays(1),
                Amount = 100m,
                Kind = PaymentReminderAnswerKind.Snoozed,
                AnsweredAt = now,
                SnoozedUntil = null
            }
        };

        var followUps = PaymentReminderPlanner.FollowUps(snoozed, now);
        var reminder = Assert.Single(followUps);

        Assert.Equal("20260918-ertele", reminder.Key);
        Assert.Equal(new DateTime(2026, 9, 18, 13, 0, 0), reminder.NotifyAt);
        Assert.Equal("Ertelediğin ödeme", reminder.Title);
        Assert.Equal("2 ödeme · toplam 35.542,00 TL: Eminevim, Burgan · 18 Eylül Cuma", reminder.Message);
        Assert.Equal(ExpectedFollowUpKeys, reminder.Payments.Select(x => x.Key));
    }

    [Fact]
    public void Sample_EnYakinOdemeGunununBildiriminiHemenUretir()
    {
        var now = new DateTime(2026, 9, 15, 14, 37, 0);
        var dues = new[]
        {
            new PaymentDue("dun", "Dün", new DateOnly(2026, 9, 14), 1m),
            new PaymentDue("e", "Eminevim", new DateOnly(2026, 9, 20), 28_167m),
            new PaymentDue("b", "Burgan Ameliyat", Due, 7_375m)
        };

        var sample = PaymentReminderPlanner.Sample(dues, now);
        Assert.NotNull(sample);
        Assert.Equal(now, sample.NotifyAt);
        Assert.Equal("20260918-deneme", sample.Key);
        Assert.Equal("Deneme bildirimi", sample.Title);
        Assert.Equal("Burgan Ameliyat · 7.375,00 TL · 18 Eylül Cuma (3 gün sonra)", sample.Message);
        Assert.Equal("b", Assert.Single(sample.Payments).Key);

        Assert.Null(PaymentReminderPlanner.Sample([], now));
    }
}
