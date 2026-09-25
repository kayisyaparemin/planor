using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;

namespace Mizan.Infrastructure.Tests.Persistence;

/// <summary>
/// İki farklı profilin verilerinin birbirine karışmadığını doğrulayan izolasyon testleri.
/// Bir profile yazılan kredi kaydı, diğer profilde görünmemelidir.
/// </summary>
public sealed class ProfileDataIsolationTests : IDisposable
{
    private readonly string _rootDirectory;
    private readonly FileSystemProfileRepository _layout;
    private readonly SqliteConnectionFactory _connectionFactory;

    public ProfileDataIsolationTests()
    {
        _rootDirectory = Path.Combine(
            Path.GetTempPath(),
            $"mizan_izolasyon_test_{Guid.NewGuid():N}");
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

    [Fact]
    public async Task IkiProfil_BirineYazilanVeri_DigerindeBelirmez()
    {
        var anahtar = new SqliteProfileStoreSwitch(_layout, _connectionFactory);
        var profilA = Guid.NewGuid();
        var profilB = Guid.NewGuid();

        // Profil A'ya bir kredi yaz
        await anahtar.OpenAsync(profilA);
        var depoA = new SqliteLoanRepository(anahtar.Connection);
        var krediId = Guid.NewGuid();
        await depoA.UpsertLoanAsync(new Loan
        {
            Id = krediId,
            Name = "Ayşe Kredisi",
            Bank = "İş Bankası",
            MonthlyPayment = 5000m,
            NextPaymentDate = new DateOnly(2026, 10, 1),
            RemainingInstallmentCount = 12
        });

        // Profil A'daki veri doğrula
        var kredilerA = await depoA.GetLoansAsync();
        Assert.Single(kredilerA);

        // Profil B'ye geç
        await anahtar.OpenAsync(profilB);
        var depoB = new SqliteLoanRepository(anahtar.Connection);

        // Profil B boş olmalı — Ayşe'nin kredisi burada olmamalı
        var kredilerB = await depoB.GetLoansAsync();
        Assert.Empty(kredilerB);

        // Profil A'ya geri dön, veri hâlâ orada
        await anahtar.OpenAsync(profilA);
        var depoATekrar = new SqliteLoanRepository(anahtar.Connection);
        var kredilerATekrar = await depoATekrar.GetLoansAsync();
        Assert.Single(kredilerATekrar);
        Assert.Equal(krediId, kredilerATekrar[0].Id);

        await anahtar.DisposeAsync();
    }

    [Fact]
    public async Task IkiProfil_BirindeSilmek_DigerininkineDokunmaz()
    {
        var anahtar = new SqliteProfileStoreSwitch(_layout, _connectionFactory);
        var profilA = Guid.NewGuid();
        var profilB = Guid.NewGuid();

        // Her iki profile birer kredi ekle
        await anahtar.OpenAsync(profilA);
        var krediA = new Loan
        {
            Id = Guid.NewGuid(),
            Name = "Profil A Kredisi",
            Bank = "Yapı Kredi",
            MonthlyPayment = 3000m,
            NextPaymentDate = new DateOnly(2026, 11, 5),
            RemainingInstallmentCount = 6
        };
        await new SqliteLoanRepository(anahtar.Connection).UpsertLoanAsync(krediA);

        await anahtar.OpenAsync(profilB);
        var krediB = new Loan
        {
            Id = Guid.NewGuid(),
            Name = "Profil B Kredisi",
            Bank = "Garanti",
            MonthlyPayment = 8000m,
            NextPaymentDate = new DateOnly(2026, 12, 1),
            RemainingInstallmentCount = 24
        };
        await new SqliteLoanRepository(anahtar.Connection).UpsertLoanAsync(krediB);

        // Profil B'deki krediyi sil
        await new SqliteLoanRepository(anahtar.Connection).DeleteLoanAsync(krediB.Id);
        var profilBSonuc = await new SqliteLoanRepository(anahtar.Connection).GetLoansAsync();
        Assert.Empty(profilBSonuc);

        // Profil A'daki kredi hâlâ duruyor
        await anahtar.OpenAsync(profilA);
        var profilASonuc = await new SqliteLoanRepository(anahtar.Connection).GetLoansAsync();
        Assert.Single(profilASonuc);
        Assert.Equal(krediA.Id, profilASonuc[0].Id);

        await anahtar.DisposeAsync();
    }
}
