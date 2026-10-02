using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// Düzenli gelirin tutar değişikliği kuralları: ekranda görünen tutarlar, silinebilirlik, yeni tutarın
/// kabulü ve bir kaydın (eklenen + silinen) denetimi (S67-5, S67 V6d2 notları).
/// </summary>
public sealed class IncomeAmountRulesTests
{
    private static readonly DateOnly Today = new(2026, 10, 12);

    [Fact]
    public void Upcoming_YururluktekiSonTutarVeIleriTarihliler_YururluktenKalkmislarGorunmez()
    {
        var old = Amount(10_000m, new DateOnly(2026, 1, 1));
        var current = Amount(12_000m, new DateOnly(2026, 7, 1));
        var raise = Amount(14_000m, new DateOnly(2027, 1, 15));
        var later = Amount(15_000m, new DateOnly(2027, 7, 15));

        var upcoming = IncomeAmountRules.Upcoming([later, old, raise, current], Today);

        Assert.Equal([current, raise, later], upcoming);
    }

    [Fact]
    public void Upcoming_BugunYururlugeGirenTutar_YururluktekiSayilirOncekiGorunmez()
    {
        var previous = Amount(12_000m, new DateOnly(2026, 7, 1));
        var startsToday = Amount(13_000m, Today);

        var upcoming = IncomeAmountRules.Upcoming([previous, startsToday], Today);

        Assert.Equal([startsToday], upcoming);
    }

    [Fact]
    public void Upcoming_HicbiriYururlukteDegilse_YalnizIleriTarihliler()
    {
        var first = Amount(12_000m, new DateOnly(2026, 11, 1));
        var raise = Amount(14_000m, new DateOnly(2027, 1, 1));

        var upcoming = IncomeAmountRules.Upcoming([raise, first], Today);

        Assert.Equal([first, raise], upcoming);
    }

    [Fact]
    public void Upcoming_TutarYoksa_BosDoner()
    {
        var upcoming = IncomeAmountRules.Upcoming([], Today);

        Assert.Empty(upcoming);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void IsRemovable_YalnizBugunVeSonrasiYururlugeGirenSilinebilir(int dayOffset, bool expected)
    {
        var amount = Amount(12_000m, Today.AddDays(dayOffset));

        var removable = IncomeAmountRules.IsRemovable(amount, Today);

        Assert.Equal(expected, removable);
    }

    [Fact]
    public void CheckAddition_GecerliTutarVeBugunTarihi_KabulEder()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 1));

        var error = IncomeAmountRules.CheckAddition([current], Amount(13_000m, Today), Today);

        Assert.Null(error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void CheckAddition_TutarSifirVeyaEksiyse_Reddeder(decimal amount)
    {
        var error = IncomeAmountRules.CheckAddition([], Amount(amount, Today), Today);

        Assert.Equal(IncomeAmountRules.AmountMustBePositiveMessage, error);
    }

    [Fact]
    public void CheckAddition_TarihBugundenOnceyse_Reddeder()
    {
        var error = IncomeAmountRules.CheckAddition([], Amount(13_000m, Today.AddDays(-1)), Today);

        Assert.Equal(IncomeAmountRules.PastDateMessage, error);
    }

    [Fact]
    public void CheckAddition_AyniTarihteTutarVarsa_Reddeder()
    {
        var planned = Amount(14_000m, new DateOnly(2027, 1, 15));

        var error = IncomeAmountRules.CheckAddition([planned], Amount(15_000m, new DateOnly(2027, 1, 15)), Today);

        Assert.Equal(IncomeAmountRules.DuplicateDateMessage, error);
    }

    [Fact]
    public void CheckChange_IleriTarihliSilinipYeniTutarEklenirse_KabulEder()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 1));
        var planned = Amount(14_000m, new DateOnly(2027, 1, 15));

        var error = IncomeAmountRules.CheckChange([current, planned], [Amount(15_000m, new DateOnly(2027, 2, 15))], [planned.Id], Today);

        Assert.Null(error);
    }

    [Fact]
    public void CheckChange_YururlugeGirmisTutarSilinirse_Reddeder()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 1));
        var planned = Amount(14_000m, new DateOnly(2027, 1, 15));

        var error = IncomeAmountRules.CheckChange([current, planned], [], [current.Id], Today);

        Assert.Equal(IncomeAmountRules.EffectiveRemovalMessage, error);
    }

    [Fact]
    public void CheckChange_GelireAitOlmayanKimlikSilinirse_Reddeder()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 1));

        var error = IncomeAmountRules.CheckChange([current], [], [Guid.NewGuid()], Today);

        Assert.Equal(IncomeAmountRules.UnknownAmountMessage, error);
    }

    [Fact]
    public void CheckChange_SonTutarSilinipYenisiEklenmezse_Reddeder()
    {
        var startsToday = Amount(12_000m, Today);

        var error = IncomeAmountRules.CheckChange([startsToday], [], [startsToday.Id], Today);

        Assert.Equal(IncomeAmountRules.LastAmountMessage, error);
    }

    [Fact]
    public void CheckChange_BugunkuTekTutarSilinipAyniGunYenisiEklenirse_KabulEder()
    {
        var mistyped = Amount(1_250m, Today);

        var error = IncomeAmountRules.CheckChange([mistyped], [Amount(12_500m, Today)], [mistyped.Id], Today);

        Assert.Null(error);
    }

    [Fact]
    public void CheckChange_EklenenlerAyniTarihteyse_Reddeder()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 1));
        var date = new DateOnly(2027, 1, 15);

        var error = IncomeAmountRules.CheckChange([current], [Amount(14_000m, date), Amount(15_000m, date)], [], Today);

        Assert.Equal(IncomeAmountRules.DuplicateDateMessage, error);
    }

    [Fact]
    public void CheckChange_EklenenTutarGecersizse_EklemeMesajiniDoner()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 1));

        var error = IncomeAmountRules.CheckChange([current], [Amount(0m, Today)], [], Today);

        Assert.Equal(IncomeAmountRules.AmountMustBePositiveMessage, error);
    }

    [Fact]
    public void CheckChange_SilmeYoksa_TutariOlmayanGelirDeKabulEdilir()
    {
        var error = IncomeAmountRules.CheckChange([], [], [], Today);

        Assert.Null(error);
    }

    private static IncomeAmountHistory Amount(decimal amount, DateOnly effectiveDate) =>
        new() { Id = Guid.NewGuid(), RecurringIncomeId = Guid.Empty, Amount = amount, EffectiveDate = effectiveDate };
}
