using Mizan.Application.Models;
using Mizan.Infrastructure.Tests.Fakes;

namespace Mizan.Infrastructure.Tests.Backup;

/// <summary>
/// Geri yükleme yarısını (I4b) doğrulayan testler. Yedeğin sözü: geri dönen her profil bıraktığı
/// gibidir; seçilenlerden biri geri yüklenemezse hiçbiri eklenmez, mevcut profillere dokunulmaz ve
/// geride hazırlık klasörü kalmaz.
/// </summary>
public sealed class ProfileBackupArchiveImportTests : IDisposable
{
    private static readonly DateTimeOffset Ocak1 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private readonly YedekTestKurulumu _kaynak = new();
    private readonly YedekTestKurulumu _hedef = new();

    public void Dispose()
    {
        _kaynak.Dispose();
        _hedef.Dispose();
    }

    // ── Başarılı geri yükleme ────────────────────────────────────

    [Fact]
    public async Task GeriYukle_YeniKurulumda_ProfillerKimlikAdVeVerileriyleGelir_VerisizProfilVerisizKalir()
    {
        var ayse = await _kaynak.ProfilEkleAsync("Ayşe", Ocak1);
        ayse = ayse with { LastOpenedAt = Ocak1.AddDays(3) };
        await _kaynak.Depo.SaveProfileAsync(ayse);
        var bos = await _kaynak.ProfilEkleAsync("Hiç Açılmadı", Ocak1.AddDays(1));
        await _kaynak.KrediYazAsync(ayse.Id, "Konut");
        await _kaynak.Anahtar.CloseAsync();
        using var yedek = await _kaynak.YedekAlAsync();

        await _hedef.Arsiv().ImportAsync(yedek, [new(ayse.Id, ayse), new(bos.Id, bos)]);

        Assert.Equal([ayse, bos], (await _hedef.Depo.GetProfilesAsync()).OrderBy(p => p.CreatedAt));
        Assert.False(File.Exists(_hedef.Depo.GetDatabasePath(bos.Id)));
        Assert.Equal(["Konut"], await _hedef.KrediAdlariAsync(ayse.Id));
        Assert.Empty(_hedef.HazirlikKlasorleri());
    }

    /// <summary>
    /// "Eski hâline bakmak": yedek alındıktan sonra profil değişti. Yedekteki hâl yeni kimlikle ayrı
    /// bir profil olarak gelir; mevcut profil olduğu gibi kalır.
    /// </summary>
    [Fact]
    public async Task GeriYukle_YeniKimlikleKopya_MevcutProfileDokunmaz()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        using var yedek = await _kaynak.YedekAlAsync();
        await _kaynak.KrediYazAsync(ayse.Id, "Taşıt");
        await _kaynak.Anahtar.CloseAsync();
        var kopya = new UserProfile { Id = Guid.NewGuid(), Name = "Ayşe (25 Eylül yedeği)", CreatedAt = Ocak1 };

        await _kaynak.Arsiv().ImportAsync(yedek, [new(ayse.Id, kopya)]);

