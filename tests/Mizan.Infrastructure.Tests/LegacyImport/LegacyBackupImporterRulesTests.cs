using Mizan.Application.Models;
using Mizan.Infrastructure.Tests.Backup;

namespace Mizan.Infrastructure.Tests.LegacyImport;

/// <summary>
/// İçe aktarmanın bütünlük kurallarını doğrulayan testler: hep-ya-hiç, tanınmayan şema sürümü, verisiz
/// profil, güncel şemaya yükseltme ve profil kaydı (S83, S58, I36).
/// </summary>
public sealed class LegacyBackupImporterRulesTests : IDisposable
{
    private const string TanimayanSurum = "UPDATE settings SET SchemaVersion = 16;";
    private readonly Eski17ImportKurulumu _kurulum = new();

    public void Dispose() => _kurulum.Dispose();

    [Fact]
    public async Task ImportAsync_Sonuc_GuncelSemadaVeBagiKopukDegildir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin);

        Assert.Equal(_kurulum.Yedek.Sema.CurrentVersion, _kurulum.Deger<int>(profil, "PRAGMA user_version;"));
        Assert.Equal(0, _kurulum.KopukBagSayisi(profil));
    }

    [Fact]
    public async Task ImportAsync_ProfilKaydi_HedefKimlikVeAdlaYazilir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        var profil = await _kurulum.AktarAsync(yedek, Eski17Yedegi.Emin, "Emin (eski)");

        var kayitlar = await _kurulum.Yedek.Depo.GetProfilesAsync();
        Assert.Equal([(profil, "Emin (eski)")], kayitlar.Select(kayit => (kayit.Id, kayit.Name)));
        Assert.NotEqual(Eski17Yedegi.Emin, profil);
    }

    [Fact]
    public async Task ImportAsync_VerisiOlmayanProfil_BosProfilOlarakEklenir()
    {
        using var yedek = Eski17Yedegi.VerisizProfil(Eski17Yedegi.Emin, "Emin");
        var hedef = new UserProfile { Id = Guid.NewGuid(), Name = "Emin", CreatedAt = YedekTestKurulumu.Simdi };

        await _kurulum.Aktarici.ImportAsync(yedek, [new ProfileImport(Eski17Yedegi.Emin, hedef)]);

        Assert.Equal([hedef.Id], (await _kurulum.Yedek.Depo.GetProfilesAsync()).Select(kayit => kayit.Id));
        Assert.False(File.Exists(_kurulum.Yedek.Depo.GetDatabasePath(hedef.Id)));
    }

    /// <summary>
    /// Şema v17 dışındaki bir veritabanı yanlış sütunlarla okunurdu; "profil boş geldi" hatasına
    /// dönmesin diye tanınmaz ve hiçbir profil eklenmez.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ProfildenBiriTanimayanSemadaysa_HicbiriEklenmez()
    {
        using var yedek = Eski17Yedegi.Olustur(
            (Eski17Yedegi.Emin, "Emin", []),
            (Eski17Yedegi.Gizem, "Gizem", [TanimayanSurum]));
        var emin = new UserProfile { Id = Guid.NewGuid(), Name = "Emin", CreatedAt = YedekTestKurulumu.Simdi };
        var gizem = new UserProfile { Id = Guid.NewGuid(), Name = "Gizem", CreatedAt = YedekTestKurulumu.Simdi };

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => _kurulum.Aktarici.ImportAsync(
            yedek,
            [new ProfileImport(Eski17Yedegi.Emin, emin), new ProfileImport(Eski17Yedegi.Gizem, gizem)]));

        Assert.Contains("Gizem", hata.Message);
        Assert.Empty(await _kurulum.Yedek.Depo.GetProfilesAsync());
        Assert.Empty(_kurulum.Yedek.HazirlikKlasorleri());
    }

    [Fact]
    public async Task ImportAsync_SemaSurumuDahaYeniyse_DahaYeniSurumMesajiylaReddedilir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin", "UPDATE settings SET SchemaVersion = 18;");
        var hedef = new UserProfile { Id = Guid.NewGuid(), Name = "Emin", CreatedAt = YedekTestKurulumu.Simdi };

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _kurulum.Aktarici.ImportAsync(yedek, [new ProfileImport(Eski17Yedegi.Emin, hedef)]));

        Assert.Equal("Bu yedek Mizan'ın daha yeni bir sürümünden alınmış. Önce uygulamayı güncelle.", hata.Message);
        Assert.Empty(await _kurulum.Yedek.Depo.GetProfilesAsync());
    }

    [Fact]
    public async Task ImportAsync_Mizan2Yedegi_EskiUygulamaYedegiDegilDiyeReddedilir()
    {
        var kaynak = await _kurulum.Yedek.ProfilEkleAsync("Ayşe", YedekTestKurulumu.Simdi);
        using var yedek = await _kurulum.Yedek.YedekAlAsync();
        var hedef = new UserProfile { Id = Guid.NewGuid(), Name = "Ayşe", CreatedAt = YedekTestKurulumu.Simdi };

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _kurulum.Aktarici.ImportAsync(yedek, [new ProfileImport(kaynak.Id, hedef)]));

        Assert.Equal("Bu yedek eski Mizan uygulamasından alınmamış.", hata.Message);
    }

    [Fact]
    public async Task ImportAsync_SecimBossa_HicbirSeyEklemeden_Reddedilir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _kurulum.Aktarici.ImportAsync(yedek, []));
    }
}
