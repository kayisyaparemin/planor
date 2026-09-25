using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Infrastructure.Tests.Backup;

/// <summary>
/// Yedek parmak izinin "veri değişti mi?" sorusunu doğru cevapladığını doğrulayan testler.
/// Gece yedeği yalnız parmak izi değişince yeni dosya yazar; bu yüzden hem değişiklikleri
/// yakalamalı hem de veriye dokunmayan olaylarda (profili açmak, dosya zamanı) sabit kalmalıdır.
/// </summary>
public sealed class ProfileBackupArchiveFingerprintTests : IDisposable
{
    private static readonly DateTimeOffset Ocak1 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private readonly YedekTestKurulumu _kurulum = new();

    public void Dispose() => _kurulum.Dispose();

    [Fact]
    public async Task ParmakIzi_VeriDegismediyse_AyniKalir()
    {
        var ayse = await VerisiOlanProfilAsync("Ayşe");
        await _kurulum.ProfilEkleAsync("Mehmet", Ocak1.AddDays(1));

        var ilk = await ParmakIziAsync();
        var ikinci = await ParmakIziAsync();

        Assert.Equal(ilk, ikinci);
        Assert.True(File.Exists(_kurulum.Depo.GetDatabasePath(ayse.Id)));
    }

    [Fact]
    public async Task ParmakIzi_ProfileKayitEklenince_Degisir()
    {
        var ayse = await VerisiOlanProfilAsync("Ayşe");
        var once = await ParmakIziAsync();

        await _kurulum.KrediYazAsync(ayse.Id, "Taşıt");
        await _kurulum.Anahtar.CloseAsync();

        Assert.NotEqual(once, await ParmakIziAsync());
    }

    [Fact]
    public async Task ParmakIzi_ProfilAdiDegisince_Degisir()
    {
        var ayse = await VerisiOlanProfilAsync("Ayşe");
        var once = await ParmakIziAsync();

        await _kurulum.Depo.SaveProfileAsync(ayse with { Name = "Ayşe Hanım" });

        Assert.NotEqual(once, await ParmakIziAsync());
    }

    [Fact]
    public async Task ParmakIzi_VerisizYeniProfilEklenince_Degisir()
    {
        await VerisiOlanProfilAsync("Ayşe");
        var once = await ParmakIziAsync();

        await _kurulum.ProfilEkleAsync("Hiç Açılmadı", Ocak1.AddDays(1));

        Assert.NotEqual(once, await ParmakIziAsync());
    }

    [Fact]
    public async Task ParmakIzi_ProfiliYalnizAcipKapatmak_VeSonAcilisTarihi_Degistirmez()
    {
        var ayse = await VerisiOlanProfilAsync("Ayşe");
        var once = await ParmakIziAsync();

        await _kurulum.Anahtar.OpenAsync(ayse.Id);
        await _kurulum.Anahtar.CloseAsync();
        await _kurulum.Depo.SaveProfileAsync(ayse with { LastOpenedAt = Ocak1.AddDays(30) });

        Assert.Equal(once, await ParmakIziAsync());
    }

    [Fact]
    public async Task ParmakIzi_VeritabaniDosyasininZamaniDegisipIcerigiAyniKalinca_Degistirmez()
    {
        var ayse = await VerisiOlanProfilAsync("Ayşe");
        var once = await ParmakIziAsync();

        File.SetLastWriteTimeUtc(_kurulum.Depo.GetDatabasePath(ayse.Id), new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(once, await ParmakIziAsync());
    }

    [Fact]
    public async Task ParmakIzi_ProfilListesininSirasindanEtkilenmez()
    {
        await VerisiOlanProfilAsync("Ayşe");
        await VerisiOlanProfilAsync("Mehmet");

        var dogal = await ParmakIziAsync();
        var ters = await ParmakIziAsync(new TersSiraliProfiller(_kurulum.Depo));

        Assert.Equal(dogal, ters);
    }

    private async Task<UserProfile> VerisiOlanProfilAsync(string ad)
    {
        var profil = await _kurulum.ProfilEkleAsync(ad, Ocak1);
        await _kurulum.KrediYazAsync(profil.Id, "Konut");
        await _kurulum.Anahtar.CloseAsync();
        return profil;
    }

    /// <summary>Parmak izini hesaplar ve SHA-256'nın 64 onaltılık karakteri olduğunu doğrular.</summary>
    private async Task<string> ParmakIziAsync(IProfileRepository? profiller = null)
    {
        var parmakIzi = await _kurulum.Arsiv(profiller).ComputeFingerprintAsync();
        Assert.Matches("^[0-9A-F]{64}$", parmakIzi);
        return parmakIzi;
    }

    private sealed class TersSiraliProfiller(IProfileRepository ic) : IProfileRepository
    {
        public async Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken = default) =>
            (await ic.GetProfilesAsync(cancellationToken)).Reverse().ToArray();

        public Task SaveProfileAsync(UserProfile profile, CancellationToken cancellationToken = default) =>
            ic.SaveProfileAsync(profile, cancellationToken);

        public Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default) =>
            ic.DeleteProfileAsync(profileId, cancellationToken);
    }
}
