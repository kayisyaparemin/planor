using Mizan.Application.Models;

namespace Mizan.Infrastructure.Tests.Backup;

/// <summary>
/// Yedeği tanıma yarısını (I4b) doğrulayan testler: kullanıcı bir dosya seçtiğinde veritabanlarına
/// dokunmadan "bu bir Mizan yedeği mi, hangi sürümden, içinde kimler var" sorularının cevabı.
/// </summary>
public sealed class ProfileBackupArchiveSummaryTests : IDisposable
{
    private const string MizanYedegiDegil = "Bu dosya bir Mizan yedeği değil.";
    private const string DahaYeniSurum = "Bu yedek Mizan'ın daha yeni bir sürümünden alınmış. Önce uygulamayı güncelle.";
    private static readonly DateTimeOffset Ocak1 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private readonly YedekTestKurulumu _kurulum = new();

    public void Dispose() => _kurulum.Dispose();

    [Fact]
    public async Task OzetOku_YazilanYedek_TarihiVeProfilleriYazildigiGibiVerir()
    {
        var ayse = await _kurulum.ProfilEkleAsync("Ayşe", Ocak1.AddDays(1));
        var mehmet = await _kurulum.ProfilEkleAsync("Mehmet", Ocak1);
        await _kurulum.Depo.SaveProfileAsync(ayse with { LastOpenedAt = Ocak1.AddDays(5) });
        await _kurulum.KrediYazAsync(ayse.Id, "Konut");
        await _kurulum.Anahtar.CloseAsync();
        using var yedek = await _kurulum.YedekAlAsync();

        var ozet = await _kurulum.Arsiv().ReadSummaryAsync(yedek);

        Assert.Equal(YedekTestKurulumu.Simdi, ozet.CreatedAt);
        Assert.Equal(
            [
                new BackupProfile(mehmet.Id, "Mehmet", Ocak1, null),
                new BackupProfile(ayse.Id, "Ayşe", Ocak1.AddDays(1), Ocak1.AddDays(5))
            ],
            ozet.Profiles);
    }

    [Fact]
    public async Task OzetOku_AkisiKapatmaz_GeriYuklemeIcinBasaSarilabilir()
    {
        await _kurulum.ProfilEkleAsync("Ayşe", Ocak1);
        using var yedek = await _kurulum.YedekAlAsync();

        await _kurulum.Arsiv().ReadSummaryAsync(yedek);

        Assert.True(yedek.CanRead);
        yedek.Position = 0;
        Assert.Equal(["Ayşe"], (await _kurulum.Arsiv().ReadSummaryAsync(yedek)).ProfileNames);
    }

    /// <summary>
    /// Eski uygulamanın yedeği manifestinde şema v17 taşır; biçime önce bakılmasaydı kullanıcıya
    /// "daha yeni bir sürümden alınmış" diye tam tersi söylenirdi (S57).
    /// </summary>
    [Fact]
    public async Task OzetOku_EskiUygulamaninYedegi_EskiUygulamaMesajiylaReddedilir()
    {
        var profilId = Guid.NewGuid();
        using var eski = YedekTestKurulumu.Zip(
            YedekTestKurulumu.ManifestMetni(bicim: 1, sema: 17, (profilId, "Profilim", true)),
            ($"profiles/{profilId:N}/coinflow.db3", new byte[64]));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _kurulum.Arsiv().ReadSummaryAsync(eski));

        Assert.Equal("Bu yedek eski Mizan uygulamasından alınmış; bu sürümde geri yüklenemez.", hata.Message);
    }

    [Theory]
    [InlineData(3, 1)]
    [InlineData(2, 2)]
    public async Task OzetOku_DahaYeniBicimYaDaSema_GuncellemeIster(int bicim, int sema)
    {
        using var yedek = YedekTestKurulumu.Zip(
            YedekTestKurulumu.ManifestMetni(bicim, sema, (Guid.NewGuid(), "Ayşe", false)));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _kurulum.Arsiv().ReadSummaryAsync(yedek));

        Assert.Equal(DahaYeniSurum, hata.Message);
    }

    [Theory]
    [InlineData("zip-degil")]
    [InlineData("manifest-yok")]
    [InlineData("bozuk-json")]
    [InlineData("bicim-yok")]
    [InlineData("profil-listesi-yok")]
    public async Task OzetOku_MizanYedegiOlmayanDosya_Reddedilir(string durum)
    {
        using var dosya = durum switch
        {
            "zip-degil" => new MemoryStream([1, 2, 3, 4, 5]),
            "manifest-yok" => YedekTestKurulumu.Zip(null, ("fotograf.jpg", new byte[16])),
            "bozuk-json" => YedekTestKurulumu.Zip("{ bozuk"),
            "bicim-yok" => YedekTestKurulumu.Zip(
                $$"""{"SchemaVersion":1,"Profiles":[{"Id":"{{Guid.NewGuid()}}","Name":"Ayşe","HasData":false}]}"""),
            _ => YedekTestKurulumu.Zip("""{"Format":2,"SchemaVersion":1}""")
        };

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _kurulum.Arsiv().ReadSummaryAsync(dosya));

        Assert.Equal(MizanYedegiDegil, hata.Message);
    }

    [Theory]
    [InlineData("bos-liste", "Yedek geri yüklenemedi: Yedekte hiç profil yok.")]
    [InlineData("bos-kimlik", "Yedek geri yüklenemedi: Yedekteki profil listesi bozuk.")]
    [InlineData("bos-ad", "Yedek geri yüklenemedi: Yedekteki profil listesi bozuk.")]
    [InlineData("tekrar-eden-kimlik", "Yedek geri yüklenemedi: Yedekteki profil listesi bozuk.")]
    public async Task OzetOku_BozukProfilListesi_Reddedilir(string durum, string mesaj)
    {
        var kimlik = Guid.NewGuid();
        (Guid, string, bool)[] profiller = durum switch
        {
            "bos-liste" => [],
            "bos-kimlik" => [(Guid.Empty, "Ayşe", false)],
            "bos-ad" => [(kimlik, "  ", false)],
            _ => [(kimlik, "Ayşe", false), (kimlik, "Mehmet", false)]
        };
        using var yedek = YedekTestKurulumu.Zip(YedekTestKurulumu.ManifestMetni(2, 1, profiller));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _kurulum.Arsiv().ReadSummaryAsync(yedek));

        Assert.Equal(mesaj, hata.Message);
    }

    [Fact]
    public async Task OzetOku_AkisYoksa_ArgumentNullExceptionFirlatir()
    {
        var arsiv = _kurulum.Arsiv();

        await Assert.ThrowsAsync<ArgumentNullException>(() => arsiv.ReadSummaryAsync(null!));
    }
}
