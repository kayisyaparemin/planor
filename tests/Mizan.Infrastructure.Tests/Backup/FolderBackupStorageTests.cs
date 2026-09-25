using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Tests.Fakes;

namespace Mizan.Infrastructure.Tests.Backup;

/// <summary>
/// Yedek klasörünü (I4c) gerçek geçici klasörde doğrulayan testler: kaydetme, yarım kalan kopya,
/// listeleme, okuma, silme, klasör dışına çıkamama ve iznin platformdan gelmesi.
/// </summary>
public sealed class FolderBackupStorageTests : IDisposable
{
    private const string BugununYedegi = "Mizan-yedegi-2026-09-26.zip";

    private readonly string _kok;
    private readonly string _klasor;
    private readonly FolderBackupStorage _depo;

    public FolderBackupStorageTests()
    {
        _kok = Path.Combine(Path.GetTempPath(), $"mizan_klasor_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_kok);
        _klasor = Path.Combine(_kok, "Mizan");
        _depo = new FolderBackupStorage(_klasor, "Dahili depolama › Mizan", new SahteDepolamaIzni());
    }

    public void Dispose()
    {
        if (Directory.Exists(_kok))
        {
            Directory.Delete(_kok, recursive: true);
        }
    }

    // ── Kaydetme ─────────────────────────────────────────────────

    [Fact]
    public async Task Kaydet_KlasorYoksa_OlusturupIcerigiKopyalar()
    {
        var kaynak = KaynakYaz([1, 2, 3]);

        await _depo.SaveAsync(BugununYedegi, kaynak);

        Assert.Equal([BugununYedegi], KlasordekiDosyalar());
        Assert.Equal([1, 2, 3], Icerik(BugununYedegi));
    }

    [Fact]
    public async Task Kaydet_AyniAdliDosyaVarsa_UzerineYazar()
    {
        KlasoreYaz(BugununYedegi, [9, 9]);
        var kaynak = KaynakYaz([1, 2, 3]);

        await _depo.SaveAsync(BugununYedegi, kaynak);

        Assert.Equal([BugununYedegi], KlasordekiDosyalar());
        Assert.Equal([1, 2, 3], Icerik(BugununYedegi));
    }

    [Fact]
    public async Task Kaydet_KopyalamaYaridaKalirsa_OncekiYedekBozulmaz_VeGeciciDosyaKalmaz()
    {
        KlasoreYaz(BugununYedegi, [9, 9]);
        var kaynak = KaynakYaz([1, 2, 3]);
        using var iptal = new CancellationTokenSource();
        await iptal.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _depo.SaveAsync(BugununYedegi, kaynak, iptal.Token));

        Assert.Equal([BugununYedegi], KlasordekiDosyalar());
        Assert.Equal([9, 9], Icerik(BugununYedegi));
    }

    [Fact]
    public async Task Kaydet_AdKlasorDisiniGosterse_KlasorIcineYazar()
    {
        var kaynak = KaynakYaz([1, 2, 3]);

        await _depo.SaveAsync("../disari.zip", kaynak);

        Assert.Equal(["disari.zip"], KlasordekiDosyalar());
        Assert.False(File.Exists(Path.Combine(_kok, "disari.zip")));
    }

    // ── Listeleme ────────────────────────────────────────────────

    [Fact]
    public async Task Listele_KlasorYoksa_BosDoner()
    {
        var liste = await _depo.ListAsync();

        Assert.Empty(liste);
        Assert.False(Directory.Exists(_klasor));
    }

    [Fact]
    public async Task Listele_KlasordekiTumDosyalari_DegisiklikZamanlariylaDoner()
    {
        var zaman = new DateTime(2026, 9, 20, 21, 30, 0, DateTimeKind.Utc);
        KlasoreYaz("Mizan-yedegi-2026-09-20.zip", [1], zaman);
        KlasoreYaz("Mizan-yedek-2026-09-19.ZIP", [2], zaman.AddDays(-1));
        KlasoreYaz("notlar.txt", [3], zaman.AddDays(-2));
        Directory.CreateDirectory(Path.Combine(_klasor, "alt-klasor"));

        var liste = await _depo.ListAsync();

        Assert.Equal(
            [
                ("Mizan-yedegi-2026-09-20.zip", new DateTimeOffset(zaman)),
                ("Mizan-yedek-2026-09-19.ZIP", new DateTimeOffset(zaman.AddDays(-1))),
                ("notlar.txt", new DateTimeOffset(zaman.AddDays(-2)))
            ],
            liste.OrderByDescending(y => y.ModifiedAt).Select(y => (y.FileName, y.ModifiedAt)));
        Assert.All(liste, y => Assert.Equal(TimeSpan.Zero, y.ModifiedAt.Offset));
    }

