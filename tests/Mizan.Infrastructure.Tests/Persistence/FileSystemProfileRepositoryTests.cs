using Mizan.Application.Models;
using Mizan.Infrastructure.Persistence;

namespace Mizan.Infrastructure.Tests.Persistence;

/// <summary>
/// Profil deposunun disk üzerindeki CRUD işlemlerini, atomik meta yazımını
/// ve bozuk/eksik meta dosyasından kurtarma davranışını doğrulayan testler.
/// </summary>
public sealed class FileSystemProfileRepositoryTests : IDisposable
{
    private readonly string _rootDirectory;

    public FileSystemProfileRepositoryTests()
    {
        _rootDirectory = Path.Combine(
            Path.GetTempPath(),
            $"mizan_profil_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_rootDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, recursive: true);
        }
    }

    private FileSystemProfileRepository Olustur() => new(_rootDirectory);

    // ── GetProfilesAsync ────────────────────────────────────────

    [Fact]
    public async Task ProfillerGetir_ProfilKlasoruYoksa_BosDiziDondurur()
    {
        var depo = Olustur();

        var profiller = await depo.GetProfilesAsync();

        Assert.Empty(profiller);
    }

    [Fact]
    public async Task ProfillerGetir_KayitliProfiliGeriOkur()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();
        var profil = new UserProfile
        {
            Id = profilId,
            Name = "Ayşe",
            CreatedAt = new DateTimeOffset(2026, 8, 20, 9, 0, 0, TimeSpan.Zero),
            LastOpenedAt = new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero)
        };
        await depo.SaveProfileAsync(profil);

        var profiller = await depo.GetProfilesAsync();

        var okunan = Assert.Single(profiller);
        Assert.Equal(profilId, okunan.Id);
        Assert.Equal("Ayşe", okunan.Name);
        Assert.Equal(profil.CreatedAt, okunan.CreatedAt);
        Assert.Equal(profil.LastOpenedAt, okunan.LastOpenedAt);
    }

    [Fact]
    public async Task ProfillerGetir_BirdenFazlaProfilListeler()
    {
        var depo = Olustur();
        var profil1 = new UserProfile { Id = Guid.NewGuid(), Name = "Ayşe" };
        var profil2 = new UserProfile { Id = Guid.NewGuid(), Name = "Mehmet" };
        await depo.SaveProfileAsync(profil1);
        await depo.SaveProfileAsync(profil2);

        var profiller = await depo.GetProfilesAsync();

        Assert.Equal(2, profiller.Count);
        Assert.Contains(profiller, p => p.Id == profil1.Id && p.Name == "Ayşe");
        Assert.Contains(profiller, p => p.Id == profil2.Id && p.Name == "Mehmet");
    }

    [Fact]
    public async Task ProfillerGetir_GecersizGuidKlasoruYokSayar()
    {
        var depo = Olustur();
        var profillerKlasoru = Path.Combine(_rootDirectory, DatabaseConstants.ProfilesDirectoryName);
        Directory.CreateDirectory(profillerKlasoru);

        // GUID olmayan klasör adı → atlanmalı
        Directory.CreateDirectory(Path.Combine(profillerKlasoru, "gecersiz-klasor"));

        // Geçerli bir profil de olsun ki boş liste yerine tek eleman geri dönmeli
        var gecerliProfil = new UserProfile { Id = Guid.NewGuid(), Name = "Geçerli" };
        await depo.SaveProfileAsync(gecerliProfil);

        var profiller = await depo.GetProfilesAsync();

        var okunan = Assert.Single(profiller);
        Assert.Equal(gecerliProfil.Id, okunan.Id);
    }

    // ── SaveProfileAsync — atomik yazma ──────────────────────────

    [Fact]
    public async Task ProfilKaydet_MetaDosyasiniOlusturur()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();
        var profil = new UserProfile { Id = profilId, Name = "İpek" };

        await depo.SaveProfileAsync(profil);

        var metaYolu = Path.Combine(
            depo.GetProfileDirectory(profilId),
            DatabaseConstants.MetadataFileName);
        Assert.True(File.Exists(metaYolu));
    }

    [Fact]
    public async Task ProfilKaydet_GeciciDosyaArkadaKalmaz()
    {
        var depo = Olustur();
        var profil = new UserProfile { Id = Guid.NewGuid(), Name = "Ayşe" };

        await depo.SaveProfileAsync(profil);

        var profilKlasoru = depo.GetProfileDirectory(profil.Id);
        var geciciDosyalar = Directory.GetFiles(profilKlasoru, "*.tmp");
        Assert.Empty(geciciDosyalar);
    }

    [Fact]
    public async Task ProfilKaydet_VarolanProfiliGunceller()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();
        var profil = new UserProfile
        {
            Id = profilId,
            Name = "Ayşe",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
        await depo.SaveProfileAsync(profil);

        var guncellenmis = profil with
        {
            Name = "Ayşe Yılmaz",
            LastOpenedAt = new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero)
        };
        await depo.SaveProfileAsync(guncellenmis);

        var profiller = await depo.GetProfilesAsync();
        var okunan = Assert.Single(profiller);
        Assert.Equal("Ayşe Yılmaz", okunan.Name);
        Assert.Equal(guncellenmis.LastOpenedAt, okunan.LastOpenedAt);
    }

    // ── DeleteProfileAsync ───────────────────────────────────────

    [Fact]
    public async Task ProfilSil_KlasoruTamamenSiler()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();
        await depo.SaveProfileAsync(new UserProfile { Id = profilId, Name = "Silinecek" });

        var profilKlasoru = depo.GetProfileDirectory(profilId);
        Assert.True(Directory.Exists(profilKlasoru));

        await depo.DeleteProfileAsync(profilId);

        Assert.False(Directory.Exists(profilKlasoru));
        var profiller = await depo.GetProfilesAsync();
        Assert.Empty(profiller);
    }

    [Fact]
    public async Task ProfilSil_OlmayanProfilHataVermez()
    {
        var depo = Olustur();

        // Mevcut olmayan bir profili silmek sessizce geçmeli
        await depo.DeleteProfileAsync(Guid.NewGuid());
    }

    // ── Bozuk meta kurtarma ──────────────────────────────────────

    [Fact]
    public async Task ProfillerGetir_BozukMetaDosyasi_VeritabaniVarsa_VarsayilanAdlaKurtarir()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();
        await depo.SaveProfileAsync(new UserProfile { Id = profilId, Name = "Ayşe" });

        // Veritabanı dosyasını oluştur (boş dosya yeter)
        var dbYolu = depo.GetDatabasePath(profilId);
        await File.WriteAllBytesAsync(dbYolu, []);

        // Meta dosyasını boz
        var metaYolu = Path.Combine(
            depo.GetProfileDirectory(profilId),
            DatabaseConstants.MetadataFileName);
        await File.WriteAllTextAsync(metaYolu, "{bozuk json");

        var profiller = await depo.GetProfilesAsync();

        var kurtarilan = Assert.Single(profiller);
        Assert.Equal(profilId, kurtarilan.Id);
        Assert.Equal(UserProfile.DefaultName, kurtarilan.Name);
    }

    [Fact]
    public async Task ProfillerGetir_MetaDosyasiYok_VeritabaniVarsa_VarsayilanAdlaKurtarir()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();

        // Klasörü ve veritabanını elle oluştur, meta dosyası yok
        var profilKlasoru = depo.GetProfileDirectory(profilId);
        Directory.CreateDirectory(profilKlasoru);
        var dbYolu = depo.GetDatabasePath(profilId);
        await File.WriteAllBytesAsync(dbYolu, []);

        var profiller = await depo.GetProfilesAsync();

        var kurtarilan = Assert.Single(profiller);
        Assert.Equal(profilId, kurtarilan.Id);
        Assert.Equal(UserProfile.DefaultName, kurtarilan.Name);
    }

    [Fact]
    public async Task ProfillerGetir_NeMetaNeVeritabani_ProfilListelenmez()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();

        // Sadece boş GUID klasörü oluştur, içinde hiçbir şey yok
        var profilKlasoru = depo.GetProfileDirectory(profilId);
        Directory.CreateDirectory(profilKlasoru);

        var profiller = await depo.GetProfilesAsync();

        Assert.Empty(profiller);
    }

    // ── IProfileFileLayout doğrulama ─────────────────────────────

    [Fact]
    public void DosyaYerlesimi_ProfilYollariDogruUretilir()
    {
        var depo = Olustur();
        var profilId = Guid.NewGuid();

        var profilKlasoru = depo.GetProfileDirectory(profilId);
        var dbYolu = depo.GetDatabasePath(profilId);

        Assert.Equal(
            Path.Combine(_rootDirectory, "profiles", profilId.ToString("N")),
            profilKlasoru);
        Assert.Equal(
            Path.Combine(profilKlasoru, "mizan.db3"),
            dbYolu);
    }

    [Fact]
    public void DosyaYerlesimi_KokDiziniDogruDondurulur()
    {
        var depo = Olustur();

        Assert.Equal(_rootDirectory, depo.RootDirectory);
    }

    // ── Yapıcı doğrulaması ───────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Yapici_BosKokDizin_ArgumentExceptionFirlatir(string? gecersiz)
    {
        Assert.Throws<ArgumentException>(() => new FileSystemProfileRepository(gecersiz!));
    }
}
