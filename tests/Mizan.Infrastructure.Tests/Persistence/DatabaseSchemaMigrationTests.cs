using System.Text.RegularExpressions;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence;

/// <summary>
/// Şema göçünün sözünü (S69) doğrulayan testler: eksik adımlar sırayla ve yalnız bir kez çalışır,
/// yükseltme ya tamamlanır ya hiç olmamış gibi geri alınır, tablo yeniden kurulurken çocuk kayıt silinmez.
/// Uygulamanın daha yeni bir sürümü, üretim listesine sahte adımlar eklenerek taklit edilir.
/// </summary>
public sealed class DatabaseSchemaMigrationTests : IAsyncLifetime
{
    private const string BozukKomut = "BU BIR SQL DEGIL;";
    private static readonly int Guncel = SchemaMigrations.CurrentVersion;

    private readonly string _yol = Path.Combine(Path.GetTempPath(), $"mizan_test_goc_{Guid.NewGuid():N}.db3");
    private readonly List<SQLiteAsyncConnection> _baglantilar = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var baglanti in _baglantilar)
        {
            await baglanti.CloseAsync();
        }

        foreach (var ek in new[] { "", "-shm", "-wal", "-journal" })
        {
            try { File.Delete(_yol + ek); } catch { /* temizlik */ }
        }
    }

    [Fact]
    public async Task EnsureInitializedAsync_IkiAdimGeride_AdimlariSiraylaCalistirir()
    {
        var baglanti = await GuncelVeritabaniAsync();
        var sema = Sema(
            Adim(1, "CREATE TABLE deneme (Deger INTEGER NOT NULL);", "INSERT INTO deneme VALUES (2);"),
            Adim(2, "UPDATE deneme SET Deger = Deger * 10;"));

        await sema.EnsureInitializedAsync(baglanti);

        Assert.Equal(Guncel + 2, await SurumAsync(baglanti));
        Assert.Equal(20, await baglanti.ExecuteScalarAsync<int>("SELECT Deger FROM deneme;"));
    }

    /// <summary>
    /// Uygulama iki kez güncellendi; ilk güncellemenin adımı ikinci açılışta yeniden çalışsaydı
    /// <c>CREATE TABLE</c> düşerdi ve profil açılmazdı.
    /// </summary>
    [Fact]
    public async Task EnsureInitializedAsync_CalismisAdimiTekrarCalistirmaz()
    {
        var baglanti = await GuncelVeritabaniAsync();
        var ilkGuncelleme = Adim(1, "CREATE TABLE deneme (Deger INTEGER NOT NULL);", "INSERT INTO deneme VALUES (2);");
        await Sema(ilkGuncelleme).EnsureInitializedAsync(baglanti);

        await Sema(ilkGuncelleme, Adim(2, "UPDATE deneme SET Deger = Deger * 10;")).EnsureInitializedAsync(baglanti);

        Assert.Equal(Guncel + 2, await SurumAsync(baglanti));
        Assert.Equal(20, await baglanti.ExecuteScalarAsync<int>("SELECT Deger FROM deneme;"));
    }

    [Fact]
    public async Task EnsureInitializedAsync_AdimlardanBiriDuserse_HicbiriYazilmazSurumDegismez()
    {
        var baglanti = await GuncelVeritabaniAsync();
        var sema = Sema(Adim(1, "CREATE TABLE deneme (Deger INTEGER);"), Adim(2, BozukKomut));

        await Assert.ThrowsAsync<SQLiteException>(() => sema.EnsureInitializedAsync(baglanti));

        Assert.Equal(Guncel, await SurumAsync(baglanti));
        Assert.DoesNotContain("deneme", await sema.GetTableNamesAsync(baglanti));
    }

    /// <summary>
    /// Kurulum yarıda kalsaydı yarım şema ve sürüm 0 kalırdı; sonraki açılış "table already exists"
    /// ile düşer, profil bir daha açılmazdı.
    /// </summary>
    [Fact]
    public async Task EnsureInitializedAsync_BosVeritabaninKurulumuDuserse_YarimSemaKalmaz()
    {
        var baglanti = Ac();
        var sema = Sema(Adim(1, BozukKomut));

        await Assert.ThrowsAsync<SQLiteException>(() => sema.EnsureInitializedAsync(baglanti));

        Assert.Equal(0, await SurumAsync(baglanti));
        Assert.Empty(await sema.GetTableNamesAsync(baglanti));
    }

    /// <summary>
    /// Yabancı anahtar denetimi açıkken eski tabloyu silmek <c>ON DELETE CASCADE</c> ile krediye bağlı
    /// erken ödemeleri de silerdi; I7b'de gözlem tablosu aynı yoldan yeniden kurulacak.
    /// </summary>
    [Fact]
    public async Task EnsureInitializedAsync_TabloYenidenKurulurken_CocukKayitlarSilinmez()
    {
        var baglanti = await GuncelVeritabaniAsync();
        await KrediVeErkenOdemeYazAsync(baglanti);
        var sema = Sema(Adim(1, await YenidenKurmaKomutlariAsync(baglanti, "loans")));

        await sema.EnsureInitializedAsync(baglanti);

        Assert.Equal(Guncel + 1, await SurumAsync(baglanti));
        Assert.Equal(1, await baglanti.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM loan_prepayments;"));
    }

    [Fact]
    public async Task EnsureInitializedAsync_GocKopukBagBirakirsa_HicbirSeyYazilmaz()
    {
        var baglanti = await GuncelVeritabaniAsync();
        await KrediVeErkenOdemeYazAsync(baglanti);
        var sema = Sema(Adim(1, "DELETE FROM loans;"));

        var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => sema.EnsureInitializedAsync(baglanti));

        Assert.Contains("yükseltilemedi", hata.Message);
        Assert.Equal(Guncel, await SurumAsync(baglanti));
        Assert.Equal(1, await baglanti.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM loans;"));
    }

    [Fact]
    public async Task EnsureInitializedAsync_GocDusse_YabanciAnahtarDenetimiAcikKalir()
    {
        var baglanti = await GuncelVeritabaniAsync();
        var sema = Sema(Adim(1, BozukKomut));

        await Assert.ThrowsAsync<SQLiteException>(() => sema.EnsureInitializedAsync(baglanti));

        Assert.Equal(1, await baglanti.ExecuteScalarAsync<int>("PRAGMA foreign_keys;"));
    }

    [Fact]
    public void Yapici_ArayaAdimEksikse_Reddeder()
    {
        Assert.Throws<ArgumentException>(() => Sema(Adim(2, "SELECT 1;")));
    }

    [Fact]
    public void Yapici_GocListesiBossa_Reddeder()
    {
        Assert.Throws<ArgumentException>(() => new DatabaseSchema([]));
    }

    private static DatabaseSchema Sema(params SchemaMigration[] ekAdimlar) =>
        new([.. SchemaMigrations.All, .. ekAdimlar]);

    /// <summary>Üretimdeki güncel sürümün <paramref name="ileri"/> adım sonrası.</summary>
    private static SchemaMigration Adim(int ileri, params string[] komutlar) =>
        new(Guncel + ileri, komutlar);

    private SQLiteAsyncConnection Ac()
    {
        var baglanti = new SQLiteAsyncConnection(_yol);
        _baglantilar.Add(baglanti);
        return baglanti;
    }

    private async Task<SQLiteAsyncConnection> GuncelVeritabaniAsync()
    {
        var baglanti = Ac();
        await new DatabaseSchema().EnsureInitializedAsync(baglanti);
        return baglanti;
    }

    private static Task<int> SurumAsync(SQLiteAsyncConnection baglanti) =>
        baglanti.ExecuteScalarAsync<int>("PRAGMA user_version;");

    private static async Task KrediVeErkenOdemeYazAsync(SQLiteAsyncConnection baglanti)
    {
        var kredi = new Loan
        {
            Id = Guid.NewGuid(),
            Name = "Konut",
            Bank = "İş Bankası",
            MonthlyPayment = 5000m,
            NextPaymentDate = new DateOnly(2026, 10, 1),
            RemainingInstallmentCount = 12
        };
        var depo = new SqliteLoanRepository(baglanti);
        await depo.UpsertLoanAsync(kredi);
        await depo.UpsertLoanPrepaymentAsync(new LoanPrepayment
        {
            Id = Guid.NewGuid(),
            LoanId = kredi.Id,
            Date = new DateOnly(2026, 11, 1),
            Mode = LoanPrepaymentMode.ReduceTerm,
            PrincipalAmount = 100000m
        });
    }

    /// <summary>
    /// SQLite'ın bir tablonun şeklini değiştirmek için önerdiği yol: yenisini kur, veriyi taşı,
    /// eskisini sil, yenisinin adını değiştir.
    /// </summary>
    private static async Task<string[]> YenidenKurmaKomutlariAsync(SQLiteAsyncConnection baglanti, string tablo)
    {
        var tanim = await baglanti.ExecuteScalarAsync<string>(
            "SELECT sql FROM sqlite_master WHERE type = 'table' AND name = ?;", tablo);
        var yeni = $"{tablo}_yeni";
        return
        [
            Regex.Replace(tanim, $@"^CREATE TABLE (IF NOT EXISTS )?{tablo}\b", $"CREATE TABLE {yeni}"),
            $"INSERT INTO {yeni} SELECT * FROM {tablo};",
            $"DROP TABLE {tablo};",
            $"ALTER TABLE {yeni} RENAME TO {tablo};"
        ];
    }
}
