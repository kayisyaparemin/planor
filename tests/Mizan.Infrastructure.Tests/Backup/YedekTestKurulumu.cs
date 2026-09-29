using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Backup;

/// <summary>
/// Yedek testleri için gerçek geçici klasörde profil kurulumu: profil meta dosyaları
/// <see cref="FileSystemProfileRepository"/> ile, profil verisi <see cref="SqliteProfileStoreSwitch"/>
/// üzerinden gerçek SQLite veritabanına yazılır.
/// </summary>
internal sealed class YedekTestKurulumu : IDisposable
{
    public static readonly DateTimeOffset Simdi = new(2026, 9, 25, 21, 30, 0, TimeSpan.Zero);

    private readonly List<string> _geciciKlasorler = [];

    /// <summary>
    /// <paramref name="sema"/> verilirse profil açılışı ve arşiv o şemayla çalışır; testler üretim
    /// listesine sahte göç adımları ekleyerek uygulamanın daha yeni bir sürümünü taklit eder.
    /// </summary>
    public YedekTestKurulumu(DatabaseSchema? sema = null)
    {
        Kok = YeniGeciciKlasor("mizan_yedek_test");
        Depo = new FileSystemProfileRepository(Kok);
        Sema = sema ?? new DatabaseSchema();
        Anahtar = new SqliteProfileStoreSwitch(Depo, new SqliteConnectionFactory(Sema));
    }

    public string Kok { get; }

    public FileSystemProfileRepository Depo { get; }

    public DatabaseSchema Sema { get; }

    public SqliteProfileStoreSwitch Anahtar { get; }

    public ProfileBackupArchive Arsiv(IProfileRepository? profiller = null) =>
        new(profiller ?? Depo, Depo, new SabitSaat(Simdi), Sema);

    public async Task<UserProfile> ProfilEkleAsync(string ad, DateTimeOffset olusturma)
    {
        var profil = new UserProfile { Id = Guid.NewGuid(), Name = ad, CreatedAt = olusturma };
        await Depo.SaveProfileAsync(profil);
        return profil;
    }

    /// <summary>Profili açar, bir kredi yazar ve profili açık bırakır.</summary>
    public async Task KrediYazAsync(Guid profilId, string krediAdi)
    {
        await Anahtar.OpenAsync(profilId);
        await new SqliteLoanRepository(Anahtar.Connection).UpsertLoanAsync(new Loan
        {
            Id = Guid.NewGuid(),
            Name = krediAdi,
            Bank = "İş Bankası",
            MonthlyPayment = 5000m,
            NextPaymentDate = new DateOnly(2026, 10, 1),
            RemainingInstallmentCount = 12
        });
    }

    public async Task<MemoryStream> YedekAlAsync()
    {
        var yedek = new MemoryStream();
        await Arsiv().WriteAsync(yedek);
        yedek.Position = 0;
        return yedek;
    }

    /// <summary>Profili açıp kredi adlarını okur ve kapatır; açılış şema kurulumundan geçer.</summary>
    public async Task<IReadOnlyList<string>> KrediAdlariAsync(Guid profilId)
    {
        await Anahtar.OpenAsync(profilId);
        try
        {
            var krediler = await new SqliteLoanRepository(Anahtar.Connection).GetLoansAsync();
            return krediler.Select(k => k.Name).Order().ToArray();
        }
        finally
        {
            await Anahtar.CloseAsync();
        }
    }

    /// <summary>Profil veritabanının <c>user_version</c> değerini doğrudan değiştirir.</summary>
    public void SemaSurumunuDegistir(Guid profilId, int surum)
    {
        using var baglanti = new SQLiteConnection(Depo.GetDatabasePath(profilId));
        baglanti.Execute($"PRAGMA user_version = {surum};");
    }

    /// <summary>Kökte kalmış geri yükleme hazırlık klasörlerini listeler.</summary>
    public IReadOnlyList<string> HazirlikKlasorleri() => Directory.GetDirectories(Kok, ".restore-*");

    /// <summary>Verilen manifest metni ve girdilerle bir zip akışı kurar; eski ya da elle bozulmuş yedekleri taklit eder.</summary>
    public static MemoryStream Zip(string? manifest, params (string Ad, byte[] Icerik)[] girdiler)
    {
        var akis = new MemoryStream();
        using (var zip = new ZipArchive(akis, ZipArchiveMode.Create, leaveOpen: true))
        {
            var hepsi = manifest is null
                ? girdiler
                : girdiler.Prepend(("mizan-backup.json", Encoding.UTF8.GetBytes(manifest)));
            foreach (var (ad, icerik) in hepsi)
            {
                using var yazilan = zip.CreateEntry(ad).Open();
                yazilan.Write(icerik);
            }
        }

        akis.Position = 0;
        return akis;
    }

    /// <summary>Yedeği girdi girdi kopyalar; <paramref name="degistir"/> girdinin yeni içeriğini ya da atmak için null döner.</summary>
    public static MemoryStream Kurcala(MemoryStream yedek, Func<string, byte[], byte[]?> degistir)
    {
        var girdiler = new List<(string Ad, byte[] Icerik)>();
        yedek.Position = 0;
        using (var zip = new ZipArchive(yedek, ZipArchiveMode.Read, leaveOpen: true))
        {
            foreach (var girdi in zip.Entries)
            {
                using var okunan = girdi.Open();
                using var bellek = new MemoryStream();
                okunan.CopyTo(bellek);
                var yeni = degistir(girdi.FullName, bellek.ToArray());
                if (yeni is not null)
                {
                    girdiler.Add((girdi.FullName, yeni));
                }
            }
        }

        yedek.Position = 0;
        return Zip(null, [.. girdiler]);
    }

    /// <summary>Elle kurulan bir manifestin JSON metni; profiller sabit tarihlerle yazılır.</summary>
    public static string ManifestMetni(int bicim, int sema, params (Guid Id, string Ad, bool Veri)[] profiller) =>
        JsonSerializer.Serialize(new
        {
            Format = bicim,
            CreatedAt = Simdi,
            SchemaVersion = sema,
            Profiles = profiller.Select(p => new
            {
                p.Id,
                Name = p.Ad,
                CreatedAt = Simdi,
                LastOpenedAt = (DateTimeOffset?)null,
                HasData = p.Veri
            })
        });

    /// <summary>Yedekteki veritabanı girdisini geçici bir dosyaya çıkarır ve yolunu döner.</summary>
    public string VeritabaniGirdisiniCikar(ZipArchive zip, Guid profilId)
    {
        var girdi = zip.GetEntry($"profiles/{profilId:N}/mizan.db3");
        Assert.NotNull(girdi);
        var hedef = Path.Combine(YeniGeciciKlasor("mizan_yedek_cikti"), "cikti.db3");
        girdi.ExtractToFile(hedef);
        return hedef;
    }

    public void Dispose()
    {
        Anahtar.Dispose();
        foreach (var klasor in _geciciKlasorler.Where(Directory.Exists))
        {
            try { Directory.Delete(klasor, recursive: true); } catch { /* temizlik */ }
        }
    }

    private string YeniGeciciKlasor(string onek)
    {
        var klasor = Path.Combine(Path.GetTempPath(), $"{onek}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(klasor);
        _geciciKlasorler.Add(klasor);
        return klasor;
    }

    private sealed class SabitSaat(DateTimeOffset simdi) : IClock
    {
        public DateOnly Today => DateOnly.FromDateTime(simdi.UtcDateTime);

        public DateTimeOffset UtcNow => simdi;
    }
}
