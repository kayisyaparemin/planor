using Mizan.Application.Models;
using Mizan.Application.Services;

namespace Mizan.Application.Tests.Services;

public sealed class PaymentReminderPayloadTests
{
    private static readonly DateOnly TestDate = new(2026, 9, 18);

    [Fact]
    public void EncodeDecodePayments_BosVeGecersizGirdiler_GuvenliDonmelidir()
    {
        Assert.Empty(PaymentReminderPayload.DecodePayments(null));
        Assert.Empty(PaymentReminderPayload.DecodePayments(string.Empty));
        Assert.Empty(PaymentReminderPayload.DecodePayments("gecersiz-base-64!!@@"));
        Assert.Empty(PaymentReminderPayload.DecodePayments("   "));
    }

    [Fact]
    public void EncodeDecodePayments_TekilVeCokluOdemeler_BasariylaDonmelidir()
    {
        PaymentDue[] payments =
        [
            new("kredi-1", "Konut Kredisi · Ziraat", TestDate, 15_250.75m),
            new("kart-1", "Bonus Genç / Şube (İstanbul)", new DateOnly(2026, 10, 5), null),
            new("fatura-1", "Elektrik & Su\tFaturası\nSatır", new DateOnly(2026, 9, 25), 850m)
        ];

        var encoded = PaymentReminderPayload.EncodePayments(payments);
        Assert.False(string.IsNullOrWhiteSpace(encoded));

        var decoded = PaymentReminderPayload.DecodePayments(encoded);
        Assert.Equal(3, decoded.Count);

        Assert.Equal("kredi-1", decoded[0].Key);
        Assert.Equal("Konut Kredisi · Ziraat", decoded[0].Name);
        Assert.Equal(TestDate, decoded[0].DueDate);
        Assert.Equal(15_250.75m, decoded[0].Amount);

        Assert.Equal("kart-1", decoded[1].Key);
        Assert.Null(decoded[1].Amount);

        Assert.Equal("fatura-1", decoded[2].Key);
        Assert.Equal(850m, decoded[2].Amount);
    }

    [Fact]
    public void EncodeDecodeAnswer_OdendiVeErtelendi_RoundTripCalismalidir()
    {
        var profileId = Guid.NewGuid();
        var answeredAt = new DateTime(2026, 9, 18, 9, 30, 0);
        var snoozedUntil = new DateTime(2026, 9, 18, 12, 30, 0);

        var snoozeAnswer = new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Snoozed,
            answeredAt,
            snoozedUntil,
            [new PaymentDue("k1", "Taşıt Kredisi", TestDate, 7_350m)]);

        var paidAnswer = new PaymentReminderAnswer(
            PaymentReminderAnswerKind.Paid,
            answeredAt,
            null,
            [new PaymentDue("k1", "Taşıt Kredisi", TestDate, 7_350m)]);

        // Snoozed testi
        var snoozeLine = PaymentReminderPayload.EncodeAnswer(profileId, snoozeAnswer);
        Assert.DoesNotContain('\n', snoozeLine);
        Assert.True(PaymentReminderPayload.TryDecodeAnswer(snoozeLine, out var decodedProfile1, out var decodedAnswer1));
        Assert.Equal(profileId, decodedProfile1);
        Assert.Equal(PaymentReminderAnswerKind.Snoozed, decodedAnswer1.Kind);
        Assert.Equal(answeredAt, decodedAnswer1.AnsweredAt);
        Assert.Equal(snoozedUntil, decodedAnswer1.SnoozedUntil);
        Assert.Single(decodedAnswer1.Payments);

        // Paid testi
        var paidLine = PaymentReminderPayload.EncodeAnswer(profileId, paidAnswer);
        Assert.True(PaymentReminderPayload.TryDecodeAnswer(paidLine, out var decodedProfile2, out var decodedAnswer2));
        Assert.Equal(profileId, decodedProfile2);
        Assert.Equal(PaymentReminderAnswerKind.Paid, decodedAnswer2.Kind);
        Assert.Null(decodedAnswer2.SnoozedUntil);
        Assert.Single(decodedAnswer2.Payments);
    }

    [Theory]
    [InlineData("")]
    [InlineData("eksik\tparca")]
    [InlineData("gecersizguid\todendi\t202609180930\t\t")]
    [InlineData("00000000000000000000000000000001\tbilinmeyen\t202609180930\t\t")]
    [InlineData("00000000000000000000000000000001\todendi\tgecersiztarih\t\t")]
    public void TryDecodeAnswer_HataliSatirlar_FalseDonmelidir(string corruptLine)
    {
        Assert.False(PaymentReminderPayload.TryDecodeAnswer(corruptLine, out var profileId, out var answer));
        Assert.Equal(Guid.Empty, profileId);
        Assert.Null(answer);
    }
}
