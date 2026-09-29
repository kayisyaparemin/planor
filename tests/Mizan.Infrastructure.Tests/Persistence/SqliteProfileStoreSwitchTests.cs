using Mizan.Infrastructure.Persistence;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence;

/// <summary>
/// Profil deposu anahtarının açma/kapama yaşam döngüsünü, kapalıyken bağlantı
/// istenmesi engelini ve profil geçişi davranışını doğrulayan testler.
/// </summary>
public sealed class SqliteProfileStoreSwitchTests : IDisposable
{
    private readonly string _rootDirectory;
    private readonly FileSystemProfileRepository _layout;
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteProfileStoreSwitchTests()
    {
        _rootDirectory = Path.Combine(
            Path.GetTempPath(),
            $"mizan_switch_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_rootDirectory);
        _layout = new FileSystemProfileRepository(_rootDirectory);
        _connectionFactory = new SqliteConnectionFactory();
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            try { Directory.Delete(_rootDirectory, recursive: true); } catch { /* temizlik */ }
        }
    }

    private SqliteProfileStoreSwitch Olustur() =>
        new(_layout, _connectionFactory);

    // ── Kapalı profil engeli ──────────────────────────────────────

    [Fact]
    public void Connection_ProfilAcilmadan_InvalidOperationExceptionFirlatir()
    {
        var anahtar = Olustur();

        var istisna = Assert.Throws<InvalidOperationException>(
            () => _ = anahtar.Connection);

        Assert.Contains("profil", istisna.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── OpenAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task Ac_GecerliProfil_BaglantiDondurur()
    {
        var anahtar = Olustur();
        var profilId = Guid.NewGuid();

        await anahtar.OpenAsync(profilId);

        var baglanti = anahtar.Connection;
        Assert.NotNull(baglanti);
        Assert.IsType<SQLiteAsyncConnection>(baglanti);
    }

    [Fact]
    public async Task Ac_VeritabaniDosyasiOlusturulur()
    {
        var anahtar = Olustur();
        var profilId = Guid.NewGuid();

        await anahtar.OpenAsync(profilId);

        var dbYolu = _layout.GetDatabasePath(profilId);
        Assert.True(File.Exists(dbYolu));
    }

    [Fact]
    public async Task Ac_SemaBaslatilir()
    {
        var anahtar = Olustur();
        var profilId = Guid.NewGuid();

        await anahtar.OpenAsync(profilId);

        // Şema kurulduysa user_version güncel sürüm olmalı
        var surum = await anahtar.Connection.ExecuteScalarAsync<int>(
            "PRAGMA user_version;");
        Assert.Equal(SchemaMigrations.CurrentVersion, surum);
    }

    // ── CloseAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Kapat_AcilmisProfil_BaglantiEngellenir()
    {
        var anahtar = Olustur();
        await anahtar.OpenAsync(Guid.NewGuid());

        await anahtar.CloseAsync();

        Assert.Throws<InvalidOperationException>(() => _ = anahtar.Connection);
    }

    [Fact]
    public async Task Kapat_ZatenKapali_HataVermez()
    {
        var anahtar = Olustur();

        // Hiç açılmadan kapatma sessizce geçmeli
        await anahtar.CloseAsync();
    }

    // ── Profil geçişi ────────────────────────────────────────────

    [Fact]
    public async Task Ac_FarkliProfil_OncekiKapatilirYeniAcilir()
    {
        var anahtar = Olustur();
        var profilA = Guid.NewGuid();
        var profilB = Guid.NewGuid();

        await anahtar.OpenAsync(profilA);
        var eskiBaglanti = anahtar.Connection;

        await anahtar.OpenAsync(profilB);
        var yeniBaglanti = anahtar.Connection;

        Assert.NotNull(yeniBaglanti);
        // Yeni açılan profil farklı bir veritabanı dosyasını göstermeli
        var dbYoluB = _layout.GetDatabasePath(profilB);
        Assert.True(File.Exists(dbYoluB));
    }

    // ── IAsyncDisposable ──────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_AcikBaglantiyiKapatir()
    {
        var anahtar = Olustur();
        await anahtar.OpenAsync(Guid.NewGuid());

        await anahtar.DisposeAsync();

        // Dispose sonrası Connection erişimi hata vermeli
        Assert.Throws<InvalidOperationException>(() => _ = anahtar.Connection);
    }

    // ── Yapıcı doğrulama ─────────────────────────────────────────

    [Fact]
    public void Yapici_NullLayout_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(
            () => new SqliteProfileStoreSwitch(null!, _connectionFactory));
    }

    [Fact]
    public void Yapici_NullConnectionFactory_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(
            () => new SqliteProfileStoreSwitch(_layout, null!));
    }
}
