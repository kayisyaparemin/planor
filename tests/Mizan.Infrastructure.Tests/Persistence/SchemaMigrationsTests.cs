using System.Security.Cryptography;
using System.Text;
using Mizan.Infrastructure.Persistence;

namespace Mizan.Infrastructure.Tests.Persistence;

/// <summary>
/// Yayımlanmış göç adımlarını donduran test (S69, kural 05). Telefondaki veritabanları bir adımı bir kez
/// çalıştırır; adımın metni sonradan değişirse yeni kurulum ile eski profil sessizce farklı şemalara varır.
/// Yeni bir adım yayımlanırken özeti bu tabloya bilerek eklenir.
/// </summary>
public sealed class SchemaMigrationsTests
{
    private static readonly IReadOnlyDictionary<int, string> DondurulmusOzetler = new Dictionary<int, string>
    {
        [1] = "82AF1E2FF84441DB4ECB081D197BE341FD2225D0B9E9124D6F1E71445BE564A2",
        [2] = "E76B731859E751CED2F0671E5E8F7A0A6A853737212B6181D15372ECE0443A00",
        [3] = "F6D72767D1C81AA1A19890FF0FBAE631200CA5668349B2DDF4DE7CF822E308FB",
    };

    [Fact]
    public void YayimlanmisAdimlar_Degismez()
    {
        var ozetler = SchemaMigrations.All.ToDictionary(adim => adim.Version, Ozet);

        Assert.Equal(DondurulmusOzetler, ozetler);
    }

    private static string Ozet(SchemaMigration adim)
    {
        // Satır sonu, dosyanın diskteki biçimine göre değişebilir; özet komutların metnini dondurur.
        var metin = string.Join("\n", adim.Commands).ReplaceLineEndings("\n");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(metin)));
    }
}