    // ── Okuma ve silme ───────────────────────────────────────────

    [Fact]
    public async Task Oku_KaydedilenDosyaninIceriginiSaltOkunurVerir()
    {
        KlasoreYaz(BugununYedegi, [4, 5, 6]);

        await using var akis = await _depo.OpenReadAsync(BugununYedegi);

        Assert.False(akis.CanWrite);
        using var bellek = new MemoryStream();
        await akis.CopyToAsync(bellek);
        Assert.Equal([4, 5, 6], bellek.ToArray());
    }

    [Fact]
    public async Task Sil_KlasordekiDosyayiSiler_DigerlerineDokunmaz()
    {
        KlasoreYaz(BugununYedegi, [1]);
        KlasoreYaz("Mizan-yedek-2026-09-26.zip", [2]);

        await _depo.DeleteAsync(BugununYedegi);

        Assert.Equal(["Mizan-yedek-2026-09-26.zip"], KlasordekiDosyalar());
    }

    [Fact]
    public async Task Sil_DosyaYoksa_SessizGecer()
    {
        var hata = await Record.ExceptionAsync(() => _depo.DeleteAsync(BugununYedegi));

        Assert.Null(hata);
    }

    [Fact]
    public async Task Sil_AdKlasorDisiniGosterse_DisaridakiDosyayaDokunmaz()
    {
        var disaridaki = Path.Combine(_kok, "disari.zip");
        await File.WriteAllBytesAsync(disaridaki, [7]);
        KlasoreYaz("disari.zip", [8]);

        await _depo.DeleteAsync("../disari.zip");

        Assert.True(File.Exists(disaridaki));
        Assert.Empty(KlasordekiDosyalar());
    }

    // ── İzin ─────────────────────────────────────────────────────

    [Fact]
    public async Task Erisim_IzinVeKonumAciklamasi_PlatformdanGelir()
    {
        var izin = new SahteDepolamaIzni { IstenirseVerir = true };
        var depo = new FolderBackupStorage(_klasor, "Dahili depolama › Mizan", izin);
        var oncekiIzin = depo.HasAccess;

        var verildi = await depo.RequestAccessAsync();

        Assert.False(oncekiIzin);
        Assert.True(verildi);
        Assert.True(depo.HasAccess);
        Assert.Equal("Dahili depolama › Mizan", depo.LocationDescription);
    }

    // ── Yardımcılar ──────────────────────────────────────────────

    private string KaynakYaz(byte[] icerik)
    {
        var yol = Path.Combine(_kok, $"kaynak-{Guid.NewGuid():N}.zip");
        File.WriteAllBytes(yol, icerik);
        return yol;
    }

    private void KlasoreYaz(string ad, byte[] icerik, DateTime? degisiklikUtc = null)
    {
        Directory.CreateDirectory(_klasor);
        var yol = Path.Combine(_klasor, ad);
        File.WriteAllBytes(yol, icerik);
        if (degisiklikUtc is { } zaman)
        {
            File.SetLastWriteTimeUtc(yol, zaman);
        }
    }

    /// <summary>Klasördeki bütün dosyalar, gizli geçici dosyalar dahil, ada göre sıralı.</summary>
    private string[] KlasordekiDosyalar() =>
        Directory.Exists(_klasor)
            ? Directory.GetFiles(_klasor).Select(yol => Path.GetFileName(yol)).Order(StringComparer.Ordinal).ToArray()
            : [];

    private byte[] Icerik(string ad) => File.ReadAllBytes(Path.Combine(_klasor, ad));
}
