using Mizan.Application.Models;
using Mizan.Infrastructure.LegacyImport;
using Mizan.Infrastructure.Tests.Backup;
using SQLite;

namespace Mizan.Infrastructure.Tests.LegacyImport;

/// <summary>
/// İçe aktarıcı testlerinin ortak zemini: gerçek geçici klasörde profil deposu, içe aktarıcı ve içe
/// aktarılan profilin veritabanını doğrudan sorgulayan yardımcılar. Sonuç, repository'ler yerine ham
/// SQL ile okunur ki dönüşümün yazdığı satırlar bir okuyucunun yorumundan bağımsız sınansın.
/// </summary>
internal sealed class Eski17ImportKurulumu : IDisposable
{
    private readonly YedekTestKurulumu _yedek = new();

    public Eski17ImportKurulumu()
    {
        Aktarici = new LegacyBackupImporter(_yedek.Depo, _yedek.Depo, _yedek.Sema);
    }

    public LegacyBackupImporter Aktarici { get; }

    public YedekTestKurulumu Yedek => _yedek;

    public void Dispose() => _yedek.Dispose();

    /// <summary>Kaynak profili yeni bir kimlik ve adla içe aktarır ve hedef kimliği döner.</summary>
    public async Task<Guid> AktarAsync(MemoryStream yedek, Guid kaynakId, string hedefAd = "Aktarılan")
    {
        var hedef = new UserProfile { Id = Guid.NewGuid(), Name = hedefAd, CreatedAt = YedekTestKurulumu.Simdi };
        await Aktarici.ImportAsync(yedek, [new ProfileImport(kaynakId, hedef)]);
        return hedef.Id;
    }

    /// <summary>İçe aktarılan profilin veritabanında tek değerli bir sorgu çalıştırır.</summary>
    public T Deger<T>(Guid profilId, string sql) => Baglan(profilId, baglanti => baglanti.ExecuteScalar<T>(sql));

    /// <summary>İçe aktarılan profilin veritabanında tek sütunlu bir sorgunun satırlarını okur.</summary>
    public IReadOnlyList<T> Satirlar<T>(Guid profilId, string sql) =>
        Baglan(profilId, baglanti => (IReadOnlyList<T>)baglanti.QueryScalars<T>(sql));

    /// <summary>Veritabanında yabancı anahtarı kopuk satır sayısı.</summary>
    public int KopukBagSayisi(Guid profilId) =>
        Deger<int>(profilId, "SELECT COUNT(*) FROM pragma_foreign_key_check;");

    private TSonuc Baglan<TSonuc>(Guid profilId, Func<SQLiteConnection, TSonuc> oku)
    {
        using var baglanti = new SQLiteConnection(_yedek.Depo.GetDatabasePath(profilId), SQLiteOpenFlags.ReadOnly);
        return oku(baglanti);
    }
}
