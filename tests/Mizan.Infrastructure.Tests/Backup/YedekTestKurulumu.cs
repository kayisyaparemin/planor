using System.IO.Compression;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;

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

    public YedekTestKurulumu()
    {
        Kok = YeniGeciciKlasor("mizan_yedek_test");
        Depo = new FileSystemProfileRepository(Kok);
        Anahtar = new SqliteProfileStoreSwitch(Depo, new SqliteConnectionFactory());
    }

    public string Kok { get; }

    public FileSystemProfileRepository Depo { get; }

    public SqliteProfileStoreSwitch Anahtar { get; }

    public ProfileBackupArchive Arsiv(IProfileRepository? profiller = null) =>
        new(profiller ?? Depo, Depo, new SabitSaat(Simdi));

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
