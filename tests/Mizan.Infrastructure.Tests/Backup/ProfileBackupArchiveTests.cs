using System.IO.Compression;
using System.Text.Json;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Backup;

/// <summary>
/// Yedek arşivinin yazma yarısını (I4a) doğrulayan testler: zip'in biçimi, veritabanı anlık
/// görüntüleri, açık profilin yedeklenmesi, çalışma klasörünün temizliği ve son yedek kaydı.
/// </summary>
public sealed class ProfileBackupArchiveTests : IDisposable
{
    private static readonly DateTimeOffset Ocak1 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private readonly YedekTestKurulumu _kurulum = new();

    public void Dispose() => _kurulum.Dispose();

    // ── Manifest ─────────────────────────────────────────────────

    [Fact]
    public async Task Yaz_ManifestBicim2_SaatinAnini_Sema1i_VeProfilleriOlusturulmaSirasiylaTasir()
    {
        var ayse = await _kurulum.ProfilEkleAsync("Ayşe", Ocak1.AddDays(2));
        var mehmet = await _kurulum.ProfilEkleAsync("Mehmet", Ocak1);
        var zeynep = await _kurulum.ProfilEkleAsync("Zeynep", Ocak1.AddDays(1));
        await _kurulum.KrediYazAsync(ayse.Id, "Konut");
        await _kurulum.Anahtar.CloseAsync();

        using var yedek = new MemoryStream();
        var ozet = await _kurulum.Arsiv().WriteAsync(yedek);

        Assert.Equal(YedekTestKurulumu.Simdi, ozet.CreatedAt);
        Assert.Equal(["Mehmet", "Zeynep", "Ayşe"], ozet.ProfileNames);
        Assert.Equal([mehmet.Id, zeynep.Id, ayse.Id], ozet.Profiles.Select(p => p.Id));

        yedek.Position = 0;
        using var zip = new ZipArchive(yedek, ZipArchiveMode.Read);
        var manifestGirdisi = zip.GetEntry("mizan-backup.json");
        Assert.NotNull(manifestGirdisi);
        using var manifest = await JsonDocument.ParseAsync(manifestGirdisi.Open());
        var kok = manifest.RootElement;
        Assert.Equal(2, kok.GetProperty("Format").GetInt32());
        Assert.Equal(YedekTestKurulumu.Simdi, kok.GetProperty("CreatedAt").GetDateTimeOffset());
        Assert.Equal(SchemaMigrations.CurrentVersion, kok.GetProperty("SchemaVersion").GetInt32());
        var profiller = kok.GetProperty("Profiles").EnumerateArray().ToArray();
        Assert.Equal(
            [(mehmet.Id, "Mehmet", false), (zeynep.Id, "Zeynep", false), (ayse.Id, "Ayşe", true)],
            profiller.Select(p => (
                p.GetProperty("Id").GetGuid(),
                p.GetProperty("Name").GetString()!,
                p.GetProperty("HasData").GetBoolean())));
    }

    [Fact]
    public async Task Yaz_ProfilinSonAcilisTarihiniOzeteVeManifesteTasir()
    {
        var profil = await _kurulum.ProfilEkleAsync("Ayşe", Ocak1);
        var acilis = Ocak1.AddDays(30);
        await _kurulum.Depo.SaveProfileAsync(profil with { LastOpenedAt = acilis });

        var ozet = await _kurulum.Arsiv().WriteAsync(new MemoryStream());

        var yedektekiProfil = Assert.Single(ozet.Profiles);
        Assert.Equal(new BackupProfile(profil.Id, "Ayşe", Ocak1, acilis), yedektekiProfil);
    }

    // ── Veritabanı girdileri ─────────────────────────────────────

    [Fact]
    public async Task Yaz_VerisiOlanProfil_KendiVeritabaniGirdisiyleYedeklenir_VeriVeSemaSurumuKorunur()
    {
        var ayse = await _kurulum.ProfilEkleAsync("Ayşe", Ocak1);
        await _kurulum.KrediYazAsync(ayse.Id, "Konut");
        await _kurulum.Anahtar.CloseAsync();

        using var yedek = await _kurulum.YedekAlAsync();

        using var zip = new ZipArchive(yedek, ZipArchiveMode.Read);
        var cikti = _kurulum.VeritabaniGirdisiniCikar(zip, ayse.Id);
        Assert.Equal(SchemaMigrations.CurrentVersion, SemaSurumu(cikti));
        Assert.Equal(["Konut"], await KrediAdlariAsync(cikti));
    }

    [Fact]
    public async Task Yaz_HicAcilmamisProfil_ManifesteVerisizGirer_VeritabaniGirdisiOlmaz()
    {
        var bos = await _kurulum.ProfilEkleAsync("Hiç Açılmadı", Ocak1);

        using var yedek = await _kurulum.YedekAlAsync();

        using var zip = new ZipArchive(yedek, ZipArchiveMode.Read);
        Assert.Equal(["mizan-backup.json"], zip.Entries.Select(e => e.FullName));
        Assert.False(File.Exists(_kurulum.Depo.GetDatabasePath(bos.Id)));
    }

    [Fact]
    public async Task Yaz_ProfilAcikkenAlinanYedek_AcikBaglantininSonYazdigiKaydiTasir()
    {
        var ayse = await _kurulum.ProfilEkleAsync("Ayşe", Ocak1);
        await _kurulum.KrediYazAsync(ayse.Id, "Konut");
        await _kurulum.KrediYazAsync(ayse.Id, "Taşıt");

        // Profil açık kalır: kullanıcı gece uygulamayı açık bırakmış olabilir.
        using var yedek = await _kurulum.YedekAlAsync();
        await _kurulum.Anahtar.CloseAsync();

        using var zip = new ZipArchive(yedek, ZipArchiveMode.Read);
        var cikti = _kurulum.VeritabaniGirdisiniCikar(zip, ayse.Id);
        Assert.Equal(["Konut", "Taşıt"], (await KrediAdlariAsync(cikti)).Order());
    }

