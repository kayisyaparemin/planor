using Mizan.Application.Models;
using Mizan.Application.Services;

namespace Mizan.Application.Tests.Services;

public sealed class ProfileNameValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BosVeyaYalnizcaBoslukIse_HataFirlatir(string? name)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ProfileNameValidator.Validate(name, []));

        Assert.Equal("Profil adı boş olamaz.", ex.Message);
    }

    [Fact]
    public void Validate_OtuzKarakterdenUzunIse_HataFirlatir()
    {
        var longName = new string('a', UserProfile.MaxNameLength + 1);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ProfileNameValidator.Validate(longName, []));

        Assert.Equal($"Profil adı en fazla {UserProfile.MaxNameLength} karakter olabilir.", ex.Message);
    }

    [Fact]
    public void Validate_GecerliAd_KirparakDondurur()
    {
        var result = ProfileNameValidator.Validate("  Kişisel Bütçe  ", []);

        Assert.Equal("Kişisel Bütçe", result);
    }

    [Fact]
    public void Validate_TurkceHarfDuyarsiz_MevcutAdlaCakistiginda_HataFirlatir()
    {
        var existing = new List<UserProfile>
        {
            new() { Name = "İpek" }
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ProfileNameValidator.Validate("ipek", existing));

        Assert.Equal("Bu adla bir profil zaten var.", ex.Message);
    }

    [Fact]
    public void Validate_ExceptIdBelirtildiginde_KendiAdiniGuncellemeyeIzinVerir()
    {
        var profileId = Guid.NewGuid();
        var existing = new List<UserProfile>
        {
            new() { Id = profileId, Name = "İpek" }
        };

        var result = ProfileNameValidator.Validate("İPEK", existing, exceptId: profileId);

        Assert.Equal("İPEK", result);
    }

    [Fact]
    public void GenerateUniqueName_Cakismiyorsa_TemelAdiKullanir()
    {
        var result = ProfileNameValidator.GenerateUniqueName("Profilim", ["Farklı Profil"]);

        Assert.Equal("Profilim", result);
    }

    [Fact]
    public void GenerateUniqueName_Cakisiyorsa_NumaraEkler()
    {
        var taken = new[] { "Profilim", "profilim 2" };

        var result = ProfileNameValidator.GenerateUniqueName("Profilim", taken);

        Assert.Equal("Profilim 3", result);
    }

    [Theory]
    [InlineData("İpek", "ipek", true)]
    [InlineData("ışık", "IŞIK", true)]
    [InlineData("Şebnem", "şebnem", true)]
    [InlineData("Ahmet", "Mehmet", false)]
    public void AreNamesEqual_TurkceKulturKurallarinaGoreKarsilastirir(string left, string right, bool expected)
    {
        var actual = ProfileNameValidator.AreNamesEqual(left, right);

        Assert.Equal(expected, actual);
    }
}
