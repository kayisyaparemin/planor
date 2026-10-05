using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;

namespace Mizan.Application.Tests.Services;

public sealed class ProfileServiceTests : IDisposable
{
    private readonly InMemoryProfileRepository _repository = new();
    private readonly InMemoryProfileStoreSwitch _storeSwitch = new();
    private readonly SteppingTestClock _clock = new();
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        _sut = new ProfileService(_repository, _storeSwitch, _clock);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public async Task GetProfilesAsync_OlusturulmaTarihineGoreArtanSiralar()
    {
        var p1 = new UserProfile { Id = Guid.NewGuid(), Name = "Bir", CreatedAt = _clock.UtcNow.AddMinutes(2) };
        var p2 = new UserProfile { Id = Guid.NewGuid(), Name = "İki", CreatedAt = _clock.UtcNow.AddMinutes(1) };
        await _repository.SaveProfileAsync(p1);
        await _repository.SaveProfileAsync(p2);

        var list = await _sut.GetProfilesAsync();

        Assert.Equal(2, list.Count);
        Assert.Equal(p2.Id, list[0].Id);
        Assert.Equal(p1.Id, list[1].Id);
    }

    [Fact]
    public async Task CreateAsync_GecerliAdlaProfilOlusturur_VeKaydeder()
    {
        var profile = await _sut.CreateAsync("  Yeni Profil  ");

        Assert.Equal("Yeni Profil", profile.Name);
        Assert.NotEqual(Guid.Empty, profile.Id);
        var inRepo = Assert.Single(await _repository.GetProfilesAsync());
        Assert.Equal(profile.Id, inRepo.Id);
        Assert.Equal("Yeni Profil", inRepo.Name);
    }

    [Fact]
    public async Task CreateAsync_AyniAdlaProfilVarsa_HataFirlatir()
    {
        await _sut.CreateAsync("Ayşe");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync("ayşe"));
    }

    [Fact]
    public async Task RenameAsync_ProfiliYenidenAdlandirir_VeAktifProfilGuncellenir()
    {
        var profile = await _sut.CreateAsync("Eski Ad");
        await _sut.OpenAsync(profile.Id);

        var renamed = await _sut.RenameAsync(profile.Id, "Yeni Ad");

        Assert.Equal("Yeni Ad", renamed.Name);
        Assert.Equal("Yeni Ad", _sut.ActiveProfile?.Name);
        var inRepo = Assert.Single(await _repository.GetProfilesAsync());
        Assert.Equal("Yeni Ad", inRepo.Name);
    }

    [Fact]
    public async Task RenameAsync_OlmayanProfilIcin_HataFirlatir()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.RenameAsync(Guid.NewGuid(), "Yeni Ad"));
    }

    [Fact]
    public async Task DeleteAsync_AcikOlanProfilIcin_HataFirlatir()
    {
        var p1 = await _sut.CreateAsync("Bir");
        var p2 = await _sut.CreateAsync("İki");
        await _sut.OpenAsync(p1.Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DeleteAsync(p1.Id));

        Assert.Equal("Açık olan profil silinemez. Önce profil seçim ekranına dön.", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_SonKalanProfilIcin_HataFirlatir()
    {
        var p1 = await _sut.CreateAsync("Tek Profil");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DeleteAsync(p1.Id));

        Assert.Equal("Son kalan profil silinemez.", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_KapaliVeCokluProfildenBirini_BasariylaSiler()
    {
        var p1 = await _sut.CreateAsync("Bir");
        var p2 = await _sut.CreateAsync("İki");

        await _sut.DeleteAsync(p2.Id);

        var profiles = await _repository.GetProfilesAsync();
        Assert.Single(profiles);
        Assert.Equal(p1.Id, profiles[0].Id);
    }

    [Fact]
    public async Task OpenAsync_ProfiliAcar_StoreSwitchiTetikler_VeSessionIdYeniler()
    {
        var profile = await _sut.CreateAsync("Birinci");

        var opened = await _sut.OpenAsync(profile.Id);

        Assert.Equal(profile.Id, opened.Id);
        Assert.Equal(profile.Id, _sut.ActiveProfile?.Id);
        Assert.NotNull(_sut.ActiveProfile?.LastOpenedAt);
        Assert.NotEqual(Guid.Empty, _sut.SessionId);
        Assert.Equal(profile.Id, _storeSwitch.OpenProfileId);

        var previousSession = _sut.SessionId;
        await _sut.OpenAsync(profile.Id);
        Assert.NotEqual(previousSession, _sut.SessionId);
    }

    [Fact]
    public async Task CloseAsync_AktifProfiliSifirlar_VeStoreSwitchiKapatir()
    {
        var profile = await _sut.CreateAsync("Birinci");
        await _sut.OpenAsync(profile.Id);

        await _sut.CloseAsync();

        Assert.Null(_sut.ActiveProfile);
        Assert.False(_storeSwitch.IsOpen);
        Assert.Equal(1, _storeSwitch.CloseCalls);
    }

    private sealed class SteppingTestClock : IClock
    {
        private DateTimeOffset _now = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

        public DateOnly Today => DateOnly.FromDateTime(_now.Date);

        public DateTimeOffset UtcNow => _now = _now.AddSeconds(1);
        public DateTimeOffset Now => UtcNow;
    }
}