        Assert.Equal(2, (await _kaynak.Depo.GetProfilesAsync()).Count);
        Assert.Equal(["Konut", "Taşıt"], await _kaynak.KrediAdlariAsync(ayse.Id));
        Assert.Equal(["Konut"], await _kaynak.KrediAdlariAsync(kopya.Id));
    }

    // ── Hep ya hiç ───────────────────────────────────────────────

    [Fact]
    public async Task GeriYukle_SecilenlerdenBirininVerisiOkunamazsa_HicbiriEklenmez_HazirlikKlasoruKalmaz()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        var mehmet = await VerisiOlanProfilAsync(_kaynak, "Mehmet");
        using var yedek = await _kaynak.YedekAlAsync();
        using var bozuk = YedekTestKurulumu.Kurcala(yedek, (ad, icerik) =>
            ad.Contains($"{mehmet.Id:N}", StringComparison.Ordinal) ? new byte[8192] : icerik);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(bozuk, [new(ayse.Id, ayse), new(mehmet.Id, mehmet)]));

        Assert.Equal("Yedek geri yüklenemedi: \"Mehmet\" profilinin verisi okunamadı.", hata.Message);
        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
        Assert.Empty(_hedef.HazirlikKlasorleri());
    }

    [Fact]
    public async Task GeriYukle_VerininBirSayfasiBozuksa_BozukDer_HicbirSeyEklenmez()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        using var yedek = await _kaynak.YedekAlAsync();
        using var bozuk = YedekTestKurulumu.Kurcala(yedek, (ad, icerik) =>
            ad.EndsWith(".db3", StringComparison.Ordinal) ? SonSayfayiSifirla(icerik) : icerik);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(bozuk, [new(ayse.Id, ayse)]));

        Assert.Equal("Yedek geri yüklenemedi: \"Ayşe\" profilinin verisi bozuk.", hata.Message);
        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
    }

    [Fact]
    public async Task GeriYukle_TasimaIkinciProfildeYaridaKalirsa_IlkProfilDeGeriAlinir()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        var mehmet = await VerisiOlanProfilAsync(_kaynak, "Mehmet");
        using var yedek = await _kaynak.YedekAlAsync();
        var arsiv = _hedef.Arsiv(new YarimKalanProfilDeposu(_hedef.Depo, patlayanKayit: 2));

        await Assert.ThrowsAsync<IOException>(
            () => arsiv.ImportAsync(yedek, [new(ayse.Id, ayse), new(mehmet.Id, mehmet)]));

        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
        Assert.False(Directory.Exists(_hedef.Depo.GetProfileDirectory(ayse.Id)));
        Assert.False(Directory.Exists(_hedef.Depo.GetProfileDirectory(mehmet.Id)));
        Assert.Empty(_hedef.HazirlikKlasorleri());
    }

    [Fact]
    public async Task GeriYukle_HedeflerdenBiriZatenVarsa_HicbirSeyTasinmaz_MevcutProfilAynenKalir()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        var mehmet = await VerisiOlanProfilAsync(_kaynak, "Mehmet");
        using var yedek = await _kaynak.YedekAlAsync();
        await _kaynak.KrediYazAsync(ayse.Id, "Taşıt");
        await _kaynak.Anahtar.CloseAsync();
        var mehmetKopya = mehmet with { Id = Guid.NewGuid() };

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _kaynak.Arsiv().ImportAsync(yedek, [new(mehmet.Id, mehmetKopya), new(ayse.Id, ayse)]));

        Assert.Equal("Eklenecek profil telefonda zaten var.", hata.Message);
        Assert.Equal(2, (await _kaynak.Depo.GetProfilesAsync()).Count);
        Assert.False(Directory.Exists(_kaynak.Depo.GetProfileDirectory(mehmetKopya.Id)));
        Assert.Equal(["Konut", "Taşıt"], await _kaynak.KrediAdlariAsync(ayse.Id));
    }

    [Fact]
    public async Task GeriYukle_IptalEdilince_HicbirSeyEklenmez()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        using var yedek = await _kaynak.YedekAlAsync();
        using var iptal = new CancellationTokenSource();
        await iptal.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _hedef.Arsiv().ImportAsync(yedek, [new(ayse.Id, ayse)], iptal.Token));

        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
        Assert.Empty(_hedef.HazirlikKlasorleri());
    }

    // ── Sürüm ────────────────────────────────────────────────────

    [Fact]
    public async Task GeriYukle_VeritabaniSurumuDahaYeni_GuncellemeIster_HicbirSeyEklenmez()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        _kaynak.SemaSurumunuDegistir(ayse.Id, 2);
        using var yedek = await _kaynak.YedekAlAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(yedek, [new(ayse.Id, ayse)]));

        Assert.Equal("Bu yedek Mizan'ın daha yeni bir sürümünden alınmış. Önce uygulamayı güncelle.", hata.Message);
        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
    }

    /// <summary>
    /// Sürümü 0 olan veritabanı açılışta "yeni" sanılıp tabloları üstüne kurulur ve profil boş açılırdı;
    /// kullanıcı verisinin kaybolduğunu sanırdı (S58).
    /// </summary>
    [Fact]
    public async Task GeriYukle_VeritabaniSurumuSifir_TaninmiyorDer_HicbirSeyEklenmez()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        _kaynak.SemaSurumunuDegistir(ayse.Id, 0);
        using var yedek = await _kaynak.YedekAlAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(yedek, [new(ayse.Id, ayse)]));

        Assert.Equal("Yedek geri yüklenemedi: \"Ayşe\" profilinin verisi tanınmıyor.", hata.Message);
        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
    }

    [Fact]
    public async Task GeriYukle_EskiUygulamaninYedegi_ReddedilirHicbirSeyEklenmez()
    {
        var profilId = Guid.NewGuid();
        using var eski = YedekTestKurulumu.Zip(
            YedekTestKurulumu.ManifestMetni(bicim: 1, sema: 17, (profilId, "Profilim", true)),
            ($"profiles/{profilId:N}/coinflow.db3", new byte[64]));
        var hedef = new UserProfile { Id = profilId, Name = "Profilim", CreatedAt = Ocak1 };

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(eski, [new(profilId, hedef)]));

        Assert.Equal("Bu yedek eski Mizan uygulamasından alınmış; bu sürümde geri yüklenemez.", hata.Message);
        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
    }

    // ── Seçim ve eksik veri ──────────────────────────────────────

    [Fact]
    public async Task GeriYukle_ManifestVeriVarDiyorAmaGirdiYok_Reddedilir()
    {
        var ayse = await VerisiOlanProfilAsync(_kaynak, "Ayşe");
        using var yedek = await _kaynak.YedekAlAsync();
        using var eksik = YedekTestKurulumu.Kurcala(yedek, (ad, icerik) =>
            ad.EndsWith(".db3", StringComparison.Ordinal) ? null : icerik);

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(eksik, [new(ayse.Id, ayse)]));

        Assert.Equal("Yedek geri yüklenemedi: \"Ayşe\" profilinin verisi yedekte yok.", hata.Message);
    }

    [Fact]
    public async Task GeriYukle_SecilenProfilYedekteYok_Reddedilir()
    {
        var ayse = await _kaynak.ProfilEkleAsync("Ayşe", Ocak1);
        using var yedek = await _kaynak.YedekAlAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(yedek, [new(Guid.NewGuid(), ayse)]));

        Assert.Equal("Yedek geri yüklenemedi: Seçilen profil yedekte yok.", hata.Message);
        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
    }

    [Fact]
    public async Task GeriYukle_SecimBos_Reddedilir()
    {
        await _kaynak.ProfilEkleAsync("Ayşe", Ocak1);
        using var yedek = await _kaynak.YedekAlAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(yedek, []));

        Assert.Equal("Eklenecek profil seçilmedi.", hata.Message);
    }

    [Fact]
    public async Task GeriYukle_AyniHedefIkiKez_Reddedilir()
    {
        var ayse = await _kaynak.ProfilEkleAsync("Ayşe", Ocak1);
        var mehmet = await _kaynak.ProfilEkleAsync("Mehmet", Ocak1);
        using var yedek = await _kaynak.YedekAlAsync();

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _hedef.Arsiv().ImportAsync(yedek, [new(ayse.Id, ayse), new(mehmet.Id, ayse)]));

        Assert.Equal("Aynı profil iki kez eklenemez.", hata.Message);
        Assert.Empty(await _hedef.Depo.GetProfilesAsync());
    }

    [Fact]
    public async Task GeriYukle_EksikArguman_ArgumentNullExceptionFirlatir()
    {
        var arsiv = _hedef.Arsiv();

        await Assert.ThrowsAsync<ArgumentNullException>(() => arsiv.ImportAsync(null!, []));
        await Assert.ThrowsAsync<ArgumentNullException>(() => arsiv.ImportAsync(new MemoryStream(), null!));
    }

    private static async Task<UserProfile> VerisiOlanProfilAsync(YedekTestKurulumu kurulum, string ad)
    {
        var profil = await kurulum.ProfilEkleAsync(ad, Ocak1);
        await kurulum.KrediYazAsync(profil.Id, "Konut");
        await kurulum.Anahtar.CloseAsync();
        return profil;
    }

    /// <summary>Veritabanının son sayfasını sıfırlar: başlık sağlam kalır, bir tablonun sayfası bozulur.</summary>
    private static byte[] SonSayfayiSifirla(byte[] veritabani)
    {
        var sayfaBoyutu = (veritabani[16] << 8) | veritabani[17];
        sayfaBoyutu = sayfaBoyutu == 1 ? 65536 : sayfaBoyutu;
        var bozuk = (byte[])veritabani.Clone();
        Array.Clear(bozuk, bozuk.Length - sayfaBoyutu, sayfaBoyutu);
        return bozuk;
    }
}
