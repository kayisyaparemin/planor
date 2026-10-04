using System.Reflection;
using System.Text.Json;
using Mizan.Infrastructure.Tests.Backup;
using SQLite;

namespace Mizan.Infrastructure.Tests.LegacyImport;

/// <summary>
/// Eski uygulamanın (şema v17) yedek zip'ini test için üretir: gömülü DDL dökümü ve uydurma örnek veriyle
/// gerçek bir SQLite dosyası kurar, <c>mizan-backup.json</c> (biçim 1) ve <c>coinflow.db3</c> girdisiyle paketler.
/// Gerçek kullanıcı verisi testlere girmez; şema telefondan alınmış bir yedekten yalnız DDL olarak çıkarıldı.
/// </summary>
internal static class Eski17Yedegi
{
    public static readonly Guid Emin = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid Gizem = Guid.Parse("22222222-2222-4222-8222-222222222222");

    /// <summary>Tek profilli eski yedek; <paramref name="ozellestir"/> örnek verinin üstüne SQL uygular.</summary>
    public static MemoryStream Olustur(Guid profilId, string ad, params string[] ozellestir) =>
        Olustur([(profilId, ad, ozellestir)]);

    /// <summary>Birden çok profilli eski yedek; her profil kendi özelleştirmesini taşır.</summary>
    public static MemoryStream Olustur(params (Guid Id, string Ad, string[] Ozellestir)[] profiller)
    {
        var girdiler = profiller
            .Select(p => ($"profiles/{p.Id:N}/coinflow.db3", VeritabaniKur(p.Ozellestir)))
            .ToArray();
        return YedekTestKurulumu.Zip(Manifest(profiller.Select(p => (p.Id, p.Ad, true))), girdiler);
    }

    /// <summary>Hiç açılmamış profil: manifestte var, veritabanı girdisi yok.</summary>
    public static MemoryStream VerisizProfil(Guid profilId, string ad) =>
        YedekTestKurulumu.Zip(Manifest([(profilId, ad, false)]));

    private static string Manifest(IEnumerable<(Guid Id, string Ad, bool VeriVar)> profiller) =>
        JsonSerializer.Serialize(new
        {
            Format = 1,
            CreatedAt = new DateTimeOffset(2026, 9, 25, 21, 30, 0, TimeSpan.Zero),
            SchemaVersion = 17,
            Profiles = profiller.Select(p => new
            {
                p.Id,
                Name = p.Ad,
                CreatedAt = new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero),
                LastOpenedAt = (DateTimeOffset?)null,
                HasData = p.VeriVar
            })
        });

    private static byte[] VeritabaniKur(IReadOnlyList<string> ozellestir)
    {
        var yol = Path.Combine(Path.GetTempPath(), $"eski17_{Guid.NewGuid():N}.db3");
        try
        {
            using (var baglanti = new SQLiteConnection(yol))
            {
                Calistir(baglanti, Kaynak("v17-sema.sql"));
                Calistir(baglanti, Kaynak("v17-ornek-veri.sql"));
                ozellestir.ToList().ForEach(sql => Calistir(baglanti, sql));
            }

            return File.ReadAllBytes(yol);
        }
        finally
        {
            File.Delete(yol);
        }
    }

    // sqlite-net bir çağrıda tek ifade çalıştırır; örnek dosyalarda ifade sonu dışında noktalı virgül yok.
    private static void Calistir(SQLiteConnection baglanti, string betik)
    {
        var ifadeler = betik
            .Split('\n')
            .Where(satir => !satir.TrimStart().StartsWith("--", StringComparison.Ordinal))
            .Aggregate(string.Empty, (toplam, satir) => $"{toplam}{satir}\n")
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var ifade in ifadeler)
        {
            baglanti.Execute(ifade);
        }
    }

    private static string Kaynak(string ad)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var kaynakAdi = assembly.GetManifestResourceNames().Single(isim => isim.EndsWith(ad, StringComparison.Ordinal));
        using var akis = assembly.GetManifestResourceStream(kaynakAdi)!;
        using var okuyucu = new StreamReader(akis);
        return okuyucu.ReadToEnd();
    }
}
