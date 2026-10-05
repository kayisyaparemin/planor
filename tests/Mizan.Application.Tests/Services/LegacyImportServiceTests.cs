using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Eski yedekten içe aktarma servisinin adlandırma, çakışma ve hata davranışlarını doğrulayan testler.
/// </summary>
public sealed class LegacyImportServiceTests
{
    private static readonly DateTimeOffset Simdi = new(2026, 10, 5, 9, 0, 0, TimeSpan.Zero);

    private readonly FakeLegacyBackupImporter _importer = new();
    private readonly InMemoryProfileRepository _profiles = new();
    private readonly LegacyImportService _sut;

    public LegacyImportServiceTests()
    {
        _sut = new LegacyImportService(_importer, _profiles, new SabitSaat(Simdi));
    }

    [Fact]
    public async Task ImportAsync_YedekteProfilVarsa_HepsiniKendiKimlikVeAdiylaEkler()
    {
        var first = EskiProfil("Ev");
        var second = EskiProfil("Is");
        _importer.ProfilesInBackup.AddRange([first, second]);

        var result = await _sut.ImportAsync(new MemoryStream([1, 2, 3]));

        Assert.Equal(2, result.Added.Count);
        Assert.All(result.Added, added => Assert.False(added.IsCopy));
        Assert.Equal([first.Id, second.Id], _importer.LastImported.Select(i => i.SourceId));
        Assert.Equal(["Ev", "Is"], _importer.LastImported.Select(i => i.Target.Name));
    }

    [Fact]
    public async Task ImportAsync_KimligiZatenVarsa_KopyaAdiylaYeniKimlikleEkler_MevcutuDegistirmez()
    {
        var old = EskiProfil("Ev");
        _importer.ProfilesInBackup.Add(old);
        await _profiles.SaveProfileAsync(new UserProfile { Id = old.Id, Name = "Ev", CreatedAt = Simdi });

        var result = await _sut.ImportAsync(new MemoryStream([1, 2, 3]));

        var added = Assert.Single(result.Added);
        Assert.True(added.IsCopy);
        Assert.NotEqual(old.Id, added.Profile.Id);
        Assert.Contains("yedeği", added.Profile.Name);
        Assert.Equal(added.Profile.Id, _importer.LastImported.Single().Target.Id);
        var kept = Assert.Single(await _profiles.GetProfilesAsync(), p => p.Id == old.Id);
        Assert.Equal("Ev", kept.Name);
    }

    [Fact]
    public async Task ImportAsync_AdiZatenVarsa_BenzersizAdVerir()
    {
        _importer.ProfilesInBackup.Add(EskiProfil("Ev"));
        await _profiles.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Ev", CreatedAt = Simdi });

        var result = await _sut.ImportAsync(new MemoryStream([1, 2, 3]));

        Assert.NotEqual("Ev", result.Added.Single().Profile.Name);
    }

    [Fact]
    public async Task ImportAsync_EskiYedekDegilse_HataFirlatirVeHicbiriniEklemez()
    {
        _importer.SummaryException = new InvalidOperationException("Bu yedek eski Mizan uygulamasından alınmamış.");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportAsync(new MemoryStream([1, 2, 3])));

        Assert.Contains("eski", error.Message);
        Assert.Equal(0, _importer.ImportCallCount);
    }

    [Fact]
    public async Task ImportAsync_YedekteProfilYoksa_HataFirlatirVeIceAktarmaz()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportAsync(new MemoryStream([1, 2, 3])));

        Assert.Equal(0, _importer.ImportCallCount);
    }

    [Fact]
    public async Task ImportAsync_OzetOkunduktanSonra_AkisiBasaSararakIceAktarir()
    {
        _importer.ProfilesInBackup.Add(EskiProfil("Ev"));

        await _sut.ImportAsync(new MemoryStream([1, 2, 3]));

        Assert.Equal(0, _importer.PositionAtImport);
    }

    [Fact]
    public async Task ImportAsync_KonumlanamayanAkisVerilirse_ArgumentExceptionFirlatir()
    {
        _importer.ProfilesInBackup.Add(EskiProfil("Ev"));

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ImportAsync(new KonumlanamayanAkis()));
    }

    private static BackupProfile EskiProfil(string name) =>
        new(Guid.NewGuid(), name, new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), null);

    private sealed class KonumlanamayanAkis : MemoryStream
    {
        public override bool CanSeek => false;
    }

    private sealed class SabitSaat(DateTimeOffset utcNow) : IClock
    {
        public DateOnly Today { get; } = DateOnly.FromDateTime(utcNow.UtcDateTime);
        public DateTimeOffset UtcNow { get; } = utcNow;
        public DateTimeOffset Now => UtcNow;
    }
}