    // ── Çalışma klasörü ──────────────────────────────────────────

    [Fact]
    public async Task Yaz_Basariyla_Bitince_CalismaKlasoruKalmaz()
    {
        var ayse = await _kurulum.ProfilEkleAsync("Ayşe", Ocak1);
        await _kurulum.KrediYazAsync(ayse.Id, "Konut");
        await _kurulum.Anahtar.CloseAsync();

        using var yedek = await _kurulum.YedekAlAsync();

        Assert.True(yedek.Length > 0);
        Assert.Empty(Directory.GetDirectories(_kurulum.Kok, ".backup-*"));
    }

    [Fact]
    public async Task Yaz_BozukVeritabani_YedegiDurdurur_CalismaKlasoruKalmaz()
    {
        var bozuk = await _kurulum.ProfilEkleAsync("Bozuk", Ocak1);
        await File.WriteAllBytesAsync(_kurulum.Depo.GetDatabasePath(bozuk.Id), new byte[8192]);

        await Assert.ThrowsAsync<SQLiteException>(() => _kurulum.Arsiv().WriteAsync(new MemoryStream()));

        Assert.Empty(Directory.GetDirectories(_kurulum.Kok, ".backup-*"));
    }

    [Fact]
    public async Task Yaz_IptalEdilince_YazmayiBirakir_CalismaKlasoruKalmaz()
    {
        await _kurulum.ProfilEkleAsync("Ayşe", Ocak1);
        using var iptal = new CancellationTokenSource();
        await iptal.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _kurulum.Arsiv().WriteAsync(new MemoryStream(), iptal.Token));

        Assert.Empty(Directory.GetDirectories(_kurulum.Kok, ".backup-*"));
    }

    // ── Son yedek kaydı ──────────────────────────────────────────

    [Fact]
    public async Task SonYedekKaydi_HicKaydedilmemisse_BosDoner()
    {
        Assert.Null(await _kurulum.Arsiv().GetStateAsync());
    }

    [Fact]
    public async Task SonYedekKaydi_KaydedileniAynenGeriVerir_GeciciDosyaBirakmaz()
    {
        var arsiv = _kurulum.Arsiv();
        var kayit = new BackupState(YedekTestKurulumu.Simdi, "Mizan-yedegi-2026-09-25.zip", "ABC123");

        await arsiv.SaveStateAsync(kayit);
        await arsiv.SaveStateAsync(kayit with { Fingerprint = "DEF456" });

        Assert.Equal(kayit with { Fingerprint = "DEF456" }, await arsiv.GetStateAsync());
        Assert.True(File.Exists(Path.Combine(_kurulum.Kok, "backup-state.json")));
        Assert.Empty(Directory.GetFiles(_kurulum.Kok, "*.tmp"));
    }

    [Fact]
    public async Task SonYedekKaydi_BozukDosya_BosDoner()
    {
        await File.WriteAllTextAsync(Path.Combine(_kurulum.Kok, "backup-state.json"), "{ bozuk");

        Assert.Null(await _kurulum.Arsiv().GetStateAsync());
    }

    [Fact]
    public async Task SonYedekKaydi_KokKlasorYokken_KlasoruOlusturupKaydeder()
    {
        var yeniKok = Path.Combine(_kurulum.Kok, "henuz-yok");
        var depo = new FileSystemProfileRepository(yeniKok);
        var arsiv = new ProfileBackupArchive(depo, depo, new SabitSaat(), new DatabaseSchema());
        var kayit = new BackupState(YedekTestKurulumu.Simdi, "Mizan-yedegi-2026-09-25.zip", "ABC123");

        await arsiv.SaveStateAsync(kayit);

        Assert.Equal(kayit, await arsiv.GetStateAsync());
    }

    // ── Yapıcı ───────────────────────────────────────────────────

    [Fact]
    public void Yapici_EksikBagimlilik_ArgumentNullExceptionFirlatir()
    {
        var depo = _kurulum.Depo;
        var saat = new SabitSaat();
        var sema = new DatabaseSchema();

        Assert.Throws<ArgumentNullException>(() => new ProfileBackupArchive(null!, depo, saat, sema));
        Assert.Throws<ArgumentNullException>(() => new ProfileBackupArchive(depo, null!, saat, sema));
        Assert.Throws<ArgumentNullException>(() => new ProfileBackupArchive(depo, depo, null!, sema));
        Assert.Throws<ArgumentNullException>(() => new ProfileBackupArchive(depo, depo, saat, null!));
    }

    private static int SemaSurumu(string veritabani)
    {
        using var baglanti = new SQLiteConnection(veritabani, SQLiteOpenFlags.ReadOnly);
        return baglanti.ExecuteScalar<int>("PRAGMA user_version;");
    }

    private static async Task<IReadOnlyList<string>> KrediAdlariAsync(string veritabani)
    {
        var baglanti = await new SqliteConnectionFactory().CreateConnectionAsync(veritabani);
        try
        {
            var krediler = await new SqliteLoanRepository(baglanti).GetLoansAsync();
            return krediler.Select(k => k.Name).ToArray();
        }
        finally
        {
            await baglanti.CloseAsync();
        }
    }

    private sealed class SabitSaat : IClock
    {
        public DateOnly Today => new(2026, 9, 25);

        public DateTimeOffset UtcNow => YedekTestKurulumu.Simdi;
    }
}
