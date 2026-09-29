using Mizan.Application.Models;
using Mizan.Infrastructure.Persistence;
using SQLite;

namespace Mizan.Infrastructure.Tests.Backup;

/// <summary>
/// Eski sürümlü bir yedeğin geri yüklenmesi (S69). Yedek uygulamanın eski bir sürümünden ya da o sürümden
/// beri hiç açılmamış bir profilden gelebilir; veritabanı yerine konmadan önce hazırlıkta yükseltilir ki
/// yükseltme düşerse geri yükleme hep-ya-hiç düşsün ve listede açılamayan bir profil kalmasın (I36).
/// Uygulamanın daha yeni sürümü, üretim listesine sahte bir adım eklenerek taklit edilir.
/// </summary>
public sealed class ProfileBackupArchiveUpgradeTests : IDisposable
{
    private static readonly DateTimeOffset Ocak1 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private readonly YedekTestKurulumu _eskiSurum = new();

    public void Dispose() => _eskiSurum.Dispose();

    [Fact]
    public async Task GeriYukle_VeritabaniEskiSurumde_YerineKonmadanYukseltilir()
    {
        var ayse = await VerisiOlanProfilAsync();
        using var yedek = await _eskiSurum.YedekAlAsync();
        using var yeniSurum = new YedekTestKurulumu(SonrakiSurum("CREATE TABLE deneme (Deger INTEGER);"));

        await yeniSurum.Arsiv().ImportAsync(yedek, [new(ayse.Id, ayse)]);

        Assert.Equal(SchemaMigrations.CurrentVersion + 1, DosyaSurumu(yeniSurum.Depo.GetDatabasePath(ayse.Id)));
        Assert.Equal(["Konut"], await yeniSurum.KrediAdlariAsync(ayse.Id));
    }

    [Fact]
    public async Task GeriYukle_EskiSurumYukseltilemezse_HicbirSeyEklenmez()
    {
        var ayse = await VerisiOlanProfilAsync();
        using var yedek = await _eskiSurum.YedekAlAsync();
        using var yeniSurum = new YedekTestKurulumu(SonrakiSurum("BU BIR SQL DEGIL;"));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(
            () => yeniSurum.Arsiv().ImportAsync(yedek, [new(ayse.Id, ayse)]));

        Assert.Equal("Yedek geri yüklenemedi: \"Ayşe\" profilinin verisi bu sürüme yükseltilemedi.", hata.Message);
        Assert.Empty(await yeniSurum.Depo.GetProfilesAsync());
        Assert.Empty(yeniSurum.HazirlikKlasorleri());
    }

    private async Task<UserProfile> VerisiOlanProfilAsync()
    {
        var profil = await _eskiSurum.ProfilEkleAsync("Ayşe", Ocak1);
        await _eskiSurum.KrediYazAsync(profil.Id, "Konut");
        await _eskiSurum.Anahtar.CloseAsync();
        return profil;
    }

    /// <summary>Üretimdeki şemadan bir sürüm sonrası: uygulamanın güncellenmiş hâli.</summary>
    private static DatabaseSchema SonrakiSurum(string komut) =>
        new([.. SchemaMigrations.All, new SchemaMigration(SchemaMigrations.CurrentVersion + 1, [komut])]);

    /// <summary>Dosyanın sürümünü profili açmadan okur; açılış kendisi yükselteceği için ondan önce bakılır.</summary>
    private static int DosyaSurumu(string veritabani)
    {
        using var baglanti = new SQLiteConnection(veritabani, SQLiteOpenFlags.ReadOnly);
        return baglanti.ExecuteScalar<int>("PRAGMA user_version;");
    }
}
