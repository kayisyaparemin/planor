using Mizan.Application.Models;
using Mizan.Infrastructure.Tests.Backup;

namespace Mizan.Infrastructure.Tests.LegacyImport;

/// <summary>
/// Eski yedeği tanıma yarısını (G1a) doğrulayan testler: kullanıcı bir dosya seçtiğinde veritabanlarına
/// dokunmadan "bu eski uygulamanın yedeği mi, içinde kimler var" sorusunun cevabı (S83).
/// </summary>
public sealed class LegacyBackupImporterSummaryTests : IDisposable
{
    private readonly Eski17ImportKurulumu _kurulum = new();

    public void Dispose() => _kurulum.Dispose();

    [Fact]
    public async Task OzetOku_EskiYedek_ProfilleriManifesttekiGibiListeler()
    {
        using var yedek = Eski17Yedegi.Olustur(
            (Eski17Yedegi.Emin, "Emin", []),
            (Eski17Yedegi.Gizem, "Gizem", []));

        var ozet = await _kurulum.Aktarici.ReadSummaryAsync(yedek);

        Assert.Equal(new DateTimeOffset(2026, 9, 25, 21, 30, 0, TimeSpan.Zero), ozet.CreatedAt);
        Assert.Equal(
            [
                new BackupProfile(Eski17Yedegi.Emin, "Emin", new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero), null),
                new BackupProfile(Eski17Yedegi.Gizem, "Gizem", new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero), null)
            ],
            ozet.Profiles);
    }

    [Fact]
    public async Task OzetOku_AkisiKapatmaz_ImportIcinBasaSarilabilir()
    {
        using var yedek = Eski17Yedegi.Olustur(Eski17Yedegi.Emin, "Emin");

        await _kurulum.Aktarici.ReadSummaryAsync(yedek);

        Assert.True(yedek.CanRead);
        yedek.Position = 0;
        Assert.Equal(["Emin"], (await _kurulum.Aktarici.ReadSummaryAsync(yedek)).ProfileNames);
    }

    /// <summary>
    /// Bu uygulamanın kendi yedeği (biçim 2) eski yedek değildir; içe aktarıcıya verilirse hangi aracın
    /// hangi dosyayı açtığı karışmasın diye açık mesajla reddedilir.
    /// </summary>
    [Fact]
    public async Task OzetOku_Mizan2Yedegi_EskiUygulamaYedegiDegilDiyeReddedilir()
    {
        await _kurulum.Yedek.ProfilEkleAsync("Ayşe", YedekTestKurulumu.Simdi);
        using var yedek = await _kurulum.Yedek.YedekAlAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => _kurulum.Aktarici.ReadSummaryAsync(yedek));

        Assert.Equal("Bu yedek eski Mizan uygulamasından alınmamış.", hata.Message);
    }

    [Fact]
    public async Task OzetOku_ZipOlmayanDosya_MizanYedegiDegilDiyeReddedilir()
    {
        using var yedek = new MemoryStream("zip degil"u8.ToArray());

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => _kurulum.Aktarici.ReadSummaryAsync(yedek));

        Assert.Equal("Bu dosya bir Mizan yedeği değil.", hata.Message);
    }
}
